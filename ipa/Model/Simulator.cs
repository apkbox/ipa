// --------------------------------------------------------------------------------
// <copyright file="Simulator.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the Simulator type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Common.Logging;

    public class Simulator
    {
        #region Static Fields

        private static readonly ILog Log = LogManager.GetLogger<Simulator>();

        #endregion

        #region Fields

        private readonly ISchedule schedule = new QuaterlySchedule();

        private bool isStoppedBySchedule = false;

        private SimulationParameters simParams;

        private IList<TradeRequest> tradeOrders;

        #endregion

        #region Constructors and Destructors

        public Simulator(SimulationParameters parameters)
        {
            this.simParams = parameters;
            Log.InfoFormat(
                "Simulating portfolio '{0}' using model portfolio '{1}'",
                parameters.InitialPortfolio.Name,
                parameters.ModelPortfolio.Name);

            this.CurrentDate = parameters.InceptionDate;
            this.Portfolio = parameters.InitialPortfolio;
            this.Portfolio.TransactionFee = parameters.TransactionFee;

            if (parameters.SetInitialBookCost)
            {
                this.UpdateHoldingsInitialBookCost();
            }

            if (parameters.ForceInitialRebalancing)
            {
                Log.InfoFormat("Performing initial rebalancing on {0:D}", this.CurrentDate);
                this.UpdateHoldingsMarketValue();
                this.tradeOrders = this.Portfolio.RebalancingStrategy.Rebalance(
                    parameters.ModelPortfolio,
                    this.Portfolio);
            }
        }

        #endregion

        #region Properties

        private DateTime CurrentDate { get; set; }

        private Portfolio Portfolio { get; set; }

        #endregion

        #region Public Methods and Operators

        public void DefaultScheduleHandler()
        {
            // TODO: Replace with callback
            var result = this.Portfolio.RebalancingStrategy.Check(this.simParams.ModelPortfolio, this.Portfolio);
            if (result)
            {
                Log.InfoFormat("Performing rebalancing on {0:D}", this.CurrentDate);
                this.tradeOrders = this.Portfolio.RebalancingStrategy.Rebalance(
                    this.simParams.ModelPortfolio,
                    this.Portfolio);
            }
            else
            {
                Log.InfoFormat("ScheduledStop time arrived on {0:D}, but was not required", this.CurrentDate);
            }
        }

        public void PrintPortfolioStats()
        {
            Log.Info(string.Empty);
            Log.Info("Ticker  Unts   Book Pr    Book Cost Mar Price    Mar Value    Dividends   MgmCost      Alloc");
            Log.Info("============================================================================================");
            foreach (var asset in this.Portfolio.Holdings)
            {
                var currentAllocation = asset.MarketValue / this.Portfolio.MarketValue;
                Log.InfoFormat(
                    "{0,7} {1,4} {2,9:C} {3,12:C} {4,9:C} {5,12:C} {6,12:C} {7,9:C} {8,10:P}",
                    asset.Security.Ticker,
                    asset.Units,
                    asset.BookPrice,
                    asset.BookCost,
                    asset.LastPrice,
                    asset.MarketValue,
                    asset.DividendsPaid,
                    asset.ManagementCost,
                    currentAllocation);
            }

            var bookCost = this.Portfolio.Holdings.Sum(o => o.BookCost);
            var dividendsPaid = this.Portfolio.Holdings.Sum(o => o.DividendsPaid);

            // BUG: This is calculated incorrectly as it does not account for trades related to removed assets.
            // HACK: Do not remove assets with zero quantity after trading for now.
            var managementExpenses = this.Portfolio.Holdings.Sum(o => o.ManagementCost);

            Log.Info("--------------------------------------------------------------------------------------------");
            Log.InfoFormat(
                "Total                  {0,12:C}           {1,12:C} {2,12:C} {3,9:C}",
                bookCost,
                this.Portfolio.MarketValue,
                dividendsPaid,
                managementExpenses);

            var totalReturn = this.Portfolio.MarketValue - bookCost + dividendsPaid - managementExpenses;
            Log.InfoFormat("Total return: {0:P}   {1:C}", totalReturn / bookCost, totalReturn);
        }

        public bool ResumeSimulation()
        {
            while (this.CurrentDate <= this.simParams.StopDate)
            {
                if (!this.isStoppedBySchedule)
                {
                    if (this.schedule.IsArrived(this.CurrentDate))
                    {
                        this.isStoppedBySchedule = true;
                        return true;
                    }
                }

                this.isStoppedBySchedule = false;

                this.SimulateIntraday(this.simParams);

                this.CurrentDate = this.CurrentDate.AddDays(1);
            }

            return false;
        }

        #endregion

        #region Methods

        private void ExecuteTradeOrders()
        {
            Log.InfoFormat("Executing trade orders on {0:D}", this.CurrentDate);

            var cashPosition = this.Portfolio.GetCashPosition();
            var cash = cashPosition.BookCost;

            Log.InfoFormat("Cash position: {0:C}", cash);

            foreach (var to in this.tradeOrders)
            {
                if (to.Amount == 0)
                {
                    Log.FatalFormat("No op trade order for {0}, Amount: {1:C}", to.Security.Ticker, to.Amount);
                    Debug.Fail("No op trade order");
                }

                var priceEntry = to.Security.GetPriceEntry(this.CurrentDate);
                if (priceEntry == null)
                {
                    Log.FatalFormat("Price for {0} on {1:D} is not available", to.Security.Ticker, this.CurrentDate);
                    Debug.Fail("Price not available");
                }

                var spotPrice = priceEntry.AveragePrice;

                var fee = to.Amount < 0
                              ? (to.Security.SellTransactionFee ?? this.Portfolio.TransactionFee)
                              : (to.Security.BuyTransactionFee ?? this.Portfolio.TransactionFee);

                var units =
                    Math.Abs(
                        to.Security.AllowsPartialShares ? to.Amount / spotPrice : Math.Truncate(to.Amount / spotPrice));

                Log.InfoFormat(
                    "Projection for {0} at {1:C} for {2} units. Fee {3:C}",
                    to.Security.Ticker,
                    spotPrice,
                    units,
                    fee);

                var asset = this.Portfolio.Holdings.FirstOrDefault(o => o.Security.Ticker == to.Security.Ticker);
                if (asset == null)
                {
                    Log.Info("Asset currently not owned.");

                    if (to.Amount < 0)
                    {
                        Log.Fatal("Attempt to sell asset not being own.");
                        Debug.Fail("Attempt to sell asset not being own.");
                    }

                    Log.InfoFormat("Creating portfolio position for {0}", to.Security.Ticker);
                    asset = new Asset(to.Security);
                    this.Portfolio.Holdings.Add(asset);
                }

                // Check if we try to sell more units than own
                if (to.Amount < 0 && asset.Units < units)
                {
                    Log.InfoFormat("Selling more units ({0}) than owned ({1})", units, asset.Units);
                    units = asset.Units;
                }

                var tradingBalance = units * spotPrice;
                Log.InfoFormat("Final price tag {0:C} for {1} units at {2:C}", tradingBalance, units, spotPrice);

                if (to.Amount < 0)
                {
                    asset.Units -= units;
                    asset.BookCost -= tradingBalance - fee;
                }
                else
                {
                    asset.Units += units;
                    asset.BookCost += tradingBalance + fee;
                }

                Log.InfoFormat("{0} units and {1:C} book cost after adjustment", asset.Units, asset.BookCost);

                asset.ManagementCost += fee;
                Log.InfoFormat("Total of management fees {0:C}", asset.ManagementCost);

                Debug.Assert(asset.Units >= 0, "Negative units");

                // HACK: Do not remove as it removes management cost
                if (false && asset.Units == 0 && asset.Security.FixedPrice == null)
                {
                    Log.InfoFormat("All units sold. Removing position from portfolio.");
                    this.Portfolio.Holdings.Remove(asset);
                }

                // Update cash position (TODO: Dang! should ignore it for cash position!)
                // HACK: Figure that security is actual cash if it has fixed price.
                // In this case do not update running cash balance.
                if (to.Security.FixedPrice == null)
                {
                    if (to.Amount < 0)
                    {
                        cash += tradingBalance - fee;
                    }
                    else
                    {
                        cash -= tradingBalance + fee;
                    }

                    Log.InfoFormat("Cash position after trade {0:C}", cash);
                }
            }

            if (cash < 0)
            {
                Log.Fatal("Negative cash after executing trade orders.");
                Debug.Fail("Negative cash after executing trade orders.");
            }

            cashPosition.BookCost = cash;
            cashPosition.MarketValue = cashPosition.BookCost;
            cashPosition.Units = cashPosition.BookCost / cashPosition.LastPrice;

            Log.InfoFormat("Cash after all trades {0:C}", cash);
        }

        private void SimulateIntraday(SimulationParameters parameters)
        {
            if (this.tradeOrders != null)
            {
                this.ExecuteTradeOrders();
                this.tradeOrders = null;
            }

            this.UpdateHoldingsMarketValue();
        }

        private void UpdateHoldingsInitialBookCost()
        {
            Log.InfoFormat("Updating portfolio initial book cost on {0:D}", this.CurrentDate);

            // Update current market value
            foreach (var asset in this.Portfolio.Holdings)
            {
                if (asset.Security.FixedPrice != null)
                {
                    continue;
                }

                var priceEntry = asset.Security.GetLastPriceEntry(this.CurrentDate);
                asset.LastPrice = priceEntry.AveragePrice;
                asset.BookCost = priceEntry.AveragePrice * asset.Units;

                Log.InfoFormat("    {0,7} {1,9:C} {2,12:C}", asset.Security.Ticker, asset.BookPrice, asset.BookCost);
            }
        }

        private void UpdateHoldingsMarketValue()
        {
            Log.InfoFormat("Updating portfolio market value on {0:D}", this.CurrentDate);

            var cashPosition = this.Portfolio.GetCashPosition();
            var cash = cashPosition.BookCost;

            // Update current market value
            foreach (var asset in this.Portfolio.Holdings)
            {
                var priceEntry = asset.Security.GetLastPriceEntry(this.CurrentDate);
                var dividendEntry = asset.Security.GetDividends(this.CurrentDate);
                asset.LastPrice = priceEntry.AveragePrice;
                asset.MarketValue = priceEntry.AveragePrice * asset.Units;
                var dividends = dividendEntry * asset.Units;
                asset.DividendsPaid += dividends;
                cash += dividends;
            }

            cashPosition.BookCost = cash;
            cashPosition.MarketValue = cashPosition.BookCost;
            cashPosition.Units = cashPosition.BookCost / cashPosition.LastPrice;

            // Calculate portfolio total market value
            this.Portfolio.MarketValue = this.Portfolio.Holdings.Sum(o => o.MarketValue);

            foreach (var asset in this.Portfolio.Holdings)
            {
                var currentAllocation = asset.MarketValue / this.Portfolio.MarketValue;
                var dividendEntry = asset.Security.GetDividends(this.CurrentDate);
                Log.InfoFormat(
                    "    {0,7} {1,9:C} {2,9} {3,12:C}, Allocation: {4:P}",
                    asset.Security.Ticker,
                    asset.LastPrice,
                    dividendEntry,
                    asset.MarketValue,
                    currentAllocation);
            }

            Log.InfoFormat("Portfolio market value {0:C}", this.Portfolio.MarketValue);
        }

        #endregion
    }
}
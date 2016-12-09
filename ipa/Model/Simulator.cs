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

        private bool isResumed;

        private SimulationParameters simParams;

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

            this.UpdateHoldingsMarketValue();

            if (parameters.ForceInitialRebalancing)
            {
                Log.InfoFormat("Performing initial rebalancing on {0:D}", this.CurrentDate);
                this.TradePlan = this.Portfolio.RebalancingStrategy.Rebalance(parameters.ModelPortfolio, this.Portfolio);
            }
        }

        #endregion

        #region Public Properties

        public IList<TradePlanItem> TradePlan { get; set; }

        public TradingQueue TradingQueue { get; private set; }

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
                this.TradePlan = this.Portfolio.RebalancingStrategy.Rebalance(
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
                if (!this.isResumed)
                {
                    this.TradingQueue = new TradingQueue();
                    var cashEntry = this.Portfolio.GetCashEntry();
                    this.TradingQueue.Cash = cashEntry.BookCost;
                    this.TradingQueue.TransactionFee = this.simParams.TransactionFee;

                    this.TradePlan = new List<TradePlanItem>();

                    if (this.schedule.IsArrived(this.CurrentDate))
                    {
                        this.isResumed = true;
                        return true;
                    }
                }

                this.isResumed = false;

                this.PrepareTradeOrders();
                this.ExecuteTradeOrders(this.TradingQueue);
                this.SettleTrades();

                this.UpdateHoldingsMarketValue();

                this.CurrentDate = this.CurrentDate.AddDays(1);
            }

            return false;
        }

        #endregion

        #region Methods

        private void ExecuteTradeOrders(TradingQueue request)
        {
            Log.InfoFormat("Executing trade orders on {0:D}", this.CurrentDate);

            var cash = request.Cash;

            Log.InfoFormat("Cash before trades: {0:C}", cash);

            foreach (var order in request.TradeOrders.OrderBy(o => o.Units))
            {
                if (order.Units == 0)
                {
                    Log.FatalFormat("No op trade order for {0}, Units: {1}", order.Security.Ticker, order.Units);
                    Debug.Fail("No op trade order");
                }

                var quote = order.Security.GetPriceEntry(this.CurrentDate);
                if (quote == null)
                {
                    Log.FatalFormat("Quote for {0} on {1:D} is not available", order.Security.Ticker, this.CurrentDate);
                    Debug.Fail("Quote is not available");
                }

                // TODO: Use selector object to pick the transaction price - high, low, open, close or average.
                var spotPrice = order.Price ?? quote.AveragePrice;

                var fee = order.TransactionFee
                          ?? (order.Units < 0
                                  ? (order.Security.SellTransactionFee ?? request.TransactionFee)
                                  : (order.Security.BuyTransactionFee ?? request.TransactionFee));

                var unitsCost = order.Units * spotPrice;
                var units = order.UnitsOwned + order.Units;
                if (units < 0)
                {
                    Log.ErrorFormat("Selling more units ({0}) than owned ({1})", order.Units, order.UnitsOwned);
                }

                cash += unitsCost;
                cash -= fee;

                order.UnitsOwned = units;
                order.Price = spotPrice;
                order.TransactionFee = fee;
                order.TotalCost = unitsCost;
                order.TotalCost -= fee;
            }

            if (cash < 0)
            {
                Log.Fatal("Negative cash after executing trade orders.");
                Debug.Fail("Negative cash after executing trade orders.");
            }

            request.Cash = cash;
            Log.InfoFormat("Cash after all trades {0:C}", cash);
        }

        private void PrepareTradeOrders()
        {
            Log.InfoFormat("Preparing trade orders on {0:D}", this.CurrentDate);

            foreach (var item in this.TradePlan)
            {
                // Skip cash
                if (item.Security.FixedPrice != null)
                {
                    continue;
                }

                if (item.Amount == 0)
                {
                    Log.FatalFormat("No op trade plan item for {0}, Amount: {1:C}", item.Security.Ticker, item.Amount);
                    Debug.Fail("No op trade plan item");
                }

                var priceEntry = item.Security.GetPriceEntry(this.CurrentDate);
                if (priceEntry == null)
                {
                    Log.FatalFormat("Price for {0} on {1:D} is not available", item.Security.Ticker, this.CurrentDate);
                    Debug.Fail("Price not available");
                }

                var spotPrice = priceEntry.AveragePrice;

                var fee = item.Amount < 0
                              ? (item.Security.SellTransactionFee ?? this.Portfolio.TransactionFee)
                              : (item.Security.BuyTransactionFee ?? this.Portfolio.TransactionFee);

                var units = item.Security.AllowsPartialShares
                                ? item.Amount / spotPrice
                                : Math.Truncate(item.Amount / spotPrice);

                Log.InfoFormat(
                    "Projection for {0} at {1:C} for {2} units. Fee {3:C}",
                    item.Security.Ticker,
                    spotPrice,
                    units,
                    fee);

                var asset = this.Portfolio.Holdings.FirstOrDefault(o => o.Security.Ticker == item.Security.Ticker);
                if (asset == null)
                {
                    Log.Info("Asset currently not owned.");

                    if (item.Amount < 0)
                    {
                        Log.Fatal("Attempt to sell asset not being own.");
                        Debug.Fail("Attempt to sell asset not being own.");
                    }
                }
                else
                {
                    // Check if we try to sell more units than own
                    if (item.Amount < 0 && asset.Units < units)
                    {
                        Log.InfoFormat("Selling more units ({0}) than owned ({1})", units, asset.Units);
                        units = asset.Units;
                    }
                }

                var tradingBalance = units * spotPrice;
                Log.InfoFormat("Final price tag {0:C} for {1} units at {2:C}", tradingBalance, units, spotPrice);

                this.TradingQueue.TradeOrders.Add(
                    new TradeOrder
                        {
                            Security = item.Security,
                            Units = units,
                            UnitsOwned = asset == null ? 0 : asset.Units,
                        });
            }
        }

        private void SettleTrades()
        {
            var cashEntry = this.Portfolio.GetCashEntry();
            var cash = cashEntry.BookCost;

            Log.InfoFormat("Cash available: {0:C}", cash);

            foreach (var order in this.TradingQueue.TradeOrders)
            {
                var asset = this.Portfolio.Holdings.FirstOrDefault(o => o.Security.Ticker == order.Security.Ticker);
                if (asset == null)
                {
                    asset = new Asset(order.Security)
                                {
                                    Units = order.UnitsOwned,
                                    BookCost = order.TotalCost,
                                    ManagementCost = (decimal)order.TransactionFee
                                };
                    this.Portfolio.Holdings.Add(asset);
                }
                else
                {
                    asset.Units = order.UnitsOwned;
                    asset.BookCost += order.TotalCost;
                    asset.ManagementCost += (decimal)order.TransactionFee;
                }

                // Log.InfoFormat("{0} units and {1:C} book cost after adjustment", asset.Units, asset.BookCost);

                // Log.InfoFormat("Total of management fees {0:C}", asset.ManagementCost);
                Debug.Assert(asset.Units >= 0, "Negative units");
            }

            cash = this.TradingQueue.Cash;

            if (cash < 0)
            {
                Log.Fatal("Negative cash after executing trade orders.");
                Debug.Fail("Negative cash after executing trade orders.");
            }

            cashEntry.BookCost = cash;
            cashEntry.MarketValue = cashEntry.BookCost;
            cashEntry.Units = cashEntry.BookCost / cashEntry.LastPrice;

            Log.InfoFormat("Cash after all trades {0:C}", cash);
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

            var cashPosition = this.Portfolio.GetCashEntry();
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

            Log.InfoFormat("   Ticker  Units   Book Pr    Book Cost   Last Pr Market Value    Dividends Allocation");

            foreach (var asset in this.Portfolio.Holdings)
            {
                var currentAllocation = asset.MarketValue / this.Portfolio.MarketValue;
                var dividendEntry = asset.Security.GetDividends(this.CurrentDate);
                Log.InfoFormat(
                    "  {0,7} {1,6} {2,9:C} {3,12:C} {4,9:C} {5,12:C} {6,12} {7,10:P}",
                    asset.Security.Ticker,
                    asset.Units,
                    asset.BookPrice,
                    asset.BookCost,
                    asset.LastPrice,
                    asset.MarketValue,
                    dividendEntry,
                    currentAllocation);
            }

            Log.InfoFormat("Portfolio market value {0:C}", this.Portfolio.MarketValue);
        }

        #endregion
    }
}
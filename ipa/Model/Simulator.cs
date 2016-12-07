﻿// --------------------------------------------------------------------------------
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

        private static ILog log = LogManager.GetLogger<Simulator>();

        #endregion

        #region Fields

        private IList<TradeOrderModel> tradeOrders;

        #endregion

        #region Properties

        private DateTime CurrentDate { get; set; }

        private DateTime LastRebalancing { get; set; }

        private PortfolioModel Portfolio { get; set; }

        #endregion

        #region Public Methods and Operators

        public void SimulatePortfolio(SimulationParameters parameters)
        {
            log.InfoFormat(
                "Simulating portfolio '{0}' using model portfolio '{1}'", 
                parameters.InitialPortfolio.Name, 
                parameters.ModelPortfolio.Name);

            this.CurrentDate = parameters.InceptionDate;
            this.Portfolio = parameters.InitialPortfolio;
            this.Portfolio.TransactionFee = parameters.TransactionFee;

            if (parameters.ForceInitialRebalancing)
            {
                log.InfoFormat("Performing initial rebalancing on {0:D}", this.CurrentDate);
                this.UpdateHoldingsMarketValue();
                this.tradeOrders = this.Portfolio.RebalancingStrategy.Rebalance(
                    parameters.ModelPortfolio, 
                    this.Portfolio);
                this.LastRebalancing = this.CurrentDate;
            }

            // Walk the timeline and update market price, calculate dividends and perform rebalancing
            // if needed.
            while (this.CurrentDate <= parameters.StopDate)
            {
                if (this.tradeOrders != null)
                {
                    this.ExecuteTradeOrders();
                    this.tradeOrders = null;
                }

                this.CurrentDate = this.CurrentDate.AddDays(1);

                this.UpdateHoldingsMarketValue();

                var elapsedSinceLastRebalancing = this.CurrentDate.Subtract(this.LastRebalancing);

                // Check if rebalancing is needed
                if (this.tradeOrders == null)
                {
                    var result = this.Portfolio.RebalancingStrategy.Check(
                        elapsedSinceLastRebalancing,
                        parameters.ModelPortfolio,
                        this.Portfolio);
                    if (result == RebalancingCheckResult.Rebalance)
                    {
                        log.InfoFormat("Performing rebalancing on {0:D}", this.CurrentDate);
                        this.tradeOrders = this.Portfolio.RebalancingStrategy.Rebalance(
                            parameters.ModelPortfolio, 
                            this.Portfolio);
                        this.LastRebalancing = this.CurrentDate;
                    }
                    else if (result == RebalancingCheckResult.Skipped)
                    {
                        log.InfoFormat("Rebalancing time arrived on {0:D}, but was not required", this.CurrentDate);
                        this.LastRebalancing = this.CurrentDate;
                    }
                }
            }
        }

        public void StartSimulation(SimulationParameters parameters)
        {
            // TODO: Sets up simulation
        }

        public SimulationState StepSimulation()
        {
            // TODO: Simulate intraday
            return SimulationState.Stopped;
        }

        #endregion

        #region Methods

        private void ExecuteTradeOrders()
        {
            log.InfoFormat("Executing trade orders on {0:D}", this.CurrentDate);

            var cashPosition = this.Portfolio.GetCashPosition();
            var cash = cashPosition.BookCost;

            log.InfoFormat("Cash position: {0:C}", cash);

            foreach (var to in this.tradeOrders)
            {
                if (to.Amount == 0)
                {
                    log.FatalFormat("No op trade order for {0}, Amount: {1:C}", to.Security.Ticker, to.Amount);
                    Debug.Fail("No op trade order");
                }

                var priceEntry = to.Security.GetPriceEntry(this.CurrentDate);
                if (priceEntry == null)
                {
                    log.FatalFormat("Price for {0} on {1:d} is not available", to.Security.Ticker, this.CurrentDate);
                    Debug.Fail("Price not available");
                }

                var spotPrice = priceEntry.AveragePrice;

                var fee = to.Amount < 0
                              ? (to.Security.SellTransactionFee ?? this.Portfolio.TransactionFee)
                              : (to.Security.BuyTransactionFee ?? this.Portfolio.TransactionFee);

                var units = Math.Abs(
                    to.Security.AllowsPartialShares
                        ? to.Amount / spotPrice
                        : Math.Truncate(to.Amount / spotPrice));

                log.InfoFormat(
                    "Planning for {0} at {1:C} for {2} units. Fee {3:C}", 
                    to.Security.Ticker, 
                    spotPrice, 
                    units, 
                    fee);

                var asset = this.Portfolio.Holdings.FirstOrDefault(o => o.Security.Ticker == to.Security.Ticker);
                if (asset == null)
                {
                    log.Info("Asset currently not owned.");

                    if (to.Amount < 0)
                    {
                        log.Fatal("Attempt to sell asset not being own.");
                        Debug.Fail("Attempt to sell asset not being own.");
                    }

                    log.InfoFormat("Creating portfolio position for {0}", to.Security.Ticker);
                    asset = new PortfolioAssetModel(to.Security);
                    this.Portfolio.Holdings.Add(asset);
                }

                // Check if we try to sell more units than own
                if (to.Amount < 0 && asset.Units < units)
                {
                    log.InfoFormat("Selling more units ({0}) than owned ({1})", units, asset.Units);
                    units = asset.Units;
                }

                var tradingBalance = units * spotPrice;
                log.InfoFormat("Final price tag {0:C} for {1} units at {2:C}", tradingBalance, units, spotPrice);

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

                log.InfoFormat("{0} units and {1:C} book cost after adjustment", asset.Units, asset.BookCost);

                asset.ManagementCost += fee;
                log.InfoFormat("Total of management fees {0:C}", asset.ManagementCost);

                Debug.Assert(asset.Units >= 0, "Negative units");

                if (asset.Units == 0)
                {
                    log.InfoFormat("All units sold. Removing position from portfolio.");
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

                    log.InfoFormat("Cash position after trade {0:C}", cash);
                }
            }

            if (cash < 0)
            {
                log.Fatal("Negative cash after executing trade orders.");
                Debug.Fail("Negative cash after executing trade orders.");
            }

            cashPosition.BookCost = cash;
            cashPosition.MarketValue = cashPosition.BookCost;
            cashPosition.Units = cashPosition.BookCost / cashPosition.LastPrice;

            log.InfoFormat("Cash after all trades {0:C}", cash);
        }

        private void UpdateHoldingsMarketValue()
        {
            log.InfoFormat("Updating portfolio market value on {0:d}", this.CurrentDate);

            // Update current market value
            foreach (var asset in this.Portfolio.Holdings)
            {
                var priceEntry = asset.Security.GetLastPriceEntry(this.CurrentDate);
                var dividendEntry = asset.Security.GetDividends(this.CurrentDate);
                asset.LastPrice = priceEntry.AveragePrice;
                asset.MarketValue = priceEntry.AveragePrice * asset.Units;
                asset.DividendsPaid += dividendEntry * asset.Units;
            }

            // Calculate portfolio total market value
            this.Portfolio.MarketValue = this.Portfolio.Holdings.Sum(o => o.MarketValue);

            foreach (var asset in this.Portfolio.Holdings)
            {
                var currentAllocation = asset.MarketValue / this.Portfolio.MarketValue;
                var dividendEntry = asset.Security.GetDividends(this.CurrentDate);
                log.InfoFormat(
                    "    {0} {1:C} {2} {3:C}, Allocation: {4:P}",
                    asset.Security.Ticker,
                    asset.LastPrice,
                    dividendEntry,
                    asset.MarketValue,
                    currentAllocation);
            }

            log.InfoFormat("Portfolio market value {0}", this.Portfolio.MarketValue);
        }

        #endregion
    }
}
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

    public class Simulator
    {
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
            this.CurrentDate = parameters.InceptionDate;
            this.LastRebalancing = this.CurrentDate;
            this.Portfolio = parameters.InitialPortfolio;
            this.Portfolio.TransactionFee = parameters.TransactionFee;

            if (parameters.ForceInitialRebalancing)
            {
                this.UpdateHoldingsMarketValue();
                this.tradeOrders = this.Portfolio.RebalancingStrategy.Rebalance(
                    parameters.ModelPortfolio,
                    this.Portfolio);
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
                    if (this.Portfolio.RebalancingStrategy.Check(
                        elapsedSinceLastRebalancing,
                        parameters.ModelPortfolio,
                        this.Portfolio))
                    {
                        this.tradeOrders = this.Portfolio.RebalancingStrategy.Rebalance(
                            parameters.ModelPortfolio,
                            this.Portfolio);
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
            var cashPosition = this.Portfolio.GetCashPosition();
            var cash = cashPosition.BookCost;

            foreach (var to in this.tradeOrders)
            {
                Debug.Assert(to.Amount != 0, "No op trade order.");

                var priceEntry = to.Security.GetPriceEntry(this.CurrentDate);
                if (priceEntry == null)
                {
                    throw new InvalidOperationException("Security price unavailable");
                }

                var spotPrice = priceEntry.AveragePrice;

                var fee = to.Amount < 0
                              ? (to.Security.SellTransactionFee ?? this.Portfolio.TransactionFee)
                              : (to.Security.BuyTransactionFee ?? this.Portfolio.TransactionFee);

                var units =
                    Math.Abs(
                        to.Security.AllowsPartialShares ? to.Amount / spotPrice : Math.Truncate(to.Amount / spotPrice));

                var asset = this.Portfolio.Holdings.FirstOrDefault(o => o.Security.Ticker == to.Security.Ticker);
                if (asset == null)
                {
                    if (to.Amount < 0)
                    {
                        throw new Exception("Attempt to sell asset not being own.");
                    }

                    asset = new PortfolioAssetModel(to.Security);
                    this.Portfolio.Holdings.Add(asset);
                }

                // Check if we try to sell more units than own
                if (to.Amount < 0 && asset.Units < units)
                {
                    units = asset.Units;
                }

                var tradingBalance = units * spotPrice;

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

                asset.ManagementCost += fee;

                Debug.Assert(asset.Units >= 0, "Negative units");

                if (asset.Units == 0)
                {
                    this.Portfolio.Holdings.Remove(asset);
                }

                // Update cash position (TODO: Dang! should ignore it for cash position!)
                // HACK: Figure that security is actual cash if it has fixed price.
                // In this case do not update running cash balance.
                if (to.Security.FixedPrice == null)
                {
                    if (to.Amount < 0)
                    {
                        cash += tradingBalance;
                    }
                    else
                    {
                        cash -= tradingBalance;
                    }
                }
            }

            Debug.Assert(cash >= 0, "Negative cash after executing trade orders.");

            cashPosition.BookCost = cash;
            cashPosition.MarketValue = cashPosition.BookCost;
            cashPosition.Units = cashPosition.BookCost / cashPosition.LastPrice;
        }

        private void UpdateHoldingsMarketValue()
        {
            // Update current market value
            foreach (var asset in this.Portfolio.Holdings)
            {
                var priceEntry = asset.Security.GetLastPriceEntry(this.CurrentDate);
                asset.LastPrice = priceEntry.AveragePrice;
                asset.MarketValue = priceEntry.AveragePrice * asset.Units;
                asset.DividendsPaid += asset.Security.GetDividends(this.CurrentDate) * asset.Units;
            }

            // Calculate portfolio total market value
            this.Portfolio.MarketValue = this.Portfolio.Holdings.Sum(o => o.MarketValue);
        }

        #endregion
    }
}
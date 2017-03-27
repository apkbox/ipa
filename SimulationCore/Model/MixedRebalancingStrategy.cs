// --------------------------------------------------------------------------------
// <copyright file="MixedRebalancingStrategy.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the MixedRebalancingStrategy type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Common.Logging;

    public class MixedRebalancingStrategy : IRebalancingStrategy
    {
        #region Static Fields

        private static readonly ILog Log = LogManager.GetLogger<MixedRebalancingStrategy>();

        #endregion

        #region Constructors and Destructors

        public MixedRebalancingStrategy()
        {
            this.Threshold = 0.01m;
            this.TradingExpenseThreshold = 0.1m;
        }

        #endregion

        #region Public Properties

        public TimeSpan Frequency { get; set; }

        public decimal Threshold { get; set; }

        /// <summary>
        /// Gets or sets trading threshold that specifies below which ratio of 
        /// transaction fee to trading amount the transaction should be executed.
        /// </summary>
        /// <remarks>
        /// This is to prevent situations where traded amount is less than trading fees.
        /// </remarks>
        public decimal TradingExpenseThreshold { get; set; }

        #endregion

        #region Public Methods and Operators

        public bool Check(Portfolio portfolio)
        {
            ModelPortfolio modelPortfolio = portfolio.ModelPortfolio;
            var marketValue = portfolio.MarketValue;
            var cashComponent = modelPortfolio.GetAsset("$CAD");
            var cashReserve = 0m;
            if (cashComponent != null)
            {
                cashReserve = cashComponent.CashReserve ?? 0;
            }

            marketValue -= cashReserve;

            foreach (var asset in portfolio.Holdings)
            {
                var currentAllocation = asset.MarketValue / marketValue;

                var modelAsset = modelPortfolio.GetAsset(asset.Security.Ticker);
                if (modelAsset == null)
                {
                    // Portfolio contains significant amount of asset that is not in the model,
                    // so the portfolio needs to be rebalanced.
                    if (currentAllocation > 0)
                    {
                        Log.TraceFormat("ScheduledStop required: non-model assets present.");
                        return true;
                    }

                    continue;
                }

                var targetAllocation = modelAsset.Allocation;
                var drift = currentAllocation - targetAllocation;

                if (Math.Abs(drift) > this.Threshold)
                {
                    Log.TraceFormat(
                        "ScheduledStop required: Asset {0} is {1:P} above target allocation {2:P} with {3:P} threshold.",
                        modelAsset.Security.Ticker,
                        drift,
                        targetAllocation,
                        this.Threshold);
                    return true;
                }
            }

            return false;
        }

        public List<TradePlanItem> Rebalance(Portfolio portfolio)
        {
            ModelPortfolio modelPortfolio = portfolio.ModelPortfolio;
            Log.TraceFormat("ScheduledStop '{0}' using '{1}'", portfolio.Name, modelPortfolio.Name);

            // TODO: We need to make sure that model portfolio allocations add up to 100% or throw an error if they dont.
            // Make combined list of current holdings and model assets.
            var assets = modelPortfolio.Assets.Select(o => new Asset(o.Security)).ToDictionary(o => o.Security.Ticker);
            Log.TraceFormat("Model assets:");
            foreach (var a in assets)
            {
                Log.TraceFormat("    {0}", a.Value.Security.Ticker);
            }

            var holdings = portfolio.Holdings.ToDictionary(o => o.Security.Ticker);
            Log.TraceFormat("Holdings:");
            foreach (var h in holdings)
            {
                Log.TraceFormat("    {0}", h.Value.Security.Ticker);
                assets[h.Key] = h.Value;
            }

            var maxTransactionCost = assets.Count * portfolio.TransactionFee;
            var expectedTransactionCost = 0m;

            var cashComponent = modelPortfolio.GetAsset("$CAD");
            var cashReserve = 0m;
            var cashAllocation = 0m;
            if (cashComponent != null)
            {
                cashAllocation = cashComponent.Allocation;
                cashReserve = cashComponent.CashReserve ?? 0;
            }

            var marketValue = portfolio.MarketValue;
            marketValue -= cashReserve;

#if true
            // Shortlist assets that are sufficiently out of balance.
            var tradesList = new List<TradePlanItem>();
            foreach (var asset in assets.Values)
            {
                Log.TraceFormat("Calculating {0}", asset.Security.Ticker);

                var modelAsset = modelPortfolio.GetAsset(asset.Security.Ticker);
                var targetAllocation = modelAsset == null ? 0 : modelAsset.Allocation;
                var currentAllocation = asset.MarketValue / marketValue;
                var drift = currentAllocation - targetAllocation;

                Log.TraceFormat(
                    "    current: {0:P}, target: {1:P}, drift: {2:P}",
                    currentAllocation,
                    targetAllocation,
                    drift);

                // Check if asset allocation deviated and skip if still within tolerance.
                if (Math.Abs(drift) < this.Threshold)
                {
                    Log.TraceFormat("    {0} is below {1:P} threshold, skipping", asset.Security.Ticker, this.Threshold);
                    continue;
                }

                var targetValue = marketValue * targetAllocation;

                // excess is positive - sell
                // excess is negative - buy
                // excess is zero - ignore
                var excess = asset.MarketValue - targetValue;

                if (excess > 0 && asset.LastPrice < asset.BookPrice)
                {
                    Log.WarnFormat(
                        "   {0} market price {1:C} is less than book price {2:C}, skipping",
                        asset.Security.Ticker,
                        asset.LastPrice,
                        asset.BookPrice);
                    continue;
                }

                // Do not care yet about transaction costs
                Log.TraceFormat("    Excess {0}", excess);

                // Check if there is at least one unit to buy or sell.
                // If amount is too small, skip it.
                // TODO: Add threshold that allows to trade in case price is really close to excess.
                // This could be tricky as if the last price exceeds few cents, that really cannot be used
                if (Math.Abs(excess) < asset.LastPrice)
                {
                    Log.TraceFormat("    Excess is less than last price, skipping");
                    continue;
                }

                if (excess != 0 && !asset.Security.IsCash)
                {
                    expectedTransactionCost += portfolio.TransactionFee;
                    tradesList.Add(new TradePlanItem { Security = asset.Security, Amount = -excess });
                }
            }

            // Then find out how much cash left and then redistribute it across all assets.
            decimal cash = 0;
            var cashAsset = portfolio.GetCashAsset("$CAD");
            if (cashAsset != null)
            {
                cash = cashAsset.BookValue;
            }

            var assetsCost = tradesList.Sum(o => o.Amount);
            Log.TraceFormat("Trade list cost balance {0:C}", assetsCost);

            var excessCash = cash - assetsCost - (cash * cashAllocation) - cashReserve - expectedTransactionCost;
            if (excessCash < 0)
            {
                excessCash = 0m;
            }

            Log.TraceFormat("Excess cash {0:C}", excessCash);

            if (excessCash > 0)
            {
                if (tradesList.Count(o => o.Amount > 0) == 0)
                {
                    Log.TraceFormat("No trades expected to add excess to.");

                    // Since there were no trades expected, correct excessCash for
                    // maximum trades.
                    excessCash -= maxTransactionCost;
                    Log.TraceFormat("Reducing excess cash to account for new transactions {0:C}", excessCash);
                    if (excessCash < 0)
                    {
                        Log.TraceFormat("Not enough cash");
                    }
                    else
                    {
                        foreach (var asset in modelPortfolio.Assets)
                        {
                            if (asset.Security.IsCash)
                            {
                                continue;
                            }

                            var excessForAsset = excessCash * asset.Allocation;
                            if (portfolio.TransactionFee / excessForAsset > this.TradingExpenseThreshold)
                            {
                                continue;
                            }

                            Log.TraceFormat("  Distributing for {0} extra {1:C}", asset.Security.Ticker, excessForAsset);
                            tradesList.Add(new TradePlanItem { Security = asset.Security, Amount = excessForAsset });
                        }
                    }
                }
                else
                {
                    foreach (var trade in tradesList)
                    {
                        if (trade.Amount < 0)
                        {
                            continue;
                        }

                        var excessForAsset = excessCash * modelPortfolio.GetAsset(trade.Security.Ticker).Allocation;
                        Log.TraceFormat("  Distributing for {0} extra {1:C}", trade.Security.Ticker, excessForAsset);
                        trade.Amount += excessForAsset;
                    }
                }
            }

#else
            foreach (var asset in assets.Values)
            {
                Log.TraceFormat("Calculating {0}", asset.Security.Ticker);

                var modelAsset = modelPortfolio.GetAsset(asset.Security.Ticker);
                var targetAllocation = modelAsset == null ? 0 : modelAsset.Allocation;
                var currentAllocation = asset.MarketValue / marketValue;
                var drift = currentAllocation - targetAllocation;

                Log.TraceFormat(
                    "    current: {0:P}, target: {1:P}, drift: {2:P}",
                    currentAllocation,
                    targetAllocation,
                    drift);

                // Check if asset allocation deviated and skip if still within tolerance.
                if (Math.Abs(drift) < this.Threshold)
                {
                    Log.TraceFormat("    {0} is below {1:P} threshold, skipping", asset.Security.Ticker, this.Threshold);
                    continue;
                }

                var targetValue = marketValue * targetAllocation;

                // excess is positive - sell
                // excess is negative - buy
                // excess is zero - ignore
                var excess = asset.MarketValue - targetValue;

                if (excess > 0 && asset.LastPrice < asset.BookPrice)
                {
                    Log.WarnFormat(
                        "   {0} market price {1:C} is less than book price {2:C}, skipping",
                        asset.Security.Ticker,
                        asset.LastPrice,
                        asset.BookPrice);
                    continue;
                }

                // Adjust excess for transaction costs
                if (excess > 0)
                {
                    // TODO: Ignore transaction fee for cash
                    var fee = portfolio.TransactionFee;
                    if (fee / excess > this.TradingExpenseThreshold)
                    {
                        excess = 0;
                    }
                    else
                    {
                        // TODO: Why do we do this?
                        // excess -= fee;
                    }
                }

                Log.TraceFormat("        Excess {0}", excess);

                // Check if there is at least one unit to buy or sell.
                // If amount is too small, skip it.
                // TODO: Add threshold that allows to trade in case price is really close to excess.
                // This could be tricky as if the last price exceeds few cents, that really cannot be used
                if (Math.Abs(excess) < asset.LastPrice)
                {
                    Log.TraceFormat("        Excess is less than last price, skipping");
                    continue;
                }

                if (excess != 0)
                {
                    tradesList.Add(new TradePlanItem { Security = asset.Security, Amount = -excess });
                }
            }
#endif

            // TODO: Check that trade balance remains positive
            return tradesList;
        }

        #endregion
    }
}
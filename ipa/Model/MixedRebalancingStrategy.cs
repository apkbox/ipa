// --------------------------------------------------------------------------------
// <copyright file="MixedRebalancingStrategy.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the MixedRebalancingStrategy type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Common.Logging;

    public class MixedRebalancingStrategy : IRebalancingStrategy
    {
        #region Static Fields

        private static ILog log = LogManager.GetLogger<MixedRebalancingStrategy>();

        #endregion

        #region Constructors and Destructors

        public MixedRebalancingStrategy()
        {
            this.Frequency = TimeSpan.FromDays(91);
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

        public RebalancingCheckResult Check(TimeSpan elapsed, ModelPortfolio modelPortfolio, Portfolio portfolio)
        {
            if (elapsed < this.Frequency)
            {
                return RebalancingCheckResult.Continue;
            }

            foreach (var asset in portfolio.Holdings)
            {
                var currentAllocation = asset.MarketValue / portfolio.MarketValue;

                var modelAsset = modelPortfolio.GetAsset(asset.Security.Ticker);
                if (modelAsset == null)
                {
                    // Portfolio contains significant amount of asset that is not in the model,
                    // so the portfolio needs to be rebalanced.
                    if (currentAllocation > 0)
                    {
                        log.InfoFormat("Rebalancing required: non-model assets present.");
                        return RebalancingCheckResult.Rebalanced;
                    }

                    continue;
                }

                var targetAllocation = modelAsset.Allocation;
                var drift = Math.Abs(targetAllocation - currentAllocation);

                if (drift > this.Threshold)
                {
                    log.InfoFormat(
                        "Rebalancing required: Asset {0} is {1:P} above target allocation {2:P} with {3:P} threshold.",
                        modelAsset.Security.Ticker,
                        drift,
                        targetAllocation,
                        this.Threshold);
                    return RebalancingCheckResult.Rebalanced;
                }
            }

            return RebalancingCheckResult.Hold;
        }

        public List<TradeOrder> Rebalance(ModelPortfolio modelPortfolio, Portfolio portfolio)
        {
            log.InfoFormat("Rebalancing '{0}' using '{1}'", portfolio.Name, modelPortfolio.Name);

            var tradesList = new List<TradeOrder>();

            // Important: Note that rebalancing should use current or next available security price 
            // instead of last in order to be correct.
            // This is because if rebalancing falls on non-trading days for certain securities, then
            // the next opportunity to trade for rebalancing lies forward.
            {
                // On the order side, the rebalancing should generate trades list only that will be executed
                // by simulation, which then correct things.
                // TODO: Do we need to make multiple iterations? First iteration excludes all assets that do not need
                // rebalancing and adjusts total amount, then the second iteration does the actual job of preparing
                // trade orders. This will avoid situation when asset has excess, but cannot be used because it
                // is below rebalancing thereshold.
                // Similar issue is when over the threshold security is sold,
                // but it is determined that other securities are
                // below threshold, the one end up with excess cash, and still unbalanced portfolio as this cash
                // shifts the balance again.
            }
            {
                // TODO: We need to make sure that model portfolio allocations add up to 100% or throw an error if they dont.
            }

            // Make combined list of current holdings and model assets.
            var assets =
                modelPortfolio.Assets.Select(o => new Asset(o.Security)).ToDictionary(o => o.Security.Ticker);
            log.InfoFormat("Model assets:");
            foreach (var a in assets)
            {
                log.InfoFormat("    {0}", a.Value.Security.Ticker);
            }

            var holdings = portfolio.Holdings.ToDictionary(o => o.Security.Ticker);
            log.InfoFormat("Holdings:");
            foreach (var h in holdings)
            {
                log.InfoFormat("    {0}", h.Value.Security.Ticker);
                assets[h.Key] = h.Value;
            }

            foreach (var asset in assets.Values)
            {
                log.InfoFormat("Calculating {0}", asset.Security.Ticker);

                var modelAsset = modelPortfolio.GetAsset(asset.Security.Ticker);
                var targetAllocation = modelAsset == null ? 0 : modelAsset.Allocation;
                var currentAllocation = asset.MarketValue / portfolio.MarketValue;
                var drift = Math.Abs(targetAllocation - currentAllocation);

                log.InfoFormat(
                    "        current: {0:P}, target: {1:P}, drift: {2:P}",
                    currentAllocation,
                    targetAllocation,
                    drift);

                // Check if asset allocation deviated and skip if still within tolerance.
                if (drift < this.Threshold)
                {
                    log.InfoFormat("        Below {0:P} threshold, skipping", this.Threshold);
                    continue;
                }

                var targetValue = portfolio.MarketValue * targetAllocation;

                // excess is positive - sell
                // excess is negative - buy
                // excess is zero - ignore
                var excess = asset.MarketValue - targetValue;

                // Adjust excess for transaction costs
                if (excess > 0)
                {
                    var fee = asset.Security.SellTransactionFee ?? portfolio.TransactionFee;
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

                log.InfoFormat("        Excess {0}", excess);

                // Check if there is at least one unit to buy or sell.
                // If amount is too small, skip it.
                // TODO: Add threshold that allows to trade in case price is really close to excess.
                // This could be tricky as if the last price exceeds few cents, that really cannot be used
                if (Math.Abs(excess) < asset.LastPrice)
                {
                    log.InfoFormat("        Excess is less than last price, skipping");
                    continue;
                }

                if (excess != 0)
                {
                    tradesList.Add(new TradeOrder { Security = asset.Security, Amount = -excess });
                }
            }

            // TODO: Check that trade balance remains positive
            return tradesList;
        }

        #endregion
    }
}
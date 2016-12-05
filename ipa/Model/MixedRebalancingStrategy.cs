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

    public class MixedRebalancingStrategy : IRebalancingStrategy
    {
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

        public bool Check(TimeSpan elapsed, ModelPortfolioModel model, PortfolioModel portfolio)
        {
            if (elapsed >= this.Frequency)
            {
                foreach (var asset in portfolio.Holdings)
                {
                    var currentAllocation = asset.MarketValue / portfolio.MarketValue;
                    var targetAllocation = model.GetAsset(asset.Security.Ticker).Allocation;
                    var drift = Math.Abs(targetAllocation - currentAllocation);

                    if (drift > this.Threshold)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<TradeOrderModel> Rebalance(ModelPortfolioModel model, PortfolioModel portfolio)
        {
            var tradesList = new List<TradeOrderModel>();

            // Important: Note that rebalancing should use current or next available security price 
            // instead of last in order to be correct.
            // This is because if rebalancing falls on non-trading days for certain securities, then
            // the next opportunity to trade for rebalancing lies forward.
            // On the order side, the rebalancing should generate trades list only that will be executed
            // by simulation, which then correct things.
            // Note that there could be no more checks and rebalancing forward until trades list
            // is completed.
            // TODO: Do we need to make multiple iterations? First iteration excludes all assets that do not need
            // rebalancing and adjusts total amount, then the second iteration does the actual job of preparing
            // trade orders. This will avoid situation when asset has excess, but cannot be used because it
            // is below rebalancing thereshold.
            foreach (var asset in portfolio.Holdings)
            {
                var targetAllocation = model.GetAsset(asset.Security.Ticker).Allocation;
                var currentAllocation = asset.MarketValue / portfolio.MarketValue;
                var drift = Math.Abs(targetAllocation - currentAllocation);

                // Check if asset allocation deviated and skip if still within tolerance.
                if (drift < this.Threshold)
                {
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
                    if (fee / excess < this.TradingExpenseThreshold)
                    {
                        excess -= fee;
                    }
                    else
                    {
                        excess = 0;
                    }
                }

                // Check if there is at least one unit to buy or sell.
                // If amount is too small, skip it.
                // TODO: Add threshold that allows to trade in case price is really close to excess.
                // This could be tricky as if the last price exceeds few cents, that really cannot be used
                if (Math.Abs(excess) < asset.LastPrice)
                {
                    continue;
                }

                if (excess != 0)
                {
                    tradesList.Add(new TradeOrderModel { Security = asset.Security, Amount = -excess });
                }
            }

            // Sort such sell trades executed first
            // tradesList = tradesList.OrderBy(o => o.Amount).ToList();

            // TODO: Check that trade balance remains positive

            return tradesList;
        }

        #endregion
    }
}
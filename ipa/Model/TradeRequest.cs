// --------------------------------------------------------------------------------
// <copyright file="TradeRequest.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the TradeOrderModel type.
// </summary>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using Common.Logging;
using System;
using System.Diagnostics;


namespace Ipa.Model
{
    /// <summary>
    /// The trader order specifies monetary amount, instead of units
    /// as the price of unit can move, but amount we want to transact for
    /// balancing purposes is expressed in money.
    /// </summary>
    public class TradeRequest
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets transaction amount.
        /// </summary>
        /// <remarks>
        /// Positive is buy, negative is sell.
        /// </remarks>
        public decimal Amount { get; set; }

        public FinSec Security { get; set; }

        #endregion
    }

    public class TradeOrderGenerator
    {
        private static readonly ILog Log = LogManager.GetLogger<TradeOrderGenerator>();

        public IList<TradeOrder> GenerateTradeOrders(DateTime tradeDay, IList<TradeRequest> requests)
        {
            var orders = new List<TradeOrder>();

            Log.InfoFormat("Creating trade orders for {0:D}", tradeDay);

            foreach (var tr in requests)
            {
                if (tr.Amount == 0)
                {
                    Log.FatalFormat("No op trade order for {0}, Amount: {1:C}", tr.Security.Ticker, tr.Amount);
                    Debug.Fail("No op trade order");
                }

                var priceEntry = tr.Security.GetPriceEntry(tradeDay);
                if (priceEntry == null)
                {
                    Log.FatalFormat("Price for {0} on {1:D} is not available", tr.Security.Ticker, this.CurrentDate);
                    Debug.Fail("Price not available");
                }

                var spotPrice = priceEntry.AveragePrice;

                var fee = tr.Amount < 0
                              ? (tr.Security.SellTransactionFee ?? this.Portfolio.TransactionFee)
                              : (tr.Security.BuyTransactionFee ?? this.Portfolio.TransactionFee);

                var units =
                    Math.Abs(
                        tr.Security.AllowsPartialShares ? tr.Amount / spotPrice : Math.Truncate(tr.Amount / spotPrice));

                Log.InfoFormat(
                    "Projection for {0} at {1:C} for {2} units. Fee {3:C}",
                    tr.Security.Ticker,
                    spotPrice,
                    units,
                    fee);

                var asset = this.Portfolio.Holdings.FirstOrDefault(o => o.Security.Ticker == tr.Security.Ticker);
                if (asset == null)
                {
                    Log.Info("Asset currently not owned.");

                    if (tr.Amount < 0)
                    {
                        Log.Fatal("Attempt to sell asset not being own.");
                        Debug.Fail("Attempt to sell asset not being own.");
                    }

                    Log.InfoFormat("Creating portfolio position for {0}", tr.Security.Ticker);
                    asset = new Asset(tr.Security);
                    this.Portfolio.Holdings.Add(asset);
                }

                // Check if we try to sell more units than own
                if (tr.Amount < 0 && asset.Units < units)
                {
                    Log.InfoFormat("Selling more units ({0}) than owned ({1})", units, asset.Units);
                    units = asset.Units;
                }

                var tradingBalance = units * spotPrice;
                Log.InfoFormat("Final price tag {0:C} for {1} units at {2:C}", tradingBalance, units, spotPrice);

                if (tr.Amount < 0)
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
                if (tr.Security.FixedPrice == null)
                {
                    if (tr.Amount < 0)
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


            return orders;
        }
    }
}
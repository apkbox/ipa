// --------------------------------------------------------------------------------
// <copyright file="FinSec.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the FinSec type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class FinSec
    {
        #region Constructors and Destructors

        public FinSec()
        {
            this.Quotes = new List<FinSecQuote>();
            this.Distributions = new List<FinSecDistribution>();
        }

        #endregion

        #region Public Properties

        public bool AllowsPartialShares { get; set; }

        public decimal? BuyTransactionFee { get; set; }

        public IList<FinSecDistribution> Distributions { get; private set; }

        /// <summary>
        /// Gets or sets fixed price.
        /// </summary>
        public decimal? FixedPrice { get; set; }

        public string Name { get; set; }

        public IList<FinSecQuote> Quotes { get; private set; }

        public decimal? SellTransactionFee { get; set; }

        public string Ticker { get; set; }

        #endregion

        #region Public Methods and Operators

        public decimal GetDividends(DateTime date)
        {
            var dividentPaymentEntry = (from entry in this.Distributions
                                        orderby entry.TransactionDate ascending
                                        where entry.TransactionDate == date
                                        select entry).FirstOrDefault();
            return dividentPaymentEntry == null ? 0m : dividentPaymentEntry.Amount;
        }

        public FinSecQuote GetLastPriceEntry(DateTime date)
        {
            return this.FixedPriceOverride(date) ?? (from entry in this.Quotes
                                                     orderby entry.TradingDayDate descending
                                                     where entry.TradingDayDate <= date
                                                     select entry).FirstOrDefault();
        }

        public FinSecQuote GetPriceEntry(DateTime date)
        {
            return this.FixedPriceOverride(date)
                   ?? (from entry in this.Quotes
                       orderby entry.TradingDayDate ascending
                       where entry.TradingDayDate >= date
                       select entry).FirstOrDefault();
        }

        #endregion

        #region Methods

        private FinSecQuote FixedPriceOverride(DateTime date)
        {
            if (this.FixedPrice == null)
            {
                return null;
            }

            return new FinSecQuote
                       {
                           TradingDayDate = date, 
                           OpenPrice = (decimal)this.FixedPrice, 
                           LowPrice = (decimal)this.FixedPrice, 
                           HighPrice = (decimal)this.FixedPrice, 
                           ClosePrice = (decimal)this.FixedPrice, 
                           AdjustedClose = (decimal)this.FixedPrice, 
                           Volume = 0
                       };
        }

        #endregion
    }
}

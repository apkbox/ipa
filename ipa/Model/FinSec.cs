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

        public FinSec(string ticker, bool isCash = false)
        {
            this.Ticker = ticker;
            this.IsCash = isCash;
            this.Quotes = new List<FinSecQuote>();
            this.Distributions = new List<FinSecDistribution>();
        }

        #endregion

        #region Public Properties

        public bool AllowsPartialShares { get; set; }

        public IList<FinSecDistribution> Distributions { get; private set; }

        /// <summary>
        /// Gets or sets fixed price of security. For cash this is the smallest
        /// currency denomination (a cent for example).
        /// </summary>
        public decimal? FixedPrice { get; set; }

        public bool IsCash { get; private set; }

        public string Name { get; set; }

        public IList<FinSecQuote> Quotes { get; private set; }

        public string Ticker { get; private set; }

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

        public FinSecQuote GetLastQuote(DateTime date)
        {
            return this.FixedQuoteOverride(date) ?? (from entry in this.Quotes
                                                     orderby entry.TradingDayDate descending
                                                     where entry.TradingDayDate <= date
                                                     select entry).FirstOrDefault();
        }

        /// <summary>
        /// Gets quote for the specified day or next available in the future
        /// if security was not traded on the specified day.
        /// </summary>
        /// <param name="date">
        /// Trading day.
        /// </param>
        /// <returns>
        /// The <see cref="FinSecQuote"/> the the specified day or closest
        /// day in the future.
        /// </returns>
        public FinSecQuote GetQuote(DateTime date)
        {
            return this.FixedQuoteOverride(date)
                   ?? (from entry in this.Quotes
                       orderby entry.TradingDayDate ascending
                       where entry.TradingDayDate >= date
                       select entry).FirstOrDefault();
        }

        /// <summary>
        /// Gets whether the security was traded at the specified date.
        /// </summary>
        /// <param name="date">
        /// Trade day.
        /// </param>
        /// <returns>
        /// True if security was traded.
        /// </returns>
        public bool IsTraded(DateTime date)
        {
            return this.Quotes.Any(o => o.TradingDayDate == date);
        }

        #endregion

        #region Methods

        private FinSecQuote FixedQuoteOverride(DateTime date)
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

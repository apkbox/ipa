// --------------------------------------------------------------------------------
// <copyright file="FinSecQuote.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SecurityPriceModel type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model
{
    using System;

    public class FinSecQuote
    {
        #region Public Properties

        public decimal AdjustedClose { get; set; }

        public decimal AveragePrice
        {
            get
            {
                return Math.Round(((this.HighPrice - this.LowPrice) / 2) + this.LowPrice, 2);
            }
        }

        public decimal ClosePrice { get; set; }

        public decimal HighPrice { get; set; }

        public decimal LowPrice { get; set; }

        public decimal OpenPrice { get; set; }

        public DateTime TradingDayDate { get; set; }

        public int Volume { get; set; }

        #endregion
    }
}

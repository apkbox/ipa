// --------------------------------------------------------------------------------
// <copyright file="SecurityPriceModel.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SecurityPriceModel type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;

    public class SecurityPriceModel
    {
        #region Public Properties

        public decimal AdjustedClose { get; set; }

        public decimal AveragePrice
        {
            get
            {
                return ((this.HighPrice - this.LowPrice) / 2) + this.LowPrice;
            }
        }

        public decimal ClosePrice { get; set; }

        public decimal HighPrice { get; set; }

        public decimal LowPrice { get; set; }

        public decimal OpenPrice { get; set; }

        public DateTime TransactionDate { get; set; }

        public int Volume { get; set; }

        #endregion
    }
}
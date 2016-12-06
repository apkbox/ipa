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

    using CsvHelper.Configuration;

    public class SecurityPriceModel : CsvClassMap<SecurityPriceModel>
    {
        #region Constructors and Destructors

        public SecurityPriceModel()
        {
            this.Map(p => p.TransactionDate).Name("Date");
            this.Map(p => p.OpenPrice).Name("Open");
            this.Map(p => p.HighPrice).Name("High");
            this.Map(p => p.LowPrice).Name("Low");
            this.Map(p => p.ClosePrice).Name("Close");
            this.Map(p => p.Volume).Name("Volume");
            this.Map(p => p.AdjustedClose).Name("Adj Close");
        }

        #endregion

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

        public DateTime TransactionDate { get; set; }

        public int Volume { get; set; }

        #endregion
    }
}

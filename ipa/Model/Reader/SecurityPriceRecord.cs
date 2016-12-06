// --------------------------------------------------------------------------------
// <copyright file="SecurityPriceRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SecurityPriceRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model.Reader
{
    using System;

    using CsvHelper.Configuration;

    internal class SecurityPriceRecord : CsvClassMap<SecurityPriceRecord>
    {
        #region Constructors and Destructors

        public SecurityPriceRecord()
        {
            this.Map(p => p.Date);
            this.Map(p => p.Open);
            this.Map(p => p.High);
            this.Map(p => p.Low);
            this.Map(p => p.Close);
            this.Map(p => p.Volume);
            this.Map(p => p.AdjClose).Name("Adj Close");
        }

        #endregion

        #region Public Properties

        public decimal AdjClose { get; set; }

        public decimal Close { get; set; }

        public DateTime Date { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Open { get; set; }

        public int Volume { get; set; }

        #endregion
    }
}
// --------------------------------------------------------------------------------
// <copyright file="FinSecRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the FinSecRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model.Reader
{
    using System.Collections.Generic;

    using CsvHelper.Configuration;

    internal class FinSecRecord : CsvClassMap<FinSecRecord>
    {
        #region Constructors and Destructors

        public FinSecRecord()
        {
            this.Map(p => p.Ticker);
            this.Map(p => p.Name);
            this.Map(p => p.IsCurrency);
            this.Map(p => p.PartialShares);
            this.Map(p => p.FixedPrice);
        }

        #endregion

        #region Public Properties

        public IList<FinSecDistributionRecord> DividendHistory { get; set; }

        public decimal? FixedPrice { get; set; }

        public bool IsCurrency { get; set; }

        public string Name { get; set; }

        public bool PartialShares { get; set; }

        public IList<FinSecQuoteRecord> PriceHistory { get; set; }

        public string Ticker { get; set; }

        #endregion
    }
}
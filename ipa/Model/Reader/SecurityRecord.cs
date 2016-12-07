// --------------------------------------------------------------------------------
// <copyright file="SecurityRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SecurityRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model.Reader
{
    using System.Collections.Generic;

    using CsvHelper.Configuration;

    internal class SecurityRecord : CsvClassMap<SecurityRecord>
    {
        #region Constructors and Destructors

        public SecurityRecord()
        {
            this.Map(p => p.Ticker);
            this.Map(p => p.Name);
            this.Map(p => p.PartialShares);
            this.Map(p => p.FixedPrice);
            this.Map(p => p.BuyFee);
            this.Map(p => p.SellFee);
        }

        #endregion

        #region Public Properties

        public decimal? BuyFee { get; set; }

        public decimal? FixedPrice { get; set; }

        public string Name { get; set; }

        public bool PartialShares { get; set; }

        public IList<SecurityPriceRecord> PriceHistory { get; set; }

        public IList<SecurityDividendRecord> DividendHistory { get; set; }

        public decimal? SellFee { get; set; }

        public string Ticker { get; set; }

        #endregion
    }
}
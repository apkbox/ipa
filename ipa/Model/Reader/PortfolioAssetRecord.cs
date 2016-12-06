// --------------------------------------------------------------------------------
// <copyright file="PortfolioAssetRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the PortfolioAssetRecord type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model.Reader
{
    using CsvHelper.Configuration;

    internal class PortfolioAssetRecord : CsvClassMap<PortfolioAssetRecord>
    {
        #region Constructors and Destructors

        public PortfolioAssetRecord()
        {
            this.Map(p => p.Ticker);
            this.Map(p => p.Units);
            this.Map(p => p.BookCost);
        }

        #endregion

        #region Public Properties

        public decimal BookCost { get; set; }

        public string Ticker { get; set; }

        public decimal Units { get; set; }

        #endregion
    }
}

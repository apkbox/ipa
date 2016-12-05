// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolioAssetRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ModelPortfolioAssetRecord type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model.Reader
{
    using CsvHelper.Configuration;

    public class ModelPortfolioAssetRecord : CsvClassMap<ModelPortfolioAssetRecord>
    {
        #region Constructors and Destructors

        public ModelPortfolioAssetRecord()
        {
            this.Map(p => p.Ticker);
            this.Map(p => p.Allocation);
        }

        #endregion

        #region Public Properties

        public decimal Allocation { get; set; }

        public string Ticker { get; set; }

        #endregion
    }
}

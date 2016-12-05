// --------------------------------------------------------------------------------
// <copyright file="PortfolioAssetModel.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the PortfolioAssetModel type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model
{
    using CsvHelper.Configuration;

    public class PortfolioAssetModel : CsvClassMap<PortfolioModel>
    {
        #region Constructors and Destructors

        public PortfolioAssetModel()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets book cost of the asset.
        /// </summary>
        public decimal BookCost { get; set; }

        /// <summary>
        /// Gets or sets total dividends paid for this asset.
        /// </summary>
        public decimal DividendsPaid { get; set; }

        /// <summary>
        /// Gets or sets last price of the security.
        /// </summary>
        public decimal LastPrice { get; set; }

        /// <summary>
        /// Gets or sets total management cost.
        /// </summary>
        public decimal ManagementCost { get; set; }

        /// <summary>
        /// Gets or sets market value of the asset.
        /// </summary>
        public decimal MarketValue { get; set; }

        /// <summary>
        /// Gets or sets security of the asset.
        /// </summary>
        public SecurityModel Security { get; set; }

        /// <summary>
        /// Gets or sets number of units held in portfolio.
        /// </summary>
        public decimal Units { get; set; }

        #endregion
    }
}

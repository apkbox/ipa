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
    public class PortfolioAssetModel
    {
        #region Constructors and Destructors

        public PortfolioAssetModel(SecurityModel security)
        {
            this.Security = security;
        }

        public PortfolioAssetModel(PortfolioAssetModel other)
        {
            this.BookCost = other.BookCost;
            this.DividendsPaid = other.DividendsPaid;
            this.LastPrice = other.LastPrice;
            this.ManagementCost = other.ManagementCost;
            this.MarketValue = other.MarketValue;
            this.Security = other.Security;
            this.Units = other.Units;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets book cost of the asset.
        /// </summary>
        public decimal BookCost { get; set; }

        public decimal BookPrice
        {
            get
            {
                if (this.Units == 0)
                {
                    return 0;
                }

                return this.BookCost / this.Units;
            }
        }

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
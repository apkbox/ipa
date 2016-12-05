// --------------------------------------------------------------------------------
// <copyright file="PortfolioModel.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the PortfolioModel type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;
    using System.Collections.Generic;

    using CsvHelper.Configuration;

    public class PortfolioModel : CsvClassMap<PortfolioModel>
    {
        #region Constructors and Destructors

        public PortfolioModel()
        {
            this.Holdings = new List<PortfolioAssetModel>();
            this.RebalancingStrategy = new MixedRebalancingStrategy();
        }

        #endregion

        #region Public Properties

        public List<PortfolioAssetModel> Holdings { get; private set; }

        public DateTime LastRebalancingDate { get; set; }

        /// <summary>
        /// Gets or sets current portfolio market value.
        /// </summary>
        public decimal MarketValue { get; set; }

        /// <summary>
        /// Gets or sets rebalancing strategy.
        /// </summary>
        public IRebalancingStrategy RebalancingStrategy { get; set; }

        /// <summary>
        /// Gets or sets default transaction fee for the portfolio.
        /// </summary>
        public decimal TransactionFee { get; set; }

        #endregion
    }
}
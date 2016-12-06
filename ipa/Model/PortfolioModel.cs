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
    using System.Linq;

    public class PortfolioModel
    {
        #region Constructors and Destructors

        public PortfolioModel()
        {
            this.Holdings = new List<PortfolioAssetModel>();
            this.RebalancingStrategy = new MixedRebalancingStrategy();
        }

        public PortfolioModel(PortfolioModel other)
        {
            this.Holdings = other.Holdings.Select(o => new PortfolioAssetModel(o)).ToList();
            this.LastRebalancingDate = other.LastRebalancingDate;
            this.MarketValue = other.MarketValue;
            this.Name = other.Name;
            this.RebalancingStrategy = other.RebalancingStrategy;
            this.TransactionFee = other.TransactionFee;
        }

        #endregion

        #region Public Properties

        public IList<PortfolioAssetModel> Holdings { get; private set; }

        public DateTime LastRebalancingDate { get; set; }

        /// <summary>
        /// Gets or sets current portfolio market value.
        /// </summary>
        public decimal MarketValue { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Gets or sets rebalancing strategy.
        /// </summary>
        public IRebalancingStrategy RebalancingStrategy { get; set; }

        /// <summary>
        /// Gets or sets default transaction fee for the portfolio.
        /// </summary>
        public decimal TransactionFee { get; set; }

        #endregion

        #region Public Methods and Operators

        public PortfolioAssetModel GetCashPosition()
        {
            return this.Holdings.FirstOrDefault(o => o.Security.Ticker == "$CAD");
        }

        #endregion
    }
}
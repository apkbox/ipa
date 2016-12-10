// --------------------------------------------------------------------------------
// <copyright file="Portfolio.cs" company="Alex Kozlov">
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

    public class Portfolio
    {
        #region Constructors and Destructors

        public Portfolio()
        {
            this.Holdings = new List<Asset>();
            this.RebalancingStrategy = new MixedRebalancingStrategy();
        }

        public Portfolio(Portfolio other)
        {
            this.Holdings = other.Holdings.Select(o => new Asset(o)).ToList();
            this.MarketValue = other.MarketValue;
            this.Name = other.Name;
            this.RebalancingStrategy = other.RebalancingStrategy;
            this.TransactionFee = other.TransactionFee;
        }

        #endregion

        #region Public Properties

        public decimal BookValue
        {
            get
            {
                return this.Holdings.Sum(o => o.BookValue);
            }
        }

        public IList<Asset> Holdings { get; private set; }

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

        public Asset GetCashAsset(string currency)
        {
            return this.Holdings.FirstOrDefault(o => o.Security.Ticker == currency && o.Security.IsCash);
        }

        #endregion
    }
}

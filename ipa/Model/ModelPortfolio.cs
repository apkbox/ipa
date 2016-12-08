// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolio.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ModelPortfolioModel type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class ModelPortfolio
    {
        #region Constructors and Destructors

        public ModelPortfolio()
        {
            this.Assets = new List<ModelPortfolioAllocation>();
        }

        #endregion

        #region Public Properties

        public IList<ModelPortfolioAllocation> Assets { get; private set; }

        public string Name { get; set; }

        #endregion

        #region Public Methods and Operators

        public ModelPortfolioAllocation GetAsset(string ticker)
        {
            return this.Assets.FirstOrDefault(o => o.Security.Ticker == ticker);
        }

        #endregion
    }
}

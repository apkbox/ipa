// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolio.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ModelPortfolioModel type.
// </summary>
// --------------------------------------------------------------------------------
namespace SimulationCore.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class ModelPortfolio
    {
        #region Constructors and Destructors

        public ModelPortfolio()
        {
            this.Assets = new List<ModelPortfolioComponent>();
        }

        #endregion

        #region Public Properties

        public IList<ModelPortfolioComponent> Assets { get; private set; }

        public string Name { get; set; }

        #endregion

        #region Public Methods and Operators

        public ModelPortfolioComponent GetAsset(string ticker)
        {
            return this.Assets.FirstOrDefault(o => o.Security.Ticker == ticker);
        }

        #endregion
    }
}

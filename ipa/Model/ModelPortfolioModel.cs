// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolioModel.cs" company="Alex Kozlov">
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

    using Ipa.Model.Reader;

    public class ModelPortfolioModel
    {
        #region Constructors and Destructors

        public ModelPortfolioModel()
        {
            this.Assets = new List<ModelPortfolioAsset>();
        }

        #endregion

        #region Public Properties

        public IList<ModelPortfolioAsset> Assets { get; private set; }

        public string Name { get; set; }

        #endregion

        #region Public Methods and Operators

        public ModelPortfolioAsset GetAsset(string ticker)
        {
            return this.Assets.FirstOrDefault(o => o.Security.Ticker == ticker);
        }

        #endregion
    }
}

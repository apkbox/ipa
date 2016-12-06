// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolioAsset.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ModelPortfolioAsset type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    public class ModelPortfolioAsset
    {
        #region Public Properties

        public decimal Allocation { get; set; }

        public SecurityModel Security { get; set; }

        #endregion
    }
}
// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolioAllocation.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ModelPortfolioAsset type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model
{
    public class ModelPortfolioAllocation
    {
        #region Public Properties

        public decimal Allocation { get; set; }

        public FinSec Security { get; set; }

        #endregion
    }
}

// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolioComponent.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ModelPortfolioComponent type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore.Model
{
    public class ModelPortfolioComponent
    {
        #region Public Properties

        public decimal Allocation { get; set; }

        public decimal? CashReserve { get; set; }

        public FinSec Security { get; set; }

        #endregion
    }
}
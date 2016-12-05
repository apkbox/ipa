// --------------------------------------------------------------------------------
// <copyright file="SimulationState.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SimulationState type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;

    public class SimulationState
    {
        #region Fields

        #endregion

        #region Public Properties

        public DateTime CurrentDate { get; set; }

        public DateTime LastRebalancing { get; set; }

        public PortfolioModel Portfolio { get; set; }

        #endregion
    }
}
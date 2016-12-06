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
    public enum SimulationState
    {
        /// <summary>
        /// Rebalancing required.
        /// </summary>
        /// <remarks>
        /// The app can choose to rebalance by creating a trade orders list,
        /// then trades will be executed on next cycle.
        /// Alternatively a model portfolio can be replaced or adjusted.
        /// </remarks>
        Rebalancing,

        /// <summary>
        /// Simulation executed a day and can continue.
        /// </summary>
        Continue,

        /// <summary>
        /// Simulation stopped.
        /// </summary>
        Stopped
    }
}
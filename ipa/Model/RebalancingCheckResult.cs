// --------------------------------------------------------------------------------
// <copyright file="RebalancingCheckResult.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the RebalancingCheckResult type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model
{
    public enum RebalancingCheckResult
    {
        /// <summary>
        /// Continue simulation.
        /// </summary>
        Continue, 

        /// <summary>
        /// ScheduledStop time arrived, but rebalancing was not required.
        /// </summary>
        Hold,

        /// <summary>
        /// ScheduledStop was performed.
        /// </summary>
        Rebalanced
    }
}

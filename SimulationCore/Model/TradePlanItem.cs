// --------------------------------------------------------------------------------
// <copyright file="TradePlanItem.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the TradeOrderModel type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore.Model
{
    /// <summary>
    /// Trade plan item represents a planned trading for the specified security.
    /// </summary>
    /// <remarks>
    /// The trade order specifies monetary amount, instead of units
    /// as the price of unit can move, but amount we want to transact for
    /// balancing purposes is expressed in money.
    /// </remarks>
    public class TradePlanItem
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets transaction amount.
        /// </summary>
        /// <remarks>
        /// Positive is buy, negative is sell.
        /// </remarks>
        public decimal Amount { get; set; }

        public FinSec Security { get; set; }

        #endregion
    }
}
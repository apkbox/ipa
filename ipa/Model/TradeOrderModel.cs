// --------------------------------------------------------------------------------
// <copyright file="TradeOrderModel.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the TradeOrderModel type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    /// <summary>
    /// The trader order specifies monetary amount, instead of units
    /// as the price of unit can move, but amount we want to transact for
    /// balancing purposes is expressed in money.
    /// </summary>
    public class TradeOrderModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets transaction amount.
        /// </summary>
        /// <remarks>
        /// Positive is buy, negative is sell.
        /// </remarks>
        public decimal Amount { get; set; }

        public SecurityModel Security { get; set; }

        #endregion
    }
}
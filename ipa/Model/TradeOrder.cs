// --------------------------------------------------------------------------------
// <copyright file="TradeOrder.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the TradeOrder type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    public class TradeOrder
    {
        #region Public Properties

        public decimal? Price { get; set; }

        public FinSec Security { get; set; }

        public decimal TotalCost { get; set; }

        public decimal? TransactionFee { get; set; }

        public decimal Units { get; set; }

        public decimal UnitsOwned { get; set; }

        #endregion
    }
}
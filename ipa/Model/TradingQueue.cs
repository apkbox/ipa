// --------------------------------------------------------------------------------
// <copyright file="TradingQueue.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the TradingQueue type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System.Collections.Generic;

    public class TradingQueue
    {
        #region Constructors and Destructors

        public TradingQueue()
        {
            this.TradeOrders = new List<TradeOrder>();
        }

        #endregion

        #region Public Properties

        public decimal Cash { get; set; }

        public IList<TradeOrder> TradeOrders { get; private set; }

        public decimal TransactionFee { get; set; }

        #endregion
    }
}
// --------------------------------------------------------------------------------
// <copyright file="IRebalancingStrategy.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the IRebalancingStrategy type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;
    using System.Collections.Generic;

    public interface IRebalancingStrategy
    {
        #region Public Methods and Operators

        RebalancingCheckResult Check(TimeSpan elapsed, ModelPortfolio modelPortfolio, Portfolio portfolio);

        List<TradeOrder> Rebalance(ModelPortfolio modelPortfolio, Portfolio portfolio);

        #endregion
    }
}
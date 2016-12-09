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
    using System.Collections.Generic;

    public interface IRebalancingStrategy
    {
        #region Public Methods and Operators

        bool Check(ModelPortfolio modelPortfolio, Portfolio portfolio);

        List<TradePlanItem> Rebalance(ModelPortfolio modelPortfolio, Portfolio portfolio);

        #endregion
    }
}
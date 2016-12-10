// --------------------------------------------------------------------------------
// <copyright file="IpaDb.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the IpaDb type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System.Collections.Generic;

    public class IpaDb
    {
        #region Constructors and Destructors

        internal IpaDb()
        {
        }

        #endregion

        #region Public Properties

        public IDictionary<string, ModelPortfolio> ModelPortfolios { get; internal set; }

        public IDictionary<string, Portfolio> Portfolios { get; internal set; }

        public IDictionary<string, FinSec> Securities { get; internal set; }

        public IList<SimulationParameters> SimulationParameters { get; internal set; }

        #endregion
    }
}
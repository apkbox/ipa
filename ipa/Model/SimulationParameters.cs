// --------------------------------------------------------------------------------
// <copyright file="SimulationParameters.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SimulationParameters type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;

    public class SimulationParameters
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether to perform initial rebalancing.
        /// </summary>
        public bool ForceInitialRebalancing { get; set; }

        public DateTime InceptionDate { get; set; }

        public PortfolioModel InitialPortfolio { get; set; }

        /// <summary>
        /// Gets or sets portfolio model to follow.
        /// </summary>
        public ModelPortfolioModel ModelPortfolio { get; set; }

        /// <summary>
        /// Gets or sets simulation stop date.
        /// </summary>
        public DateTime StopDate { get; set; }

        /// <summary>
        /// Gets or sets default transaction fee.
        /// </summary>
        public decimal TransactionFee { get; set; }

        #endregion
    }
}
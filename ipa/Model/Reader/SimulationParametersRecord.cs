// --------------------------------------------------------------------------------
// <copyright file="SimulationParametersRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SimulationParametersRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model.Reader
{
    using System;

    using CsvHelper.Configuration;

    internal class SimulationParametersRecord : CsvClassMap<SimulationParametersRecord>
    {
        #region Constructors and Destructors

        public SimulationParametersRecord()
        {
            this.Map(p => p.ModelPortfolioId);
            this.Map(p => p.PortfolioId);
            this.Map(p => p.InceptionDate);
            this.Map(p => p.StopDate).Default(null);
            this.Map(p => p.TransactionFee);
            this.Map(p => p.ForceInitialRebalancing);
        }

        #endregion

        #region Public Properties

        public bool ForceInitialRebalancing { get; set; }

        public DateTime InceptionDate { get; set; }

        public string ModelPortfolioId { get; set; }

        public string PortfolioId { get; set; }

        public DateTime? StopDate { get; set; }

        public decimal TransactionFee { get; set; }

        #endregion
    }
}
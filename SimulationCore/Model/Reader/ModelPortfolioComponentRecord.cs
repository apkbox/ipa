// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolioComponentRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ModelPortfolioComponentRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore.Model.Reader
{
    using CsvHelper.Configuration;

    internal class ModelPortfolioComponentRecord : CsvClassMap<ModelPortfolioComponentRecord>
    {
        #region Constructors and Destructors

        public ModelPortfolioComponentRecord()
        {
            this.Map(p => p.Ticker);
            this.Map(p => p.Allocation);
            this.Map(p => p.CashReserve);
        }

        #endregion

        #region Public Properties

        public decimal Allocation { get; set; }

        public decimal? CashReserve { get; set; }

        public string Ticker { get; set; }

        #endregion
    }
}
// --------------------------------------------------------------------------------
// <copyright file="StatRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the StatRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore
{
    using System;

    using CsvHelper.Configuration;

    public class StatRecord : CsvClassMap<StatRecord>
    {
        #region Constructors and Destructors

        public StatRecord()
        {
            this.Map(p => p.SimulationId);
            this.Map(p => p.StartDate).TypeConverterOption("yyyy-MM-dd");
            this.Map(p => p.InitialValue).TypeConverterOption("C");
            this.Map(p => p.MarketValue).TypeConverterOption("C");
            this.Map(p => p.DividendsPaid).TypeConverterOption("C");
            this.Map(p => p.ManagementExpenses).TypeConverterOption("C");
            this.Map(p => p.TotalReturn).TypeConverterOption("C");
            this.Map(p => p.TotalReturnRate).TypeConverterOption("P");
            this.Map(p => p.AnnualizedReturnRate).TypeConverterOption("P");
        }

        #endregion

        #region Public Properties

        public decimal AnnualizedReturnRate { get; set; }

        public decimal DividendsPaid { get; set; }

        public decimal InitialValue { get; set; }

        public decimal ManagementExpenses { get; set; }

        public decimal MarketValue { get; set; }

        public string SimulationId { get; set; }

        public DateTime StartDate { get; set; }

        public decimal TotalReturn { get; set; }

        public decimal TotalReturnRate { get; set; }

        #endregion
    }
}
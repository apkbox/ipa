// --------------------------------------------------------------------------------
// <copyright file="StatRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the StatRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa
{
    using System;

    using CsvHelper.Configuration;

    public class StatRecord : CsvClassMap<StatRecord>
    {
        public StatRecord()
        {
            this.Map(p => p.StartDate).TypeConverterOption("yyyy-MM-dd");
            this.Map(p => p.TotalReturn).TypeConverterOption("C");
            this.Map(p => p.TotalReturnRate).TypeConverterOption("P");
            this.Map(p => p.AnnualizedReturnRate).TypeConverterOption("P");
        }
        #region Public Properties

        public decimal AnnualizedReturnRate { get; set; }

        public DateTime StartDate { get; set; }

        public decimal TotalReturn { get; set; }

        public decimal TotalReturnRate { get; set; }

        #endregion
    }
}
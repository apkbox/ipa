// --------------------------------------------------------------------------------
// <copyright file="FinSecDistributionRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the FinSecDistributionRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model.Reader
{
    using System;

    using CsvHelper.Configuration;

    internal class FinSecDistributionRecord : CsvClassMap<FinSecDistributionRecord>
    {
        #region Constructors and Destructors

        public FinSecDistributionRecord()
        {
            this.Map(p => p.Date);
            this.Map(p => p.Dividends);
        }

        #endregion

        #region Public Properties

        public DateTime Date { get; set; }

        public decimal Dividends { get; set; }

        #endregion
    }
}
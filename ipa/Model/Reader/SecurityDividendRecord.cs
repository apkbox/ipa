// --------------------------------------------------------------------------------
// <copyright file="SecurityDividendRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SecurityDividendRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model.Reader
{
    using System;

    using CsvHelper.Configuration;

    internal class SecurityDividendRecord : CsvClassMap<SecurityDividendRecord>
    {
        #region Constructors and Destructors

        public SecurityDividendRecord()
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
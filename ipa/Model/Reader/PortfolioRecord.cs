// --------------------------------------------------------------------------------
// <copyright file="PortfolioRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the PortfolioRecord type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model.Reader
{
    using System.Collections.Generic;

    using CsvHelper.Configuration;

    internal class PortfolioRecord : CsvClassMap<PortfolioRecord>
    {
        #region Constructors and Destructors

        public PortfolioRecord()
        {
            this.Map(p => p.PortfolioId);
            this.Map(p => p.Name);
            this.Map(p => p.TransactionFee);
        }

        #endregion

        #region Public Properties

        public IDictionary<string, AssetRecord> Holdings { get; set; }

        public string Name { get; set; }

        public string PortfolioId { get; set; }

        public decimal TransactionFee { get; set; }

        #endregion
    }
}
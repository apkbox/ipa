// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolioRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ModelPortfolioRecord type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model.Reader
{
    using System.Collections.Generic;

    using CsvHelper.Configuration;

    internal class ModelPortfolioRecord : CsvClassMap<ModelPortfolioRecord>
    {
        #region Constructors and Destructors

        public ModelPortfolioRecord()
        {
            this.Map(p => p.ModelPortfolioId);
            this.Map(p => p.Name);
        }

        #endregion

        #region Public Properties

        public IDictionary<string, ModelPortfolioComponentRecord> Assets { get; set; }

        public string ModelPortfolioId { get; set; }

        public string Name { get; set; }

        #endregion
    }
}

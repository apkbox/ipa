// --------------------------------------------------------------------------------
// <copyright file="AssetRecord.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the AssetRecord type.
// </summary>
// --------------------------------------------------------------------------------
namespace SimulationCore.Model.Reader
{
    using CsvHelper.Configuration;

    internal class AssetRecord : CsvClassMap<AssetRecord>
    {
        #region Constructors and Destructors

        public AssetRecord()
        {
            this.Map(p => p.Ticker);
            this.Map(p => p.Units);
            this.Map(p => p.BookCost);
        }

        #endregion

        #region Public Properties

        public decimal BookCost { get; set; }

        public string Ticker { get; set; }

        public decimal Units { get; set; }

        #endregion
    }
}

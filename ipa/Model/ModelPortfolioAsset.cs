// --------------------------------------------------------------------------------
// <copyright file="ModelPortfolioAsset.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ModelPortfolioAsset type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using Ipa.Model.Reader;

    public class ModelPortfolioAsset
    {
        public ModelPortfolioAsset()
        {
        }

        #region Public Properties

        public decimal Allocation { get; set; }

        public SecurityModel Security { get; set; }

        #endregion

        public static ModelPortfolioAsset FromRecord(ModelPortfolioAssetRecord record)
        {
            return new ModelPortfolioAsset
                       {
                           Allocation = record.Allocation,
                           Security = record.Ticker
                       };
        }
    }
}
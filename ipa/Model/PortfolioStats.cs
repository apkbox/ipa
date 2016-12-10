// --------------------------------------------------------------------------------
// <copyright file="PortfolioStats.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the PortfolioStats type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    public class PortfolioStats
    {
        #region Public Properties

        public decimal AnnualizedReturnRate { get; set; }

        public decimal BookCost { get; set; }

        public decimal DividendsPaid { get; set; }

        public decimal ManagementExpenses { get; set; }

        public decimal MarketValue { get; set; }

        public decimal TotalReturn { get; set; }

        public decimal TotalReturnRate { get; set; }

        #endregion
    }
}
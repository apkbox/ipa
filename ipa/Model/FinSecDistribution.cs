// --------------------------------------------------------------------------------
// <copyright file="FinSecDistribution.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the FinSecDistribution type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model
{
    using System;

    public class FinSecDistribution
    {
        #region Public Properties

        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; }

        #endregion
    }
}

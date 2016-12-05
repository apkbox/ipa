// --------------------------------------------------------------------------------
// <copyright file="SecurityDividendModel.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SecurityDividendModel type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;

    public class SecurityDividendModel
    {
        #region Public Properties

        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; }

        #endregion
    }
}
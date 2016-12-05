// --------------------------------------------------------------------------------
// <copyright file="SecurityModel.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SecurityModel type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CsvHelper.Configuration;

    public class SecurityModel : CsvClassMap<SecurityModel>
    {
        #region Constructors and Destructors

        public SecurityModel()
        {
            this.Map(p => p.Ticker).Name("Ticker");
            this.Map(p => p.Name).Name("Name");
            this.Map(p => p.AllowsPartialShares).Name("PartialShares");
            this.Map(p => p.FixedPrice).Name("FixedPrice").Default(null);
            this.Map(p => p.BuyTransactionFee).Name("BuyFee").Default(null);
            this.Map(p => p.SellTransactionFee).Name("SellFee").Default(null);

            this.AllowsPartialShares = false;
            this.PriceHistory = new List<SecurityPriceModel>();
            this.DividendHistory = new List<SecurityDividendModel>();
        }

        #endregion

        #region Public Properties

        public bool AllowsPartialShares { get; set; }

        public decimal? BuyTransactionFee { get; set; }

        public IList<SecurityDividendModel> DividendHistory { get; private set; }

        public decimal? FixedPrice { get; set; }

        public string Name { get; set; }

        public IList<SecurityPriceModel> PriceHistory { get; private set; }

        public decimal? SellTransactionFee { get; set; }

        public string Ticker { get; set; }

        #endregion

        #region Public Methods and Operators

        public decimal GetDividends(DateTime date)
        {
            var dividentPaymentEntry = (from entry in this.DividendHistory
                                        orderby entry.TransactionDate ascending
                                        where entry.TransactionDate == date
                                        select entry).FirstOrDefault();
            return dividentPaymentEntry == null ? 0m : dividentPaymentEntry.Amount;
        }

        public SecurityPriceModel GetLastPriceEntry(DateTime date)
        {
            return this.FixedPriceOverride(date) ?? (from entry in this.PriceHistory
                                                     orderby entry.TransactionDate descending
                                                     where entry.TransactionDate <= date
                                                     select entry).FirstOrDefault();
        }

        public SecurityPriceModel GetPriceEntry(DateTime date)
        {
            return this.FixedPriceOverride(date) ?? (from entry in this.PriceHistory
                                                     orderby entry.TransactionDate ascending
                                                     where entry.TransactionDate >= date
                                                     select entry).FirstOrDefault();
        }

        #endregion

        #region Methods

        private SecurityPriceModel FixedPriceOverride(DateTime date)
        {
            if (this.FixedPrice == null)
            {
                return null;
            }

            return new SecurityPriceModel
                       {
                           TransactionDate = date, 
                           OpenPrice = (decimal)this.FixedPrice, 
                           LowPrice = (decimal)this.FixedPrice, 
                           HighPrice = (decimal)this.FixedPrice, 
                           ClosePrice = (decimal)this.FixedPrice, 
                           AdjustedClose = (decimal)this.FixedPrice, 
                           Volume = 0
                       };
        }

        #endregion
    }
}

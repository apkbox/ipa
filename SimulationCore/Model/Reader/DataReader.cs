﻿// --------------------------------------------------------------------------------
// <copyright file="DataReader.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the DataReader type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore.Model.Reader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CsvHelper;

    public class DataReader
    {
        #region Fields

        private readonly Dictionary<string, ModelPortfolio> modelPortfolios = new Dictionary<string, ModelPortfolio>();

        private readonly IDictionary<string, Portfolio> portfolios = new Dictionary<string, Portfolio>();

        private readonly IDictionary<string, FinSec> securities = new Dictionary<string, FinSec>();

        private readonly IList<SimulationParameters> simulationParameters = new List<SimulationParameters>();

        private IDictionary<string, ModelPortfolioRecord> modelPortfolioRecords;

        private IDictionary<string, PortfolioRecord> portfolioRecords;

        private IDictionary<string, FinSecRecord> securityRecords;

        private IList<SimulationParametersRecord> simParamsRecords;

        #endregion

        #region Constructors and Destructors

        private DataReader()
        {
        }

        #endregion

        #region Public Methods and Operators

        public static IpaDb LoadDb()
        {
            var reader = new DataReader();
            reader.LoadData();
            return new IpaDb
                       {
                           Securities = reader.securities,
                           ModelPortfolios = reader.modelPortfolios,
                           Portfolios = reader.portfolios,
                           SimulationParameters = reader.simulationParameters,
                       };
        }

        #endregion

        #region Methods

        private ModelPortfolio GetModelPortfolio(string modelPortfolioId)
        {
            if (this.modelPortfolioRecords == null)
            {
                this.ReadModelPortfolios();
                foreach (var record in this.modelPortfolioRecords)
                {
                    var mpm = new ModelPortfolio { Name = record.Value.Name };
                    foreach (var ar in record.Value.Assets)
                    {
                        var asset = new ModelPortfolioComponent
                                        {
                                            Security = this.GetSecurity(ar.Value.Ticker),
                                            Allocation = ar.Value.Allocation,
                                            CashReserve = ar.Value.CashReserve
                                        };
                        mpm.Assets.Add(asset);
                    }

                    this.modelPortfolios.Add(record.Key, mpm);
                }
            }

            return this.modelPortfolios[modelPortfolioId];
        }

        private Portfolio GetPortfolio(string portfolioId)
        {
            if (this.portfolioRecords == null)
            {
                this.ReadPortfolios();
                foreach (var record in this.portfolioRecords)
                {
                    var pm = new Portfolio { Name = record.Value.Name, TransactionFee = record.Value.TransactionFee };
                    foreach (var hr in record.Value.Holdings)
                    {
                        var asset = new Asset(this.GetSecurity(hr.Value.Ticker))
                                        {
                                            Units = hr.Value.Units,
                                            BookValue = hr.Value.BookCost
                                        };
                        pm.Holdings.Add(asset);
                    }

                    this.portfolios.Add(record.Key, pm);
                }
            }

            return this.portfolios[portfolioId];
        }

        private FinSec GetSecurity(string ticker)
        {
            if (this.securityRecords == null)
            {
                this.ReadSecurities();
                foreach (var record in this.securityRecords)
                {
                    var sm = new FinSec(record.Value.Ticker, record.Value.IsCurrency)
                                 {
                                     Name = record.Value.Name,
                                     AllowsPartialShares =
                                         record.Value.PartialShares,
                                     FixedPrice =
                                         record.Value.FixedPrice,
                                 };

                    if (record.Value.PriceHistory != null)
                    {
                        foreach (var spr in record.Value.PriceHistory)
                        {
                            var sp = new FinSecQuote
                                         {
                                             TradingDayDate = spr.Date,
                                             OpenPrice = spr.Open,
                                             HighPrice = spr.High,
                                             LowPrice = spr.Low,
                                             ClosePrice = spr.Close,
                                             Volume = spr.Volume,
                                             AdjustedClose = spr.AdjClose,
                                         };
                            sm.Quotes.Add(sp);
                        }
                    }

                    if (record.Value.DividendHistory != null)
                    {
                        foreach (var dhr in record.Value.DividendHistory)
                        {
                            var d = new FinSecDistribution { TransactionDate = dhr.Date, Amount = dhr.Dividends };
                            sm.Distributions.Add(d);
                        }
                    }

                    this.securities.Add(record.Key, sm);
                }
            }

            return this.securities[ticker];
        }

        private void LoadData()
        {
            this.ReadSimulationParameters();

            foreach (var spr in this.simParamsRecords)
            {
                var sp = new SimulationParameters
                             {
                                 SimulationId = spr.SimulationId,
                                 ModelPortfolio = this.GetModelPortfolio(spr.ModelPortfolioId),
                                 InitialPortfolio = this.GetPortfolio(spr.PortfolioId),
                                 InceptionDate = spr.InceptionDate,
                                 StopDate = spr.StopDate ?? DateTime.Today,
                                 ForceInitialRebalancing = spr.ForceInitialRebalancing,
                                 SetInitialBookCost = spr.SetInitialBookCost
                             };
                this.simulationParameters.Add(sp);
            }
        }

        private IDictionary<string, ModelPortfolioComponentRecord> ReadModelPortfolioAssets(string modelPortfolioId)
        {
            var fileName = string.Format("config/{0}_ModelPortfolioAssets.csv", modelPortfolioId);
            using (var reader = new StreamReader(fileName))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<ModelPortfolioComponentRecord>();
                return csv.GetRecords<ModelPortfolioComponentRecord>().ToDictionary(o => o.Ticker);
            }
        }

        private void ReadModelPortfolios()
        {
            using (var reader = new StreamReader(string.Format("config/ModelPortfolios.csv")))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<ModelPortfolioRecord>();
                this.modelPortfolioRecords = csv.GetRecords<ModelPortfolioRecord>()
                    .ToDictionary(o => o.ModelPortfolioId);
            }

            foreach (var r in this.modelPortfolioRecords)
            {
                r.Value.Assets = this.ReadModelPortfolioAssets(r.Key);
            }
        }

        private IDictionary<string, AssetRecord> ReadPortfolioHoldings(string portfolioId)
        {
            var fileName = string.Format("config/{0}_Holdings.csv", portfolioId);
            using (var reader = new StreamReader(fileName))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<AssetRecord>();
                return csv.GetRecords<AssetRecord>().ToDictionary(o => o.Ticker);
            }
        }

        private void ReadPortfolios()
        {
            using (var reader = new StreamReader(string.Format("config/Portfolios.csv")))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<PortfolioRecord>();
                this.portfolioRecords = csv.GetRecords<PortfolioRecord>().ToDictionary(o => o.PortfolioId);
            }

            foreach (var r in this.portfolioRecords)
            {
                r.Value.Holdings = this.ReadPortfolioHoldings(r.Key);
            }
        }

        private void ReadSecurities()
        {
            using (var reader = new StreamReader(string.Format("config/Securities.csv")))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<FinSecRecord>();
                this.securityRecords = csv.GetRecords<FinSecRecord>().ToDictionary(o => o.Ticker);
            }

            foreach (var r in this.securityRecords)
            {
                if (r.Value.FixedPrice != null)
                {
                    continue;
                }

                r.Value.PriceHistory = this.ReadSecurityPriceHistory(r.Key);
                r.Value.DividendHistory = this.ReadSecurityDividendHistory(r.Key);
            }
        }

        private IList<FinSecDistributionRecord> ReadSecurityDividendHistory(string ticker)
        {
            using (var reader = new StreamReader(string.Format("config/quotes/{0}_SecurityDividends.csv", ticker)))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<FinSecDistributionRecord>();
                return csv.GetRecords<FinSecDistributionRecord>().OrderBy(o => o.Date).ToList();
            }
        }

        private IList<FinSecQuoteRecord> ReadSecurityPriceHistory(string ticker)
        {
            using (var reader = new StreamReader(string.Format("config/quotes/{0}_SecurityPrices.csv", ticker)))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<FinSecQuoteRecord>();
                return csv.GetRecords<FinSecQuoteRecord>().OrderBy(o => o.Date).ToList();
            }
        }

        private void ReadSimulationParameters()
        {
            using (var reader = new StreamReader("config/SimulationParameters.csv"))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<SimulationParametersRecord>();
                this.simParamsRecords = csv.GetRecords<SimulationParametersRecord>().ToList();
            }
        }

        #endregion
    }
}
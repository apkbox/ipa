// --------------------------------------------------------------------------------
// <copyright file="DataReader.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the DataReader type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model.Reader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CsvHelper;

    public class DataReader
    {
        #region Fields

        private readonly Dictionary<string, ModelPortfolioModel> modelPortfolios =
            new Dictionary<string, ModelPortfolioModel>();

        private readonly IDictionary<string, PortfolioModel> portfolios = new Dictionary<string, PortfolioModel>();

        private readonly IDictionary<string, SecurityModel> securities = new Dictionary<string, SecurityModel>();

        private readonly IList<SimulationParameters> simulationParameters = new List<SimulationParameters>();

        private IDictionary<string, ModelPortfolioRecord> modelPortfolioRecords;

        private IDictionary<string, PortfolioRecord> portfolioRecords;

        private IDictionary<string, SecurityRecord> securityRecords;

        private IList<SimulationParametersRecord> simParamsRecords;

        #endregion

        #region Public Methods and Operators

        public IList<SimulationParameters> BuildDb()
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
                                 TransactionFee = spr.TransactionFee,
                                 ForceInitialRebalancing = spr.ForceInitialRebalancing
                             };
                this.simulationParameters.Add(sp);
            }

            return this.simulationParameters;
        }

        #endregion

        #region Methods

        private ModelPortfolioModel GetModelPortfolio(string modelPortfolioId)
        {
            if (this.modelPortfolioRecords == null)
            {
                this.ReadModelPortfolios();
                foreach (var record in this.modelPortfolioRecords)
                {
                    var mpm = new ModelPortfolioModel { Name = record.Value.Name };
                    foreach (var ar in record.Value.Assets)
                    {
                        var asset = new ModelPortfolioAsset
                                        {
                                            Security = this.GetSecurity(ar.Value.Ticker),
                                            Allocation = ar.Value.Allocation
                                        };
                        mpm.Assets.Add(asset);
                    }

                    this.modelPortfolios.Add(record.Key, mpm);
                }
            }

            return this.modelPortfolios[modelPortfolioId];
        }

        private PortfolioModel GetPortfolio(string portfolioId)
        {
            if (this.portfolioRecords == null)
            {
                this.ReadPortfolios();
                foreach (var record in this.portfolioRecords)
                {
                    var pm = new PortfolioModel
                                 {
                                     Name = record.Value.Name,
                                     TransactionFee = record.Value.TransactionFee
                                 };
                    foreach (var hr in record.Value.Holdings)
                    {
                        var asset = new PortfolioAssetModel(this.GetSecurity(hr.Value.Ticker))
                                        {
                                            Units = hr.Value.Units,
                                            BookCost =
                                                hr.Value.BookCost
                                        };
                        pm.Holdings.Add(asset);
                    }

                    this.portfolios.Add(record.Key, pm);
                }
            }

            return this.portfolios[portfolioId];
        }

        private SecurityModel GetSecurity(string ticker)
        {
            if (this.securityRecords == null)
            {
                this.ReadSecurities();
                foreach (var record in this.securityRecords)
                {
                    var sm = new SecurityModel
                                 {
                                     Ticker = record.Value.Ticker,
                                     Name = record.Value.Name,
                                     AllowsPartialShares = record.Value.PartialShares,
                                     FixedPrice = record.Value.FixedPrice,
                                     BuyTransactionFee = record.Value.BuyFee,
                                     SellTransactionFee = record.Value.SellFee
                                 };

                    if (record.Value.PriceHistory != null)
                    {
                        foreach (var spr in record.Value.PriceHistory)
                        {
                            var sp = new SecurityPriceModel
                                         {
                                             TransactionDate = spr.Date,
                                             OpenPrice = spr.Open,
                                             HighPrice = spr.High,
                                             LowPrice = spr.Low,
                                             ClosePrice = spr.Close,
                                             Volume = spr.Volume,
                                             AdjustedClose = spr.AdjClose,
                                         };
                            sm.PriceHistory.Add(sp);
                        }
                    }

                    if (record.Value.DividendHistory != null)
                    {
                        foreach (var dhr in record.Value.DividendHistory)
                        {
                            var d = new SecurityDividendModel
                                        {
                                            TransactionDate = dhr.Date,
                                            Amount = dhr.Dividends
                                        };
                            sm.DividendHistory.Add(d);
                        }
                    }

                    this.securities.Add(record.Key, sm);
                }
            }

            return this.securities[ticker];
        }

        private IDictionary<string, ModelPortfolioAssetRecord> ReadModelPortfolioAssets(string modelPortfolioId)
        {
            var fileName = string.Format("config/{0}_ModelPortfolioAssets.csv", modelPortfolioId);
            using (var reader = new StreamReader(fileName))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<ModelPortfolioAssetRecord>();
                return csv.GetRecords<ModelPortfolioAssetRecord>().ToDictionary(o => o.Ticker);
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

        private IDictionary<string, PortfolioAssetRecord> ReadPortfolioHoldings(string portfolioId)
        {
            var fileName = string.Format("config/{0}_Holdings.csv", portfolioId);
            using (var reader = new StreamReader(fileName))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<PortfolioAssetRecord>();
                return csv.GetRecords<PortfolioAssetRecord>().ToDictionary(o => o.Ticker);
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
                csv.Configuration.RegisterClassMap<SecurityRecord>();
                this.securityRecords = csv.GetRecords<SecurityRecord>().ToDictionary(o => o.Ticker);
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

        private IList<SecurityDividendRecord> ReadSecurityDividendHistory(string ticker)
        {
            using (var reader = new StreamReader(string.Format("config/{0}_SecurityDividends.csv", ticker)))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<SecurityDividendRecord>();
                return csv.GetRecords<SecurityDividendRecord>().OrderBy(o => o.Date).ToList();
            }
        }

        private IList<SecurityPriceRecord> ReadSecurityPriceHistory(string ticker)
        {
            using (var reader = new StreamReader(string.Format("config/{0}_SecurityPrices.csv", ticker)))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<SecurityPriceRecord>();
                return csv.GetRecords<SecurityPriceRecord>().OrderBy(o => o.Date).ToList();
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
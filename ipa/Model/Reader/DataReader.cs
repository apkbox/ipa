// --------------------------------------------------------------------------------
// <copyright file="DataReader.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the DataReader type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa.Model
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CsvHelper;

    using Ipa.Model.Reader;

    public class DataReader
    {
        #region Constants

        private const string PriceHistoryFileName = @"config/{0}.csv";

        private const string SecuritiesFileName = @"config/securities.csv";

        #endregion

        #region Fields

        private IDictionary<string, ModelPortfolioRecord> modelPortfolioRecords;

        private IDictionary<string, PortfolioRecord> portfolioRecords;

        private IDictionary<string, SecurityRecord> securityRecords;

        private IList<SimulationParametersRecord> simParamsRecords;

        private Dictionary<string, ModelPortfolioModel> modelPortfolios;

        #endregion

        #region Public Methods and Operators

        public IList<SecurityModel> ReadSecurities()
        {
            List<SecurityModel> securities;

            using (var reader = new StreamReader(SecuritiesFileName))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<SecurityModel>();
                securities = csv.GetRecords<SecurityModel>().ToList();
            }

            foreach (var security in securities)
            {
                if (security.FixedPrice != null)
                {
                    continue;
                }

                using (var reader = new StreamReader(string.Format(PriceHistoryFileName, security.Ticker)))
                {
                    var csv = new CsvReader(reader);
                    csv.Configuration.RegisterClassMap<SecurityPriceModel>();
                    foreach (var rec in csv.GetRecords<SecurityPriceModel>())
                    {
                        security.PriceHistory.Add(rec);
                    }
                }
            }

            return securities;
        }

        #endregion

        #region Methods

        private void BuildDb()
        {
            this.ReadSimulationParameters();
            this.ReadPortfolios();
            this.ReadModelPortfolios();

            foreach (var spr in this.simParamsRecords)
            {
                var sp = new SimulationParameters
                             {
                                 ModelPortfolio = this.GetModelPortfolio(spr.ModelPortfolioId),
                                 InitialPortfolio = this.GetPortfolio(spr.PortfolioId),
                                 InceptionDate = spr.InceptionDate,
                                 StopDate = spr.StopDate,
                                 TransactionFee = spr.TransacionFee,
                                 ForceInitialRebalancing = spr.ForceInitialRebalancing
                             };
            }
        }

        private PortfolioModel GetPortfolio(string portfolioId)
        {
            
        }

        private ModelPortfolioModel GetModelPortfolio(string modelPortfolioId)
        {
            if (this.modelPortfolios == null)
            {
                this.modelPortfolios = new Dictionary<string, ModelPortfolioModel>();
            }

            this.modelPortfolios.TryGetValue()
        }

        // public SimulationParameters ReadSimulationParameters()
        // {
        // using (var reader = new StreamReader(SimulationParametersFileName))
        // {
        // var csv = new CsvReader(reader);
        // csv.Configuration.RegisterClassMap<SimulationParameters>();
        // var parameters = csv.GetRecords<SimulationParameters>();
        // return parameters.FirstOrDefault();
        // }
        // }
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
                this.modelPortfolioRecords = csv.GetRecords<ModelPortfolioRecord>().ToDictionary(o => o.ModelPortfolioId);
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

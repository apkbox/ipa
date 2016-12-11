// --------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------
namespace Ipa
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Common.Logging;

    using CsvHelper;
    using CsvHelper.TypeConversion;

    using Ipa.Model;
    using Ipa.Model.Reader;

    using NLog.Fluent;

    internal class Program
    {
        #region Constants

        private const int DefaultWindowsHeight = 60;

        private const int DefaultWindowsWidth = 140;

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            var logger = LogManager.GetLogger<Program>();
            logger.Info("Started");

            if (Debugger.IsAttached)
            {
                new Program().Run(args);
            }
            else
            {
                try
                {
                    new Program().Run(args);
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                    Debug.WriteLine(ex);
                }
            }

            logger.Info("Exiting");

            // Console.ReadKey();
        }

        private void Run(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

            Console.BufferWidth = 140;
            Console.BufferHeight = 3000;
            Console.WindowWidth = Console.LargestWindowWidth < DefaultWindowsWidth
                                      ? Console.LargestWindowWidth
                                      : DefaultWindowsWidth;
            Console.WindowHeight = Console.LargestWindowHeight < DefaultWindowsHeight
                                       ? Console.LargestWindowHeight
                                       : DefaultWindowsHeight;

            var db = DataReader.LoadDb();
            var simParams = db.SimulationParameters;

            string simId = args[0];
            var p = simParams.First(o => o.SimulationId == simId);

            var newFile = !File.Exists("Stats.csv");

            using (var stream = new StreamWriter("Stats.csv", true))
            {
                var csv = new CsvWriter(stream);
                csv.Configuration.RegisterClassMap<StatRecord>();
                if (newFile)
                {
                    csv.WriteHeader<StatRecord>();
                }

                Console.WriteLine("{0:d}", p.InceptionDate);

                var sim = new Simulator(p);
                while (sim.ResumeSimulation())
                {
                    sim.DefaultScheduleHandler();
                }

                sim.PrintPortfolioHoldingsStats();
                var stats = sim.CalculatePortfolioStats();
                Simulator.PrintPortfolioStats(stats);

                csv.WriteRecord(
                    new StatRecord
                        {
                            SimulationId = simId,
                            StartDate = p.InceptionDate,
                            TotalReturn = stats.TotalReturn,
                            TotalReturnRate = stats.TotalReturnRate,
                            AnnualizedReturnRate = stats.AnnualizedReturnRate
                        });
            }
        }

        private void RunThrough()
        {
            Console.BufferWidth = 140;
            Console.BufferHeight = 3000;
            Console.WindowWidth = Console.LargestWindowWidth < DefaultWindowsWidth
                                      ? Console.LargestWindowWidth
                                      : DefaultWindowsWidth;
            Console.WindowHeight = Console.LargestWindowHeight < DefaultWindowsHeight
                                       ? Console.LargestWindowHeight
                                       : DefaultWindowsHeight;

            var db = DataReader.LoadDb();
            var simParams = db.SimulationParameters;

            const string SimId = "Garth1_CASH_20k";
            var p = simParams.First(o => o.SimulationId == SimId);

            var initialPortfolio = new Portfolio(p.InitialPortfolio);

            var qstream = new StreamWriter("q.csv");
            using (var stream = new StreamWriter("InceptionStart.csv"))
            {
                var csv = new CsvWriter(stream);
                csv.Configuration.RegisterClassMap<StatRecord>();
                csv.WriteHeader<StatRecord>();

                var quotes = new CsvWriter(qstream);
                foreach (var h in p.ModelPortfolio.Assets)
                {
                    quotes.WriteField(h.Security.Ticker);
                }

                quotes.NextRecord();

                var startDate = p.InceptionDate;
                var endDate = DateTime.Today.Subtract(TimeSpan.FromDays(30));
                p.StopDate = endDate;
                while (startDate < endDate)
                {
                    Console.WriteLine("{0:d}", startDate);
                    p.InceptionDate = startDate;
                    p.InitialPortfolio = new Portfolio(initialPortfolio);

                    var sim = new Simulator(p);
                    while (sim.ResumeSimulation())
                    {
                        sim.DefaultScheduleHandler();
                    }

                    sim.PrintPortfolioHoldingsStats();
                    var stats = sim.CalculatePortfolioStats();
                    Simulator.PrintPortfolioStats(stats);

                    foreach (var h in p.ModelPortfolio.Assets)
                    {
                        var q = h.Security.GetLastQuote(startDate);
                        var avg = q.AveragePrice;
                        quotes.WriteField(avg);
                    }

                    quotes.NextRecord();

                    csv.WriteRecord(new StatRecord
                                        {
                                            StartDate = p.InceptionDate,
                                            TotalReturn = stats.TotalReturn,
                                            TotalReturnRate = stats.TotalReturnRate,
                                            AnnualizedReturnRate = stats.AnnualizedReturnRate
                                        });

                    //stream.Flush();
                    //qstream.Flush();

                    startDate = startDate.AddDays(10);
                }
            }
        }

        #endregion
    }
}

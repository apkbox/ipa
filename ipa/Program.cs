// --------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------

namespace ipa
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Common.Logging;

    using CsvHelper;

    using SimulationCore;
    using SimulationCore.Model;
    using SimulationCore.Model.Reader;

    internal class Program
    {
        #region Constants

        private const int DefaultWindowsHeight = 60;

        private const int DefaultWindowsWidth = 140;

        #endregion

        #region Static Fields

        private static readonly ILog Log = LogManager.GetLogger<Program>();

        private static int bheight;

        private static int bwidth;

        private static int wheight;

        private static int wwidth;

        #endregion

        #region Methods

        private static void AdjustConsoleWindow()
        {
            Console.WindowWidth = Console.LargestWindowWidth < DefaultWindowsWidth
                                      ? Console.LargestWindowWidth
                                      : DefaultWindowsWidth;
            Console.WindowHeight = Console.LargestWindowHeight < DefaultWindowsHeight
                                       ? Console.LargestWindowHeight
                                       : DefaultWindowsHeight;
            Console.BufferWidth = Math.Max(Console.WindowWidth, DefaultWindowsWidth);
            Console.BufferHeight = Math.Max(Console.WindowHeight, 3000);
        }

        private static void Main(string[] args)
        {
            var logger = LogManager.GetLogger<Program>();
            logger.Info("Started");

            SaveConsoleParameters();
            AdjustConsoleWindow();

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
            RestoreConsoleParameters();
        }

        private static void RestoreConsoleParameters()
        {
            Console.SetWindowPosition(0, 0);

            if (wwidth > Console.BufferWidth)
            {
                Console.BufferWidth = bwidth;
                Console.WindowWidth = wwidth;
            }
            else
            {
                Console.WindowWidth = wwidth;
                Console.BufferWidth = bwidth;
            }

            if (wheight > Console.BufferHeight)
            {
                Console.BufferHeight = bheight;
                Console.WindowHeight = wheight;
            }
            else
            {
                Console.WindowHeight = wheight;
                Console.BufferHeight = bheight;
            }
        }

        private static void SaveConsoleParameters()
        {
            wwidth = Console.WindowWidth;
            wheight = Console.WindowHeight;
            bwidth = Console.BufferWidth;
            bheight = Console.BufferHeight;
        }

        private void Run(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

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

                Log.InfoFormat(
                    "Simulating portfolio '{0}' using model portfolio '{1}'",
                    p.InitialPortfolio.Name,
                    p.ModelPortfolio.Name);

                p.InitialPortfolio.ModelPortfolio = p.ModelPortfolio;
                var sim = new Simulator(p.InitialPortfolio, p.InceptionDate, p.StopDate, p.SetInitialBookCost, p.ForceInitialRebalancing);
                sim.Schedule = new QuarterlySchedule();

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
                            InitialValue = stats.InitialPortfolioValue,
                            MarketValue = stats.MarketValue,
                            DividendsPaid = stats.DividendsPaid,
                            ManagementExpenses = stats.ManagementExpenses,
                            TotalReturn = stats.TotalReturn,
                            TotalReturnRate = stats.TotalReturnRate,
                            AnnualizedReturnRate = stats.AnnualizedReturnRate
                        });
            }
        }

        #endregion
    }
}
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
    using System.Linq;

    using Common.Logging;

    using Ipa.Model;
    using Ipa.Model.Reader;

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

            try
            {
                Console.BufferWidth = 140;
                Console.BufferHeight = 3000;
                Console.WindowWidth = Console.LargestWindowWidth < DefaultWindowsWidth
                                          ? Console.LargestWindowWidth
                                          : DefaultWindowsWidth;
                Console.WindowHeight = Console.LargestWindowHeight < DefaultWindowsHeight
                                           ? Console.LargestWindowHeight
                                           : DefaultWindowsHeight;

                var reader = new DataReader();
                var simParams = reader.BuildDb();
                var sim = new Simulator();
                //const string SimId = "Lousy";
                const string SimId = "Garth1_Ex";
                // const string SimId = "Garth1_CASH_20k";
                // const string SimId = "RBC_ETF_CASH_20k";
                // const string SimId = "XUS_VSB_CASH_20k";
                sim.StartSimulation(simParams.First(o => o.SimulationId == SimId));
                while (sim.ResumeSimulation())
                {
                    sim.DefaultScheduleHandler();
                }

                sim.PrintPortfolioStats();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }

            logger.Info("Exiting");

            // Console.ReadKey();
        }

        #endregion
    }
}
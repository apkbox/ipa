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

    using NLog.Fluent;

    internal class Program
    {
        private const int DefaultWindowsWidth = 140;
        private const int DefaultWindowsHeight = 60;


        #region Methods

        private static void Main(string[] args)
        {
            var logger = LogManager.GetLogger<Program>();
            logger.Info("Started");

            try {
                Console.BufferWidth = 140;
                Console.BufferHeight = 3000;
                Console.WindowWidth = Console.LargestWindowWidth < DefaultWindowsWidth ?
                    Console.LargestWindowWidth : DefaultWindowsWidth;
                Console.WindowHeight = Console.LargestWindowHeight < DefaultWindowsHeight ?
                    Console.LargestWindowHeight : DefaultWindowsHeight;

                var reader = new DataReader();
                var simParams = reader.BuildDb();
                new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "Lousy"));
                //new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "Garth1_Ex"));
                //new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "Garth1_CASH_20k"));
                //new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "RBC_ETF_CASH_20k"));
                //new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "XUS_VSB_CASH_20k"));
            }
            catch(Exception ex) {
                logger.Fatal(ex);
            }
            logger.Info("Exiting");
            // Console.ReadKey();
        }

        #endregion
    }
}

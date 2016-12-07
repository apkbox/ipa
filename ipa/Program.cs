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
        #region Methods

        private static void Main(string[] args)
        {
            Console.BufferWidth = 140;
            Console.BufferHeight = 3000;
            Console.WindowWidth = 150;
            Console.WindowHeight = 60;

            var logger = LogManager.GetLogger<Program>();
            logger.Info("Started");
            var reader = new DataReader();
            var simParams = reader.BuildDb();
            new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "Garth1_Ex"));
            //new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "Garth1_CASH_20k"));
            //new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "RBC_ETF_CASH_20k"));
            //new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "XUS_VSB_CASH_20k"));

            logger.Info("Exiting");
            // Console.ReadKey();
        }

        #endregion
    }
}

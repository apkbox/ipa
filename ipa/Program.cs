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
            new Simulator().SimulatePortfolio(simParams.First(o => o.SimulationId == "SMALL"));

            logger.Info("Exiting");
            // Console.ReadKey();
        }

        #endregion
    }
}

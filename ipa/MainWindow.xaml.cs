// --------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa
{
    using System;
    using System.Windows;

    using Ipa.Model;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors and Destructors

        public MainWindow()
        {
            this.InitializeComponent();
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var reader = new DataReader();
            var simParams = reader.ReadSimulationParameters();
            var securities = reader.ReadSecurities();
            /*
            SimulationParameters simParams = new SimulationParameters();
            simParams.ForceInitialRebalancing = true;
            simParams.InceptionDate = new DateTime(2016, 1, 1);
            simParams.TransactionFee = 9.95m;
            simParams.ModelPortfolio = new ModelPortfolioModel()
                                           {
                                               Name = "Test",
                                               Assets
                                           }
             * */
        }
    }
}
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;

    using Ipa.Annotations;
    using Ipa.Model;
    using Ipa.Model.Reader;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        private IList<SimulationParameters> simParams;

        #endregion

        #region Constructors and Destructors

        public MainWindow()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public IList<SimulationParameters> SimParams
        {
            get
            {
                return this.simParams;
            }

            private set
            {
                if (value == this.simParams)
                {
                    return;
                }

                this.simParams = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var h = this.PropertyChanged;
            if (h != null)
            {
                h(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var reader = new DataReader();
            this.SimParams = reader.BuildDb();
            new Simulator().SimulatePortfolio(this.simParams.First());
        }

        #endregion
    }
}
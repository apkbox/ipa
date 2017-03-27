// --------------------------------------------------------------------------------
// <copyright file="Asset.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the PortfolioAssetModel type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore.Model
{
    using System.Diagnostics;

    using Common.Logging;

    /// <summary>
    /// The owned asset.
    /// </summary>
    public class Asset
    {
        #region Static Fields

        private static readonly ILog Log = LogManager.GetLogger<Asset>();

        #endregion

        #region Fields

        private decimal lastPrice;

        private decimal units;

        #endregion

        #region Constructors and Destructors

        public Asset(FinSec security)
        {
            this.Security = security;
        }

        public Asset(Asset other)
        {
            this.BookValue = other.BookValue;
            this.DividendsPaid = other.DividendsPaid;
            this.lastPrice = other.LastPrice;
            this.ManagementCost = other.ManagementCost;
            this.Security = other.Security;
            this.units = other.Units;
        }

        #endregion

        #region Public Properties

        public decimal BookPrice
        {
            get
            {
                if (this.Units == 0)
                {
                    return 0;
                }

                return this.BookValue / this.Units;
            }
        }

        /// <summary>
        /// Gets or sets book cost of the asset.
        /// </summary>
        public decimal BookValue { get; set; }

        /// <summary>
        /// Gets or sets total dividends paid for this asset.
        /// </summary>
        public decimal DividendsPaid { get; set; }

        /// <summary>
        /// Gets whether the asset is cash.
        /// </summary>
        public bool IsCash
        {
            get
            {
                return this.Security.IsCash;
            }
        }

        /// <summary>
        /// Gets or sets last price of the security.
        /// </summary>
        public decimal LastPrice
        {
            get
            {
                if (this.IsCash)
                {
                    if (this.Security.FixedPrice == null || this.Security.FixedPrice == 0)
                    {
                        Log.WarnFormat("Cash asset {0} references currency with zero fixed price", this.Security.Ticker);
                    }

                    return this.Security.FixedPrice ?? 0.01m;
                }

                return this.lastPrice;
            }

            set
            {
                if (this.IsCash)
                {
                    Debug.Assert(!this.IsCash, "Cannot set last price for cash asset");
                    Log.ErrorFormat("Attempt to set last price for cash asset {0}.", this.Security.Ticker);
                }

                this.lastPrice = value;
            }
        }

        /// <summary>
        /// Gets or sets total management cost.
        /// </summary>
        public decimal ManagementCost { get; set; }

        /// <summary>
        /// Gets market value of the asset.
        /// </summary>
        public decimal MarketValue
        {
            get
            {
                if (this.IsCash)
                {
                    return this.BookValue;
                }

                return this.LastPrice * this.Units;
            }
        }

        /// <summary>
        /// Gets security of the asset.
        /// </summary>
        public FinSec Security { get; private set; }

        /// <summary>
        /// Gets or sets number of units held in portfolio.
        /// </summary>
        public decimal Units
        {
            get
            {
                if (this.IsCash)
                {
                    if (this.Security.FixedPrice == null || this.Security.FixedPrice == 0)
                    {
                        Log.WarnFormat("Cash asset {0} references currency with zero fixed price", this.Security.Ticker);
                    }

                    return this.BookValue / (this.Security.FixedPrice ?? 0.01m);
                }

                return this.units;
            }

            set
            {
                if (this.IsCash)
                {
                    Log.WarnFormat("Attempt to set units for cash asset {0}. Ignored.", this.Security.Ticker);
                }

                this.units = value;
            }
        }

        #endregion
    }
}
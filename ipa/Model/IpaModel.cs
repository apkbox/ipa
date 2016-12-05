// --------------------------------------------------------------------------------
// <copyright file="IpaModel.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the IpaModel type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System.Linq;

    public class IpaModel
    {
        #region Fields

        private readonly SimulationState simState = new SimulationState();

        #endregion

        #region Public Methods and Operators

        public void SimulatePortfolio(SimulationParameters parameters)
        {
            // Create portfolio holdings
            // Just go straight to rebalance! (Assuming cash is implemented as asset with
            // price $0.01.
            /*
            foreach (var asset in parameters.ModelPortfolio.Assets)
            {
                var priceEntry = asset.Security.GetPriceEntry(parameters.InceptionDate);
                if (priceEntry == null)
                {
                    throw new InvalidOperationException("Security price unavailable");
                }

                var cashPerAsset = parameters.Cash * asset.Allocation;
                var shares = asset.Security.AllowsPartialShares
                                 ? cashPerAsset / priceEntry.AveragePrice
                                 : Math.Truncate(cashPerAsset / priceEntry.AveragePrice);

                var pfAsset = new PortfolioAssetModel();
                pfAsset.Security = asset.Security;
                pfAsset.Units = shares;
                pfAsset.BookCost = (shares * priceEntry.AveragePrice) + parameters.TransactionFee;
                pfAsset.ManagementCost = parameters.TransactionFee;

                simState.Holdings.Add(pfAsset);
            }
             */
            this.simState.CurrentDate = parameters.InceptionDate;
            this.simState.LastRebalancing = this.simState.CurrentDate;
            this.simState.Portfolio.TransactionFee = parameters.TransactionFee;

            if (parameters.ForceInitialRebalancing)
            {
                // TODO: Perform initial rebalancing.
            }

            // Walk the timeline and update market price, calculate dividends and perform rebalancing
            // if needed.
            while (this.simState.CurrentDate <= parameters.StopDate)
            {
                this.simState.CurrentDate = this.simState.CurrentDate.AddDays(1);

                // Update current market value
                foreach (var asset in this.simState.Portfolio.Holdings)
                {
                    var priceEntry = asset.Security.GetLastPriceEntry(this.simState.CurrentDate);
                    asset.LastPrice = priceEntry.AveragePrice;
                    asset.MarketValue = priceEntry.AveragePrice * asset.Units;
                    asset.DividendsPaid += asset.Security.GetDividends(this.simState.CurrentDate) * asset.Units;
                }

                // Calculate portfolio total market value
                this.simState.Portfolio.MarketValue = this.simState.Portfolio.Holdings.Sum(o => o.MarketValue);

                var elapsedSinceLastRebalancing = this.simState.CurrentDate.Subtract(this.simState.LastRebalancing);

                // Check if rebalancing is needed
                if (this.simState.Portfolio.RebalancingStrategy.Check(
                    elapsedSinceLastRebalancing,
                    parameters.ModelPortfolio,
                    this.simState.Portfolio))
                {
                    this.simState.Portfolio.RebalancingStrategy.Rebalance(
                        parameters.ModelPortfolio,
                        this.simState.Portfolio);
                }
            }
        }

        #endregion
    }
}
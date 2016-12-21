// --------------------------------------------------------------------------------
// <copyright file="Simulator.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the Simulator type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Common.Logging;

    public class Simulator
    {
        #region Static Fields

        private static readonly ILog Log = LogManager.GetLogger<Simulator>();

        #endregion

        #region Fields

        // private readonly ISchedule schedule = new MonthlySchedule();
        private readonly ISchedule schedule = new QuarterlySchedule();
        //private readonly ISchedule schedule = new SemiannualSchedule();

        private readonly SimulationParameters simParams;

        private bool isResumed;

        private decimal initialPortfolioValue;

        #endregion

        #region Constructors and Destructors

        public Simulator(SimulationParameters parameters)
        {
            Log.InfoFormat(
                "Simulating portfolio '{0}' using model portfolio '{1}'",
                parameters.InitialPortfolio.Name,
                parameters.ModelPortfolio.Name);

            this.simParams = parameters;
            this.TradingQueue = new List<TradeOrder>();
            this.CurrentDate = parameters.InceptionDate;
            this.Portfolio = parameters.InitialPortfolio;

            this.initialPortfolioValue = parameters.InitialPortfolio.BookValue;

            if (parameters.SetInitialBookCost)
            {
                this.UpdateHoldingsInitialBookCost();
            }

            this.UpdateHoldingsMarketValue();

            if (parameters.ForceInitialRebalancing)
            {
                Log.InfoFormat("Performing initial rebalancing on {0:D}", this.CurrentDate);
                this.TradePlan = this.Portfolio.RebalancingStrategy.Rebalance(parameters.ModelPortfolio, this.Portfolio);
            }
        }

        #endregion

        #region Public Properties

        public IList<TradePlanItem> TradePlan { get; set; }

        public IList<TradeOrder> TradingQueue { get; private set; }

        #endregion

        #region Properties

        private DateTime CurrentDate { get; set; }

        private Portfolio Portfolio { get; set; }

        #endregion

        #region Public Methods and Operators

        public static void PrintPortfolioStats(PortfolioStats stats)
        {
            Log.InfoFormat(
                "Total                    {0,12:C}           {1,12:C} {2,12:C} {3,9:C}",
                stats.BookCost,
                stats.MarketValue,
                stats.DividendsPaid,
                stats.ManagementExpenses);

            Log.InfoFormat("Total return: {0:P}   {1:C}", stats.TotalReturnRate, stats.TotalReturn);
            Log.InfoFormat("Annualized return: {0:P}", stats.AnnualizedReturnRate);
        }

        public PortfolioStats CalculatePortfolioStats()
        {
            var bookCost = this.Portfolio.Holdings.Sum(o => o.BookValue);
            var marketValue = this.Portfolio.Holdings.Sum(o => o.MarketValue);
            var dividendsPaid = this.Portfolio.Holdings.Sum(o => o.DividendsPaid);
            var managementExpenses = this.Portfolio.Holdings.Sum(o => o.ManagementCost);
            var totalReturn = this.Portfolio.MarketValue - this.initialPortfolioValue + dividendsPaid - managementExpenses;
            var totalReturnRate = totalReturn / this.initialPortfolioValue;
            var period = this.CurrentDate.Subtract(this.simParams.InceptionDate);
            var annualizedReturnRate = Math.Pow((double)(1.0m + totalReturnRate), 365.0 / period.TotalDays) - 1.0;
            return new PortfolioStats()
                       {
                           BookCost = bookCost,
                           MarketValue = marketValue,
                           InitialPortfolioValue = this.initialPortfolioValue,
                           DividendsPaid = dividendsPaid,
                           ManagementExpenses = managementExpenses,
                           TotalReturn = totalReturn,
                           TotalReturnRate = totalReturnRate,
                           AnnualizedReturnRate = (decimal)annualizedReturnRate
                       };
        }

        public void DefaultScheduleHandler()
        {
            // TODO: Replace with callback
            var result = this.Portfolio.RebalancingStrategy.Check(this.simParams.ModelPortfolio, this.Portfolio);
            if (result)
            {
                Log.InfoFormat("Performing rebalancing on {0:D}", this.CurrentDate);
                this.TradePlan = this.Portfolio.RebalancingStrategy.Rebalance(
                    this.simParams.ModelPortfolio,
                    this.Portfolio);
            }
            else
            {
                Log.InfoFormat("ScheduledStop time arrived on {0:D}, but was not required", this.CurrentDate);
            }
        }

        public void PrintPortfolioHoldingsStats()
        {
            Log.Info(string.Empty);
            Log.Info(" Ticker  Units   Book Pr   Book Value   Last Pr Market Value    Dividends Mgmt Cost Allocation");
            Log.Info("------- ------ --------- ------------ --------- ------------ ------------ --------- ----------");
            foreach (var asset in this.Portfolio.Holdings)
            {
                var currentAllocation = asset.MarketValue / this.Portfolio.MarketValue;
                Log.InfoFormat(
                    "{0,7} {1,6} {2,9:C} {3,12:C} {4,9:C} {5,12:C} {6,12} {7,9:C} {8,10:P}",
                    asset.Security.Ticker,
                    asset.Units,
                    asset.BookPrice,
                    asset.BookValue,
                    asset.LastPrice,
                    asset.MarketValue,
                    asset.DividendsPaid,
                    asset.ManagementCost,
                    currentAllocation);
            }

            Log.Info("------- ------ --------- ------------ --------- ------------ ------------ --------- ----------");
        }

        public bool ResumeSimulation()
        {
            while (this.CurrentDate <= this.simParams.StopDate)
            {
                if (!this.isResumed)
                {
                    this.TradingQueue.Clear();
                    this.TradePlan = new List<TradePlanItem>();

                    if (this.schedule.IsArrived(this.CurrentDate))
                    {
                        this.isResumed = true;
                        return true;
                    }
                }

                this.isResumed = false;

                this.PrepareTradeOrders();
                this.ExecuteTradeOrders();

                this.UpdateHoldingsMarketValue();

                this.CurrentDate = this.CurrentDate.AddDays(1);
            }

            return false;
        }

        #endregion

        #region Methods

        private void ExecuteTradeOrders()
        {
            if (this.TradingQueue.Count == 0)
            {
                return;
            }

            Log.InfoFormat("Executing trade orders on {0:D}", this.CurrentDate);

            Log.Info("Cash before trades");
            foreach (var c in this.Portfolio.Holdings.Where(o => o.IsCash))
            {
                Log.InfoFormat("  {0,7} {1:C}", c.Security.Ticker, c.BookValue);
            }

            foreach (var order in this.TradingQueue.OrderBy(o => o.Units))
            {
                if (order.Security.IsCash)
                {
                    Log.FatalFormat(
                        "Attempt to execute trade order on cash for {0}, Units: {1}",
                        order.Security.Ticker,
                        order.Units);
                    Debug.Fail("Cash trade order");
                }

                if (order.Units == 0)
                {
                    Log.FatalFormat("No op trade order for {0}, Units: {1}", order.Security.Ticker, order.Units);
                    Debug.Fail("No op trade order");
                }

                var quote = order.Security.GetQuote(this.CurrentDate);
                if (quote == null)
                {
                    Log.FatalFormat("Quote for {0} on {1:D} is not available", order.Security.Ticker, this.CurrentDate);
                    Debug.Fail("Quote is not available");
                }

                var asset = this.Portfolio.Holdings.FirstOrDefault(o => o.Security.Ticker == order.Security.Ticker);
                if (asset == null)
                {
                    asset = new Asset(order.Security);
                    this.Portfolio.Holdings.Add(asset);
                }

                // If there is a price override - use it.
                // TODO: Use selector object to pick the transaction price - high, low, open, close or average.
                var spotPrice = order.Price ?? quote.AveragePrice;
                var fee = this.Portfolio.TransactionFee;
                var unitsCost = order.Units * spotPrice;
                var units = asset.Units + order.Units;
                if (units < 0)
                {
                    Log.ErrorFormat("Selling more units ({0}) than owned ({1})", order.Units, asset.Units);
                }

                var cashAsset = this.Portfolio.GetCashAsset("$CAD");
                cashAsset.BookValue -= unitsCost;
                cashAsset.BookValue -= fee;

                if (cashAsset.BookValue < 0)
                {
                    Log.FatalFormat("Negative cash ({0:C}) after executing trade order.", cashAsset.BookValue);
                    Debug.Fail("Negative cash after executing trade order.");
                }

                asset.Units = units;

                if (asset.Units == 0)
                {
                    asset.BookValue = 0;
                }
                else
                {
                    asset.BookValue += unitsCost;
                    asset.BookValue -= fee;
                }

                asset.ManagementCost += fee;

                Log.InfoFormat(
                    "{0,7} {1,4} {2,6} {3,9:C} {4,12:C}",
                    order.Security.Ticker,
                    order.Units < 0 ? "Sell" : "Buy",
                    order.Units,
                    spotPrice,
                    Math.Abs(unitsCost) + fee);
            }

            Log.Info("Cash after trades");
            foreach (var c in this.Portfolio.Holdings.Where(o => o.IsCash))
            {
                Log.InfoFormat("  {0,7} {1:C}", c.Security.Ticker, c.BookValue);
            }
        }

        private void PrepareTradeOrders()
        {
            if (this.TradePlan.Count == 0)
            {
                return;
            }

            Log.TraceFormat("Preparing trade orders on {0:D}", this.CurrentDate);

            foreach (var item in this.TradePlan)
            {
                // Skip cash
                if (item.Security.IsCash)
                {
                    continue;
                }

                if (item.Amount == 0)
                {
                    Log.FatalFormat("No op trade plan item for {0}, Amount: {1:C}", item.Security.Ticker, item.Amount);
                    Debug.Fail("No op trade plan item");
                }

                var priceEntry = item.Security.GetQuote(this.CurrentDate);
                if (priceEntry == null)
                {
                    Log.FatalFormat("Price for {0} on {1:D} is not available", item.Security.Ticker, this.CurrentDate);
                    Debug.Fail("Price not available");
                }

                var spotPrice = priceEntry.AveragePrice;

                var fee = this.Portfolio.TransactionFee;

                var units = item.Security.AllowsPartialShares
                                ? item.Amount / spotPrice
                                : Math.Truncate(item.Amount / spotPrice);

                Log.TraceFormat(
                    "Projection for {0} at {1:C} for {2} units. Fee {3:C}",
                    item.Security.Ticker,
                    spotPrice,
                    units,
                    fee);

                var asset = this.Portfolio.Holdings.FirstOrDefault(o => o.Security.Ticker == item.Security.Ticker);
                if (asset == null)
                {
                    Log.Trace("Asset currently not owned.");

                    if (item.Amount < 0)
                    {
                        Log.Fatal("Attempt to sell asset not being own.");
                        Debug.Fail("Attempt to sell asset not being own.");
                    }
                }
                else
                {
                    // Check if we try to sell more units than own
                    if (item.Amount < 0 && asset.Units < units)
                    {
                        Log.TraceFormat("Selling more units ({0}) than owned ({1})", units, asset.Units);
                        units = asset.Units;
                    }
                }

                var tradingBalance = units * spotPrice;
                Log.TraceFormat("Final price tag {0:C} for {1} units at {2:C}", tradingBalance, units, spotPrice);

                this.TradingQueue.Add(new TradeOrder { Security = item.Security, Units = units, });
            }
        }

        private void UpdateHoldingsInitialBookCost()
        {
            Log.InfoFormat("Updating portfolio initial book cost on {0:D}", this.CurrentDate);

            // Update current market value
            foreach (var asset in this.Portfolio.Holdings)
            {
                if (asset.IsCash)
                {
                    continue;
                }

                var priceEntry = asset.Security.GetLastQuote(this.CurrentDate);
                asset.LastPrice = priceEntry.AveragePrice;
                asset.BookValue = priceEntry.AveragePrice * asset.Units;

                Log.TraceFormat("    {0,7} {1,9:C} {2,12:C}", asset.Security.Ticker, asset.BookPrice, asset.BookValue);
            }
        }

        private void UpdateHoldingsMarketValue()
        {
            Log.TraceFormat("Updating portfolio market value on {0:D}", this.CurrentDate);

            var cashEntry = this.Portfolio.GetCashAsset("$CAD");
            var cash = cashEntry.BookValue;

            decimal totalDividends = 0;

            // Update current market value
            foreach (var asset in this.Portfolio.Holdings)
            {
                if (asset.IsCash)
                {
                    continue;
                }

                var priceEntry = asset.Security.GetLastQuote(this.CurrentDate);
                var dividendEntry = asset.Security.GetDividends(this.CurrentDate);
                asset.LastPrice = priceEntry.AveragePrice;
                var dividends = dividendEntry * asset.Units;
                asset.DividendsPaid += dividends;
                cash += dividends;
                totalDividends += dividends;
            }

            cashEntry.BookValue = cash;

            // Calculate portfolio total market value
            this.Portfolio.MarketValue = this.Portfolio.Holdings.Sum(o => o.MarketValue);

            Log.Trace(" Ticker  Units   Book Pr   Book Value   Last Pr Market Value    Dividends Allocation");
            Log.Trace("------- ------ --------- ------------ --------- ------------ ------------ ----------");

            var totalAllocation = 0m;
            foreach (var asset in this.Portfolio.Holdings)
            {
                var currentAllocation = asset.MarketValue / this.Portfolio.MarketValue;
                var dividendEntry = asset.Security.GetDividends(this.CurrentDate);
                Log.TraceFormat(
                    "{0,7} {1,6} {2,9:C} {3,12:C} {4,9:C} {5,12:C} {6,12} {7,10:P}",
                    asset.Security.Ticker,
                    asset.Units,
                    asset.BookPrice,
                    asset.BookValue,
                    asset.LastPrice,
                    asset.MarketValue,
                    dividendEntry,
                    currentAllocation);

                totalAllocation += currentAllocation;
            }

            Log.Trace("------- ------ --------- ------------ --------- ------------ ------------ ----------");
            Log.TraceFormat(
                "Total                    {0,12:C}           {1,12:C} {2,12} {3,10:P}",
                this.Portfolio.BookValue,
                this.Portfolio.MarketValue,
                totalDividends,
                totalAllocation);
        }

        #endregion
    }
}
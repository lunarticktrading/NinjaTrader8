#region Using declarations
using NinjaTrader.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Developer", 2)]
    public class DailySMA : Indicator
	{
        internal class DailyValue
        {
            public DailyValue(DateTime barStartTime, DateTime barEndTime, int barIndex, double value)
            {
                BarStartTime = barStartTime;
                BarEndTime = barEndTime;
                BarIndex = barIndex;
                Value = value;
            }

            public DateTime BarStartTime { get; }
            public DateTime BarEndTime { get; }
            public int BarIndex { get; }
            public double Value { get; }
        }

        #region Constants

        public const string Version = "1.0.0";

        #endregion

        #region Members

        private SMA _dailySMA;
        private Dictionary<int, DailyValue> _historicDailyValues = new();
        private bool _backfillHistoricBars = false;
        private double _lastDailyValue = 0;
		private int _firstBarOfSession = 0;

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period", Order = 1, GroupName = "Parameters")]
        public int Period
        { get; set; }

        [ReadOnly(true)]
        [XmlIgnore]
        [Display(Name = "Version", Description = "Version information.", Order = 1, GroupName = "Developer")]
        public string VersionInformation
        { get; set; }

        [Display(Name = "Debug", Description = "Toggle debug logging.", Order = 2, GroupName = "Developer")]
        public bool Debug
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Value
        {
            get { return Values[0]; }
        }

        #endregion

        #region Indicator methods

        protected override void OnStateChange()
		{
            DebugPrint($"OnStateChange({State})");

            if (State == State.SetDefaults)
			{
				Description									= @"Plots the daily SMA on intraday charts.";
				Name										= "DailySMA";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsAutoScale                                 = false;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

				Period										= 1;

                VersionInformation = $"{Version} - {Assembly.GetAssembly(typeof(DailySMA)).GetName().Version}";
                Debug = false;

                AddPlot(Brushes.White, "DailySMA");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Day, 1);

                _historicDailyValues.Clear();
                _backfillHistoricBars = false;
				_lastDailyValue = 0.0;
            }
            else if (State == State.DataLoaded)
			{
				_dailySMA = SMA(BarsArray[1], Period);
			}
            else if (State == State.Transition)
            {
                _backfillHistoricBars = true;
            }
        }

        protected override void OnBarUpdate()
		{
            if (BarsInProgress == 0)
            {
                // Chart bars.

                if (!Bars.BarsType.IsIntraday)
                    return;

                if (Bars.IsFirstBarOfSession)
                    _firstBarOfSession = CurrentBar;

                if (State == State.Realtime)
                {
                    if (_backfillHistoricBars)
                    {
                        // Backfill historic bars.
                        DebugPrint("Backfilling historic bars");
                        int dailyBarIndex = 0;
                        for (int i = 1; i < CurrentBar; i++)
                        {
                            int barsAgo = CurrentBar - i;
                            DateTime barStartTime = Time[barsAgo + 1];
                            if (TryGetDailyValue(barStartTime, dailyBarIndex, out DailyValue dailyValue))
                            {
                                Value[barsAgo] = dailyValue.Value;
                                dailyBarIndex = dailyValue.BarIndex; // Start from here next lookup, to save time.
                                DebugPrint($"Backfilling: i={i}, barsAgo={barsAgo}, barStartTime={barStartTime}, dailyBarIndex={dailyBarIndex}, historicValue={dailyValue.Value}");
                            }
                        }
                        _backfillHistoricBars = false;
                    }
                    Value[0] = _lastDailyValue;
                    // Plot all DailySMA values for the current day.
                    for (int barsAgo = 0; barsAgo <= CurrentBar - _firstBarOfSession; barsAgo++)
                        Value[barsAgo] = _lastDailyValue;
                }
            }
            else if (BarsInProgress == 1)
            {
                // Daily bars.

                if (State == State.Historical)
                {
                    // Called once at close of historic bar, so just record daily SMA value.
                    if (CurrentBar > 0)
                    {
                        _dailySMA.Update();
                        _historicDailyValues[CurrentBar] = new DailyValue(Time[1], Time[0], CurrentBar, _dailySMA[0]);
                        DebugPrint($"Recorded historical DailySMA [{CurrentBar}][{Time[0]}] = [{_dailySMA[0]}]");
                    }
                }
                else if (State == State.Realtime)
                {
                    _dailySMA.Update();
                    _lastDailyValue = _dailySMA[0];
                }
            }
        }

        #endregion

        #region Private methods

        private bool TryGetDailyValue(DateTime dt, int startIndex, out DailyValue result)
        {
            result = null;

            // Starting from startIndex, look for which Daily bar contains dt.
            for (int i = Math.Max(startIndex, 0); i < _historicDailyValues.Count; i++)
            {
                if (_historicDailyValues.TryGetValue(i, out var currDailyValue) && dt >= currDailyValue.BarStartTime && dt < currDailyValue.BarEndTime)
                {
                    // Found matching Daily bar.
                    DebugPrint($"TryGetDailyValue({dt}, {startIndex}): i={i}, dailyBarStartDateTime={currDailyValue.BarStartTime}, dailyBarEndDateTime={currDailyValue.BarEndTime}");
                    result = currDailyValue;
                    return true;
                }
            }

            // Not found.
            result = null;
            return false;
        }

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"DailySMA[{Instrument.FullName}]: {msg}");
                else
                    Print($"DailySMA: {msg}");
            }
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LunarTick.DailySMA[] cacheDailySMA;
		public LunarTick.DailySMA DailySMA(int period)
		{
			return DailySMA(Input, period);
		}

		public LunarTick.DailySMA DailySMA(ISeries<double> input, int period)
		{
			if (cacheDailySMA != null)
				for (int idx = 0; idx < cacheDailySMA.Length; idx++)
					if (cacheDailySMA[idx] != null && cacheDailySMA[idx].Period == period && cacheDailySMA[idx].EqualsInput(input))
						return cacheDailySMA[idx];
			return CacheIndicator<LunarTick.DailySMA>(new LunarTick.DailySMA(){ Period = period }, input, ref cacheDailySMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.DailySMA DailySMA(int period)
		{
			return indicator.DailySMA(Input, period);
		}

		public Indicators.LunarTick.DailySMA DailySMA(ISeries<double> input , int period)
		{
			return indicator.DailySMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.DailySMA DailySMA(int period)
		{
			return indicator.DailySMA(Input, period);
		}

		public Indicators.LunarTick.DailySMA DailySMA(ISeries<double> input , int period)
		{
			return indicator.DailySMA(input, period);
		}
	}
}

#endregion

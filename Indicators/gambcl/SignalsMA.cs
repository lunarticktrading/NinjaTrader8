#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.gambcl
{
    namespace SignalsMAEnums
    {
        public enum MATypeEnum
        {
            SMA,
            EMA
        }
    }

    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Signals", 2)]
    public class SignalsMA : Indicator
    {
        #region Members
        private Series<bool> _longEntrySignal;
        private Series<bool> _shortEntrySignal;
        #endregion

        #region Indicator methods
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description									= @"Moving Average that generates signals when price crosses and closes beyond the Moving Average.";
                Name										= "SignalsMA";
                Calculate									= Calculate.OnPriceChange;
                IsOverlay									= true;
                DisplayInDataBox							= true;
                DrawOnPricePanel							= true;
                DrawHorizontalGridLines						= true;
                DrawVerticalGridLines						= true;
                PaintPriceMarkers							= true;
                ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                MAType										= NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums.MATypeEnum.SMA;
                Period										= 9;
                SignalOffset								= 10;
                UseSignalColors								= true;
                LongSignalBrush								= Brushes.Green;
                ShortSignalBrush							= Brushes.Red;
                SignalPrefix								= "SignalsMA_";
                EnableAlerts                                = false;
                AlertSoundsPath                             = DefaultAlertFilePath();
                LongEntryAlert                              = "LongEntry.wav";
                ShortEntryAlert                             = "ShortEntry.wav";
                AddPlot(Brushes.White, "MovingAverage");
                AddPlot(Brushes.Transparent, "Signals");
            }
            else if (State == State.Configure)
            {
                // Disable IsSuspendedWhileInactive if alerts are enabled.
                IsSuspendedWhileInactive = !EnableAlerts;

                _longEntrySignal = new Series<bool>(this);
                _shortEntrySignal = new Series<bool>(this);
            }
        }

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                return Name + "(" + MAType + "," + Period + ")";
            }
        }
        
        protected override void OnBarUpdate()
        {
            _longEntrySignal[0] = false;
            _shortEntrySignal[0] = false;
            Signals[0] = 0;

            if (CurrentBar < Period)
                return;

            MovingAverage[0] = (MAType == NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums.MATypeEnum.EMA) ? (EMA(Close, Period)[0]) : (SMA(Close, Period)[0]);

            var barTime = Time[0];
            string longEntrySignalTag = string.Format("{0}LongEntry{1}", SignalPrefix, CurrentBar);
            string shortEntrySignalTag = string.Format("{0}ShortEntry{1}", SignalPrefix, CurrentBar);

            if ((Close[1] <= MovingAverage[1]) && (Close[0] > MovingAverage[0]))
            {
                // LONG entry signal.
                RemoveDrawObject(shortEntrySignalTag);
                _longEntrySignal[0] = true;
                Signals[0] = 1;
                Draw.ArrowUp(this,
                    longEntrySignalTag,
                    true,
                    barTime,
                    Low[0] - (SignalOffset * TickSize),
                    UseSignalColors ? LongSignalBrush : Plots[0].Brush);
            }
            else if ((Close[1] >= MovingAverage[1]) && (Close[0] < MovingAverage[0]))
            {
                // SHORT entry signal.
                RemoveDrawObject(longEntrySignalTag);
                _shortEntrySignal[0] = true;
                Signals[0] = -1;
                Draw.ArrowDown(this,
                    shortEntrySignalTag,
                    true,
                    barTime,
                    High[0] + (SignalOffset * TickSize),
                    UseSignalColors ? ShortSignalBrush : Plots[0].Brush);
            }
            else
            {
                // No signal.
                RemoveDrawObject(longEntrySignalTag);
                RemoveDrawObject(shortEntrySignalTag);
            }

            // Alerts
            if (EnableAlerts && (State == State.Realtime) && IsFirstTickOfBar)
            {
                if (Signals[1] > 0 && !string.IsNullOrWhiteSpace(LongEntryAlert))
                {
                    // Long entry alert.
                    string audioFile = ResolveAlertFilePath(LongEntryAlert, AlertSoundsPath);
                    Alert("LongEntryAlert", Priority.High, "Long Entry", audioFile, 10, Brushes.Black, LongSignalBrush);
                }
                if (Signals[1] < 0 && !string.IsNullOrWhiteSpace(ShortEntryAlert))
                {
                    // Short entry alert.
                    string audioFile = ResolveAlertFilePath(ShortEntryAlert, AlertSoundsPath);
                    Alert("ShortEntryAlert", Priority.High, "Short Entry", audioFile, 10, Brushes.Black, ShortSignalBrush);
                }
            }
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Display(Name="MA Type", Description="The type of Moving Average.", Order=1, GroupName="Parameters")]
        public NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums.MATypeEnum MAType
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Period", Description="The period of the Moving Average.", Order=2, GroupName="Parameters")]
        public int Period
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name="Signal Offset", Description="The vertical offset between signal and bar.", Order=1, GroupName="Signals")]
        public int SignalOffset
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name="Use Signal Colors", Description="Use specified signal colors.", Order=2, GroupName="Signals")]
        public bool UseSignalColors
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="Long Signal Color", Description="Color used for LONG signals.", Order=3, GroupName="Signals")]
        public Brush LongSignalBrush
        { get; set; }

        [Browsable(false)]
        public string LongSignalBrushSerializable
        {
            get { return Serialize.BrushToString(LongSignalBrush); }
            set { LongSignalBrush = Serialize.StringToBrush(value); }
        }			

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="Short Signal Color", Description="Color used for SHORT signals.", Order=4, GroupName="Signals")]
        public Brush ShortSignalBrush
        { get; set; }

        [Browsable(false)]
        public string ShortSignalBrushSerializable
        {
            get { return Serialize.BrushToString(ShortSignalBrush); }
            set { ShortSignalBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "SignalPrefix", Description = "Prefix used when naming signal drawing objects.", Order = 5, GroupName = "Signals")]
        public string SignalPrefix
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Alerts", Description = "Trigger alerts for confirmed signals.", Order = 1, GroupName = "Alerts")]
        public bool EnableAlerts
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Alert Sounds Path", Description = "Location of alert audio files used for confirmed signals.", Order = 2, GroupName = "Alerts")]
        public string AlertSoundsPath
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Long Entry Alert", Description = "Alert sound used for confirmed LONG entry signals.", Order = 3, GroupName = "Alerts")]
        public string LongEntryAlert
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Short Entry Alert", Description = "Alert sound used for confirmed SHORT entry signals.", Order = 4, GroupName = "Alerts")]
        public string ShortEntryAlert
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MovingAverage
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Signals
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> LongEntrySignal
        {
            get { return _longEntrySignal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> ShortEntrySignal
        {
            get { return _shortEntrySignal; }
        }
        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private gambcl.SignalsMA[] cacheSignalsMA;
		public gambcl.SignalsMA SignalsMA(NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums.MATypeEnum mAType, int period, int signalOffset, bool useSignalColors, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return SignalsMA(Input, mAType, period, signalOffset, useSignalColors, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}

		public gambcl.SignalsMA SignalsMA(ISeries<double> input, NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums.MATypeEnum mAType, int period, int signalOffset, bool useSignalColors, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			if (cacheSignalsMA != null)
				for (int idx = 0; idx < cacheSignalsMA.Length; idx++)
					if (cacheSignalsMA[idx] != null && cacheSignalsMA[idx].MAType == mAType && cacheSignalsMA[idx].Period == period && cacheSignalsMA[idx].SignalOffset == signalOffset && cacheSignalsMA[idx].UseSignalColors == useSignalColors && cacheSignalsMA[idx].LongSignalBrush == longSignalBrush && cacheSignalsMA[idx].ShortSignalBrush == shortSignalBrush && cacheSignalsMA[idx].SignalPrefix == signalPrefix && cacheSignalsMA[idx].EnableAlerts == enableAlerts && cacheSignalsMA[idx].AlertSoundsPath == alertSoundsPath && cacheSignalsMA[idx].LongEntryAlert == longEntryAlert && cacheSignalsMA[idx].ShortEntryAlert == shortEntryAlert && cacheSignalsMA[idx].EqualsInput(input))
						return cacheSignalsMA[idx];
			return CacheIndicator<gambcl.SignalsMA>(new gambcl.SignalsMA(){ MAType = mAType, Period = period, SignalOffset = signalOffset, UseSignalColors = useSignalColors, LongSignalBrush = longSignalBrush, ShortSignalBrush = shortSignalBrush, SignalPrefix = signalPrefix, EnableAlerts = enableAlerts, AlertSoundsPath = alertSoundsPath, LongEntryAlert = longEntryAlert, ShortEntryAlert = shortEntryAlert }, input, ref cacheSignalsMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.gambcl.SignalsMA SignalsMA(NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums.MATypeEnum mAType, int period, int signalOffset, bool useSignalColors, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return indicator.SignalsMA(Input, mAType, period, signalOffset, useSignalColors, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}

		public Indicators.gambcl.SignalsMA SignalsMA(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums.MATypeEnum mAType, int period, int signalOffset, bool useSignalColors, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return indicator.SignalsMA(input, mAType, period, signalOffset, useSignalColors, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.gambcl.SignalsMA SignalsMA(NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums.MATypeEnum mAType, int period, int signalOffset, bool useSignalColors, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return indicator.SignalsMA(Input, mAType, period, signalOffset, useSignalColors, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}

		public Indicators.gambcl.SignalsMA SignalsMA(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.gambcl.SignalsMAEnums.MATypeEnum mAType, int period, int signalOffset, bool useSignalColors, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return indicator.SignalsMA(input, mAType, period, signalOffset, useSignalColors, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}
	}
}

#endregion

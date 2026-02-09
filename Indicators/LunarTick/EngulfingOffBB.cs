#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("[01] Parameters", 1)]
    [Gui.CategoryOrder("[02] Display", 2)]
    [Gui.CategoryOrder("[03] Alerts", 3)]
    [Gui.CategoryOrder("[04] Developer", 4)]
    public class EngulfingOffBB : Indicator
	{
        #region Constants

        public const string Version = "1.1.1";

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BB Length", Description = "Bollinger Bands length", Order = 1, GroupName = "[01] Parameters")]
        public int BBLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "BB StdDev Multiplier", Description = "Bollinger Bands Std Deviation Multiplier", Order = 2, GroupName = "[01] Parameters")]
        public double BBStdDevMultiplier
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Bullish Color", Order = 1, GroupName = "[02] Display")]
        public Brush BullishColor
        { get; set; }

        [Browsable(false)]
        public string BullishColorSerializable
        {
            get { return Serialize.BrushToString(BullishColor); }
            set { BullishColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bearish Color", Order = 2, GroupName = "[02] Display")]
        public Brush BearishColor
        { get; set; }

        [Browsable(false)]
        public string BearishColorSerializable
        {
            get { return Serialize.BrushToString(BearishColor); }
            set { BearishColor = Serialize.StringToBrush(value); }
        }

        [Display(Name = "Enable Alerts", Description = "Trigger alerts for detected engulfing candles off Bollinger Bands.", Order = 1, GroupName = "[03] Alerts")]
        public bool EnableAlerts
        { get; set; }

        [Display(Name = "Alert Sounds Path", Description = "Location of alert audio files.", Order = 2, GroupName = "[03] Alerts")]
        public string AlertSoundsPath
        { get; set; }

        [Display(Name = "Bullish Engulfing Off BB Alert", Description = "Alert sound used for detected bullish engulfing candle off lower Bollinger Band.", Order = 3, GroupName = "[03] Alerts")]
        public string BullishEngulfingOffBBAlert
        { get; set; }

        [Display(Name = "Bearish Engulfing Off BB Alert", Description = "Alert sound used for detected bearish engulfing candle off upper Bollinger Band.", Order = 4, GroupName = "[03] Alerts")]
        public string BearishEngulfingOffBBAlert
        { get; set; }

        [ReadOnly(true)]
        [XmlIgnore]
        [Display(Name = "Version", Description = "Version information.", Order = 1, GroupName = "[04] Developer")]
        public string VersionInformation
        { get; set; }

        [Display(Name = "Debug", Description = "Toggle debug logging.", Order = 2, GroupName = "[04] Developer")]
        public bool Debug
        { get; set; }

        #endregion

        #region Indicator methods

        protected override void OnStateChange()
		{
            DebugPrint($"OnStateChange({State})");

            if (State == State.SetDefaults)
			{
				Description									= @"Highlights engulfing candles off the Bollinger Bands.";
				Name										= "Engulfing Off BB";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				BBLength									= 20;
				BBStdDevMultiplier							= 2;
				BullishColor								= Brushes.Aqua;
				BearishColor								= Brushes.Fuchsia;
                EnableAlerts                                = false;
                AlertSoundsPath                             = DefaultAlertFilePath();
                BullishEngulfingOffBBAlert                  = "EngulfingBB.wav";
                BearishEngulfingOffBBAlert                  = "EngulfingBB.wav";
                VersionInformation                          = $"{Version} - {Assembly.GetAssembly(typeof(EngulfingOffBB)).GetName().Version}";
                Debug                                       = false;
            }
            else if (State == State.Configure)
			{
                // Disable IsSuspendedWhileInactive if alerts are enabled.
                IsSuspendedWhileInactive = !EnableAlerts;
            }
        }

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 2)
				return;

            if (IsFirstTickOfBar)
            {
                var bb = Bollinger(BBStdDevMultiplier, BBLength);

                int lastClosedBarsAgo = (Calculate == Calculate.OnBarClose) ? 0 : 1;

                bool bullishEngulfing = (Close[lastClosedBarsAgo] > Open[lastClosedBarsAgo]) && (Low[lastClosedBarsAgo] < bb.Lower[lastClosedBarsAgo]) && (Close[lastClosedBarsAgo] > bb.Lower[lastClosedBarsAgo]) && (Close[lastClosedBarsAgo + 1] < Open[lastClosedBarsAgo + 1]) && ((Close[lastClosedBarsAgo] - Open[lastClosedBarsAgo]) > (Open[lastClosedBarsAgo + 1] - Close[lastClosedBarsAgo + 1]));
                bool bearishEngulfing = (Close[lastClosedBarsAgo] < Open[lastClosedBarsAgo]) && (High[lastClosedBarsAgo] > bb.Upper[lastClosedBarsAgo]) && (Close[lastClosedBarsAgo] < bb.Upper[lastClosedBarsAgo]) && (Close[lastClosedBarsAgo + 1] > Open[lastClosedBarsAgo + 1]) && ((Open[lastClosedBarsAgo] - Close[lastClosedBarsAgo]) > (Close[lastClosedBarsAgo + 1] - Open[lastClosedBarsAgo + 1]));

                if (bullishEngulfing)
                {
					BarBrushes[lastClosedBarsAgo] = BullishColor;
                    DebugPrint($"Detected bullish engulfing candle off lower BB");

                    if (EnableAlerts && (State == State.Realtime) && !string.IsNullOrWhiteSpace(BullishEngulfingOffBBAlert))
                    {
                        string audioFile = ResolveAlertFilePath(BullishEngulfingOffBBAlert, AlertSoundsPath);
                        Alert("BullishEngulfingOffBBAlert", Priority.High, "Detected bullish engulfing candle off lower BB", audioFile, 10, Brushes.Black, BullishColor);
                    }
                }
                else if (bearishEngulfing)
                {
					BarBrushes[lastClosedBarsAgo] = BearishColor;
                    DebugPrint($"Detected bearish engulfing candle off upper BB");

                    if (EnableAlerts && (State == State.Realtime) && !string.IsNullOrWhiteSpace(BearishEngulfingOffBBAlert))
                    {
                        string audioFile = ResolveAlertFilePath(BearishEngulfingOffBBAlert, AlertSoundsPath);
                        Alert("BearishEngulfingOffBBAlert", Priority.High, "Detected bearish engulfing candle off upper BB", audioFile, 10, Brushes.Black, BearishColor);
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"EngulfingOffBB[{Instrument.FullName}]: {msg}");
                else
                    Print($"EngulfingOffBB: {msg}");
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
		private LunarTick.EngulfingOffBB[] cacheEngulfingOffBB;
		public LunarTick.EngulfingOffBB EngulfingOffBB(int bBLength, double bBStdDevMultiplier)
		{
			return EngulfingOffBB(Input, bBLength, bBStdDevMultiplier);
		}

		public LunarTick.EngulfingOffBB EngulfingOffBB(ISeries<double> input, int bBLength, double bBStdDevMultiplier)
		{
			if (cacheEngulfingOffBB != null)
				for (int idx = 0; idx < cacheEngulfingOffBB.Length; idx++)
					if (cacheEngulfingOffBB[idx] != null && cacheEngulfingOffBB[idx].BBLength == bBLength && cacheEngulfingOffBB[idx].BBStdDevMultiplier == bBStdDevMultiplier && cacheEngulfingOffBB[idx].EqualsInput(input))
						return cacheEngulfingOffBB[idx];
			return CacheIndicator<LunarTick.EngulfingOffBB>(new LunarTick.EngulfingOffBB(){ BBLength = bBLength, BBStdDevMultiplier = bBStdDevMultiplier }, input, ref cacheEngulfingOffBB);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.EngulfingOffBB EngulfingOffBB(int bBLength, double bBStdDevMultiplier)
		{
			return indicator.EngulfingOffBB(Input, bBLength, bBStdDevMultiplier);
		}

		public Indicators.LunarTick.EngulfingOffBB EngulfingOffBB(ISeries<double> input , int bBLength, double bBStdDevMultiplier)
		{
			return indicator.EngulfingOffBB(input, bBLength, bBStdDevMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.EngulfingOffBB EngulfingOffBB(int bBLength, double bBStdDevMultiplier)
		{
			return indicator.EngulfingOffBB(Input, bBLength, bBStdDevMultiplier);
		}

		public Indicators.LunarTick.EngulfingOffBB EngulfingOffBB(ISeries<double> input , int bBLength, double bBStdDevMultiplier)
		{
			return indicator.EngulfingOffBB(input, bBLength, bBStdDevMultiplier);
		}
	}
}

#endregion

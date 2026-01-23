#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("[01] Parameters", 1)]
    [Gui.CategoryOrder("[02] Display", 2)]
    [Gui.CategoryOrder("[03] Alerts", 3)]
    [Gui.CategoryOrder("[04] Developer", 4)]
    public class ArrowsOffBB : Indicator
	{
        #region Constants

        public const string Version = "1.1.0";

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BB Length", Description = "Bollinger Bands length", Order = 1, GroupName = "[01] Parameters")]
        public int BBLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "BB StdDev Multiplier", Description = "BollingerBands Std Deviation Multiplier", Order = 2, GroupName = "[01] Parameters")]
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

        [Display(Name = "Enable Alerts", Description = "Trigger alerts for detected entry signal off Bollinger Bands.", Order = 1, GroupName = "[03] Alerts")]
        public bool EnableAlerts
        { get; set; }

        [Display(Name = "Alert Sounds Path", Description = "Location of alert audio files.", Order = 2, GroupName = "[03] Alerts")]
        public string AlertSoundsPath
        { get; set; }

        [Display(Name = "Bullish Arrow Off BB Alert", Description = "Alert sound used for detected bullish entry signal off lower Bollinger Band.", Order = 3, GroupName = "[03] Alerts")]
        public string BullishArrowOffBBAlert
        { get; set; }

        [Display(Name = "Bearish Arrow Off BB Alert", Description = "Alert sound used for detected bearish entry signal off upper Bollinger Band.", Order = 4, GroupName = "[03] Alerts")]
        public string BearishArrowOffBBAlert
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
				Description									= @"Shows entry signals off the Bollinger Bands.";
				Name										= "Arrows Off BB";
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
                BullishArrowOffBBAlert                      = "BuySignal.wav";
                BearishArrowOffBBAlert                      = "SellSignal.wav";
                VersionInformation                          = $"{Version} - {Assembly.GetAssembly(typeof(ArrowsOffBB)).GetName().Version}";
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

				bool bullishEntry = (Close[1] > Open[1]) && (Close[2]< Open[2]) && (Low[1] < bb.Lower[1]) && (Low[2] < bb.Lower[2]);
				bool bearishEntry = (Close[1] < Open[1]) && (Close[2] > Open[2]) && (High[1] > bb.Upper[1]) && (High[2] > bb.Upper[2]);

				if (bullishEntry)
				{
					Draw.ArrowUp(this, $"LongEntry{CurrentBar}", false, 1, Low[1] - TickSize, BullishColor);
                    DebugPrint($"Detected bullish entry signal off lower BB");

                    if (EnableAlerts && (State == State.Realtime) && !string.IsNullOrWhiteSpace(BullishArrowOffBBAlert))
                    {
                        string audioFile = ResolveAlertFilePath(BullishArrowOffBBAlert, AlertSoundsPath);
                        Alert("BullishArrowOffBBAlert", Priority.High, "Detected bullish entry signal off lower BB", audioFile, 10, Brushes.Black, BullishColor);
                    }
                }
                else if (bearishEntry)
				{
                    Draw.ArrowDown(this, $"ShortEntry{CurrentBar}", false, 1, High[1] + TickSize, BearishColor);
                    DebugPrint($"Detected bearish entry signal off upper BB");

                    if (EnableAlerts && (State == State.Realtime) && !string.IsNullOrWhiteSpace(BearishArrowOffBBAlert))
                    {
                        string audioFile = ResolveAlertFilePath(BearishArrowOffBBAlert, AlertSoundsPath);
                        Alert("BearishArrowOffBBAlert", Priority.High, "Detected bearish entry signal off upper BB", audioFile, 10, Brushes.Black, BearishColor);
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
                    Print($"ArrowsOffBB[{Instrument.FullName}]: {msg}");
                else
                    Print($"ArrowsOffBB: {msg}");
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
		private LunarTick.ArrowsOffBB[] cacheArrowsOffBB;
		public LunarTick.ArrowsOffBB ArrowsOffBB(int bBLength, double bBStdDevMultiplier)
		{
			return ArrowsOffBB(Input, bBLength, bBStdDevMultiplier);
		}

		public LunarTick.ArrowsOffBB ArrowsOffBB(ISeries<double> input, int bBLength, double bBStdDevMultiplier)
		{
			if (cacheArrowsOffBB != null)
				for (int idx = 0; idx < cacheArrowsOffBB.Length; idx++)
					if (cacheArrowsOffBB[idx] != null && cacheArrowsOffBB[idx].BBLength == bBLength && cacheArrowsOffBB[idx].BBStdDevMultiplier == bBStdDevMultiplier && cacheArrowsOffBB[idx].EqualsInput(input))
						return cacheArrowsOffBB[idx];
			return CacheIndicator<LunarTick.ArrowsOffBB>(new LunarTick.ArrowsOffBB(){ BBLength = bBLength, BBStdDevMultiplier = bBStdDevMultiplier }, input, ref cacheArrowsOffBB);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.ArrowsOffBB ArrowsOffBB(int bBLength, double bBStdDevMultiplier)
		{
			return indicator.ArrowsOffBB(Input, bBLength, bBStdDevMultiplier);
		}

		public Indicators.LunarTick.ArrowsOffBB ArrowsOffBB(ISeries<double> input , int bBLength, double bBStdDevMultiplier)
		{
			return indicator.ArrowsOffBB(input, bBLength, bBStdDevMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.ArrowsOffBB ArrowsOffBB(int bBLength, double bBStdDevMultiplier)
		{
			return indicator.ArrowsOffBB(Input, bBLength, bBStdDevMultiplier);
		}

		public Indicators.LunarTick.ArrowsOffBB ArrowsOffBB(ISeries<double> input , int bBLength, double bBStdDevMultiplier)
		{
			return indicator.ArrowsOffBB(input, bBLength, bBStdDevMultiplier);
		}
	}
}

#endregion

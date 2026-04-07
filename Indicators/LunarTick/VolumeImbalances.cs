#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Reflection;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("[01] Parameters", 1)]
    [Gui.CategoryOrder("[02] Display", 2)]
    [Gui.CategoryOrder("[03] Alerts", 3)]
    [Gui.CategoryOrder("[04] Developer", 4)]
    public class VolumeImbalances : Indicator
	{
		private enum Direction
		{
			Bullish,
			Bearish
		}

		private class VolumeImbalance
		{
			private string? _lineTag;
            private string? _dotTag;
            
			public int StartBarIndex { get; }
			public Direction Direction { get; }
			public double High { get; }
            public double Low { get; }
            public bool Filled { get; set; }
            public int FilledBarIndex { get; set; }
            public bool Active { get; set; }
			public string? DotTag { get; set; }
            public string? LineTag { get; set; }

            public VolumeImbalance(int startBarIndex, Direction direction, double high, double low)
			{
				StartBarIndex = startBarIndex;
				Direction = direction;
				High = high;
				Low = low;
				Filled = false;
				FilledBarIndex = -1;
				Active = false;
			}
        }

        #region Constants

        public const string Version = "1.2.0";
        private const int MinTicks = 1;

        #endregion

        #region Members

        List<VolumeImbalance> _unfilled = new();
        List<VolumeImbalance> _filled = new();
        double _prevHigh = Double.MinValue;
        double _prevLow = Double.MaxValue;

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Show Bullish", Order = 1, GroupName = "[01] Parameters")]
        public bool ShowBullish
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Bearish", Order = 2, GroupName = "[01] Parameters")]
        public bool ShowBearish
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

        [Display(Name = "Line Width", Order = 3, GroupName = "[02] Display")]
        public int LineWidth
        { get; set; }

        [Display(Name = "Show Dots", Order = 4, GroupName = "[02] Display")]
        public bool ShowDots
        { get; set; }

        [Display(Name = "Dot Offset", Description = "Vertical offset (in ticks) between candle and dot.", Order = 5, GroupName = "[02] Display")]
        public int DotOffset
        { get; set; }

        [Display(Name = "Enable Alerts", Description = "Trigger alerts for detected volume imbalances.", Order = 1, GroupName = "[03] Alerts")]
        public bool EnableAlerts
        { get; set; }

        [Display(Name = "Alert Sounds Path", Description = "Location of alert audio files.", Order = 2, GroupName = "[03] Alerts")]
        public string AlertSoundsPath
        { get; set; }

        [Display(Name = "Bullish Volume Imbalance Alert", Description = "Alert sound used for detected bullish volume imbalance.", Order = 3, GroupName = "[03] Alerts")]
        public string BullishVolumeImbalanceAlert
        { get; set; }

        [Display(Name = "Bearish Volume Imbalance Alert", Description = "Alert sound used for detected bearish volume imbalance.", Order = 4, GroupName = "[03] Alerts")]
        public string BearishVolumeImbalanceAlert
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
				Description									= @"Highlights volume imbalances, by looking for a gap between a candle's close and the following candle's open.";
				Name										= "Volume Imbalances";
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
                ShowBullish                                 = true;
                ShowBearish                                 = true;
                BullishColor                                = Brushes.Aqua;
                BearishColor								= Brushes.Fuchsia;
				LineWidth									= 6;
                ShowDots                                    = true;
                DotOffset                                   = 1;
                EnableAlerts                                = false;
                AlertSoundsPath                             = DefaultAlertFilePath();
                BullishVolumeImbalanceAlert                 = "VolumeImbalance.wav";
                BearishVolumeImbalanceAlert                 = "VolumeImbalance.wav";
                VersionInformation                          = $"{Version} - {Assembly.GetAssembly(typeof(VolumeImbalances)).GetName().Version}";
                Debug										= false;
            }
            else if (State == State.Configure)
			{
                // Disable IsSuspendedWhileInactive if alerts are enabled.
                IsSuspendedWhileInactive = !EnableAlerts;

                RemoveDrawObjects();
				_unfilled.Clear();
				_filled.Clear();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;

            if (IsFirstTickOfBar)
            {
                _prevHigh = Double.MinValue;
                _prevLow = Double.MaxValue;
            }
            bool checkHigh = High[0] > _prevHigh;
            bool checkLow = Low[0] < _prevLow;

            if (IsFirstTickOfBar || checkHigh || checkLow)
			{
				DetectFilledVolumeImbalances(checkHigh, checkLow);
			}
        
			DetectUnfilledVolumeImbalances();

            _prevHigh = High[0];
            _prevLow = Low[0];
        }

        #endregion

        #region Private methods

        private void DetectUnfilledVolumeImbalances()
		{
			VolumeImbalance? volumeImbalance = null;
			bool active = false;
			var imbalanceTicks = (Open[0] - Close[1]) / Instrument.MasterInstrument.TickSize;
			var absImbalanceTicks = Math.Abs(imbalanceTicks);
			var currOpen = Open[0];
			var prevClose = Close[1];

            // Check to see if a VolumeImbalance has already been created for this bar.
            if (_unfilled != null && _unfilled.Count > 0 && _unfilled.Last().StartBarIndex == CurrentBar)
				volumeImbalance = _unfilled.Last();

			if (ShowBullish && IsBullish(0) && IsBullish(1) && (currOpen > prevClose) && (absImbalanceTicks >= MinTicks))
			{
				// Bullish imbalance.
				active = true;

				if (volumeImbalance == null)
				{
                    DebugPrint($"Detected bullish VolumeImbalance at {Time[0].ToString("O")}");
                    volumeImbalance = new VolumeImbalance(CurrentBar, Direction.Bullish, currOpen, prevClose);
					_unfilled.Add(volumeImbalance);

                    if (EnableAlerts && (State == State.Realtime) && !string.IsNullOrWhiteSpace(BullishVolumeImbalanceAlert))
                    {
                        string audioFile = ResolveAlertFilePath(BullishVolumeImbalanceAlert, AlertSoundsPath);
                        Alert("BullishVolumeImbalanceAlert", Priority.High, string.Format("Detected bullish volume imbalance {0:0.##}", currOpen), audioFile, 10, Brushes.Black, BullishColor);
                    }
                }
			}
            else if (ShowBearish && IsBearish(0) && IsBearish(1) && (currOpen < prevClose) && (absImbalanceTicks >= MinTicks))
            {
                // Bearish imbalance.
                active = true;

                if (volumeImbalance == null)
                {
                    DebugPrint($"Detected bearish VolumeImbalance at {Time[0].ToString("O")}");
                    volumeImbalance = new VolumeImbalance(CurrentBar, Direction.Bearish, prevClose, currOpen);
                    _unfilled.Add(volumeImbalance);

                    if (EnableAlerts && (State == State.Realtime) && !string.IsNullOrWhiteSpace(BearishVolumeImbalanceAlert))
                    {
                        string audioFile = ResolveAlertFilePath(BearishVolumeImbalanceAlert, AlertSoundsPath);
                        Alert("BearishVolumeImbalanceAlert", Priority.High, string.Format("Detected bearish volume imbalance {0:0.##}", currOpen), audioFile, 10, Brushes.Black, BearishColor);
                    }
                }
            }

			if (volumeImbalance != null)
			{
				volumeImbalance.Active = active;
				UpdateDrawObjects(volumeImbalance);
			}
		}
    
		private void DetectFilledVolumeImbalances(bool checkHigh, bool checkLow)
        {
			foreach(VolumeImbalance volumeImbalance in _unfilled)
			{
				if (CurrentBar <= volumeImbalance.StartBarIndex || volumeImbalance.Filled || !volumeImbalance.Active)
					continue;

				UpdateDrawObjects(volumeImbalance);

				if ((checkHigh && (High[0] >= volumeImbalance.Low) && (Low[0] < volumeImbalance.Low)) ||
					(checkLow && (Low[0] <= volumeImbalance.High) && (High[0] > volumeImbalance.High)))
				{
					// Volume imbalance filled
					volumeImbalance.Filled = true;
					volumeImbalance.FilledBarIndex = CurrentBar;
					_filled.Add(volumeImbalance);
					DebugPrint($"VolumeImbalance filled at {Time[0].ToString("O")}");
					// TODO: Alert
				}
			}

			_unfilled.RemoveAll(vi => vi.Filled);
        }

		private bool IsBullish(int barsAgo)
		{
			return (Close[barsAgo] > Open[barsAgo]);
		}

        private bool IsBearish(int barsAgo)
        {
            return (Close[barsAgo] < Open[barsAgo]);
        }

        private void UpdateDrawObjects(VolumeImbalance volumeImbalance)
        {
            if (volumeImbalance.Active)
            {
				int barsAgo = CurrentBar - volumeImbalance.StartBarIndex;

                if (ShowDots)
                {
                    volumeImbalance.DotTag = $"VolumeImbalanceDot{volumeImbalance.StartBarIndex}";
                    Draw.Dot(this, volumeImbalance.DotTag, false, barsAgo, volumeImbalance.Direction == Direction.Bullish ? (Low[barsAgo] - (DotOffset * TickSize)) : (High[barsAgo] + (DotOffset * TickSize)), volumeImbalance.Direction == Direction.Bullish ? BullishColor : BearishColor);
                }

                volumeImbalance.LineTag = $"VolumeImbalanceLine{volumeImbalance.StartBarIndex}";
				var lineY = volumeImbalance.Direction == Direction.Bullish ? volumeImbalance.High : volumeImbalance.Low;
                Draw.Line(this, volumeImbalance.LineTag, false, barsAgo, lineY, 0, lineY, volumeImbalance.Direction == Direction.Bullish ? BullishColor : BearishColor, DashStyleHelper.Solid, LineWidth);
            }
            else
            {
                if (!string.IsNullOrEmpty(volumeImbalance.LineTag))
                {
                    RemoveDrawObject(volumeImbalance.LineTag);
                    volumeImbalance.LineTag = null;
                }
                if (!string.IsNullOrEmpty(volumeImbalance.DotTag))
                {
                    RemoveDrawObject(volumeImbalance.DotTag);
                    volumeImbalance.DotTag = null;
                }
            }
        }

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"VolumeImbalances[{Instrument.FullName}]: {msg}");
                else
                    Print($"VolumeImbalances: {msg}");
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
		private LunarTick.VolumeImbalances[] cacheVolumeImbalances;
		public LunarTick.VolumeImbalances VolumeImbalances(bool showBullish, bool showBearish)
		{
			return VolumeImbalances(Input, showBullish, showBearish);
		}

		public LunarTick.VolumeImbalances VolumeImbalances(ISeries<double> input, bool showBullish, bool showBearish)
		{
			if (cacheVolumeImbalances != null)
				for (int idx = 0; idx < cacheVolumeImbalances.Length; idx++)
					if (cacheVolumeImbalances[idx] != null && cacheVolumeImbalances[idx].ShowBullish == showBullish && cacheVolumeImbalances[idx].ShowBearish == showBearish && cacheVolumeImbalances[idx].EqualsInput(input))
						return cacheVolumeImbalances[idx];
			return CacheIndicator<LunarTick.VolumeImbalances>(new LunarTick.VolumeImbalances(){ ShowBullish = showBullish, ShowBearish = showBearish }, input, ref cacheVolumeImbalances);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.VolumeImbalances VolumeImbalances(bool showBullish, bool showBearish)
		{
			return indicator.VolumeImbalances(Input, showBullish, showBearish);
		}

		public Indicators.LunarTick.VolumeImbalances VolumeImbalances(ISeries<double> input , bool showBullish, bool showBearish)
		{
			return indicator.VolumeImbalances(input, showBullish, showBearish);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.VolumeImbalances VolumeImbalances(bool showBullish, bool showBearish)
		{
			return indicator.VolumeImbalances(Input, showBullish, showBearish);
		}

		public Indicators.LunarTick.VolumeImbalances VolumeImbalances(ISeries<double> input , bool showBullish, bool showBearish)
		{
			return indicator.VolumeImbalances(input, showBullish, showBearish);
		}
	}
}

#endregion

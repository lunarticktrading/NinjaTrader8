#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Display", 2)]
    public class MACloudDots : Indicator
	{
        #region Members
        private MACloud _maCloud;
        #endregion

        #region Indicator methods
        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicator showing the MA Cloud trend.";
				Name										= "MACloudDots";
				Calculate									= Calculate.OnPriceChange;
                IsOverlay									= false;
                DisplayInDataBox							= false;
                DrawOnPricePanel							= false;
                DrawHorizontalGridLines						= true;
                DrawVerticalGridLines						= true;
                PaintPriceMarkers							= false;
                ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive                    = true;
                MAType                                      = NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum.EMA;
                FastPeriod                                  = 9;
                SlowPeriod                                  = 21;
                DisplayLevel                                = 50;
                BullishTrendBrush                           = Brushes.Green;
                BearishTrendBrush                           = Brushes.Red;
                ShowLabel                                   = true;
                AddPlot(new Stroke(Brushes.Transparent, 6), PlotStyle.Dot, "Dots");
            }
            else if (State == State.Configure)
			{
                _maCloud = MACloud(MAType, FastPeriod, SlowPeriod, Brushes.Transparent, Brushes.Transparent, 0, false, 0, string.Empty, false, string.Empty, string.Empty, string.Empty);
			}
		}

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                return Name + "(" + MAType + "," + FastPeriod + "," + SlowPeriod + ")";
            }
        }

        protected override void OnBarUpdate()
		{
            if (CurrentBar < Math.Max(FastPeriod, SlowPeriod))
                return;

            _maCloud.Update();

            var maFast = _maCloud.FastMA[0];
            var maSlow = _maCloud.SlowMA[0];

            Dots[0] = DisplayLevel;
            if (maFast > maSlow)
            {
                PlotBrushes[0][0] = BullishTrendBrush;
            }
            else if (maFast < maSlow)
            {
                PlotBrushes[0][0] = BearishTrendBrush;
            }
            else
            {
                PlotBrushes[0][0] = Brushes.Transparent;
            }

            if (ShowLabel)
            {
                Draw.Text(this, Name + "Label", false, MAType + " Cloud", -1, DisplayLevel, 0, Brushes.LightGray, ChartControl.Properties.LabelFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "MA Type", Description = "The type of Moving Average.", Order = 1, GroupName = "Parameters")]
        public NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum MAType
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "FastPeriod", Description = "The period of the fast Moving Average.", Order = 2, GroupName = "Parameters")]
        public int FastPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SlowPeriod", Description = "The period of the slow Moving Average.", Order = 3, GroupName = "Parameters")]
        public int SlowPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "DisplayLevel", Description = "Value level at which the row of dots will be displayed.", Order = 1, GroupName = "Display")]
        public int DisplayLevel
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "BullishTrendBrush", Description = "Dot color used to indicate a bullish trend.", Order = 2, GroupName = "Display")]
        public Brush BullishTrendBrush
        { get; set; }

        [Browsable(false)]
        public string BullishTrendBrushSerializable
        {
            get { return Serialize.BrushToString(BullishTrendBrush); }
            set { BullishTrendBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "BearishTrendBrush", Description = "Dot color used to indicate a bearish trend.", Order = 3, GroupName = "Display")]
        public Brush BearishTrendBrush
        { get; set; }

        [Browsable(false)]
        public string BearishTrendBrushSerializable
        {
            get { return Serialize.BrushToString(BearishTrendBrush); }
            set { BearishTrendBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "ShowLabel", Description = "Display label next to row of dots.", Order = 4, GroupName = "Display")]
        public bool ShowLabel
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Dots
        {
            get { return Values[0]; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LunarTick.MACloudDots[] cacheMACloudDots;
		public LunarTick.MACloudDots MACloudDots(NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, int displayLevel, Brush bullishTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return MACloudDots(Input, mAType, fastPeriod, slowPeriod, displayLevel, bullishTrendBrush, bearishTrendBrush, showLabel);
		}

		public LunarTick.MACloudDots MACloudDots(ISeries<double> input, NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, int displayLevel, Brush bullishTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			if (cacheMACloudDots != null)
				for (int idx = 0; idx < cacheMACloudDots.Length; idx++)
					if (cacheMACloudDots[idx] != null && cacheMACloudDots[idx].MAType == mAType && cacheMACloudDots[idx].FastPeriod == fastPeriod && cacheMACloudDots[idx].SlowPeriod == slowPeriod && cacheMACloudDots[idx].DisplayLevel == displayLevel && cacheMACloudDots[idx].BullishTrendBrush == bullishTrendBrush && cacheMACloudDots[idx].BearishTrendBrush == bearishTrendBrush && cacheMACloudDots[idx].ShowLabel == showLabel && cacheMACloudDots[idx].EqualsInput(input))
						return cacheMACloudDots[idx];
			return CacheIndicator<LunarTick.MACloudDots>(new LunarTick.MACloudDots(){ MAType = mAType, FastPeriod = fastPeriod, SlowPeriod = slowPeriod, DisplayLevel = displayLevel, BullishTrendBrush = bullishTrendBrush, BearishTrendBrush = bearishTrendBrush, ShowLabel = showLabel }, input, ref cacheMACloudDots);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.MACloudDots MACloudDots(NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, int displayLevel, Brush bullishTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return indicator.MACloudDots(Input, mAType, fastPeriod, slowPeriod, displayLevel, bullishTrendBrush, bearishTrendBrush, showLabel);
		}

		public Indicators.LunarTick.MACloudDots MACloudDots(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, int displayLevel, Brush bullishTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return indicator.MACloudDots(input, mAType, fastPeriod, slowPeriod, displayLevel, bullishTrendBrush, bearishTrendBrush, showLabel);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.MACloudDots MACloudDots(NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, int displayLevel, Brush bullishTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return indicator.MACloudDots(Input, mAType, fastPeriod, slowPeriod, displayLevel, bullishTrendBrush, bearishTrendBrush, showLabel);
		}

		public Indicators.LunarTick.MACloudDots MACloudDots(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, int displayLevel, Brush bullishTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return indicator.MACloudDots(input, mAType, fastPeriod, slowPeriod, displayLevel, bullishTrendBrush, bearishTrendBrush, showLabel);
		}
	}
}

#endregion

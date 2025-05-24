#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.gambcl
{
    public class HeikenAshiDots : Indicator
    {
        #region Members
        private HeikenAshi8 _ha;
        #endregion

        #region Indicator methods
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description									= @"Indicator showing changes in Heiken Ashi trend.";
                Name										= "HeikenAshiDots";
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
                IsSuspendedWhileInactive					= true;
                DisplayLevel								= 50;
                BullishTrendBrush							= Brushes.Green;
                ChangingTrendBrush							= Brushes.Yellow;
                BearishTrendBrush							= Brushes.Red;
                ShowLabel                                   = true;
                AddPlot(new Stroke(Brushes.White, 6), PlotStyle.Dot, "Dots");
            }
            else if (State == State.Configure)
            {
                _ha = HeikenAshi8(Brushes.Transparent, Brushes.Transparent, Brushes.Transparent);
            }
        }

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                return Name;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1)
                return;

            Dots[0] = DisplayLevel;

            _ha.Update();
            var currHAOpen = _ha.HAOpen[0];
            var currHAClose = _ha.HAClose[0];
            var prevHAOpen = _ha.HAOpen[1];
            var prevHAClose = _ha.HAClose[1];

            int currHATrend = (currHAClose >= currHAOpen) ? 1 : -1;
            int prevHATrend = (prevHAClose >= prevHAOpen) ? 1 : -1;

            if (currHATrend == prevHATrend && currHATrend > 0)
            {
                PlotBrushes[0][0] = BullishTrendBrush;
            }
            else if (currHATrend == prevHATrend && currHATrend < 0)
            {
                PlotBrushes[0][0] = BearishTrendBrush;
            }
            else
            {
                PlotBrushes[0][0] = ChangingTrendBrush;
            }

            if (ShowLabel)
            {
                Draw.Text(this, Name + "Label", false, "Heiken Ashi", -1, DisplayLevel, 0, Brushes.LightGray, ChartControl.Properties.LabelFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name="DisplayLevel", Description="Value level at which the row of dots will be displayed.", Order=1, GroupName= "Display")]
        public int DisplayLevel
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="BullishTrendBrush", Description="Dot color used to indicate a bullish trend.", Order=2, GroupName= "Display")]
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
        [Display(Name="ChangingTrendBrush", Description="Dot color used to indicate a changing trend.", Order=3, GroupName= "Display")]
        public Brush ChangingTrendBrush
        { get; set; }

        [Browsable(false)]
        public string ChangingTrendBrushSerializable
        {
            get { return Serialize.BrushToString(ChangingTrendBrush); }
            set { ChangingTrendBrush = Serialize.StringToBrush(value); }
        }			

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="BearishTrendBrush", Description="Dot color used to indicate a bearish trend.", Order=4, GroupName= "Display")]
        public Brush BearishTrendBrush
        { get; set; }

        [Browsable(false)]
        public string BearishTrendBrushSerializable
        {
            get { return Serialize.BrushToString(BearishTrendBrush); }
            set { BearishTrendBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "ShowLabel", Description = "Display label next to row of dots.", Order = 5, GroupName = "Display")]
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
		private gambcl.HeikenAshiDots[] cacheHeikenAshiDots;
		public gambcl.HeikenAshiDots HeikenAshiDots(int displayLevel, Brush bullishTrendBrush, Brush changingTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return HeikenAshiDots(Input, displayLevel, bullishTrendBrush, changingTrendBrush, bearishTrendBrush, showLabel);
		}

		public gambcl.HeikenAshiDots HeikenAshiDots(ISeries<double> input, int displayLevel, Brush bullishTrendBrush, Brush changingTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			if (cacheHeikenAshiDots != null)
				for (int idx = 0; idx < cacheHeikenAshiDots.Length; idx++)
					if (cacheHeikenAshiDots[idx] != null && cacheHeikenAshiDots[idx].DisplayLevel == displayLevel && cacheHeikenAshiDots[idx].BullishTrendBrush == bullishTrendBrush && cacheHeikenAshiDots[idx].ChangingTrendBrush == changingTrendBrush && cacheHeikenAshiDots[idx].BearishTrendBrush == bearishTrendBrush && cacheHeikenAshiDots[idx].ShowLabel == showLabel && cacheHeikenAshiDots[idx].EqualsInput(input))
						return cacheHeikenAshiDots[idx];
			return CacheIndicator<gambcl.HeikenAshiDots>(new gambcl.HeikenAshiDots(){ DisplayLevel = displayLevel, BullishTrendBrush = bullishTrendBrush, ChangingTrendBrush = changingTrendBrush, BearishTrendBrush = bearishTrendBrush, ShowLabel = showLabel }, input, ref cacheHeikenAshiDots);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.gambcl.HeikenAshiDots HeikenAshiDots(int displayLevel, Brush bullishTrendBrush, Brush changingTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return indicator.HeikenAshiDots(Input, displayLevel, bullishTrendBrush, changingTrendBrush, bearishTrendBrush, showLabel);
		}

		public Indicators.gambcl.HeikenAshiDots HeikenAshiDots(ISeries<double> input , int displayLevel, Brush bullishTrendBrush, Brush changingTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return indicator.HeikenAshiDots(input, displayLevel, bullishTrendBrush, changingTrendBrush, bearishTrendBrush, showLabel);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.gambcl.HeikenAshiDots HeikenAshiDots(int displayLevel, Brush bullishTrendBrush, Brush changingTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return indicator.HeikenAshiDots(Input, displayLevel, bullishTrendBrush, changingTrendBrush, bearishTrendBrush, showLabel);
		}

		public Indicators.gambcl.HeikenAshiDots HeikenAshiDots(ISeries<double> input , int displayLevel, Brush bullishTrendBrush, Brush changingTrendBrush, Brush bearishTrendBrush, bool showLabel)
		{
			return indicator.HeikenAshiDots(input, displayLevel, bullishTrendBrush, changingTrendBrush, bearishTrendBrush, showLabel);
		}
	}
}

#endregion

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
    // This indicator makes use of Volume Delta by Gill
    // https://ninjatraderecosystem.com/user-app-share-download/delta-volume-update/
    public class VolumeDeltaDots : Indicator
	{
        #region Members
        private VolumeDelta _volumeDelta;
        #endregion

        #region Indicator methods
        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicator showing the Volume Delta direction.";
				Name										= "VolumeDeltaDots";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                MaximumBarsLookBack                         = MaximumBarsLookBack.Infinite;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                DisplayLevel								= 50;
                BullishVolumeDeltaBrush						= Brushes.Green;
                EqualVolumeDeltaBrush                       = Brushes.Yellow;
                BearishVolumeDeltaBrush						= Brushes.Red;
                ShowLabel									= true;
                AddPlot(new Stroke(Brushes.White, 6), PlotStyle.Dot, "Dots");
            }
            else if (State == State.Configure)
			{
                _volumeDelta = VolumeDelta(Brushes.Transparent, Brushes.Transparent, Brushes.Transparent, 1, false, 0, false);
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
            _volumeDelta.Update();

            Dots[0] = DisplayLevel;
            if (_volumeDelta.DeltaClose[0] > 0)
                PlotBrushes[0][0] = BullishVolumeDeltaBrush;
            else if (_volumeDelta.DeltaClose[0] < 0)
                PlotBrushes[0][0] = BearishVolumeDeltaBrush;
            else
                PlotBrushes[0][0] = EqualVolumeDeltaBrush;

            if (ShowLabel)
            {
                Draw.Text(this, Name + "Label", false, "Volume Delta", -1, DisplayLevel, 0, Brushes.LightGray, ChartControl.Properties.LabelFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "DisplayLevel", Description = "Value level at which the row of dots will be displayed.", Order = 1, GroupName = "Display")]
        public int DisplayLevel
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "BullishVolumeDeltaBrush", Description = "Dot color used to indicate a bullish volume delta.", Order = 2, GroupName = "Display")]
        public Brush BullishVolumeDeltaBrush
        { get; set; }

        [Browsable(false)]
        public string BullishVolumeDeltaBrushSerializable
        {
            get { return Serialize.BrushToString(BullishVolumeDeltaBrush); }
            set { BullishVolumeDeltaBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "EqualVolumeDeltaBrush", Description = "Dot color used to indicate an equal volume delta.", Order = 3, GroupName = "Display")]
        public Brush EqualVolumeDeltaBrush
        { get; set; }

        [Browsable(false)]
        public string EqualVolumeDeltaBrushSerializable
        {
            get { return Serialize.BrushToString(EqualVolumeDeltaBrush); }
            set { EqualVolumeDeltaBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "BearishVolumeDeltaBrush", Description = "Dot color used to indicate a bearish volume delta.", Order = 4, GroupName = "Display")]
        public Brush BearishVolumeDeltaBrush
        { get; set; }

        [Browsable(false)]
        public string BearishVolumeDeltaBrushSerializable
        {
            get { return Serialize.BrushToString(BearishVolumeDeltaBrush); }
            set { BearishVolumeDeltaBrush = Serialize.StringToBrush(value); }
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
		private gambcl.VolumeDeltaDots[] cacheVolumeDeltaDots;
		public gambcl.VolumeDeltaDots VolumeDeltaDots(int displayLevel, Brush bullishVolumeDeltaBrush, Brush equalVolumeDeltaBrush, Brush bearishVolumeDeltaBrush, bool showLabel)
		{
			return VolumeDeltaDots(Input, displayLevel, bullishVolumeDeltaBrush, equalVolumeDeltaBrush, bearishVolumeDeltaBrush, showLabel);
		}

		public gambcl.VolumeDeltaDots VolumeDeltaDots(ISeries<double> input, int displayLevel, Brush bullishVolumeDeltaBrush, Brush equalVolumeDeltaBrush, Brush bearishVolumeDeltaBrush, bool showLabel)
		{
			if (cacheVolumeDeltaDots != null)
				for (int idx = 0; idx < cacheVolumeDeltaDots.Length; idx++)
					if (cacheVolumeDeltaDots[idx] != null && cacheVolumeDeltaDots[idx].DisplayLevel == displayLevel && cacheVolumeDeltaDots[idx].BullishVolumeDeltaBrush == bullishVolumeDeltaBrush && cacheVolumeDeltaDots[idx].EqualVolumeDeltaBrush == equalVolumeDeltaBrush && cacheVolumeDeltaDots[idx].BearishVolumeDeltaBrush == bearishVolumeDeltaBrush && cacheVolumeDeltaDots[idx].ShowLabel == showLabel && cacheVolumeDeltaDots[idx].EqualsInput(input))
						return cacheVolumeDeltaDots[idx];
			return CacheIndicator<gambcl.VolumeDeltaDots>(new gambcl.VolumeDeltaDots(){ DisplayLevel = displayLevel, BullishVolumeDeltaBrush = bullishVolumeDeltaBrush, EqualVolumeDeltaBrush = equalVolumeDeltaBrush, BearishVolumeDeltaBrush = bearishVolumeDeltaBrush, ShowLabel = showLabel }, input, ref cacheVolumeDeltaDots);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.gambcl.VolumeDeltaDots VolumeDeltaDots(int displayLevel, Brush bullishVolumeDeltaBrush, Brush equalVolumeDeltaBrush, Brush bearishVolumeDeltaBrush, bool showLabel)
		{
			return indicator.VolumeDeltaDots(Input, displayLevel, bullishVolumeDeltaBrush, equalVolumeDeltaBrush, bearishVolumeDeltaBrush, showLabel);
		}

		public Indicators.gambcl.VolumeDeltaDots VolumeDeltaDots(ISeries<double> input , int displayLevel, Brush bullishVolumeDeltaBrush, Brush equalVolumeDeltaBrush, Brush bearishVolumeDeltaBrush, bool showLabel)
		{
			return indicator.VolumeDeltaDots(input, displayLevel, bullishVolumeDeltaBrush, equalVolumeDeltaBrush, bearishVolumeDeltaBrush, showLabel);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.gambcl.VolumeDeltaDots VolumeDeltaDots(int displayLevel, Brush bullishVolumeDeltaBrush, Brush equalVolumeDeltaBrush, Brush bearishVolumeDeltaBrush, bool showLabel)
		{
			return indicator.VolumeDeltaDots(Input, displayLevel, bullishVolumeDeltaBrush, equalVolumeDeltaBrush, bearishVolumeDeltaBrush, showLabel);
		}

		public Indicators.gambcl.VolumeDeltaDots VolumeDeltaDots(ISeries<double> input , int displayLevel, Brush bullishVolumeDeltaBrush, Brush equalVolumeDeltaBrush, Brush bearishVolumeDeltaBrush, bool showLabel)
		{
			return indicator.VolumeDeltaDots(input, displayLevel, bullishVolumeDeltaBrush, equalVolumeDeltaBrush, bearishVolumeDeltaBrush, showLabel);
		}
	}
}

#endregion

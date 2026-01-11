#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    public class EngulfingOffBB : Indicator
	{
        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BB Length", Description = "Bollinger Bands length", Order = 1, GroupName = "Parameters")]
        public int BBLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "BB StdDev Multiplier", Description = "BollingerBands Std Deviation Multiplier", Order = 2, GroupName = "Parameters")]
        public double BBStdDevMultiplier
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish Color", Order = 3, GroupName = "Parameters")]
        public Brush BullishColor
        { get; set; }

        [Browsable(false)]
        public string BullishColorSerializable
        {
            get { return Serialize.BrushToString(BullishColor); }
            set { BullishColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish Color", Order = 4, GroupName = "Parameters")]
        public Brush BearishColor
        { get; set; }

        [Browsable(false)]
        public string BearishColorSerializable
        {
            get { return Serialize.BrushToString(BearishColor); }
            set { BearishColor = Serialize.StringToBrush(value); }
        }

        #endregion

        #region Indicator methods

        protected override void OnStateChange()
		{
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
				IsSuspendedWhileInactive					= true;
				BBLength									= 20;
				BBStdDevMultiplier							= 2;
				BullishColor								= Brushes.Aqua;
				BearishColor								= Brushes.Fuchsia;
			}
			else if (State == State.Configure)
			{
			}			
        }

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 2)
				return;

            if (IsFirstTickOfBar)
            {
                var bb = Bollinger(BBStdDevMultiplier, BBLength);

                bool bullishEngulfing = (Close[1] > Open[1]) && (Low[1] < bb.Lower[1]) && (Close[1] > bb.Lower[1]) && (Close[2] < Open[2]) && ((Close[1] - Open[1]) > (Open[2] - Close[2]));
                bool bearishEngulfing = (Close[1] < Open[1]) && (High[1] > bb.Upper[1]) && (Close[1] < bb.Upper[1]) && (Close[2] > Open[2]) && ((Open[1] - Close[1]) > (Close[2] - Open[2]));

                if (bullishEngulfing)
                {
					BarBrushes[1] = BullishColor;
                }
                else if (bearishEngulfing)
                {
					BarBrushes[1] = BearishColor;
                }
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
		public LunarTick.EngulfingOffBB EngulfingOffBB(int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return EngulfingOffBB(Input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}

		public LunarTick.EngulfingOffBB EngulfingOffBB(ISeries<double> input, int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			if (cacheEngulfingOffBB != null)
				for (int idx = 0; idx < cacheEngulfingOffBB.Length; idx++)
					if (cacheEngulfingOffBB[idx] != null && cacheEngulfingOffBB[idx].BBLength == bBLength && cacheEngulfingOffBB[idx].BBStdDevMultiplier == bBStdDevMultiplier && cacheEngulfingOffBB[idx].BullishColor == bullishColor && cacheEngulfingOffBB[idx].BearishColor == bearishColor && cacheEngulfingOffBB[idx].EqualsInput(input))
						return cacheEngulfingOffBB[idx];
			return CacheIndicator<LunarTick.EngulfingOffBB>(new LunarTick.EngulfingOffBB(){ BBLength = bBLength, BBStdDevMultiplier = bBStdDevMultiplier, BullishColor = bullishColor, BearishColor = bearishColor }, input, ref cacheEngulfingOffBB);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.EngulfingOffBB EngulfingOffBB(int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return indicator.EngulfingOffBB(Input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}

		public Indicators.LunarTick.EngulfingOffBB EngulfingOffBB(ISeries<double> input , int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return indicator.EngulfingOffBB(input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.EngulfingOffBB EngulfingOffBB(int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return indicator.EngulfingOffBB(Input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}

		public Indicators.LunarTick.EngulfingOffBB EngulfingOffBB(ISeries<double> input , int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return indicator.EngulfingOffBB(input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}
	}
}

#endregion

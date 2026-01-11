#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    public class ArrowsOffBB : Indicator
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

				bool bullishEntry = (Close[1] > Open[1]) && (Close[2]< Open[2]) && (Low[1] < bb.Lower[1]) && (Low[2] < bb.Lower[2]);
				bool bearishEntry = (Close[1] < Open[1]) && (Close[2] > Open[2]) && (High[1] > bb.Upper[1]) && (High[2] > bb.Upper[2]);

				if (bullishEntry)
				{
					Draw.ArrowUp(this, $"LongEntry{CurrentBar}", false, 1, Low[1] - TickSize, BullishColor);
				}
				else if (bearishEntry)
				{
                    Draw.ArrowDown(this, $"ShortEntry{CurrentBar}", false, 1, High[1] + TickSize, BearishColor);
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
		private LunarTick.ArrowsOffBB[] cacheArrowsOffBB;
		public LunarTick.ArrowsOffBB ArrowsOffBB(int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return ArrowsOffBB(Input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}

		public LunarTick.ArrowsOffBB ArrowsOffBB(ISeries<double> input, int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			if (cacheArrowsOffBB != null)
				for (int idx = 0; idx < cacheArrowsOffBB.Length; idx++)
					if (cacheArrowsOffBB[idx] != null && cacheArrowsOffBB[idx].BBLength == bBLength && cacheArrowsOffBB[idx].BBStdDevMultiplier == bBStdDevMultiplier && cacheArrowsOffBB[idx].BullishColor == bullishColor && cacheArrowsOffBB[idx].BearishColor == bearishColor && cacheArrowsOffBB[idx].EqualsInput(input))
						return cacheArrowsOffBB[idx];
			return CacheIndicator<LunarTick.ArrowsOffBB>(new LunarTick.ArrowsOffBB(){ BBLength = bBLength, BBStdDevMultiplier = bBStdDevMultiplier, BullishColor = bullishColor, BearishColor = bearishColor }, input, ref cacheArrowsOffBB);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.ArrowsOffBB ArrowsOffBB(int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return indicator.ArrowsOffBB(Input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}

		public Indicators.LunarTick.ArrowsOffBB ArrowsOffBB(ISeries<double> input , int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return indicator.ArrowsOffBB(input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.ArrowsOffBB ArrowsOffBB(int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return indicator.ArrowsOffBB(Input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}

		public Indicators.LunarTick.ArrowsOffBB ArrowsOffBB(ISeries<double> input , int bBLength, double bBStdDevMultiplier, Brush bullishColor, Brush bearishColor)
		{
			return indicator.ArrowsOffBB(input, bBLength, bBStdDevMultiplier, bullishColor, bearishColor);
		}
	}
}

#endregion

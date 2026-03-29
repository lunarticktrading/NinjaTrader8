#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Reflection;
using Brushes = System.Windows.Media.Brushes;
using TextAlignment = System.Windows.TextAlignment;
using Brush = System.Windows.Media.Brush;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("[01] Parameters", 1)]
    [Gui.CategoryOrder("[02] Wick Reversals", 2)]
    [Gui.CategoryOrder("[03] Extreme Reversals", 3)]
    [Gui.CategoryOrder("[04] Outside Reversals", 4)]
    [Gui.CategoryOrder("[05] Doji Reversals", 5)]
    [Gui.CategoryOrder("[06] Display", 6)]
    [Gui.CategoryOrder("[07] Developer", 7)]
    public class ReversalCandles : Indicator
	{
        #region Constants

        public const string Version = "1.0.1";

        #endregion

        #region Members

        private ATR _atr;
        private SMA _sma10;
        private Brush _bullishBrush;
        private Brush _bearishBrush;

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATRPeriod", Description = "ATR period used for determining the average candle length.", Order = 1, GroupName = "[01] Parameters")]
        public int ATRPeriod
        { get; set; }

        [Display(Name = "EnableWickReversals", Order = 1, GroupName = "[02] Wick Reversals")]
        public bool EnableWickReversals
        { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "WickReversalsWickBodyRatio", Description = "A reversal wick length must be at least the chosen multiple of the body length.", Order = 2, GroupName = "[02] Wick Reversals")]
        public double WickReversalsWickBodyRatio
        { get; set; }

        [Range(0, 100)]
        [Display(Name = "WickReversalsClosePercent", Description = "A bullish candle must close within the chosen percentage distance from the high.nA bearish candle must close within the chosen percentage distance from the low.", Order = 3, GroupName = "[02] Wick Reversals")]
        public double WickReversalsClosePercent
        { get; set; }

        [Display(Name = "EnableExtremeReversals", Order = 1, GroupName = "[03] Extreme Reversals")]
        public bool EnableExtremeReversals
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "ExtremeReversalsExtremeBarMultipleOfAverage", Description = "Extreme candle length must be at least the chosen multiple of the average candle length.", Order = 2, GroupName = "[03] Extreme Reversals")]
        public int ExtremeReversalsExtremeBarMultipleOfAverage
        { get; set; }

        [Range(0, 100)]
        [Display(Name = "ExtremeReversalsExtremeBarMinBodyPercent", Description = "Extreme candle body must be at least the chosen percentage of total candle length.", Order = 3, GroupName = "[03] Extreme Reversals")]
        public double ExtremeReversalsExtremeBarMinBodyPercent
        { get; set; }

        [Range(0, 100)]
        [Display(Name = "ExtremeReversalsExtremeBarMaxBodyPercent", Description = "Extreme candle body must be less than the chosen percentage of total candle length.", Order = 4, GroupName = "[03] Extreme Reversals")]
        public double ExtremeReversalsExtremeBarMaxBodyPercent
        { get; set; }

        [Display(Name = "EnableOutsideReversals", Order = 1, GroupName = "[04] Outside Reversals")]
        public bool EnableOutsideReversals
        { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "OutsideReversalsEngulfingLargerThanAveragePercent", Description = "Engulfing candle must be the chosen percentage larger than the average candle.", Order = 2, GroupName = "[04] Outside Reversals")]
        public double OutsideReversalsEngulfingLargerThanAveragePercent
        { get; set; }

        [Display(Name = "EnableDojiReversals", Order = 1, GroupName = "[05] Doji Reversals")]
        public bool EnableDojiReversals
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish Reversal Color", Description = "Color used for bullish reversals.", Order = 1, GroupName = "[06] Display")]
        public Brush BullishReversalBrush
        { get; set; }

        [Browsable(false)]
        public string BullishReversalBrushSerializable
        {
            get { return Serialize.BrushToString(BullishReversalBrush); }
            set { BullishReversalBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish Reversal Color", Description = "Color used for bearish reversals.", Order = 2, GroupName = "[06] Display")]
        public Brush BearishReversalBrush
        { get; set; }

        [Browsable(false)]
        public string BearishReversalBrushSerializable
        {
            get { return Serialize.BrushToString(BearishReversalBrush); }
            set { BearishReversalBrush = Serialize.StringToBrush(value); }
        }

        [Range(1, 100)]
        [Display(Name = "Draw Size", Order = 3, GroupName = "[06] Display")]
        public int DrawSize
        { get; set; }

        [Range(1, 100)]
        [Display(Name = "Draw Opacity", Order = 4, GroupName = "[06] Display")]
        public int DrawOpacity
        { get; set; }

        [Range(0, 100)]
        [Display(Name = "Draw Offset", Order = 5, GroupName = "[06] Display")]
        public int DrawOffset
        { get; set; }

        [Display(Name = "ShowLabels", Order = 6, GroupName = "[06] Display")]
        public bool ShowLabels
        { get; set; }

        [ReadOnly(true)]
        [XmlIgnore]
        [Display(Name = "Version", Description = "Version information.", Order = 1, GroupName = "[07] Developer")]
        public string VersionInformation
        { get; set; }

        [Display(Name = "Debug", Description = "Toggle debug logging.", Order = 2, GroupName = "[07] Developer")]
        public bool Debug
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ReversalCandle
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
				Description									= @"Four candle-patterns that help to identify reversals, as shown in Frank Ochoa's book 'Secrets of a Pivot Boss'.";
				Name										= "Reversal Candles";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                ShowTransparentPlotsInDataBox               = true;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

                ATRPeriod                                           = 14;
                EnableWickReversals                                 = true;
				WickReversalsWickBodyRatio					        = 2.5;
				WickReversalsClosePercent					        = 35;
				EnableExtremeReversals					            = true;
				ExtremeReversalsExtremeBarMultipleOfAverage	        = 2;
				ExtremeReversalsExtremeBarMinBodyPercent	        = 50;
				ExtremeReversalsExtremeBarMaxBodyPercent	        = 85;
				EnableOutsideReversals					            = true;
				OutsideReversalsEngulfingLargerThanAveragePercent	= 20;
				EnableDojiReversals					                = true;
                BullishReversalBrush                                = Brushes.LimeGreen;
                BearishReversalBrush                                = Brushes.Red;
                DrawSize                                            = 20;
                DrawOpacity                                         = 25;
                DrawOffset                                          = 1;
				ShowLabels					                        = true;

                VersionInformation                                  = $"{Version} - {Assembly.GetAssembly(typeof(ReversalCandles)).GetName().Version}";
                Debug                                               = false;

                AddPlot(Brushes.Transparent, "Reversal Candle");
            }
            else if (State == State.Configure)
			{
                _bullishBrush = BullishReversalBrush.CloneCurrentValue();
                _bullishBrush.Opacity = DrawOpacity / 100.0;
                _bullishBrush.Freeze();

                _bearishBrush = BearishReversalBrush.CloneCurrentValue();
                _bearishBrush.Opacity = DrawOpacity / 100.0;
                _bearishBrush.Freeze();
            }
            else if (State == State.DataLoaded)
            {
                _atr = ATR(ATRPeriod);
                _sma10 = SMA(10);
            }
        }

        protected override void OnBarUpdate()
		{
            if (CurrentBar < Math.Max(10, ATRPeriod))
                return;

            if (IsFirstTickOfBar)
            {
                int barsAgo = 1;

                var barRange = High[barsAgo] - Low[barsAgo];
                var barBodyLength = Math.Max(Open[barsAgo], Close[barsAgo]) - Math.Min(Open[barsAgo], Close[barsAgo]);
                var barUpperWickLength = High[barsAgo] - Math.Max(Open[barsAgo], Close[barsAgo]);
                var barLowerWickLength = Math.Min(Open[barsAgo], Close[barsAgo]) - Low[barsAgo];
                var barRange1 = High[barsAgo + 1] - Low[barsAgo + 1];
                var barBodyLength1 = Math.Max(Open[barsAgo + 1], Close[barsAgo + 1]) - Math.Min(Open[barsAgo + 1], Close[barsAgo + 1]);

                var barRange2 = High[barsAgo + 2] - Low[barsAgo + 2];
                var barBodyLength2 = Math.Max(Open[barsAgo + 2], Close[barsAgo + 2]) - Math.Min(Open[barsAgo + 2], Close[barsAgo + 2]);

                // Wick reversals
                bool bullishWickReversal = EnableWickReversals && ((barLowerWickLength / barBodyLength) >= WickReversalsWickBodyRatio) && ((((High[barsAgo] - Close[barsAgo]) / barRange) * 100) <= WickReversalsClosePercent);
                bool bearishWickReversal = EnableWickReversals && ((barUpperWickLength / barBodyLength) >= WickReversalsWickBodyRatio) && ((((Close[barsAgo] - Low[barsAgo]) / barRange) * 100) <= WickReversalsClosePercent);

                // Extreme reversals
                bool bullishExtremeReversal = EnableExtremeReversals && (barRange1 >= (ExtremeReversalsExtremeBarMultipleOfAverage * _atr[barsAgo])) && (((barBodyLength1 / barRange1) * 100) >= ExtremeReversalsExtremeBarMinBodyPercent) && (((barBodyLength1 / barRange1) * 100) <= ExtremeReversalsExtremeBarMaxBodyPercent) && (Close[barsAgo + 1] < Open[barsAgo + 1]) && (Close[barsAgo] > Open[barsAgo]);
                bool bearishExtremeReversal = EnableExtremeReversals && (barRange1 >= (ExtremeReversalsExtremeBarMultipleOfAverage * _atr[barsAgo])) && (((barBodyLength1 / barRange1) * 100) >= ExtremeReversalsExtremeBarMinBodyPercent) && (((barBodyLength1 / barRange1) * 100) <= ExtremeReversalsExtremeBarMaxBodyPercent) && (Close[barsAgo + 1] > Open[barsAgo + 1]) && (Close[barsAgo] < Open[barsAgo]);

                // Outside reversals
                bool bullishOutsideReversal = EnableOutsideReversals && (Low[barsAgo] < Low[barsAgo + 1]) && (Close[barsAgo] > High[barsAgo + 1]) && (barRange > (_atr[barsAgo] * ((OutsideReversalsEngulfingLargerThanAveragePercent + 100) / 100.0)));
                bool bearishOutsideReversal = EnableOutsideReversals && (High[barsAgo] > High[barsAgo + 1]) && (Close[barsAgo] < Low[barsAgo + 1]) && (barRange > (_atr[barsAgo] * ((OutsideReversalsEngulfingLargerThanAveragePercent + 100) / 100.0)));

                // Doji reversals
                bool bullishDojiReversal = EnableDojiReversals && ((((barBodyLength1 / barRange1) <= 0.1) && (High[barsAgo + 1] < _sma10[barsAgo]) && (Close[barsAgo] > High[barsAgo + 1])) || (((barBodyLength2 / barRange2) <= 0.1) && (High[barsAgo + 2] < _sma10[barsAgo]) && (Close[barsAgo] > High[barsAgo + 2])));
                bool bearishDojiReversal = EnableDojiReversals && ((((barBodyLength1 / barRange1) <= 0.1) && (Low[barsAgo + 1] > _sma10[barsAgo]) && (Close[barsAgo] < Low[barsAgo + 1])) || (((barBodyLength2 / barRange2) <= 0.1) && (Low[barsAgo + 2] > _sma10[barsAgo]) && (Close[barsAgo] < Low[barsAgo + 2])));

                // Summary
                bool bullishReversal = bullishWickReversal || bullishExtremeReversal || bullishOutsideReversal || bullishDojiReversal;
                bool bearishReversal = bearishWickReversal || bearishExtremeReversal || bearishOutsideReversal || bearishDojiReversal;

                // Draw reversals
                if (bullishReversal)
                {
                    ReversalCandle[barsAgo] = 1.0;
                    SimpleFont font = new SimpleFont("Arial", DrawSize);
                    string label = "\u25B2";
                    int numLines = 1;
                    if (ShowLabels)
                    {
                        if (bullishWickReversal)
                        {
                            label += "\nWR";
                            numLines++;
                        }
                        if (bullishExtremeReversal)
                        {
                            label += "\nER";
                            numLines++;
                        }
                        if (bullishOutsideReversal)
                        {
                            label += "\nOR";
                            numLines++;
                        }
                        if (bullishDojiReversal)
                        {
                            label += "\nDR";
                            numLines++;
                        }
                    }
                    int yOffset = (int)(((numLines * font.TextFormatHeight) / 2.0) + (DrawOffset * font.TextFormatHeight));
                    Draw.Text(this, $"BullishReversalCandle-{CurrentBar}", false, label, barsAgo, Low[barsAgo], -yOffset, _bullishBrush, font, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                }
                if (bearishReversal)
                {
                    ReversalCandle[barsAgo] = -1.0;
                    SimpleFont font = new SimpleFont("Arial", DrawSize);
                    string label = "";
                    int numLines = 1;
                    if (ShowLabels)
                    {
                        if (bearishWickReversal)
                        {
                            label += "WR\n";
                            numLines++;
                        }
                        if (bearishExtremeReversal)
                        {
                            label += "ER\n";
                            numLines++;
                        }
                        if (bearishOutsideReversal)
                        {
                            label += "OR\n";
                            numLines++;
                        }
                        if (bearishDojiReversal)
                        {
                            label += "DR\n";
                            numLines++;
                        }
                    }
                    label += "\u25BC";
                    int yOffset = (int)(((numLines * font.TextFormatHeight) / 2.0) + (DrawOffset * font.TextFormatHeight));
                    Draw.Text(this, $"BearishReversalCandle-{CurrentBar}", false, label, barsAgo, High[barsAgo], yOffset, _bearishBrush, font, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
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
                    Print($"ReversalCandles[{Instrument.FullName}]: {msg}");
                else
                    Print($"ReversalCandles: {msg}");
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
		private LunarTick.ReversalCandles[] cacheReversalCandles;
		public LunarTick.ReversalCandles ReversalCandles(int aTRPeriod, Brush bullishReversalBrush, Brush bearishReversalBrush)
		{
			return ReversalCandles(Input, aTRPeriod, bullishReversalBrush, bearishReversalBrush);
		}

		public LunarTick.ReversalCandles ReversalCandles(ISeries<double> input, int aTRPeriod, Brush bullishReversalBrush, Brush bearishReversalBrush)
		{
			if (cacheReversalCandles != null)
				for (int idx = 0; idx < cacheReversalCandles.Length; idx++)
					if (cacheReversalCandles[idx] != null && cacheReversalCandles[idx].ATRPeriod == aTRPeriod && cacheReversalCandles[idx].BullishReversalBrush == bullishReversalBrush && cacheReversalCandles[idx].BearishReversalBrush == bearishReversalBrush && cacheReversalCandles[idx].EqualsInput(input))
						return cacheReversalCandles[idx];
			return CacheIndicator<LunarTick.ReversalCandles>(new LunarTick.ReversalCandles(){ ATRPeriod = aTRPeriod, BullishReversalBrush = bullishReversalBrush, BearishReversalBrush = bearishReversalBrush }, input, ref cacheReversalCandles);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.ReversalCandles ReversalCandles(int aTRPeriod, Brush bullishReversalBrush, Brush bearishReversalBrush)
		{
			return indicator.ReversalCandles(Input, aTRPeriod, bullishReversalBrush, bearishReversalBrush);
		}

		public Indicators.LunarTick.ReversalCandles ReversalCandles(ISeries<double> input , int aTRPeriod, Brush bullishReversalBrush, Brush bearishReversalBrush)
		{
			return indicator.ReversalCandles(input, aTRPeriod, bullishReversalBrush, bearishReversalBrush);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.ReversalCandles ReversalCandles(int aTRPeriod, Brush bullishReversalBrush, Brush bearishReversalBrush)
		{
			return indicator.ReversalCandles(Input, aTRPeriod, bullishReversalBrush, bearishReversalBrush);
		}

		public Indicators.LunarTick.ReversalCandles ReversalCandles(ISeries<double> input , int aTRPeriod, Brush bullishReversalBrush, Brush bearishReversalBrush)
		{
			return indicator.ReversalCandles(input, aTRPeriod, bullishReversalBrush, bearishReversalBrush);
		}
	}
}

#endregion

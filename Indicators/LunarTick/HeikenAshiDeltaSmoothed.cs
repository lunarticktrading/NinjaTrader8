#region Using declarations
using NinjaTrader.Cbi;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Developer", 2)]
    public class HeikenAshiDeltaSmoothed : Indicator
	{
        #region Constants

        public const string Version = "1.0.0";

        #endregion

        #region Members

        private HeikenAshi8 _ha;
		private Series<double> _haDelta;

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Smoothing Length", Order = 1, GroupName = "Parameters")]
        public int SmoothingLength
        { get; set; }

        [ReadOnly(true)]
        [XmlIgnore]
        [Display(Name = "Version", Description = "Version information.", Order = 1, GroupName = "Developer")]
        public string VersionInformation
        { get; set; }

        [Display(Name = "Debug", Description = "Toggle debug logging.", Order = 2, GroupName = "Developer")]
        public bool Debug
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> HADeltaSmoothed
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
				Description									= @"An oscillator showing smoothed HeikenAshi delta (Close - Open).";
				Name										= "HeikenAshi Delta Smoothed";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

                SmoothingLength								= 4;

                VersionInformation							= $"{Version} - {Assembly.GetAssembly(typeof(HeikenAshiDeltaSmoothed)).GetName().Version}";
                Debug										= false;
                
				AddPlot(Brushes.White, "HeikenAshi Delta Smoothed");
			}
			else if (State == State.Configure)
			{
            }
            else if (State == State.DataLoaded)
            {
                _ha = HeikenAshi8(Brushes.Transparent, Brushes.Transparent, Brushes.Transparent);
				_haDelta = new Series<double>(this, MaximumBarsLookBack.Infinite);
            }
        }

        protected override void OnBarUpdate()
		{
			if (CurrentBar < SmoothingLength)
				return;

			_ha.Update();

			_haDelta[0] = _ha.HAClose[0] - _ha.HAOpen[0];
			HADeltaSmoothed[0] = SMA(_haDelta, SmoothingLength)[0];
        }

        #endregion

        #region Private methods

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"HeikenAshiDeltaSmoothed[{Instrument.FullName}]: {msg}");
                else
                    Print($"HeikenAshiDeltaSmoothed: {msg}");
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
		private LunarTick.HeikenAshiDeltaSmoothed[] cacheHeikenAshiDeltaSmoothed;
		public LunarTick.HeikenAshiDeltaSmoothed HeikenAshiDeltaSmoothed(int smoothingLength)
		{
			return HeikenAshiDeltaSmoothed(Input, smoothingLength);
		}

		public LunarTick.HeikenAshiDeltaSmoothed HeikenAshiDeltaSmoothed(ISeries<double> input, int smoothingLength)
		{
			if (cacheHeikenAshiDeltaSmoothed != null)
				for (int idx = 0; idx < cacheHeikenAshiDeltaSmoothed.Length; idx++)
					if (cacheHeikenAshiDeltaSmoothed[idx] != null && cacheHeikenAshiDeltaSmoothed[idx].SmoothingLength == smoothingLength && cacheHeikenAshiDeltaSmoothed[idx].EqualsInput(input))
						return cacheHeikenAshiDeltaSmoothed[idx];
			return CacheIndicator<LunarTick.HeikenAshiDeltaSmoothed>(new LunarTick.HeikenAshiDeltaSmoothed(){ SmoothingLength = smoothingLength }, input, ref cacheHeikenAshiDeltaSmoothed);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.HeikenAshiDeltaSmoothed HeikenAshiDeltaSmoothed(int smoothingLength)
		{
			return indicator.HeikenAshiDeltaSmoothed(Input, smoothingLength);
		}

		public Indicators.LunarTick.HeikenAshiDeltaSmoothed HeikenAshiDeltaSmoothed(ISeries<double> input , int smoothingLength)
		{
			return indicator.HeikenAshiDeltaSmoothed(input, smoothingLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.HeikenAshiDeltaSmoothed HeikenAshiDeltaSmoothed(int smoothingLength)
		{
			return indicator.HeikenAshiDeltaSmoothed(Input, smoothingLength);
		}

		public Indicators.LunarTick.HeikenAshiDeltaSmoothed HeikenAshiDeltaSmoothed(ISeries<double> input , int smoothingLength)
		{
			return indicator.HeikenAshiDeltaSmoothed(input, smoothingLength);
		}
	}
}

#endregion

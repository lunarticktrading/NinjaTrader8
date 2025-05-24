#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.Indicators.gambcl.PaperFeetEnums;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.gambcl
{
    namespace PaperFeetEnums
    {
        public enum LRSITypeEnum
        {
            LaguerreRSI,
            LaguerreRSIWithFractalEnergy
        }
    }

    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Laguerre RSI", 2)]
    [Gui.CategoryOrder("Laguerre RSI with Fractal Energy", 3)]
    [Gui.CategoryOrder("Thresholds", 4)]
    [Gui.CategoryOrder("Signals", 5)]
    [Gui.CategoryOrder("Alerts", 6)]
    public class PaperFeet : Indicator
	{
        #region Members
        private LaguerreRSI _lrsi;
        private SharpDX.Direct2D1.Brush _overboughtBrushDx;
        private SharpDX.Direct2D1.Brush _oversoldBrushDx;
        private Brush _longEntrySignalBrush;
        private Brush _shortEntrySignalBrush;
        private bool _isRsiInitialized;
        private Series<bool> _longEntrySignal;
        private Series<bool> _shortEntrySignal;
        #endregion

        #region Indicator methods
        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicator using LaguerreRSI for entry signals.";
				Name										= "PaperFeet";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                LRSIType                                    = LRSITypeEnum.LaguerreRSIWithFractalEnergy;
                Alpha                                       = 0.2;
                NFE                                         = 8;
                GLength                                     = 13;
                BetaDev                                     = 8;
                OverboughtLevel                             = 80;
                OversoldLevel                               = 20;
                OverboughtRegionBrush                       = Brushes.Red;
                OversoldRegionBrush                         = Brushes.Green;
                RegionOpacity                               = 40;
                EnableLongEntrySignals                      = true;
                EnableShortEntrySignals                     = true;
                LongEntrySignalBrush                        = Brushes.Green;
                ShortEntrySignalBrush                       = Brushes.Red;
                SignalsOpacity                              = 50;
                EnableAlerts                                = false;
                AlertSoundsPath                             = DefaultAlertFilePath();
                LongEntryAlert                              = "LongEntry.wav";
                ShortEntryAlert                             = "ShortEntry.wav";
                AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "LRSI");
                AddPlot(Brushes.Transparent, "Signals");
                _isRsiInitialized = false;
            }
            else if (State == State.Configure)
			{
                // Disable IsSuspendedWhileInactive if alerts are enabled.
                IsSuspendedWhileInactive = !EnableAlerts;

                _lrsi = LaguerreRSI(LRSIType == LRSITypeEnum.LaguerreRSIWithFractalEnergy, Alpha, NFE, GLength, BetaDev, OverboughtLevel, OversoldLevel, OverboughtRegionBrush, OversoldRegionBrush, RegionOpacity, false, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                _isRsiInitialized = false;

                _longEntrySignalBrush = LongEntrySignalBrush.CloneCurrentValue();
                _longEntrySignalBrush.Opacity = SignalsOpacity / 100.0;
                _longEntrySignalBrush.Freeze();

                _shortEntrySignalBrush = ShortEntrySignalBrush.CloneCurrentValue();
                _shortEntrySignalBrush.Opacity = SignalsOpacity / 100.0;
                _shortEntrySignalBrush.Freeze();

                _longEntrySignal = new Series<bool>(this);
                _shortEntrySignal = new Series<bool>(this);
            }
        }

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                if (LRSIType == LRSITypeEnum.LaguerreRSI)
                    return Name + "(" + LRSIType + "," + Alpha + ")";
                else
                    return Name + "(" + LRSIType + "," + NFE + "," + GLength + "," + BetaDev + ")";
            }
        }

        protected override void OnBarUpdate()
		{
            if (CurrentBar < 2)
                return;

            _lrsi.Update();
            LRSI[0] = _lrsi.LRSI[0];

            if (!_isRsiInitialized && LRSI[1] != 0.0)
                _isRsiInitialized = true;

            if (!_isRsiInitialized)
                return;

            // Signals
            LongEntrySignal[0] = EnableLongEntrySignals && (LRSI[1] <= OversoldLevel) && (LRSI[0] > OversoldLevel);
            ShortEntrySignal[0] = EnableShortEntrySignals && (LRSI[1] >= OverboughtLevel) && (LRSI[0] < OverboughtLevel);
            if (LongEntrySignal[0])
            {
                // LONG entry signal.
                Signals[0] = 1;
                BackBrush = _longEntrySignalBrush;
            }
            else if (ShortEntrySignal[0])
            {
                // SHORT entry signal.
                Signals[0] = -1;
                BackBrush = _shortEntrySignalBrush;
            }
            else
            {
                // No signal.
                Signals[0] = 0;
                BackBrush = Brushes.Transparent;
            }

            // Alerts
            if (EnableAlerts && (State == State.Realtime) && IsFirstTickOfBar)
            {
                if (EnableLongEntrySignals && !string.IsNullOrWhiteSpace(LongEntryAlert) && LongEntrySignal[1])
                {
                    string audioFile = ResolveAlertFilePath(LongEntryAlert, AlertSoundsPath);
                    Alert("LongEntryAlert", Priority.High, string.Format("Enter LONG: Laguerre RSI leaving oversold region {0:0.#}", LRSI[1]), audioFile, 10, Brushes.Black, LongEntrySignalBrush);
                }
                if (EnableShortEntrySignals && !string.IsNullOrWhiteSpace(ShortEntryAlert) && ShortEntrySignal[1])
                {
                    string audioFile = ResolveAlertFilePath(ShortEntryAlert, AlertSoundsPath);
                    Alert("ShortEntryAlert", Priority.High, string.Format("Enter SHORT: Laguerre RSI leaving overbought region {0:0.#}", LRSI[1]), audioFile, 10, Brushes.Black, ShortEntrySignalBrush);
                }
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            if (!IsVisible)
                return;

            if (chartControl == null || chartScale == null || ChartBars == null || RenderTarget == null)
                return;

            var leftX = chartControl.GetXByBarIndex(ChartBars, Math.Max(ChartBars.FromIndex - 1, 0));
            var rightX = chartControl.GetXByBarIndex(ChartBars, ChartBars.ToIndex);
            var width = rightX - leftX;

            // Draw overbought region.
            if (OverboughtRegionBrush != null && OverboughtRegionBrush != Brushes.Transparent && _overboughtBrushDx != null && !_overboughtBrushDx.IsDisposed)
            {
                var topY = chartScale.GetYByValue(100);
                var bottomY = chartScale.GetYByValue(OverboughtLevel);
                var height = bottomY - topY;
                SharpDX.RectangleF overboughtRect = new SharpDX.RectangleF(leftX, topY, width, height);
                RenderTarget.FillRectangle(overboughtRect, _overboughtBrushDx);
            }

            // Draw oversold region.
            if (OversoldRegionBrush != null && OversoldRegionBrush != Brushes.Transparent && _oversoldBrushDx != null && !_oversoldBrushDx.IsDisposed)
            {
                var topY = chartScale.GetYByValue(OversoldLevel);
                var bottomY = chartScale.GetYByValue(0);
                var height = bottomY - topY;
                SharpDX.RectangleF oversoldRect = new SharpDX.RectangleF(leftX, topY, width, height);
                RenderTarget.FillRectangle(oversoldRect, _oversoldBrushDx);
            }

            // NOTE: Call base.OnRender as we also want the Plots to appear on the chart.
            base.OnRender(chartControl, chartScale);
        }

        public override void OnRenderTargetChanged()
        {
            if (_overboughtBrushDx != null)
                _overboughtBrushDx.Dispose();

            if (_oversoldBrushDx != null)
                _oversoldBrushDx.Dispose();

            if (RenderTarget != null)
            {
                try
                {
                    _overboughtBrushDx = OverboughtRegionBrush.ToDxBrush(RenderTarget, (float)(RegionOpacity / 100.0f));
                    _oversoldBrushDx = OversoldRegionBrush.ToDxBrush(RenderTarget, (float)(RegionOpacity / 100.0f));
                }
                catch (Exception e) { }
            }
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "Laguerre RSI Type", Description = "Specifies the type of Laguerre RSI.", Order = 1, GroupName = "Parameters")]
        public NinjaTrader.NinjaScript.Indicators.gambcl.PaperFeetEnums.LRSITypeEnum LRSIType
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Alpha", Order = 1, GroupName = "Laguerre RSI")]
        public double Alpha
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "NFE", Description = "Number of bars used in Fractal Energy calculations.", Order = 1, GroupName = "Laguerre RSI with Fractal Energy")]
        public int NFE
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "GLength", Description = "Period length for Go/Gh/Gl/Gc filter.", Order = 2, GroupName = "Laguerre RSI with Fractal Energy")]
        public int GLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BetaDev", Description = "Controls reactivity in alpha/beta computations.", Order = 3, GroupName = "Laguerre RSI with Fractal Energy")]
        public int BetaDev
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "OverboughtLevel", Order = 1, GroupName = "Thresholds")]
        public double OverboughtLevel
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "OversoldLevel", Order = 2, GroupName = "Thresholds")]
        public double OversoldLevel
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "OverboughtRegionBrush", Order = 3, GroupName = "Thresholds")]
        public Brush OverboughtRegionBrush
        { get; set; }

        [Browsable(false)]
        public string OverboughtRegionBrushSerializable
        {
            get { return Serialize.BrushToString(OverboughtRegionBrush); }
            set { OverboughtRegionBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "OversoldRegionBrush", Order = 4, GroupName = "Thresholds")]
        public Brush OversoldRegionBrush
        { get; set; }

        [Browsable(false)]
        public string OversoldRegionBrushSerializable
        {
            get { return Serialize.BrushToString(OversoldRegionBrush); }
            set { OversoldRegionBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Range(0, 100)]
        [Display(Name = "Region Opacity", Description = "The opacity of the overbought/oversold regions (0 = completely transparent, 100 = no opacity).", Order = 5, GroupName = "Thresholds")]
        public int RegionOpacity
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Long Entry Signals", Description = "Display LONG entry signals.", Order = 1, GroupName = "Signals")]
        public bool EnableLongEntrySignals
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Short Entry Signals", Description = "Display SHORT entry signals.", Order = 2, GroupName = "Signals")]
        public bool EnableShortEntrySignals
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Long Entry Signal Color", Description = "The color of the LONG entry signals.", Order = 3, GroupName = "Signals")]
        public Brush LongEntrySignalBrush
        { get; set; }

        [Browsable(false)]
        public string LongEntrySignalBrushSerializable
        {
            get { return Serialize.BrushToString(LongEntrySignalBrush); }
            set { LongEntrySignalBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Short Entry Signal Color", Description = "The color of the SHORT entry signals.", Order = 4, GroupName = "Signals")]
        public Brush ShortEntrySignalBrush
        { get; set; }

        [Browsable(false)]
        public string ShortEntrySignalBrushSerializable
        {
            get { return Serialize.BrushToString(ShortEntrySignalBrush); }
            set { ShortEntrySignalBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Range(0, 100)]
        [Display(Name = "Signals Opacity", Description = "The opacity of the entry signals (0 = completely transparent, 100 = no opacity).", Order = 5, GroupName = "Signals")]
        public int SignalsOpacity
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Alerts", Description = "Trigger alerts for confirmed signals.", Order = 1, GroupName = "Alerts")]
        public bool EnableAlerts
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Alert Sounds Path", Description = "Location of alert audio files used for confirmed signals.", Order = 2, GroupName = "Alerts")]
        public string AlertSoundsPath
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Long Entry Alert", Description = "Alert sound used for confirmed LONG entry signals.", Order = 3, GroupName = "Alerts")]
        public string LongEntryAlert
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Short Entry Alert", Description = "Alert sound used for confirmed SHORT entry signals.", Order = 4, GroupName = "Alerts")]
        public string ShortEntryAlert
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> LRSI
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Signals
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> LongEntrySignal
        {
            get { return _longEntrySignal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> ShortEntrySignal
        {
            get { return _shortEntrySignal; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private gambcl.PaperFeet[] cachePaperFeet;
		public gambcl.PaperFeet PaperFeet(NinjaTrader.NinjaScript.Indicators.gambcl.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableLongEntrySignals, bool enableShortEntrySignals, Brush longEntrySignalBrush, Brush shortEntrySignalBrush, int signalsOpacity, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return PaperFeet(Input, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableLongEntrySignals, enableShortEntrySignals, longEntrySignalBrush, shortEntrySignalBrush, signalsOpacity, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}

		public gambcl.PaperFeet PaperFeet(ISeries<double> input, NinjaTrader.NinjaScript.Indicators.gambcl.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableLongEntrySignals, bool enableShortEntrySignals, Brush longEntrySignalBrush, Brush shortEntrySignalBrush, int signalsOpacity, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			if (cachePaperFeet != null)
				for (int idx = 0; idx < cachePaperFeet.Length; idx++)
					if (cachePaperFeet[idx] != null && cachePaperFeet[idx].LRSIType == lRSIType && cachePaperFeet[idx].Alpha == alpha && cachePaperFeet[idx].NFE == nFE && cachePaperFeet[idx].GLength == gLength && cachePaperFeet[idx].BetaDev == betaDev && cachePaperFeet[idx].OverboughtLevel == overboughtLevel && cachePaperFeet[idx].OversoldLevel == oversoldLevel && cachePaperFeet[idx].OverboughtRegionBrush == overboughtRegionBrush && cachePaperFeet[idx].OversoldRegionBrush == oversoldRegionBrush && cachePaperFeet[idx].RegionOpacity == regionOpacity && cachePaperFeet[idx].EnableLongEntrySignals == enableLongEntrySignals && cachePaperFeet[idx].EnableShortEntrySignals == enableShortEntrySignals && cachePaperFeet[idx].LongEntrySignalBrush == longEntrySignalBrush && cachePaperFeet[idx].ShortEntrySignalBrush == shortEntrySignalBrush && cachePaperFeet[idx].SignalsOpacity == signalsOpacity && cachePaperFeet[idx].EnableAlerts == enableAlerts && cachePaperFeet[idx].AlertSoundsPath == alertSoundsPath && cachePaperFeet[idx].LongEntryAlert == longEntryAlert && cachePaperFeet[idx].ShortEntryAlert == shortEntryAlert && cachePaperFeet[idx].EqualsInput(input))
						return cachePaperFeet[idx];
			return CacheIndicator<gambcl.PaperFeet>(new gambcl.PaperFeet(){ LRSIType = lRSIType, Alpha = alpha, NFE = nFE, GLength = gLength, BetaDev = betaDev, OverboughtLevel = overboughtLevel, OversoldLevel = oversoldLevel, OverboughtRegionBrush = overboughtRegionBrush, OversoldRegionBrush = oversoldRegionBrush, RegionOpacity = regionOpacity, EnableLongEntrySignals = enableLongEntrySignals, EnableShortEntrySignals = enableShortEntrySignals, LongEntrySignalBrush = longEntrySignalBrush, ShortEntrySignalBrush = shortEntrySignalBrush, SignalsOpacity = signalsOpacity, EnableAlerts = enableAlerts, AlertSoundsPath = alertSoundsPath, LongEntryAlert = longEntryAlert, ShortEntryAlert = shortEntryAlert }, input, ref cachePaperFeet);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.gambcl.PaperFeet PaperFeet(NinjaTrader.NinjaScript.Indicators.gambcl.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableLongEntrySignals, bool enableShortEntrySignals, Brush longEntrySignalBrush, Brush shortEntrySignalBrush, int signalsOpacity, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return indicator.PaperFeet(Input, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableLongEntrySignals, enableShortEntrySignals, longEntrySignalBrush, shortEntrySignalBrush, signalsOpacity, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}

		public Indicators.gambcl.PaperFeet PaperFeet(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.gambcl.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableLongEntrySignals, bool enableShortEntrySignals, Brush longEntrySignalBrush, Brush shortEntrySignalBrush, int signalsOpacity, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return indicator.PaperFeet(input, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableLongEntrySignals, enableShortEntrySignals, longEntrySignalBrush, shortEntrySignalBrush, signalsOpacity, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.gambcl.PaperFeet PaperFeet(NinjaTrader.NinjaScript.Indicators.gambcl.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableLongEntrySignals, bool enableShortEntrySignals, Brush longEntrySignalBrush, Brush shortEntrySignalBrush, int signalsOpacity, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return indicator.PaperFeet(Input, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableLongEntrySignals, enableShortEntrySignals, longEntrySignalBrush, shortEntrySignalBrush, signalsOpacity, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}

		public Indicators.gambcl.PaperFeet PaperFeet(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.gambcl.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableLongEntrySignals, bool enableShortEntrySignals, Brush longEntrySignalBrush, Brush shortEntrySignalBrush, int signalsOpacity, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string shortEntryAlert)
		{
			return indicator.PaperFeet(input, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableLongEntrySignals, enableShortEntrySignals, longEntrySignalBrush, shortEntrySignalBrush, signalsOpacity, enableAlerts, alertSoundsPath, longEntryAlert, shortEntryAlert);
		}
	}
}

#endregion

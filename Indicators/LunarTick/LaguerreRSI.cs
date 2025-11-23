#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Laguerre RSI", 2)]
    [Gui.CategoryOrder("Laguerre RSI with Fractal Energy", 3)]
    [Gui.CategoryOrder("Thresholds", 4)]
    [Gui.CategoryOrder("Alerts", 5)]
    public class LaguerreRSI : Indicator
    {
        #region Members
        private Series<double> _l0Series;
        private Series<double> _l1Series;
        private Series<double> _l2Series;
        private Series<double> _l3Series;
        private Series<double> _gOSeries;
        private Series<double> _gHSeries;
        private Series<double> _gLSeries;
        private Series<double> _gCSeries;
        private SharpDX.Direct2D1.Brush _overboughtBrushDx;
        private SharpDX.Direct2D1.Brush _oversoldBrushDx;
        #endregion

        #region Indicator methods
        public LaguerreRSI()
        {
            VendorLicense(283);
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description									= @"Laguerre RSI.";
                Name										= "LaguerreRSI";
                Calculate									= Calculate.OnPriceChange;
                IsOverlay									= false;
                DisplayInDataBox							= true;
                DrawOnPricePanel							= false;
                DrawHorizontalGridLines						= true;
                DrawVerticalGridLines						= true;
                PaintPriceMarkers							= true;
                ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                UseFractalEnergy					        = true;
                Alpha					                    = 0.2;
                NFE					                        = 8;
                GLength					                    = 13;
                BetaDev					                    = 8;
                OverboughtLevel					            = 80;
                OversoldLevel					            = 20;
                OverboughtRegionBrush					    = Brushes.Red;
                OversoldRegionBrush					        = Brushes.Green;
                RegionOpacity                               = 40;
                EnableAlerts                                = false;
                AlertSoundsPath                             = DefaultAlertFilePath();
                EnterOverboughtAlert                        = "RSIOverbought.wav";
                ExitOverboughtAlert                         = "RSILeavingOverbought.wav";
                EnterOversoldAlert                          = "RSIOversold.wav";
                ExitOversoldAlert                           = "RSILeavingOversold.wav";
                AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "LRSI");
            }
            else if (State == State.Configure)
            {
                // Disable IsSuspendedWhileInactive if alerts are enabled.
                IsSuspendedWhileInactive = !EnableAlerts;

                _l0Series = new Series<double>(this);
                _l1Series = new Series<double>(this);
                _l2Series = new Series<double>(this);
                _l3Series = new Series<double>(this);
                _gOSeries = new Series<double>(this);
                _gHSeries = new Series<double>(this);
                _gLSeries = new Series<double>(this);
                _gCSeries = new Series<double>(this);
            }
        }

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                if (UseFractalEnergy)
                    return Name + "(" + UseFractalEnergy + "," + NFE + "," + GLength + "," + BetaDev + ")";
                else
                    return Name + "(" + UseFractalEnergy + "," + Alpha + ")";
            }
        }

        protected override void OnBarUpdate()
        {
            if (!UseFractalEnergy)
            {
                ////////////////////////////////////////////////////////////////////////////////
                // Laguerre RSI
                // This source code is subject to the terms of the Mozilla Public License 2.0 at https://mozilla.org/MPL/2.0/
                // Developer: John EHLERS
                ////////////////////////////////////////////////////////////////////////////////

                if (CurrentBar < 2)
                    return;

                double gamma = 1.0 - Alpha;
                _l0Series[0] = (1.0 - gamma) * Close[0] + gamma * _l0Series[1];
                _l1Series[0] = -gamma * _l0Series[0] + _l0Series[1] + gamma * _l1Series[1];
                _l2Series[0] = -gamma * _l1Series[0] + _l1Series[1] + gamma * _l2Series[1];
                _l3Series[0] = -gamma * _l2Series[0] + _l2Series[1] + gamma * _l3Series[1];
                double cu = (_l0Series[0] > _l1Series[0] ? _l0Series[0] - _l1Series[0] : 0) + (_l1Series[0] > _l2Series[0] ? _l1Series[0] - _l2Series[0] : 0) + (_l2Series[0] > _l3Series[0] ? _l2Series[0] - _l3Series[0] : 0);
                double cd = (_l0Series[0] < _l1Series[0] ? _l1Series[0] - _l0Series[0] : 0) + (_l1Series[0] < _l2Series[0] ? _l2Series[0] - _l1Series[0] : 0) + (_l2Series[0] < _l3Series[0] ? _l3Series[0] - _l2Series[0] : 0);
                double temp = (cu + cd == 0.0) ? -1.0 : cu + cd;
                LRSI[0] = 100.0 * (temp == -1 ? 0 : cu / temp);
            }
            else
            {
                ////////////////////////////////////////////////////////////////////////////////
                // Laguerre RSI with Fractal Energy
                // https://usethinkscript.com/threads/rsi-laguerre-with-fractal-energy-for-thinkorswim.116/
                ////////////////////////////////////////////////////////////////////////////////

                if (CurrentBar < (NFE + 1))
                    return;

                double w = (2.0 * Math.PI / GLength);
                double beta = (1.0 - Math.Cos(w)) / (Math.Pow(1.414, 2.0 / BetaDev) - 1.0);
                double alpha = (-beta + Math.Sqrt((beta * beta + 2.0 * beta)));

                _gOSeries[0] = Math.Pow(alpha, 4.0) * Open[0] + 4.0 * (1.0 - alpha) * _gOSeries[1] - 6.0 * Math.Pow(1.0 - alpha, 2.0) * _gOSeries[2] + 4.0 * Math.Pow(1.0 - alpha, 3.0) * _gOSeries[3] - Math.Pow(1.0 - alpha, 4.0) * _gOSeries[4];
                _gHSeries[0] = Math.Pow(alpha, 4.0) * High[0] + 4.0 * (1.0 - alpha) * _gHSeries[1] - 6.0 * Math.Pow(1.0 - alpha, 2.0) * _gHSeries[2] + 4.0 * Math.Pow(1.0 - alpha, 3.0) * _gHSeries[3] - Math.Pow(1.0 - alpha, 4.0) * _gHSeries[4];
                _gLSeries[0] = Math.Pow(alpha, 4.0) * Low[0] + 4.0 * (1.0 - alpha) * _gLSeries[1] - 6.0 * Math.Pow(1.0 - alpha, 2.0) * _gLSeries[2] + 4.0 * Math.Pow(1.0 - alpha, 3.0) * _gLSeries[3] - Math.Pow(1.0 - alpha, 4.0) * _gLSeries[4];
                _gCSeries[0] = Math.Pow(alpha, 4.0) * Close[0] + 4.0 * (1.0 - alpha) * _gCSeries[1] - 6.0 * Math.Pow(1.0 - alpha, 2.0) * _gCSeries[2] + 4.0 * Math.Pow(1.0 - alpha, 3.0) * _gCSeries[3] - Math.Pow(1.0 - alpha, 4.0) * _gCSeries[4];

                // Calculations
                double o = (_gOSeries[0] + _gCSeries[1]) / 2.0;
                double h = Math.Max(_gHSeries[0], _gCSeries[1]);
                double l = Math.Min(_gLSeries[0], _gCSeries[1]);
                double c = (o + h + l + _gCSeries[0]) / 4.0;
                double tempSum = 0.0;
                for (int idx = 0; idx < NFE; idx++)
                {
                    tempSum += (Math.Max(_gHSeries[idx], _gCSeries[idx + 1]) - Math.Min(_gLSeries[idx], _gCSeries[idx + 1]));
                }
                double gamma = (Math.Log((tempSum / (MAX(_gHSeries, NFE)[0] - MIN(_gLSeries, NFE)[0])))
                                /
                                Math.Log(NFE));

                _l0Series[0] = ((1.0 - gamma) * _gCSeries[0]) + (gamma * _l0Series[1]);
                _l1Series[0] = -gamma * _l0Series[0] + _l0Series[1] + gamma * _l1Series[1];
                _l2Series[0] = -gamma * _l1Series[0] + _l1Series[1] + gamma * _l2Series[1];
                _l3Series[0] = -gamma * _l2Series[0] + _l2Series[1] + gamma * _l3Series[1];

                double cu1 = 0.0;
                double cd1 = 0.0;
                double cu2 = 0.0;
                double cd2 = 0.0;
                double cu = 0.0;
                double cd = 0.0;

                if (_l0Series[0] >= _l1Series[0])
                {
                    cu1 = _l0Series[0] - _l1Series[0];
                    cd1 = 0.0;
                }
                else
                {
                    cd1 = _l1Series[0] - _l0Series[0];
                    cu1 = 0.0;
                }

                if (_l1Series[0] >= _l2Series[0])
                {
                    cu2 = cu1 + _l1Series[0] - _l2Series[0];
                    cd2 = cd1;
                }
                else
                {
                    cd2 = cd1 + _l2Series[0] - _l1Series[0];
                    cu2 = cu1;
                }

                if (_l2Series[0] >= _l3Series[0])
                {
                    cu = cu2 + _l2Series[0] - _l3Series[0];
                    cd = cd2;
                }
                else
                {
                    cu = cu2;
                    cd = cd2 + _l3Series[0] - _l2Series[0];
                }

                LRSI[0] = 100.0 * ((cu + cd) != 0.0 ? (cu / (cu + cd)) : 0.0);
            }

            // Alerts
            if (EnableAlerts && (State == State.Realtime) && IsFirstTickOfBar)
            {
                if (!string.IsNullOrWhiteSpace(EnterOverboughtAlert) && (LRSI[2] < OverboughtLevel) && (LRSI[1] >= OverboughtLevel))
                {
                    // Enter Overbought alert.
                    string audioFile = ResolveAlertFilePath(EnterOverboughtAlert, AlertSoundsPath);
                    Alert("EnterOverboughtAlert", Priority.High, "Laguerre RSI entered overbought region", audioFile, 10, Brushes.Black, OverboughtRegionBrush);
                }
                if (!string.IsNullOrWhiteSpace(ExitOverboughtAlert) && (LRSI[2] >= OverboughtLevel) && (LRSI[1] < OverboughtLevel))
                {
                    // Exit Overbought alert.
                    string audioFile = ResolveAlertFilePath(ExitOverboughtAlert, AlertSoundsPath);
                    Alert("ExitOverboughtAlert", Priority.High, "Laguerre RSI exited overbought region", audioFile, 10, Brushes.Black, OverboughtRegionBrush);
                }
                if (!string.IsNullOrWhiteSpace(EnterOversoldAlert) && (LRSI[2] > OversoldLevel) && (LRSI[1] <= OversoldLevel))
                {
                    // Enter Oversold alert.
                    string audioFile = ResolveAlertFilePath(EnterOversoldAlert, AlertSoundsPath);
                    Alert("EnterOversoldAlert", Priority.High, "Laguerre RSI entered oversold region", audioFile, 10, Brushes.Black, OversoldRegionBrush);
                }
                if (!string.IsNullOrWhiteSpace(ExitOversoldAlert) && (LRSI[2] <= OversoldLevel) && (LRSI[1] > OversoldLevel))
                {
                    // Exit Oversold alert.
                    string audioFile = ResolveAlertFilePath(ExitOversoldAlert, AlertSoundsPath);
                    Alert("ExitOversoldAlert", Priority.High, "Laguerre RSI exited oversold region", audioFile, 10, Brushes.Black, OversoldRegionBrush);
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
        [Display(Name="UseFractalEnergy", Description="Toggles the use of the Fractal Energy calculation.", Order=1, GroupName="Parameters")]
        public bool UseFractalEnergy
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name="Alpha", Order=1, GroupName= "Laguerre RSI")]
        public double Alpha
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="NFE", Description="Number of bars used in Fractal Energy calculations.", Order=1, GroupName= "Laguerre RSI with Fractal Energy")]
        public int NFE
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="GLength", Description="Period length for Go/Gh/Gl/Gc filter.", Order=2, GroupName= "Laguerre RSI with Fractal Energy")]
        public int GLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="BetaDev", Description="Controls reactivity in alpha/beta computations.", Order=3, GroupName= "Laguerre RSI with Fractal Energy")]
        public int BetaDev
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name="OverboughtLevel", Order=1, GroupName= "Thresholds")]
        public double OverboughtLevel
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name="OversoldLevel", Order=2, GroupName= "Thresholds")]
        public double OversoldLevel
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="OverboughtRegionBrush", Order=3, GroupName= "Thresholds")]
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
        [Display(Name="OversoldRegionBrush", Order=4, GroupName= "Thresholds")]
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
        [Display(Name = "Enable Alerts", Description = "Trigger alerts for confirmed signals.", Order = 1, GroupName = "Alerts")]
        public bool EnableAlerts
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Alert Sounds Path", Description = "Location of alert audio files used for confirmed signals.", Order = 2, GroupName = "Alerts")]
        public string AlertSoundsPath
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enter Overbought", Description = "Alert sound used when Laguerre RSI enters the overbought region.", Order = 3, GroupName = "Alerts")]
        public string EnterOverboughtAlert
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Exit Overbought", Description = "Alert sound used when Laguerre RSI exits the overbought region.", Order = 4, GroupName = "Alerts")]
        public string ExitOverboughtAlert
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enter Oversold", Description = "Alert sound used when Laguerre RSI enters the oversold region.", Order = 5, GroupName = "Alerts")]
        public string EnterOversoldAlert
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Exit Oversold", Description = "Alert sound used when Laguerre RSI exits the oversold region.", Order = 6, GroupName = "Alerts")]
        public string ExitOversoldAlert
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> LRSI
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
		private LunarTick.LaguerreRSI[] cacheLaguerreRSI;
		public LunarTick.LaguerreRSI LaguerreRSI(bool useFractalEnergy, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableAlerts, string alertSoundsPath, string enterOverboughtAlert, string exitOverboughtAlert, string enterOversoldAlert, string exitOversoldAlert)
		{
			return LaguerreRSI(Input, useFractalEnergy, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableAlerts, alertSoundsPath, enterOverboughtAlert, exitOverboughtAlert, enterOversoldAlert, exitOversoldAlert);
		}

		public LunarTick.LaguerreRSI LaguerreRSI(ISeries<double> input, bool useFractalEnergy, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableAlerts, string alertSoundsPath, string enterOverboughtAlert, string exitOverboughtAlert, string enterOversoldAlert, string exitOversoldAlert)
		{
			if (cacheLaguerreRSI != null)
				for (int idx = 0; idx < cacheLaguerreRSI.Length; idx++)
					if (cacheLaguerreRSI[idx] != null && cacheLaguerreRSI[idx].UseFractalEnergy == useFractalEnergy && cacheLaguerreRSI[idx].Alpha == alpha && cacheLaguerreRSI[idx].NFE == nFE && cacheLaguerreRSI[idx].GLength == gLength && cacheLaguerreRSI[idx].BetaDev == betaDev && cacheLaguerreRSI[idx].OverboughtLevel == overboughtLevel && cacheLaguerreRSI[idx].OversoldLevel == oversoldLevel && cacheLaguerreRSI[idx].OverboughtRegionBrush == overboughtRegionBrush && cacheLaguerreRSI[idx].OversoldRegionBrush == oversoldRegionBrush && cacheLaguerreRSI[idx].RegionOpacity == regionOpacity && cacheLaguerreRSI[idx].EnableAlerts == enableAlerts && cacheLaguerreRSI[idx].AlertSoundsPath == alertSoundsPath && cacheLaguerreRSI[idx].EnterOverboughtAlert == enterOverboughtAlert && cacheLaguerreRSI[idx].ExitOverboughtAlert == exitOverboughtAlert && cacheLaguerreRSI[idx].EnterOversoldAlert == enterOversoldAlert && cacheLaguerreRSI[idx].ExitOversoldAlert == exitOversoldAlert && cacheLaguerreRSI[idx].EqualsInput(input))
						return cacheLaguerreRSI[idx];
			return CacheIndicator<LunarTick.LaguerreRSI>(new LunarTick.LaguerreRSI(){ UseFractalEnergy = useFractalEnergy, Alpha = alpha, NFE = nFE, GLength = gLength, BetaDev = betaDev, OverboughtLevel = overboughtLevel, OversoldLevel = oversoldLevel, OverboughtRegionBrush = overboughtRegionBrush, OversoldRegionBrush = oversoldRegionBrush, RegionOpacity = regionOpacity, EnableAlerts = enableAlerts, AlertSoundsPath = alertSoundsPath, EnterOverboughtAlert = enterOverboughtAlert, ExitOverboughtAlert = exitOverboughtAlert, EnterOversoldAlert = enterOversoldAlert, ExitOversoldAlert = exitOversoldAlert }, input, ref cacheLaguerreRSI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.LaguerreRSI LaguerreRSI(bool useFractalEnergy, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableAlerts, string alertSoundsPath, string enterOverboughtAlert, string exitOverboughtAlert, string enterOversoldAlert, string exitOversoldAlert)
		{
			return indicator.LaguerreRSI(Input, useFractalEnergy, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableAlerts, alertSoundsPath, enterOverboughtAlert, exitOverboughtAlert, enterOversoldAlert, exitOversoldAlert);
		}

		public Indicators.LunarTick.LaguerreRSI LaguerreRSI(ISeries<double> input , bool useFractalEnergy, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableAlerts, string alertSoundsPath, string enterOverboughtAlert, string exitOverboughtAlert, string enterOversoldAlert, string exitOversoldAlert)
		{
			return indicator.LaguerreRSI(input, useFractalEnergy, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableAlerts, alertSoundsPath, enterOverboughtAlert, exitOverboughtAlert, enterOversoldAlert, exitOversoldAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.LaguerreRSI LaguerreRSI(bool useFractalEnergy, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableAlerts, string alertSoundsPath, string enterOverboughtAlert, string exitOverboughtAlert, string enterOversoldAlert, string exitOversoldAlert)
		{
			return indicator.LaguerreRSI(Input, useFractalEnergy, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableAlerts, alertSoundsPath, enterOverboughtAlert, exitOverboughtAlert, enterOversoldAlert, exitOversoldAlert);
		}

		public Indicators.LunarTick.LaguerreRSI LaguerreRSI(ISeries<double> input , bool useFractalEnergy, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, Brush overboughtRegionBrush, Brush oversoldRegionBrush, int regionOpacity, bool enableAlerts, string alertSoundsPath, string enterOverboughtAlert, string exitOverboughtAlert, string enterOversoldAlert, string exitOversoldAlert)
		{
			return indicator.LaguerreRSI(input, useFractalEnergy, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, overboughtRegionBrush, oversoldRegionBrush, regionOpacity, enableAlerts, alertSoundsPath, enterOverboughtAlert, exitOverboughtAlert, enterOversoldAlert, exitOversoldAlert);
		}
	}
}

#endregion

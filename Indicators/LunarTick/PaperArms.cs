#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.LunarTick.PaperFeetEnums;
using NinjaTrader.NinjaScript.Indicators.LunarTick.PaperArmsEnums;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    namespace PaperArmsEnums
    {
        public enum SignalPositionEnum
        {
            RegularCandles,
            HeikenAshiCandles
        }
    }

    [Gui.CategoryOrder("MACloud - Parameters", 1)]
    [Gui.CategoryOrder("MACloud - Display", 2)]
    [Gui.CategoryOrder("LRSI - Parameters", 3)]
    [Gui.CategoryOrder("LRSI - Laguerre RSI", 4)]
    [Gui.CategoryOrder("LRSI - Laguerre RSI with Fractal Energy", 5)]
    [Gui.CategoryOrder("LRSI - Thresholds", 6)]
    [Gui.CategoryOrder("Signals", 7)]
    [Gui.CategoryOrder("Alerts", 8)]
    public class PaperArms : Indicator
	{
        #region Members
        private HeikenAshi8 _ha;
        private MACloud _maCloud;
        private PaperFeet _paperFeet;
        private bool _isRsiInitialized;
        private Series<bool> _longEntrySignal;
        private Series<bool> _longReentrySignal;
        private Series<bool> _shortEntrySignal;
        private Series<bool> _shortReentrySignal;
        private Series<bool> _longExitSignal;
        private Series<bool> _shortExitSignal;
        #endregion

        #region Indicator methods
        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Overlay indicator showing signals based on Heiken Ashi and Laguerre RSI.";
				Name										= "PaperArms";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
                ShowTransparentPlotsInDataBox               = true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
                MAType                                      = NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum.EMA;
                FastPeriod                                  = 9;
                SlowPeriod                                  = 21;
                BullishCloudBrush                           = Brushes.Green;
                BearishCloudBrush                           = Brushes.Red;
                CloudOpacity                                = 40;
                LRSIType                                    = LRSITypeEnum.LaguerreRSIWithFractalEnergy;
                Alpha                                       = 0.2;
                NFE                                         = 8;
                GLength                                     = 13;
                BetaDev                                     = 8;
                OverboughtLevel                             = 80;
                OversoldLevel                               = 20;
                EnableLongEntrySignals                      = true;
                EnableLongReentrySignals                    = true;
                EnableLongExitSignals                       = true;
                EnableShortEntrySignals                     = true;
                EnableShortReentrySignals                   = true;
                EnableShortExitSignals                      = true;
                SignalPosition                              = SignalPositionEnum.HeikenAshiCandles;
                EntrySignalOffset                           = 10;
                ExitSignalOffset                            = 5;
                LongSignalBrush                             = Brushes.Green;
                ShortSignalBrush                            = Brushes.Red;
                SignalPrefix                                = "PaperArms_";
                EnableAlerts                                = false;
                AlertSoundsPath                             = DefaultAlertFilePath();
                LongEntryAlert                              = "LongEntry.wav";
                LongReentryAlert                            = "LongEntry.wav";
                LongExitAlert                               = "ExitLong.wav";
                ShortEntryAlert                             = "ShortEntry.wav";
                ShortReentryAlert                           = "ShortEntry.wav";
                ShortExitAlert                              = "ExitShort.wav";
                AddPlot(Brushes.Transparent, "LRSI");
                AddPlot(Brushes.Transparent, "FastMA");
                AddPlot(Brushes.Transparent, "SlowMA");
                AddPlot(Brushes.Transparent, "EntrySignals");
                AddPlot(Brushes.Transparent, "ReentrySignals");
                AddPlot(Brushes.Transparent, "ExitSignals");
                _isRsiInitialized = false;
            }
            else if (State == State.Configure)
			{
                // Disable IsSuspendedWhileInactive if alerts are enabled.
                IsSuspendedWhileInactive = !EnableAlerts;

                _isRsiInitialized = false;
                _ha = HeikenAshi8(Brushes.Transparent, Brushes.Transparent, Brushes.Transparent);
                _maCloud = MACloud(MAType, FastPeriod, SlowPeriod, BullishCloudBrush, BearishCloudBrush, CloudOpacity, false, 0, string.Empty, false, string.Empty, string.Empty, string.Empty);
                _paperFeet = PaperFeet(LRSIType, Alpha, NFE, GLength, BetaDev, OverboughtLevel, OversoldLevel, Brushes.Transparent, Brushes.Transparent, 0, true, true, Brushes.Transparent, Brushes.Transparent, 0, false, string.Empty, string.Empty, string.Empty);

                _longEntrySignal = new Series<bool>(this);
                _longReentrySignal = new Series<bool>(this);
                _shortEntrySignal = new Series<bool>(this);
                _shortReentrySignal = new Series<bool>(this);
                _longExitSignal = new Series<bool>(this);
                _shortExitSignal = new Series<bool>(this);
            }
        }

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                if (LRSIType == LRSITypeEnum.LaguerreRSI)
                    return Name + "(" + MAType + "," + FastPeriod + "," + SlowPeriod + "," + LRSIType + "," + Alpha + ")";
                else
                    return Name + "(" + MAType + "," + FastPeriod + "," + SlowPeriod + "," + LRSIType + "," + NFE + "," + GLength + "," + BetaDev + ")";
            }
        }

        protected override void OnBarUpdate()
		{
            _ha.Update();
            _maCloud.Update();
            _paperFeet.Update();

            LRSI[0] = _paperFeet.LRSI[0];
            FastMA[0] = _maCloud.FastMA[0];
            SlowMA[0] = _maCloud.SlowMA[0];

            if (CurrentBar < 1)
                return;

            if (!_isRsiInitialized && _paperFeet.LRSI[1] != 0.0)
                _isRsiInitialized = true;

            if (!_isRsiInitialized)
                return;

            if (CurrentBar >= Math.Max(FastPeriod, SlowPeriod))
                _maCloud.DrawCloud(this, SignalPrefix);

            // Signals
            int trend0 = _ha.HAClose[0] - _ha.HAOpen[0] >= 0 ? 1 : -1;
            int trend1 = _ha.HAClose[1] - _ha.HAOpen[1] >= 0 ? 1 : -1;
            int trend2 = _ha.HAClose[2] - _ha.HAOpen[2] >= 0 ? 1 : -1;
            string longEntrySignalTag = string.Format("{0}LongEntry{1}", SignalPrefix, CurrentBar);
            string shortEntrySignalTag = string.Format("{0}ShortEntry{1}", SignalPrefix, CurrentBar);
            string longExitSignalTag = string.Format("{0}LongExit{1}", SignalPrefix, CurrentBar);
            string shortExitSignalTag = string.Format("{0}ShortExit{1}", SignalPrefix, CurrentBar);

            // Initial entry signals (Laguerre RSI leaving overbought/oversold).
            LongEntrySignal[0] = EnableLongEntrySignals && _paperFeet.LongEntrySignal[0];
            ShortEntrySignal[0] = EnableShortEntrySignals && _paperFeet.ShortEntrySignal[0];
            // Re-entry signals (returning to green/red trend dot also with Laguerre RSI above/below 50).
            LongReentrySignal[0] = EnableLongReentrySignals && (_paperFeet.LRSI[0] > 50) && (trend0 == 1) && (trend1 == 1) && (trend1 != trend2);
            ShortReentrySignal[0] = EnableShortReentrySignals && (_paperFeet.LRSI[0] < 50) && (trend0 == -1) && (trend1 == -1) && (trend1 != trend2);

            if (LongEntrySignal[0] || LongReentrySignal[0])
            {
                Draw.ArrowUp(this, longEntrySignalTag, false, 0, (SignalPosition == SignalPositionEnum.HeikenAshiCandles ? _ha.HALow[0] : Low[0]) - (EntrySignalOffset * TickSize), LongSignalBrush);
            }
            else
            {
                RemoveDrawObject(longEntrySignalTag);
            }

            if (ShortEntrySignal[0] || ShortReentrySignal[0])
            {
                Draw.ArrowDown(this, shortEntrySignalTag, false, 0, (SignalPosition == SignalPositionEnum.HeikenAshiCandles ? _ha.HAHigh[0] : High[0]) + (EntrySignalOffset * TickSize), ShortSignalBrush);
            }
            else
            {
                RemoveDrawObject(shortEntrySignalTag);
            }


            // Exit signal (opposite color trend dot).
            bool exitLongOppositeFlatTop = (_maCloud.FastMA[0] > _maCloud.SlowMA[0]) && (_ha.HAClose[0] < _ha.HAOpen[0]) && (_ha.HAHigh[0] == _ha.HAOpen[0]);
            bool exitShortOppositeFlatBottom = (_maCloud.FastMA[0] < _maCloud.SlowMA[0]) && (_ha.HAClose[0] > _ha.HAOpen[0]) && (_ha.HALow[0] == _ha.HAOpen[0]);
            // Exit signal (opposite color HA candle with flat top/bottom).
            bool exitLongOppositeTrendDot = (_maCloud.FastMA[0] > _maCloud.SlowMA[0]) && (trend0 == -1) && (trend1 == -1) && (trend1 != trend2);
            bool exitShortOppositeTrendDot = (_maCloud.FastMA[0] < _maCloud.SlowMA[0]) && (trend0 == 1) && (trend1 == 1) && (trend1 != trend2);

            LongExitSignal[0] = EnableLongExitSignals && (exitLongOppositeFlatTop || exitLongOppositeTrendDot);
            ShortExitSignal[0] = EnableShortExitSignals && (exitShortOppositeFlatBottom || exitShortOppositeTrendDot);

            if (LongExitSignal[0])
            {
                Draw.Diamond(this, longExitSignalTag, false, 0, (SignalPosition == SignalPositionEnum.HeikenAshiCandles ? _ha.HAHigh[0] : High[0]) + (ExitSignalOffset * TickSize), LongSignalBrush);
            }
            else
            {
                RemoveDrawObject(longExitSignalTag);
            }

            if (ShortExitSignal[0])
            {
                Draw.Diamond(this, shortExitSignalTag, false, 0, (SignalPosition == SignalPositionEnum.HeikenAshiCandles ? _ha.HALow[0] : Low[0]) - (ExitSignalOffset * TickSize), ShortSignalBrush);
            }
            else
            {
                RemoveDrawObject(shortExitSignalTag);
            }

            // Signal plots (for Strategy Builder)
            if (LongEntrySignal[0])
                EntrySignals[0] = 1;
            else if (ShortEntrySignal[0])
                EntrySignals[0] = -1;
            else
                EntrySignals[0] = 0;

            if (LongReentrySignal[0])
                ReentrySignals[0] = 1;
            else if (ShortReentrySignal[0])
                ReentrySignals[0] = -1;
            else
                ReentrySignals[0] = 0;

            if (LongExitSignal[0])
                ExitSignals[0] = 1;
            else if (ShortExitSignal[0])
                ExitSignals[0] = -1;
            else
                ExitSignals[0] = 0;


            // Alerts
            if (EnableAlerts && (State == State.Realtime) && IsFirstTickOfBar)
            {
                if (EnableLongEntrySignals && !string.IsNullOrWhiteSpace(LongEntryAlert) && LongEntrySignal[1])
                {
                    string audioFile = ResolveAlertFilePath(LongEntryAlert, AlertSoundsPath);
                    Alert("LongEntryAlert", Priority.High, string.Format("Enter LONG: Laguerre RSI leaving oversold region {0:0.#}", LRSI[1]), audioFile, 10, Brushes.Black, LongSignalBrush);
                }
                if (EnableLongReentrySignals && !string.IsNullOrWhiteSpace(LongReentryAlert) && LongReentrySignal[1])
                {
                    string audioFile = ResolveAlertFilePath(LongReentryAlert, AlertSoundsPath);
                    Alert("LongReentryAlert", Priority.High, "Re-enter LONG: Trend returning to bullish", audioFile, 10, Brushes.Black, LongSignalBrush);
                }
                if (EnableLongExitSignals && !string.IsNullOrWhiteSpace(LongExitAlert) && LongExitSignal[1])
                {
                    string audioFile = ResolveAlertFilePath(LongExitAlert, AlertSoundsPath);
                    Alert("LongExitAlert", Priority.High, "Exit LONG: Possible trend change", audioFile, 10, Brushes.Black, LongSignalBrush);
                }
                if (EnableShortEntrySignals && !string.IsNullOrWhiteSpace(ShortEntryAlert) && ShortEntrySignal[1])
                {
                    string audioFile = ResolveAlertFilePath(ShortEntryAlert, AlertSoundsPath);
                    Alert("ShortEntryAlert", Priority.High, string.Format("Enter SHORT: Laguerre RSI leaving overbought region {0:0.#}", LRSI[1]), audioFile, 10, Brushes.Black, ShortSignalBrush);
                }
                if (EnableShortReentrySignals && !string.IsNullOrWhiteSpace(ShortReentryAlert) && ShortReentrySignal[1])
                {
                    string audioFile = ResolveAlertFilePath(ShortReentryAlert, AlertSoundsPath);
                    Alert("ShortReentryAlert", Priority.High, "Re-enter SHORT: Trend returning to bearish", audioFile, 10, Brushes.Black, ShortSignalBrush);
                }
                if (EnableShortExitSignals && !string.IsNullOrWhiteSpace(ShortExitAlert) && ShortExitSignal[1])
                {
                    string audioFile = ResolveAlertFilePath(ShortExitAlert, AlertSoundsPath);
                    Alert("ShortExitAlert", Priority.High, "Exit SHORT: Possible trend change", audioFile, 10, Brushes.Black, ShortSignalBrush);
                }
            }
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "MA Type", Description = "The type of Moving Average.", Order = 1, GroupName = "MACloud - Parameters")]
        public NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum MAType
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "FastPeriod", Description = "The period of the fast Moving Average.", Order = 2, GroupName = "MACloud - Parameters")]
        public int FastPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SlowPeriod", Description = "The period of the slow Moving Average.", Order = 3, GroupName = "MACloud - Parameters")]
        public int SlowPeriod
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish Cloud Color", Description = "Color used for the bullish cloud.", Order = 1, GroupName = "MACloud - Display")]
        public Brush BullishCloudBrush
        { get; set; }

        [Browsable(false)]
        public string BullishCloudBrushSerializable
        {
            get { return Serialize.BrushToString(BullishCloudBrush); }
            set { BullishCloudBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish Cloud Color", Description = "Color used for the bearish cloud.", Order = 2, GroupName = "MACloud - Display")]
        public Brush BearishCloudBrush
        { get; set; }

        [Browsable(false)]
        public string BearishCloudBrushSerializable
        {
            get { return Serialize.BrushToString(BearishCloudBrush); }
            set { BearishCloudBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Range(0, 100)]
        [Display(Name = "Cloud Opacity", Description = "The opacity of the cloud (0 = completely transparent, 100 = no opacity).", Order = 3, GroupName = "MACloud - Display")]
        public int CloudOpacity
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Laguerre RSI Type", Description = "Specifies the type of Laguerre RSI.", Order = 1, GroupName = "LRSI - Parameters")]
        public NinjaTrader.NinjaScript.Indicators.LunarTick.PaperFeetEnums.LRSITypeEnum LRSIType
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Alpha", Order = 1, GroupName = "LRSI - Laguerre RSI")]
        public double Alpha
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "NFE", Description = "Number of bars used in Fractal Energy calculations.", Order = 1, GroupName = "LRSI - Laguerre RSI with Fractal Energy")]
        public int NFE
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "GLength", Description = "Period length for Go/Gh/Gl/Gc filter.", Order = 2, GroupName = "LRSI - Laguerre RSI with Fractal Energy")]
        public int GLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BetaDev", Description = "Controls reactivity in alpha/beta computations.", Order = 3, GroupName = "LRSI - Laguerre RSI with Fractal Energy")]
        public int BetaDev
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "OverboughtLevel", Order = 1, GroupName = "LRSI - Thresholds")]
        public double OverboughtLevel
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "OversoldLevel", Order = 2, GroupName = "LRSI - Thresholds")]
        public double OversoldLevel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Long Entry Signals", Description = "Display LONG entry signals.", Order = 1, GroupName = "Signals")]
        public bool EnableLongEntrySignals
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Long Re-entry Signals", Description = "Display LONG re-entry signals.", Order = 2, GroupName = "Signals")]
        public bool EnableLongReentrySignals
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Long Exit Signals", Description = "Display LONG exit signals.", Order = 3, GroupName = "Signals")]
        public bool EnableLongExitSignals
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Short Entry Signals", Description = "Display SHORT entry signals.", Order = 4, GroupName = "Signals")]
        public bool EnableShortEntrySignals
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Short Re-entry Signals", Description = "Display SHORT re-entry signals.", Order = 5, GroupName = "Signals")]
        public bool EnableShortReentrySignals
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Short Exit Signals", Description = "Display SHORT exit signals.", Order = 6, GroupName = "Signals")]
        public bool EnableShortExitSignals
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Signal Position", Description = "Display signal relative to the specified bar type.", Order = 7, GroupName = "Signals")]
        public NinjaTrader.NinjaScript.Indicators.LunarTick.PaperArmsEnums.SignalPositionEnum SignalPosition
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Entry Signal Offset", Description = "The vertical offset between entry signal and bar.", Order = 8, GroupName = "Signals")]
        public int EntrySignalOffset
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Exit Signal Offset", Description = "The vertical offset between exit signal and bar.", Order = 9, GroupName = "Signals")]
        public int ExitSignalOffset
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Long Signal Color", Description = "Color used for LONG signals.", Order = 10, GroupName = "Signals")]
        public Brush LongSignalBrush
        { get; set; }

        [Browsable(false)]
        public string LongSignalBrushSerializable
        {
            get { return Serialize.BrushToString(LongSignalBrush); }
            set { LongSignalBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Short Signal Color", Description = "Color used for SHORT signals.", Order = 11, GroupName = "Signals")]
        public Brush ShortSignalBrush
        { get; set; }

        [Browsable(false)]
        public string ShortSignalBrushSerializable
        {
            get { return Serialize.BrushToString(ShortSignalBrush); }
            set { ShortSignalBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "SignalPrefix", Description = "Prefix used when naming signal drawing objects.", Order = 12, GroupName = "Signals")]
        public string SignalPrefix
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
        [Display(Name = "Long Re-entry Alert", Description = "Alert sound used for confirmed LONG re-entry signals.", Order = 4, GroupName = "Alerts")]
        public string LongReentryAlert
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Long Exit Alert", Description = "Alert sound used for confirmed LONG exit signals.", Order = 5, GroupName = "Alerts")]
        public string LongExitAlert
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Short Entry Alert", Description = "Alert sound used for confirmed SHORT entry signals.", Order = 6, GroupName = "Alerts")]
        public string ShortEntryAlert
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Short Re-entry Alert", Description = "Alert sound used for confirmed SHORT re-entry signals.", Order = 7, GroupName = "Alerts")]
        public string ShortReentryAlert
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Short Exit Alert", Description = "Alert sound used for confirmed SHORT exit signals.", Order = 8, GroupName = "Alerts")]
        public string ShortExitAlert
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> LRSI
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> FastMA
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SlowMA
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> EntrySignals
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ReentrySignals
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ExitSignals
        {
            get { return Values[5]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> LongEntrySignal
        {
            get { return _longEntrySignal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> LongReentrySignal
        {
            get { return _longReentrySignal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> ShortEntrySignal
        {
            get { return _shortEntrySignal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> ShortReentrySignal
        {
            get { return _shortReentrySignal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> LongExitSignal
        {
            get { return _longExitSignal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> ShortExitSignal
        {
            get { return _shortExitSignal; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LunarTick.PaperArms[] cachePaperArms;
		public LunarTick.PaperArms PaperArms(NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, Brush bullishCloudBrush, Brush bearishCloudBrush, int cloudOpacity, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, bool enableLongEntrySignals, bool enableLongReentrySignals, bool enableLongExitSignals, bool enableShortEntrySignals, bool enableShortReentrySignals, bool enableShortExitSignals, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperArmsEnums.SignalPositionEnum signalPosition, int entrySignalOffset, int exitSignalOffset, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string longReentryAlert, string longExitAlert, string shortEntryAlert, string shortReentryAlert, string shortExitAlert)
		{
			return PaperArms(Input, mAType, fastPeriod, slowPeriod, bullishCloudBrush, bearishCloudBrush, cloudOpacity, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, enableLongEntrySignals, enableLongReentrySignals, enableLongExitSignals, enableShortEntrySignals, enableShortReentrySignals, enableShortExitSignals, signalPosition, entrySignalOffset, exitSignalOffset, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, longReentryAlert, longExitAlert, shortEntryAlert, shortReentryAlert, shortExitAlert);
		}

		public LunarTick.PaperArms PaperArms(ISeries<double> input, NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, Brush bullishCloudBrush, Brush bearishCloudBrush, int cloudOpacity, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, bool enableLongEntrySignals, bool enableLongReentrySignals, bool enableLongExitSignals, bool enableShortEntrySignals, bool enableShortReentrySignals, bool enableShortExitSignals, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperArmsEnums.SignalPositionEnum signalPosition, int entrySignalOffset, int exitSignalOffset, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string longReentryAlert, string longExitAlert, string shortEntryAlert, string shortReentryAlert, string shortExitAlert)
		{
			if (cachePaperArms != null)
				for (int idx = 0; idx < cachePaperArms.Length; idx++)
					if (cachePaperArms[idx] != null && cachePaperArms[idx].MAType == mAType && cachePaperArms[idx].FastPeriod == fastPeriod && cachePaperArms[idx].SlowPeriod == slowPeriod && cachePaperArms[idx].BullishCloudBrush == bullishCloudBrush && cachePaperArms[idx].BearishCloudBrush == bearishCloudBrush && cachePaperArms[idx].CloudOpacity == cloudOpacity && cachePaperArms[idx].LRSIType == lRSIType && cachePaperArms[idx].Alpha == alpha && cachePaperArms[idx].NFE == nFE && cachePaperArms[idx].GLength == gLength && cachePaperArms[idx].BetaDev == betaDev && cachePaperArms[idx].OverboughtLevel == overboughtLevel && cachePaperArms[idx].OversoldLevel == oversoldLevel && cachePaperArms[idx].EnableLongEntrySignals == enableLongEntrySignals && cachePaperArms[idx].EnableLongReentrySignals == enableLongReentrySignals && cachePaperArms[idx].EnableLongExitSignals == enableLongExitSignals && cachePaperArms[idx].EnableShortEntrySignals == enableShortEntrySignals && cachePaperArms[idx].EnableShortReentrySignals == enableShortReentrySignals && cachePaperArms[idx].EnableShortExitSignals == enableShortExitSignals && cachePaperArms[idx].SignalPosition == signalPosition && cachePaperArms[idx].EntrySignalOffset == entrySignalOffset && cachePaperArms[idx].ExitSignalOffset == exitSignalOffset && cachePaperArms[idx].LongSignalBrush == longSignalBrush && cachePaperArms[idx].ShortSignalBrush == shortSignalBrush && cachePaperArms[idx].SignalPrefix == signalPrefix && cachePaperArms[idx].EnableAlerts == enableAlerts && cachePaperArms[idx].AlertSoundsPath == alertSoundsPath && cachePaperArms[idx].LongEntryAlert == longEntryAlert && cachePaperArms[idx].LongReentryAlert == longReentryAlert && cachePaperArms[idx].LongExitAlert == longExitAlert && cachePaperArms[idx].ShortEntryAlert == shortEntryAlert && cachePaperArms[idx].ShortReentryAlert == shortReentryAlert && cachePaperArms[idx].ShortExitAlert == shortExitAlert && cachePaperArms[idx].EqualsInput(input))
						return cachePaperArms[idx];
			return CacheIndicator<LunarTick.PaperArms>(new LunarTick.PaperArms(){ MAType = mAType, FastPeriod = fastPeriod, SlowPeriod = slowPeriod, BullishCloudBrush = bullishCloudBrush, BearishCloudBrush = bearishCloudBrush, CloudOpacity = cloudOpacity, LRSIType = lRSIType, Alpha = alpha, NFE = nFE, GLength = gLength, BetaDev = betaDev, OverboughtLevel = overboughtLevel, OversoldLevel = oversoldLevel, EnableLongEntrySignals = enableLongEntrySignals, EnableLongReentrySignals = enableLongReentrySignals, EnableLongExitSignals = enableLongExitSignals, EnableShortEntrySignals = enableShortEntrySignals, EnableShortReentrySignals = enableShortReentrySignals, EnableShortExitSignals = enableShortExitSignals, SignalPosition = signalPosition, EntrySignalOffset = entrySignalOffset, ExitSignalOffset = exitSignalOffset, LongSignalBrush = longSignalBrush, ShortSignalBrush = shortSignalBrush, SignalPrefix = signalPrefix, EnableAlerts = enableAlerts, AlertSoundsPath = alertSoundsPath, LongEntryAlert = longEntryAlert, LongReentryAlert = longReentryAlert, LongExitAlert = longExitAlert, ShortEntryAlert = shortEntryAlert, ShortReentryAlert = shortReentryAlert, ShortExitAlert = shortExitAlert }, input, ref cachePaperArms);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.PaperArms PaperArms(NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, Brush bullishCloudBrush, Brush bearishCloudBrush, int cloudOpacity, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, bool enableLongEntrySignals, bool enableLongReentrySignals, bool enableLongExitSignals, bool enableShortEntrySignals, bool enableShortReentrySignals, bool enableShortExitSignals, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperArmsEnums.SignalPositionEnum signalPosition, int entrySignalOffset, int exitSignalOffset, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string longReentryAlert, string longExitAlert, string shortEntryAlert, string shortReentryAlert, string shortExitAlert)
		{
			return indicator.PaperArms(Input, mAType, fastPeriod, slowPeriod, bullishCloudBrush, bearishCloudBrush, cloudOpacity, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, enableLongEntrySignals, enableLongReentrySignals, enableLongExitSignals, enableShortEntrySignals, enableShortReentrySignals, enableShortExitSignals, signalPosition, entrySignalOffset, exitSignalOffset, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, longReentryAlert, longExitAlert, shortEntryAlert, shortReentryAlert, shortExitAlert);
		}

		public Indicators.LunarTick.PaperArms PaperArms(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, Brush bullishCloudBrush, Brush bearishCloudBrush, int cloudOpacity, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, bool enableLongEntrySignals, bool enableLongReentrySignals, bool enableLongExitSignals, bool enableShortEntrySignals, bool enableShortReentrySignals, bool enableShortExitSignals, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperArmsEnums.SignalPositionEnum signalPosition, int entrySignalOffset, int exitSignalOffset, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string longReentryAlert, string longExitAlert, string shortEntryAlert, string shortReentryAlert, string shortExitAlert)
		{
			return indicator.PaperArms(input, mAType, fastPeriod, slowPeriod, bullishCloudBrush, bearishCloudBrush, cloudOpacity, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, enableLongEntrySignals, enableLongReentrySignals, enableLongExitSignals, enableShortEntrySignals, enableShortReentrySignals, enableShortExitSignals, signalPosition, entrySignalOffset, exitSignalOffset, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, longReentryAlert, longExitAlert, shortEntryAlert, shortReentryAlert, shortExitAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.PaperArms PaperArms(NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, Brush bullishCloudBrush, Brush bearishCloudBrush, int cloudOpacity, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, bool enableLongEntrySignals, bool enableLongReentrySignals, bool enableLongExitSignals, bool enableShortEntrySignals, bool enableShortReentrySignals, bool enableShortExitSignals, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperArmsEnums.SignalPositionEnum signalPosition, int entrySignalOffset, int exitSignalOffset, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string longReentryAlert, string longExitAlert, string shortEntryAlert, string shortReentryAlert, string shortExitAlert)
		{
			return indicator.PaperArms(Input, mAType, fastPeriod, slowPeriod, bullishCloudBrush, bearishCloudBrush, cloudOpacity, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, enableLongEntrySignals, enableLongReentrySignals, enableLongExitSignals, enableShortEntrySignals, enableShortReentrySignals, enableShortExitSignals, signalPosition, entrySignalOffset, exitSignalOffset, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, longReentryAlert, longExitAlert, shortEntryAlert, shortReentryAlert, shortExitAlert);
		}

		public Indicators.LunarTick.PaperArms PaperArms(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.LunarTick.MACloudEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod, Brush bullishCloudBrush, Brush bearishCloudBrush, int cloudOpacity, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperFeetEnums.LRSITypeEnum lRSIType, double alpha, int nFE, int gLength, int betaDev, double overboughtLevel, double oversoldLevel, bool enableLongEntrySignals, bool enableLongReentrySignals, bool enableLongExitSignals, bool enableShortEntrySignals, bool enableShortReentrySignals, bool enableShortExitSignals, NinjaTrader.NinjaScript.Indicators.LunarTick.PaperArmsEnums.SignalPositionEnum signalPosition, int entrySignalOffset, int exitSignalOffset, Brush longSignalBrush, Brush shortSignalBrush, string signalPrefix, bool enableAlerts, string alertSoundsPath, string longEntryAlert, string longReentryAlert, string longExitAlert, string shortEntryAlert, string shortReentryAlert, string shortExitAlert)
		{
			return indicator.PaperArms(input, mAType, fastPeriod, slowPeriod, bullishCloudBrush, bearishCloudBrush, cloudOpacity, lRSIType, alpha, nFE, gLength, betaDev, overboughtLevel, oversoldLevel, enableLongEntrySignals, enableLongReentrySignals, enableLongExitSignals, enableShortEntrySignals, enableShortReentrySignals, enableShortExitSignals, signalPosition, entrySignalOffset, exitSignalOffset, longSignalBrush, shortSignalBrush, signalPrefix, enableAlerts, alertSoundsPath, longEntryAlert, longReentryAlert, longExitAlert, shortEntryAlert, shortReentryAlert, shortExitAlert);
		}
	}
}

#endregion

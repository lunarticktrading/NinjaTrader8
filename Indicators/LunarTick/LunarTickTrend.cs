#region Using declarations
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    namespace LunarTickTrendEnums
    {
        public enum MATypeEnum
        {
            SMA,
            EMA
        }

        public enum TrendType
        {
            Bullish,
            Bearish,
            Retracement
        }
    }

    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Display", 2)]
    [Gui.CategoryOrder("Signals", 3)]
    [Gui.CategoryOrder("Alerts", 4)]
    [Gui.CategoryOrder("Developer", 5)]
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendTypeConverter")]
    public class LunarTickTrend : Indicator
    {
        internal class RegionInfo
        {
            public int StartBarIndex { get; private set; }
            public TrendType TrendType { get; private set; }

            public RegionInfo(int startBarIndex, TrendType trendType)
            {
                StartBarIndex = startBarIndex;
                TrendType = trendType;
            }

            public string GetRegionTag(string prefix)
            {
                return string.Format("{0}{1}Cloud{2}", (string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix), TrendType, StartBarIndex);
            }
        }

        #region Constants

        public const string Version = "1.0.1";
        private const string DrawObjectPrefix = "LunarTickTrend_";

        #endregion

        #region Members

        private SMA _fastSMA;
        private EMA _fastEMA;
        private SMA _slowSMA;
        private EMA _slowEMA;

        private Series<TrendType> _trendType;
        private Series<bool> _longEntrySignal;
        private Series<bool> _shortEntrySignal;
        private List<RegionInfo> _regions = new List<RegionInfo>();

        private Brush _fastBullishBrush;
        private Brush _fastBearishBrush;
        private Brush _fastRetracementBrush;

        private Brush _slowBullishBrush;
        private Brush _slowBearishBrush;
        private Brush _slowRetracementBrush;

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Use Custom Timeframe", Description = "Use a custom timeframe for the Moving Averages.", Order = 1, GroupName = "Parameters")]
        public bool UseCustomTimeframe
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Custom Timeframe", Description = "The custom timeframe in minutes.", Order = 2, GroupName = "Parameters")]
        public int CustomTimeframe
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "MA Type", Description = "The type of Moving Average.", Order = 3, GroupName = "Parameters")]
        public NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum MAType
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Fast Period", Description = "The period of the fast Moving Average.", Order = 4, GroupName = "Parameters")]
        public int FastPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Slow Period", Description = "The period of the slow Moving Average.", Order = 5, GroupName = "Parameters")]
        public int SlowPeriod
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Bullish Cloud Color", Description = "Color used for the bullish cloud.", Order = 1, GroupName = "Display")]
        public Brush BullishCloudBrush
        { get; set; }

        [Browsable(false)]
        public string BullishCloudBrushSerializable
        {
            get { return Serialize.BrushToString(BullishCloudBrush); }
            set { BullishCloudBrush = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bearish Cloud Color", Description = "Color used for the bearish cloud.", Order = 2, GroupName = "Display")]
        public Brush BearishCloudBrush
        { get; set; }

        [Browsable(false)]
        public string BearishCloudBrushSerializable
        {
            get { return Serialize.BrushToString(BearishCloudBrush); }
            set { BearishCloudBrush = Serialize.StringToBrush(value); }
        }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Retracements", Description = "Highlight retracements into the cloud.", Order = 3, GroupName = "Display")]
        public bool ShowRetracements
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Retracement Cloud Color", Description = "Color used for the cloud retracements.", Order = 4, GroupName = "Display")]
        public Brush RetracementCloudBrush
        { get; set; }

        [Browsable(false)]
        public string RetracementCloudBrushSerializable
        {
            get { return Serialize.BrushToString(RetracementCloudBrush); }
            set { RetracementCloudBrush = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Fast Opacity", Description = "The opacity of the fast Moving Average (0 = completely transparent, 100 = no opacity).", Order = 5, GroupName = "Display")]
        public int FastOpacity
        { get; set; }

        [Range(0, 100)]
        [Display(Name = "Slow Opacity", Description = "The opacity of the slow Moving Average (0 = completely transparent, 100 = no opacity).", Order = 6, GroupName = "Display")]
        public int SlowOpacity
        { get; set; }

        [Range(0, 100)]
        [Display(Name = "Cloud Opacity", Description = "The opacity of the cloud (0 = completely transparent, 100 = no opacity).", Order = 7, GroupName = "Display")]
        public int CloudOpacity
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Enable Signals", Description = "Display entry signals on the chart.", Order = 1, GroupName = "Signals")]
        public bool EnableSignals
        { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Signal Offset", Description = "The vertical offset between signal and bar.", Order = 2, GroupName = "Signals")]
        public int SignalOffset
        { get; set; }

        [Display(Name = "Signal Prefix", Description = "Prefix used when naming signal drawing objects.", Order = 3, GroupName = "Signals")]
        public string SignalPrefix
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Enable Alerts", Description = "Trigger alerts for confirmed signals.", Order = 1, GroupName = "Alerts")]
        public bool EnableAlerts
        { get; set; }

        [Display(Name = "Alert Sounds Path", Description = "Location of alert audio files used for confirmed signals.", Order = 2, GroupName = "Alerts")]
        public string AlertSoundsPath
        { get; set; }

        [Display(Name = "Long Entry Alert", Description = "Alert sound used for confirmed LONG entry signals.", Order = 3, GroupName = "Alerts")]
        public string LongEntryAlert
        { get; set; }

        [Display(Name = "Short Entry Alert", Description = "Alert sound used for confirmed SHORT entry signals.", Order = 4, GroupName = "Alerts")]
        public string ShortEntryAlert
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
        public Series<double> FastMA
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SlowMA
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Signals
        {
            get { return Values[2]; }
        }

        #endregion

        #region Indicator methods

        protected override void OnStateChange()
        {
            DebugPrint($"OnStateChange({State})");

            if (State == State.SetDefaults)
            {
                Description = @"Trend cloud based on two fast/slow moving averages.";
                Name = "LunarTick Trend";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                UseCustomTimeframe = false;
                CustomTimeframe = 5;
                MAType = NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum.EMA;
                FastPeriod = 9;
                SlowPeriod = 21;
                BullishCloudBrush = Brushes.Green;
                BearishCloudBrush = Brushes.Red;
                ShowRetracements = false;
                RetracementCloudBrush = Brushes.Yellow;
                FastOpacity = 100;
                SlowOpacity = 0;
                CloudOpacity = 40;
                EnableSignals = false;
                SignalOffset = 10;
                SignalPrefix = "LunarTickTrend_";
                EnableAlerts = false;
                AlertSoundsPath = DefaultAlertFilePath();
                LongEntryAlert = "LongEntry.wav";
                ShortEntryAlert = "ShortEntry.wav";
                VersionInformation = $"{Version} - {Assembly.GetAssembly(typeof(LunarTickTrend)).GetName().Version}";
                Debug = false;

                AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "FastMA");
                AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "SlowMA");
                AddPlot(Brushes.Transparent, "Signals");
            }
            else if (State == State.Configure)
            {
                _fastBullishBrush = BullishCloudBrush.Clone();
                _fastBullishBrush.Opacity = FastOpacity / 100.0;
                _fastBullishBrush.Freeze();
                _fastBearishBrush = BearishCloudBrush.Clone();
                _fastBearishBrush.Opacity = FastOpacity / 100.0;
                _fastBearishBrush.Freeze();
                _fastRetracementBrush = RetracementCloudBrush.Clone();
                _fastRetracementBrush.Opacity = FastOpacity / 100.0;
                _fastRetracementBrush.Freeze();

                _slowBullishBrush = BullishCloudBrush.Clone();
                _slowBullishBrush.Opacity = SlowOpacity / 100.0;
                _slowBullishBrush.Freeze();
                _slowBearishBrush = BearishCloudBrush.Clone();
                _slowBearishBrush.Opacity = SlowOpacity / 100.0;
                _slowBearishBrush.Freeze();
                _slowRetracementBrush = RetracementCloudBrush.Clone();
                _slowRetracementBrush.Opacity = SlowOpacity / 100.0;
                _slowRetracementBrush.Freeze();

                // Disable IsSuspendedWhileInactive if alerts are enabled.
                IsSuspendedWhileInactive = !EnableAlerts;

                SetZOrder(-1); // Draw behind price bars.
                _trendType = new Series<TrendType>(this);
                _longEntrySignal = new Series<bool>(this);
                _shortEntrySignal = new Series<bool>(this);

                if (UseCustomTimeframe)
                    AddDataSeries(Data.BarsPeriodType.Minute, CustomTimeframe);
            }
            else if (State == State.DataLoaded)
            {
                int barsInProgress = UseCustomTimeframe ? 1 : 0;
                _fastSMA = SMA(BarsArray[barsInProgress], FastPeriod);
                _fastEMA = EMA(BarsArray[barsInProgress], FastPeriod);
                _slowSMA = SMA(BarsArray[barsInProgress], SlowPeriod);
                _slowEMA = EMA(BarsArray[barsInProgress], SlowPeriod);
            }
        }

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                return Name + "(" + Instrument.FullName + " (" + (UseCustomTimeframe ? BarsPeriods[1] : BarsPeriods[0]) + ")," + FastPeriod + "," + SlowPeriod + "," + MAType + ")";
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0)
                return;

            _longEntrySignal[0] = false;
            _shortEntrySignal[0] = false;
            Signals[0] = 0;

            if (CurrentBar < Math.Max(FastPeriod, SlowPeriod))
                return;

            double fastMA = (MAType == NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum.EMA) ? (_fastEMA[0]) : (_fastSMA[0]);
            double slowMA = (MAType == NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum.EMA) ? (_slowEMA[0]) : (_slowSMA[0]);

            FastMA[0] = fastMA;
            SlowMA[0] = slowMA;

            _trendType[0] = GetTrendType(0, ShowRetracements);

            PlotBrushes[0][0] = GetFastTrendBrush(_trendType[0]);
            PlotBrushes[1][0] = GetSlowTrendBrush(_trendType[0]);

            if (!IsVisible)
                return;

            DrawCloud(this, DrawObjectPrefix);

            // Signals
            if (EnableSignals)
            {
                var barTime = Time[0];
                string longEntrySignalTag = string.Format("{0}LongEntry{1}", SignalPrefix, CurrentBar);
                string shortEntrySignalTag = string.Format("{0}ShortEntry{1}", SignalPrefix, CurrentBar);

                if (FastMA[0] >= SlowMA[0] && FastMA[1] < SlowMA[1])
                {
                    // LONG entry signal.
                    RemoveDrawObject(shortEntrySignalTag);
                    _longEntrySignal[0] = true;
                    Signals[0] = 1;
                    Draw.ArrowUp(this,
                        longEntrySignalTag,
                        true,
                        barTime,
                        Math.Min(Math.Min(FastMA[0], SlowMA[0]), Low[0]) - (SignalOffset * TickSize),
                        BullishCloudBrush);
                }
                else if (FastMA[0] <= SlowMA[0] && FastMA[1] > SlowMA[1])
                {
                    // SHORT entry signal.
                    RemoveDrawObject(longEntrySignalTag);
                    _shortEntrySignal[0] = true;
                    Signals[0] = -1;
                    Draw.ArrowDown(this,
                        shortEntrySignalTag,
                        true,
                        barTime,
                        Math.Max(Math.Max(FastMA[0], SlowMA[0]), High[0]) + (SignalOffset * TickSize),
                        BearishCloudBrush);
                }
                else
                {
                    // No signal.
                    RemoveDrawObject(longEntrySignalTag);
                    RemoveDrawObject(shortEntrySignalTag);
                }

                // Alerts
                if (EnableAlerts && EnableSignals && (State == State.Realtime) && IsFirstTickOfBar)
                {
                    if (Signals[1] > 0 && !string.IsNullOrWhiteSpace(LongEntryAlert))
                    {
                        // Long entry alert.
                        string audioFile = ResolveAlertFilePath(LongEntryAlert, AlertSoundsPath);
                        Alert("LongEntryAlert", Priority.High, "Long Entry - Bullish cross", audioFile, 10, Brushes.Black, BullishCloudBrush);
                    }
                    if (Signals[1] < 0 && !string.IsNullOrWhiteSpace(ShortEntryAlert))
                    {
                        // Short entry alert.
                        string audioFile = ResolveAlertFilePath(ShortEntryAlert, AlertSoundsPath);
                        Alert("ShortEntryAlert", Priority.High, "Short Entry - Bearish cross", audioFile, 10, Brushes.Black, BearishCloudBrush);
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private TrendType GetTrendType(int barsAgo = 0, bool enableRetracements = false)
        {
            TrendType result = TrendType.Bullish;

            if ((CurrentBar - barsAgo) < 0)
                return result;

            if (FastMA[barsAgo] > SlowMA[barsAgo])
                result = TrendType.Bullish;
            else if (FastMA[barsAgo] < SlowMA[barsAgo])
                result = TrendType.Bearish;
            else
                result = GetTrendType(barsAgo + 1, false);

            if (enableRetracements)
            {
                if (result == TrendType.Bullish && Close[barsAgo] < FastMA[barsAgo])
                    result = TrendType.Retracement;
                else if (result == TrendType.Bearish && Close[barsAgo] > FastMA[barsAgo])
                    result = TrendType.Retracement;
            }

            return result;
        }

        private Brush GetFastTrendBrush(TrendType trendType)
        {
            switch (trendType)
            {
                case TrendType.Bullish:
                    return _fastBullishBrush;
                case TrendType.Bearish:
                    return _fastBearishBrush;
                case TrendType.Retracement:
                    return _fastRetracementBrush;
                default:
                    return Brushes.Transparent;
            }
        }

        private Brush GetSlowTrendBrush(TrendType trendType)
        {
            switch (trendType)
            {
                case TrendType.Bullish:
                    return _slowBullishBrush;
                case TrendType.Bearish:
                    return _slowBearishBrush;
                case TrendType.Retracement:
                    return _slowRetracementBrush;
                default:
                    return Brushes.Transparent;
            }
        }

        private Brush GetTrendBrush(TrendType trendType)
        {
            switch (trendType)
            {
                case TrendType.Bullish:
                    return BullishCloudBrush;
                case TrendType.Bearish:
                    return BearishCloudBrush;
                case TrendType.Retracement:
                    return RetracementCloudBrush;
                default:
                    return Brushes.Transparent;
            }
        }

        private void DrawCloud(NinjaScriptBase owner, string drawingObjectPrefix)
        {
            TrendType currTrend = _trendType[0];
            TrendType prevTrend = _trendType[1];

            // Get last created RegionInfo, or create one if none created yet.
            RegionInfo currRegionInfo = null;
            if (_regions.Count == 0)
                _regions.Add(new RegionInfo(CurrentBar, currTrend));
            currRegionInfo = _regions.Last();

            // If current region was created for the current live bar, and bar has changed trend then delete it.
            if (currRegionInfo.StartBarIndex == CurrentBar && currRegionInfo.TrendType != currTrend)
            {
                RemoveDrawObject(currRegionInfo.GetRegionTag(drawingObjectPrefix));
                _regions.RemoveAt(_regions.Count - 1);
                if (_regions.Count == 0)
                    _regions.Add(new RegionInfo(CurrentBar, currTrend));
                currRegionInfo = _regions.Last();
            }

            // If trend type has changed from current region then add a new region.
            if (currRegionInfo.TrendType != currTrend)
            {
                _regions.Add(new RegionInfo(CurrentBar, currTrend));
                currRegionInfo = _regions.Last();
            }

            // Update/Draw the current region
            if (currRegionInfo != null)
            {
                int startBarsAgo = CurrentBar - currRegionInfo.StartBarIndex;
                Brush areaBrush = GetTrendBrush(currRegionInfo.TrendType);
                Draw.Region(owner, currRegionInfo.GetRegionTag(drawingObjectPrefix), startBarsAgo + 1, 0, FastMA, SlowMA, Brushes.Transparent, areaBrush, CloudOpacity);
            }
        }

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"LunarTickTrend[{Instrument.FullName}]: {msg}");
                else
                    Print($"LunarTickTrend: {msg}");
            }
        }

        #endregion
    }

    public class LunarTickTrendTypeConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            // We need the indicator instance which actually exists on the grid
            LunarTickTrend indicator = component as LunarTickTrend;

            // base.GetProperties ensures we have all the properties (and associated property grid editors)
            // NinjaTrader internal logic determines for a given indicator
            PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context)
                                                                        ? base.GetProperties(context, component, attrs)
                                                                        : TypeDescriptor.GetProperties(component, attrs);

            if (indicator == null || propertyDescriptorCollection == null)
                return propertyDescriptorCollection;


            // Show/hide properties related to UseCustomTimeframe
            PropertyDescriptor customTimeframePropertyDesc = propertyDescriptorCollection["CustomTimeframe"];
            propertyDescriptorCollection.Remove(customTimeframePropertyDesc);
            if (indicator.UseCustomTimeframe)
            {
                propertyDescriptorCollection.Add(customTimeframePropertyDesc);
            }

            // Show/hide properties related to ShowRetracements
            PropertyDescriptor retracementCloudBrushPropertyDesc = propertyDescriptorCollection["RetracementCloudBrush"];
            propertyDescriptorCollection.Remove(retracementCloudBrushPropertyDesc);
            if (indicator.ShowRetracements)
            {
                propertyDescriptorCollection.Add(retracementCloudBrushPropertyDesc);
            }

            // Show/hide properties related to EnableSignals
            PropertyDescriptor signalOffsetPropertyDesc = propertyDescriptorCollection["SignalOffset"];
            PropertyDescriptor signalPrefixPropertyDesc = propertyDescriptorCollection["SignalPrefix"];
            propertyDescriptorCollection.Remove(signalOffsetPropertyDesc);
            propertyDescriptorCollection.Remove(signalPrefixPropertyDesc);
            if (indicator.EnableSignals)
            {
                propertyDescriptorCollection.Add(signalOffsetPropertyDesc);
                propertyDescriptorCollection.Add(signalPrefixPropertyDesc);
            }

            // Show/hide properties related to EnableAlerts
            PropertyDescriptor alertSoundsPathPropertyDesc = propertyDescriptorCollection["AlertSoundsPath"];
            PropertyDescriptor longEntryAlertPropertyDesc = propertyDescriptorCollection["LongEntryAlert"];
            PropertyDescriptor shortEntryAlertPropertyDesc = propertyDescriptorCollection["ShortEntryAlert"];
            propertyDescriptorCollection.Remove(alertSoundsPathPropertyDesc);
            propertyDescriptorCollection.Remove(longEntryAlertPropertyDesc);
            propertyDescriptorCollection.Remove(shortEntryAlertPropertyDesc);
            if (indicator.EnableAlerts)
            {
                propertyDescriptorCollection.Add(alertSoundsPathPropertyDesc);
                propertyDescriptorCollection.Add(longEntryAlertPropertyDesc);
                propertyDescriptorCollection.Add(shortEntryAlertPropertyDesc);
            }

            return propertyDescriptorCollection;
        }

        // Important: This must return true otherwise the type convetor will not be called
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        { return true; }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LunarTick.LunarTickTrend[] cacheLunarTickTrend;
		public LunarTick.LunarTickTrend LunarTickTrend(bool useCustomTimeframe, int customTimeframe, NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod)
		{
			return LunarTickTrend(Input, useCustomTimeframe, customTimeframe, mAType, fastPeriod, slowPeriod);
		}

		public LunarTick.LunarTickTrend LunarTickTrend(ISeries<double> input, bool useCustomTimeframe, int customTimeframe, NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod)
		{
			if (cacheLunarTickTrend != null)
				for (int idx = 0; idx < cacheLunarTickTrend.Length; idx++)
					if (cacheLunarTickTrend[idx] != null && cacheLunarTickTrend[idx].UseCustomTimeframe == useCustomTimeframe && cacheLunarTickTrend[idx].CustomTimeframe == customTimeframe && cacheLunarTickTrend[idx].MAType == mAType && cacheLunarTickTrend[idx].FastPeriod == fastPeriod && cacheLunarTickTrend[idx].SlowPeriod == slowPeriod && cacheLunarTickTrend[idx].EqualsInput(input))
						return cacheLunarTickTrend[idx];
			return CacheIndicator<LunarTick.LunarTickTrend>(new LunarTick.LunarTickTrend(){ UseCustomTimeframe = useCustomTimeframe, CustomTimeframe = customTimeframe, MAType = mAType, FastPeriod = fastPeriod, SlowPeriod = slowPeriod }, input, ref cacheLunarTickTrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.LunarTickTrend LunarTickTrend(bool useCustomTimeframe, int customTimeframe, NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod)
		{
			return indicator.LunarTickTrend(Input, useCustomTimeframe, customTimeframe, mAType, fastPeriod, slowPeriod);
		}

		public Indicators.LunarTick.LunarTickTrend LunarTickTrend(ISeries<double> input , bool useCustomTimeframe, int customTimeframe, NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod)
		{
			return indicator.LunarTickTrend(input, useCustomTimeframe, customTimeframe, mAType, fastPeriod, slowPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.LunarTickTrend LunarTickTrend(bool useCustomTimeframe, int customTimeframe, NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod)
		{
			return indicator.LunarTickTrend(Input, useCustomTimeframe, customTimeframe, mAType, fastPeriod, slowPeriod);
		}

		public Indicators.LunarTick.LunarTickTrend LunarTickTrend(ISeries<double> input , bool useCustomTimeframe, int customTimeframe, NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickTrendEnums.MATypeEnum mAType, int fastPeriod, int slowPeriod)
		{
			return indicator.LunarTickTrend(input, useCustomTimeframe, customTimeframe, mAType, fastPeriod, slowPeriod);
		}
	}
}

#endregion

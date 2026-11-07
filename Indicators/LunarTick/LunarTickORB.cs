#region Using declarations
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.LunarTick.ORBEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    namespace ORBEnums
    {
        public enum ORBExtensionLevels
        {
            None,
            One,
            Two,
            Three
        }

        public enum ORBLevels
        {
            ORBHighExt3,
            ORBHighExt3Mid,
            ORBHighExt2,
            ORBHighExt2Mid,
            ORBHighExt1,
            ORBHighExt1Mid,
            ORBHigh,
            ORBMid,
            ORBLow,
            ORBLowExt1Mid,
            ORBLowExt1,
            ORBLowExt2Mid,
            ORBLowExt2,
            ORBLowExt3Mid,
            ORBLowExt3
        }
    }

    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Display", 2)]
    [Gui.CategoryOrder("Developer", 3)]
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickORBTypeConverter")]
    public class LunarTickORB : Indicator
    {
        public class ORBRegion
        {
            public ORBRegion(MasterInstrument instrument, DateTime startTime, DateTime endTime, DateTime sessionEndTime)
            {
                Instrument = instrument;
                StartTime = startTime;
                EndTime = endTime;
                SessionEndTime = sessionEndTime;
                RegionTag = null;
                High = null;
                Low = null;
                IsConfirmed = false;
                IsDisplayed = false;
                StartBarIndex = -1;
                EndBarIndex = -1;
            }

            public MasterInstrument Instrument { get; }
            public DateTime StartTime { get; }
            public DateTime EndTime { get; }
            public DateTime SessionEndTime { get; }
            public string? RegionTag { get; set; }
            public string? RangeTag { get; set; }
            public double? High { get; private set; }
            public double? Low { get; private set; }
            public double? Mid
            {
                get
                {
                    if (!IsUpdated)
                        return null;

                    return Instrument.RoundToTickSize((High.Value + Low.Value) / 2.0);
                }
            }
            public bool IsUpdated { get => (High != null) && (Low != null); }
            public bool IsConfirmed
            {
                get => _isConfirmed;
                set
                {
                    _isConfirmed = (IsUpdated && value);
                }
            }
            public bool IsDisplayed { get; set; }
            public int StartBarIndex { get; set; }
            public int EndBarIndex { get; set; }

            public bool UpdateRange(double low, double high)
            {
                bool updated = false;

                if (Low == null || low < Low)
                {
                    Low = low;
                    updated = true;
                }
                if (High == null || high > High)
                {
                    High = high;
                    updated = true;
                }

                return updated;
            }

            public bool IsTimeInside(int time)
            {
                return (time >= ToTime(StartTime)) && (time < ToTime(EndTime));
            }

            public double GetORBLevel(ORBLevels level)
            {
                if (!(IsUpdated && IsConfirmed))
                    return 0;

                double delta = High.Value - Low.Value;

                switch (level)
                {
                    case ORBLevels.ORBHighExt3:
                        return High.Value + (3 * delta);
                    case ORBLevels.ORBHighExt3Mid:
                        return Mid.Value + (3 * delta);
                    case ORBLevels.ORBHighExt2:
                        return High.Value + (2 * delta);
                    case ORBLevels.ORBHighExt2Mid:
                        return Mid.Value + (2 * delta);
                    case ORBLevels.ORBHighExt1:
                        return High.Value + delta;
                    case ORBLevels.ORBHighExt1Mid:
                        return Mid.Value + delta;
                    case ORBLevels.ORBHigh:
                        return High.Value;
                    case ORBLevels.ORBMid:
                        return Mid.Value;
                    case ORBLevels.ORBLow:
                        return Low.Value;
                    case ORBLevels.ORBLowExt1Mid:
                        return Mid.Value - delta;
                    case ORBLevels.ORBLowExt1:
                        return Low.Value - delta;
                    case ORBLevels.ORBLowExt2Mid:
                        return Mid.Value - (2 * delta);
                    case ORBLevels.ORBLowExt2:
                        return Low.Value - (2 * delta);
                    case ORBLevels.ORBLowExt3Mid:
                        return Mid.Value - (3 * delta);
                    case ORBLevels.ORBLowExt3:
                        return Low.Value - (3 * delta);
                }

                return 0;
            }

            private bool _isConfirmed = false;
        }

        #region Constants

        public const string Version = "1.1.0";

        #endregion

        #region Members

        private List<ORBRegion> _orbRegions = new List<ORBRegion>();
        private Brush _highlightTimeframeBrush;
        private SimpleFont _labelFont;
        private int _decimalPlaces = 0;
        private int _lastBarUpdated = -1;

        private Series<double> _orbHighExt3;
        private Series<double> _orbHighExt3Mid;
        private Series<double> _orbHighExt2;
        private Series<double> _orbHighExt2Mid;
        private Series<double> _orbHighExt1;
        private Series<double> _orbHighExt1Mid;
        private Series<double> _orbHigh;
        private Series<double> _orbMid;
        private Series<double> _orbLow;
        private Series<double> _orbLowExt1Mid;
        private Series<double> _orbLowExt1;
        private Series<double> _orbLowExt2Mid;
        private Series<double> _orbLowExt2;
        private Series<double> _orbLowExt3Mid;
        private Series<double> _orbLowExt3;

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Session Start Time", Order = 1, GroupName = "Parameters")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime SessionStartTime
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Session End Time", Order = 2, GroupName = "Parameters")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime SessionEndTime
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, 60)]
        [Display(Name = "Opening Range Duration (Minutes)", Order = 3, GroupName = "Parameters")]
        public int OpeningRangeDurationMinutes
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [NinjaScriptProperty]
        [Display(Name = "Extension Levels", Order = 4, GroupName = "Parameters")]
        public NinjaTrader.NinjaScript.Indicators.LunarTick.ORBEnums.ORBExtensionLevels ExtensionLevels
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ext Level 1 Multiplier", Order = 5, GroupName = "Parameters")]
        public double ExtLevel1Multiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ext Level 2 Multiplier", Order = 6, GroupName = "Parameters")]
        public double ExtLevel2Multiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ext Level 3 Multiplier", Order = 7, GroupName = "Parameters")]
        public double ExtLevel3Multiplier
        { get; set; }

        [Range(1, 365)]
        [Display(Name = "Num Days To Show", Order = 1, GroupName = "Display")]
        public int NumDays
        { get; set; }

        [XmlIgnore]
        [Display(Name = "High Color", Order = 2, GroupName = "Display")]
        public Brush HighColor
        { get; set; }

        [Browsable(false)]
        public string HighColorSerializable
        {
            get { return Serialize.BrushToString(HighColor); }
            set { HighColor = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "High Line Width", Order = 3, GroupName = "Display")]
        public int HighLineWidth
        { get; set; }

        [Display(Name = "High Line Style", Order = 4, GroupName = "Display")]
        public DashStyleHelper HighLineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Mid Levels", Order = 5, GroupName = "Display")]
        public bool ShowMidLevels
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Mid Color", Order = 6, GroupName = "Display")]
        public Brush MidColor
        { get; set; }

        [Browsable(false)]
        public string MidColorSerializable
        {
            get { return Serialize.BrushToString(MidColor); }
            set { MidColor = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Mid Line Width", Order = 7, GroupName = "Display")]
        public int MidLineWidth
        { get; set; }

        [Display(Name = "Mid Line Style", Order = 8, GroupName = "Display")]
        public DashStyleHelper MidLineStyle
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Low Color", Order = 9, GroupName = "Display")]
        public Brush LowColor
        { get; set; }

        [Browsable(false)]
        public string LowColorSerializable
        {
            get { return Serialize.BrushToString(LowColor); }
            set { LowColor = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Low Line Width", Order = 10, GroupName = "Display")]
        public int LowLineWidth
        { get; set; }

        [Display(Name = "Low Line Style", Order = 11, GroupName = "Display")]
        public DashStyleHelper LowLineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight ORB Timeframe", Order = 12, GroupName = "Display")]
        public bool HighlightORBTimeframe
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Highlight ORB Timeframe Color", Order = 13, GroupName = "Display")]
        public Brush HighlightORBTimeframeColor
        { get; set; }

        [Browsable(false)]
        public string HighlightORBTimeframeColorSerializable
        {
            get { return Serialize.BrushToString(HighlightORBTimeframeColor); }
            set { HighlightORBTimeframeColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Highlight ORB Timeframe Opacity", Order = 14, GroupName = "Display")]
        public int HighlightORBTimeframeOpacity
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight ORB Region", Order = 15, GroupName = "Display")]
        public bool HighlightORBRegion
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Highlight ORB Region Color", Order = 16, GroupName = "Display")]
        public Brush HighlightORBRegionColor
        { get; set; }

        [Browsable(false)]
        public string HighlightORBRegionColorSerializable
        {
            get { return Serialize.BrushToString(HighlightORBRegionColor); }
            set { HighlightORBRegionColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Highlight ORB Region Opacity", Order = 17, GroupName = "Display")]
        public int HighlightORBRegionOpacity
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight ORB Range", Order = 18, GroupName = "Display")]
        public bool HighlightORBRange
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Highlight ORB Range Color", Order = 19, GroupName = "Display")]
        public Brush HighlightORBRangeColor
        { get; set; }

        [Browsable(false)]
        public string HighlightORBRangeColorSerializable
        {
            get { return Serialize.BrushToString(HighlightORBRangeColor); }
            set { HighlightORBRangeColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Highlight ORB Range Opacity", Order = 20, GroupName = "Display")]
        public int HighlightORBRangeOpacity
        { get; set; }

        [Display(Name = "Show Developing Region", Order = 21, GroupName = "Display")]
        public bool ShowDevelopingRegion
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Labels", Order = 22, GroupName = "Display")]
        public bool ShowLabels
        { get; set; }

        [Range(1, 100)]
        [Display(Name = "Label Size", Order = 23, GroupName = "Display")]
        public int LabelSize
        { get; set; }

        [Display(Name = "Label Bold", Order = 24, GroupName = "Display")]
        public bool LabelBold
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
        public Series<double> ORBHighExt3
        {
            get { Update(); return _orbHighExt3; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBHighExt3Mid
        {
            get { Update(); return _orbHighExt3Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBHighExt2
        {
            get { Update(); return _orbHighExt2; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBHighExt2Mid
        {
            get { Update(); return _orbHighExt2Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBHighExt1
        {
            get { Update(); return _orbHighExt1; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBHighExt1Mid
        {
            get { Update(); return _orbHighExt1Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBHigh
        {
            get { Update(); return _orbHigh; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBMid
        {
            get { Update(); return _orbMid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBLow
        {
            get { Update(); return _orbLow; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBLowExt1Mid
        {
            get { Update(); return _orbLowExt1Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBLowExt1
        {
            get { Update(); return _orbLowExt1; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBLowExt2Mid
        {
            get { Update(); return _orbLowExt2Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBLowExt2
        {
            get { Update(); return _orbLowExt2; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBLowExt3Mid
        {
            get { Update(); return _orbLowExt3Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORBLowExt3
        {
            get { Update(); return _orbLowExt3; }
        }

        #endregion

        #region Indicator methods

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                return Name + "(" + SessionStartTime.ToString("HH:mm") + "," + SessionEndTime.ToString("HH:mm") + "," + OpeningRangeDurationMinutes + "," + ExtensionLevels + "," + ExtLevel1Multiplier + "," + ExtLevel2Multiplier + "," + ExtLevel3Multiplier + ")";
            }
        }

        protected override void OnStateChange()
        {
            DebugPrint($"OnStateChange({State})");

            if (State == State.SetDefaults)
            {
                Description = @"An Opening Range indicator, supporting extension levels and mid levels.";
                Name = "LunarTick ORB";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsAutoScale = false;
                ZOrder = 0;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                SessionStartTime = DateTime.Parse("09:30:00");
                SessionEndTime = DateTime.Parse("16:00:00");
                OpeningRangeDurationMinutes = 15;
                ExtensionLevels = ORBEnums.ORBExtensionLevels.None;
                ExtLevel1Multiplier = 1;
                ExtLevel2Multiplier = 2;
                ExtLevel3Multiplier = 3;
                NumDays = 5;
                HighColor = Brushes.Purple;
                HighLineWidth = 2;
                HighLineStyle = DashStyleHelper.Solid;
                ShowMidLevels = true;
                MidColor = Brushes.Purple;
                MidLineWidth = 1;
                MidLineStyle = DashStyleHelper.Dash;
                LowColor = Brushes.Purple;
                LowLineWidth = 2;
                LowLineStyle = DashStyleHelper.Solid;
                HighlightORBTimeframe = false;
                HighlightORBTimeframeColor = Brushes.Magenta;
                HighlightORBTimeframeOpacity = 20;
                HighlightORBRegion = true;
                HighlightORBRegionColor = Brushes.Magenta;
                HighlightORBRegionOpacity = 20;
                HighlightORBRange = true;
                HighlightORBRangeColor = Brushes.Magenta;
                HighlightORBRangeOpacity = 10;
                ShowDevelopingRegion = true;
                ShowLabels = true;
                LabelSize = 11;
                LabelBold = true;

                VersionInformation = $"{Version} - {Assembly.GetAssembly(typeof(LunarTickORB)).GetName().Version}";
                Debug = false;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Minute, 1);

                SolidColorBrush highlightTimeframeBrush = HighlightORBTimeframeColor as SolidColorBrush;
                _highlightTimeframeBrush = new SolidColorBrush(Color.FromArgb((byte)((HighlightORBTimeframeOpacity / 100.0) * 255.0), highlightTimeframeBrush.Color.R, highlightTimeframeBrush.Color.G, highlightTimeframeBrush.Color.B));
                _highlightTimeframeBrush.Freeze();
            }
            else if (State == State.DataLoaded)
            {
                // Detect number of decimal places.
                var tickSize = Instrument.MasterInstrument.TickSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
                tickSize.LastIndexOf('.');
                _decimalPlaces = tickSize.Contains(".") ? tickSize.Split('.')[1].Length : 0;
                _lastBarUpdated = -1;

                _orbRegions.Clear();
                _labelFont = new SimpleFont() { Family = ChartControl.Properties.LabelFont.Family, Size = LabelSize, Bold = LabelBold };

                _orbHighExt3 = new Series<double>(this);
                _orbHighExt3Mid = new Series<double>(this);
                _orbHighExt2 = new Series<double>(this);
                _orbHighExt2Mid = new Series<double>(this);
                _orbHighExt1 = new Series<double>(this);
                _orbHighExt1Mid = new Series<double>(this);
                _orbHigh = new Series<double>(this);
                _orbMid = new Series<double>(this);
                _orbLow = new Series<double>(this);
                _orbLowExt1Mid = new Series<double>(this);
                _orbLowExt1 = new Series<double>(this);
                _orbLowExt2Mid = new Series<double>(this);
                _orbLowExt2 = new Series<double>(this);
                _orbLowExt3Mid = new Series<double>(this);
                _orbLowExt3 = new Series<double>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            if (!Bars.BarsType.IsIntraday)
                return;
            if (CurrentBar < 1)
                return;

            int orbStartTime = ToTime(SessionStartTime);
            int orbEndTime = ToTime(SessionStartTime.AddMinutes(OpeningRangeDurationMinutes));
            int sessionEndTime = ToTime(SessionEndTime);
            int prevBarEndTime = ToTime(Time[1]);
            int currentBarEndTime = ToTime(Time[0]);
            int currentBarStartTime = prevBarEndTime;

            if (BarsInProgress == 0)
            {
                if (_orbRegions.Count == 0)
                    return;

                if (NumDays > 0 && _orbRegions.Count > NumDays)
                {
                    // Need to trim oldest ORB region.
                    var oldestORBRegion = _orbRegions[0];
                    _orbRegions.RemoveAt(0);
                    CleanupORBRegion(oldestORBRegion);
                }

                var currentORBRegion = _orbRegions.Last();

                if (currentORBRegion.StartBarIndex == -1)
                {
                    DateTime regionStartTime = currentORBRegion.StartTime;
                    int regionStartBar = Bars.GetBar(regionStartTime);
                    if (ToTime(Time[CurrentBar - regionStartBar]) == orbStartTime)
                        regionStartBar++; // We found the bar that ENDS at the ORB start time, but we want to draw region from the start bar onwards, so start from next bar on chart.
                    currentORBRegion.StartBarIndex = regionStartBar;
                }

                // Highlight timeframe
                if (HighlightORBTimeframe)
                {
                    if (Time[1] >= currentORBRegion.StartTime && Time[0] <= currentORBRegion.EndTime)
                    {
                        BackBrush = _highlightTimeframeBrush;
                    }
                }

                // Highlight region
                if (HighlightORBRegion)
                {
                    if ((currentORBRegion.IsConfirmed && string.IsNullOrEmpty(currentORBRegion.RegionTag)) ||
                        (!currentORBRegion.IsConfirmed && ShowDevelopingRegion && (string.IsNullOrEmpty(currentORBRegion.RegionTag) || IsFirstTickOfBar)))
                    {
                        currentORBRegion.RegionTag = $"LunarTickORB-{currentORBRegion.StartTime.ToString("yyyyMMdd-HHmm")}-Region";
                        DateTime regionEndTime = Time[0] < currentORBRegion.EndTime ? Time[0] : currentORBRegion.EndTime;
                        int regionEndBar = Bars.GetBar(regionEndTime);
                        currentORBRegion.EndBarIndex = regionEndBar;
                        Draw.Rectangle(this, currentORBRegion.RegionTag, false, CurrentBar - currentORBRegion.StartBarIndex, currentORBRegion.High.Value, CurrentBar - currentORBRegion.EndBarIndex, currentORBRegion.Low.Value, Brushes.Transparent, HighlightORBRegionColor, HighlightORBRegionOpacity);
                    }
                }

                // Draw lines and (optional) range (only after ORB region is confirmed).
                if (currentORBRegion.IsConfirmed && (Time[0] > currentORBRegion.StartTime) && !currentORBRegion.IsDisplayed)
                {
                    DisplayORB(currentORBRegion);
                }

                // Populate data series (only after ORB region is confirmed).
                if (currentORBRegion.IsConfirmed && (Time[0] > currentORBRegion.StartTime) && (CurrentBar > _lastBarUpdated))
                {
                    PopulateDataSeries(currentORBRegion, 0);
                    _lastBarUpdated = CurrentBar;
                }

                // Cleanup labels after end of ORB range.
                if (IsFirstTickOfBar && (Time[1] <= currentORBRegion.SessionEndTime) && (Time[0] > currentORBRegion.SessionEndTime))
                {
                    CleanupORBLabels();
                }
            }
            else if (BarsInProgress == 1)
            {
                ORBRegion? currentORBRegion = null;

                // Check for start of new ORB region.
                if (IsFirstTickOfBar && currentBarStartTime == orbStartTime)
                {
                    DateTime dtStartTime = Time[1];
                    DateTime dtEndTime = dtStartTime.AddMinutes(OpeningRangeDurationMinutes);
                    DateTime dtSessionEndTime = new DateTime(dtStartTime.Year, dtStartTime.Month, dtStartTime.Day, SessionEndTime.Hour, SessionEndTime.Minute, 0, dtStartTime.Kind);
                    if (sessionEndTime < orbStartTime)
                        dtSessionEndTime = dtSessionEndTime.AddDays(1);
                    currentORBRegion = new ORBRegion(Instrument.MasterInstrument, dtStartTime, dtEndTime, dtSessionEndTime);
                    _orbRegions.Add(currentORBRegion);
                }
                else if (_orbRegions.Count > 0)
                {
                    currentORBRegion = _orbRegions.Last();
                }

                if (currentORBRegion == null)
                    return;

                if (!currentORBRegion.IsConfirmed)
                {
                    bool regionChanged = false;

                    if (Time[1] >= currentORBRegion.EndTime)
                    {
                        // ORB region is now confirmed.
                        currentORBRegion.IsConfirmed = true;
                        currentORBRegion.RegionTag = null;
                    }
                    else
                    {
                        regionChanged = currentORBRegion.UpdateRange(Low[0], High[0]);
                        if (regionChanged)
                            currentORBRegion.RegionTag = null;
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private void PopulateDataSeries(ORBRegion orbRegion, int barsAgo)
        {
            if (orbRegion == null)
                return;
            if (!orbRegion.IsConfirmed)
                return;
            if (!orbRegion.IsDisplayed)
                return;

            int barIdx = CurrentBar - barsAgo;
            int rangeEndBar = Bars.GetBar(orbRegion.SessionEndTime);

            if (barIdx < orbRegion.StartBarIndex || barIdx > rangeEndBar)
                return;

            bool showExt1 = ExtensionLevels != ORBEnums.ORBExtensionLevels.None;
            bool showExt2 = ExtensionLevels == ORBEnums.ORBExtensionLevels.Two || ExtensionLevels == ORBEnums.ORBExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == ORBEnums.ORBExtensionLevels.Three;


            ORBHigh[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBHigh);
            ORBLow[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBLow);
            if (showExt1)
            {
                ORBHighExt1[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBHighExt1);
                ORBLowExt1[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBLowExt1);
            }
            if (showExt2)
            {
                ORBHighExt2[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBHighExt2);
                ORBLowExt2[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBLowExt2);
            }
            if (showExt3)
            {
                ORBHighExt3[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBHighExt3);
                ORBLowExt3[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBLowExt3);
            }
            if (ShowMidLevels)
            {
                ORBMid[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBMid);
                if (showExt1)
                {
                    ORBHighExt1Mid[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBHighExt1Mid);
                    ORBLowExt1Mid[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBLowExt1Mid);
                }
                if (showExt2)
                {
                    ORBHighExt2Mid[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBHighExt2Mid);
                    ORBLowExt2Mid[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBLowExt2Mid);
                }
                if (showExt3)
                {
                    ORBHighExt3Mid[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBHighExt3Mid);
                    ORBLowExt3Mid[barsAgo] = orbRegion.GetORBLevel(ORBLevels.ORBLowExt3Mid);
                }
            }
        }

        private void DisplayORB(ORBRegion orbRegion)
        {
            if (orbRegion == null)
                return;
            if (!orbRegion.IsConfirmed)
                return;
            if (orbRegion.IsDisplayed)
                return;

            int rangeEndBar = Bars.GetBar(orbRegion.SessionEndTime);

            // Draw range.
            if (HighlightORBRange && string.IsNullOrEmpty(orbRegion.RangeTag))
            {
                orbRegion.RangeTag = $"LunarTickORB-{orbRegion.StartTime.ToString("yyyyMMdd-HHmm")}-Range";
                Draw.Rectangle(this, orbRegion.RangeTag, false, Time[CurrentBar - orbRegion.StartBarIndex], orbRegion.High.Value, orbRegion.SessionEndTime, orbRegion.Low.Value, Brushes.Transparent, HighlightORBRangeColor, HighlightORBRangeOpacity);
            }

            // Draw lines.
            bool showExt1 = ExtensionLevels != ORBEnums.ORBExtensionLevels.None;
            bool showExt2 = ExtensionLevels == ORBEnums.ORBExtensionLevels.Two || ExtensionLevels == ORBEnums.ORBExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == ORBEnums.ORBExtensionLevels.Three;
            DebugPrint($"orbRegion.StartTime: {orbRegion.StartTime.ToString("yyyyMMdd-HHmm")}");
            DebugPrint($"Time[CurrentBar - orbRegion.StartBarIndex]: {Time[CurrentBar - orbRegion.StartBarIndex].ToString("yyyyMMdd-HHmm")}");
            UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBHigh", orbRegion.GetORBLevel(ORBLevels.ORBHigh), HighColor, HighLineStyle, HighLineWidth, "ORB High", _labelFont);
            UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBLow", orbRegion.GetORBLevel(ORBLevels.ORBLow), LowColor, LowLineStyle, LowLineWidth, "ORB Low", _labelFont);
            if (showExt1)
            {
                UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBHighExt1", orbRegion.GetORBLevel(ORBLevels.ORBHighExt1), HighColor, HighLineStyle, HighLineWidth, "ORB High Ext 1", _labelFont);
                UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBLowExt1", orbRegion.GetORBLevel(ORBLevels.ORBLowExt1), LowColor, LowLineStyle, LowLineWidth, "ORB Low Ext 1", _labelFont);
            }
            if (showExt2)
            {
                UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBHighExt2", orbRegion.GetORBLevel(ORBLevels.ORBHighExt2), HighColor, HighLineStyle, HighLineWidth, "ORB High Ext 2", _labelFont);
                UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBLowExt2", orbRegion.GetORBLevel(ORBLevels.ORBLowExt2), LowColor, LowLineStyle, LowLineWidth, "ORB Low Ext 2", _labelFont);
            }
            if (showExt3)
            {
                UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBHighExt3", orbRegion.GetORBLevel(ORBLevels.ORBHighExt3), HighColor, HighLineStyle, HighLineWidth, "ORB High Ext 3", _labelFont);
                UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBLowExt3", orbRegion.GetORBLevel(ORBLevels.ORBLowExt3), LowColor, LowLineStyle, LowLineWidth, "ORB Low Ext 3", _labelFont);
            }
            if (ShowMidLevels)
            {
                UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBMid", orbRegion.Mid.Value, MidColor, MidLineStyle, MidLineWidth, "ORB Mid", _labelFont);
                if (showExt1)
                {
                    UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBHighExt1Mid", orbRegion.GetORBLevel(ORBLevels.ORBHighExt1Mid), MidColor, MidLineStyle, MidLineWidth, "ORB High Ext 1 Mid", _labelFont);
                    UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBLowExt1Mid", orbRegion.GetORBLevel(ORBLevels.ORBLowExt1Mid), MidColor, MidLineStyle, MidLineWidth, "ORB Low Ext 1 Mid", _labelFont);
                }
                if (showExt2)
                {
                    UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBHighExt2Mid", orbRegion.GetORBLevel(ORBLevels.ORBHighExt2Mid), MidColor, MidLineStyle, MidLineWidth, "ORB High Ext 2 Mid", _labelFont);
                    UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBLowExt2Mid", orbRegion.GetORBLevel(ORBLevels.ORBLowExt2Mid), MidColor, MidLineStyle, MidLineWidth, "ORB Low Ext 2 Mid", _labelFont);
                }
                if (showExt3)
                {
                    UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBHighExt3Mid", orbRegion.GetORBLevel(ORBLevels.ORBHighExt3Mid), MidColor, MidLineStyle, MidLineWidth, "ORB High Ext 3 Mid", _labelFont);
                    UpdateLineAndLabel(orbRegion, "LunarTickORB", "ORBLowExt3Mid", orbRegion.GetORBLevel(ORBLevels.ORBLowExt3Mid), MidColor, MidLineStyle, MidLineWidth, "ORB Low Ext 3 Mid", _labelFont);
                }
            }

            orbRegion.IsDisplayed = true;
        }

        private void UpdateLineAndLabel(ORBRegion orbRegion, string tagPrefix, string name, double yValue, Brush lineBrush, DashStyleHelper lineStyle, int lineWidth, string labelText, SimpleFont labelFont)
        {
            DateTime dtStartTime = Time[CurrentBar - orbRegion.StartBarIndex];
            DateTime dtEndTime = orbRegion.SessionEndTime;
            Draw.Line(this, $"{tagPrefix}-{orbRegion.StartTime.ToString("yyyyMMdd-HHmm")}-{name}-Line", false, dtStartTime, yValue, dtEndTime, yValue, lineBrush, lineStyle, lineWidth);
            if (ShowLabels)
            {
                Draw.Text(this, $"{tagPrefix}-{name}-Label", false, $" {yValue.ToString($"F{_decimalPlaces}")} {labelText}", dtEndTime, yValue, 0, lineBrush, labelFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }
        }

        private void CleanupORBRegion(ORBRegion deadORBRegion)
        {
            RemoveDrawObjectsWithPrefix($"LunarTickORB-{deadORBRegion.StartTime.ToString("yyyyMMdd-HHmm")}");
        }

        private void CleanupORBLabels()
        {
            RemoveDrawObjectsWithPrefixAndSuffix("LunarTickORB", "Label");
        }

        private void RemoveDrawObjectsWithPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return;

            DebugPrint($"Removing DrawObjects with prefix [{prefix}]");
            foreach (var drawObj in DrawObjects)
            {
                if (drawObj.Tag.StartsWith(prefix))
                {
                    DebugPrint($"Removing DrawObject [{drawObj.Tag}]");
                    RemoveDrawObject(drawObj.Tag);
                }
            }
        }

        private void RemoveDrawObjectsWithPrefixAndSuffix(string prefix, string suffix)
        {
            if (string.IsNullOrEmpty(prefix))
                return;
            if (string.IsNullOrEmpty(suffix))
                return;

            DebugPrint($"Removing DrawObjects with prefix [{prefix}], suffix [{suffix}]");
            foreach (var drawObj in DrawObjects)
            {
                if (drawObj.Tag.StartsWith(prefix) && drawObj.Tag.EndsWith(suffix))
                {
                    DebugPrint($"Removing DrawObject [{drawObj.Tag}]");
                    RemoveDrawObject(drawObj.Tag);
                }
            }
        }

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"LunarTickORB[{Instrument.FullName}]: {msg}");
                else
                    Print($"LunarTickORB: {msg}");
            }
        }

        #endregion
    }

    public class LunarTickORBTypeConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            // We need the indicator instance which actually exists on the grid
            LunarTickORB indicator = component as LunarTickORB;

            // base.GetProperties ensures we have all the properties (and associated property grid editors)
            // NinjaTrader internal logic determines for a given indicator
            PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context)
                                                                        ? base.GetProperties(context, component, attrs)
                                                                        : TypeDescriptor.GetProperties(component, attrs);

            if (indicator == null || propertyDescriptorCollection == null)
                return propertyDescriptorCollection;


            // Show/hide properties related to ExtensionLevels
            PropertyDescriptor extLevel1MultiplierPropertyDesc = propertyDescriptorCollection["ExtLevel1Multiplier"];
            PropertyDescriptor extLevel2MultiplierPropertyDesc = propertyDescriptorCollection["ExtLevel2Multiplier"];
            PropertyDescriptor extLevel3MultiplierPropertyDesc = propertyDescriptorCollection["ExtLevel3Multiplier"];

            propertyDescriptorCollection.Remove(extLevel1MultiplierPropertyDesc);
            propertyDescriptorCollection.Remove(extLevel2MultiplierPropertyDesc);
            propertyDescriptorCollection.Remove(extLevel3MultiplierPropertyDesc);

            if (indicator.ExtensionLevels == ORBExtensionLevels.One)
            {
                propertyDescriptorCollection.Add(extLevel1MultiplierPropertyDesc);
            }
            else if (indicator.ExtensionLevels == ORBExtensionLevels.Two)
            {
                propertyDescriptorCollection.Add(extLevel1MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(extLevel2MultiplierPropertyDesc);
            }
            else if (indicator.ExtensionLevels == ORBExtensionLevels.Three)
            {
                propertyDescriptorCollection.Add(extLevel1MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(extLevel2MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(extLevel3MultiplierPropertyDesc);
            }


            // Show/hide properties related to ShowMidLevels
            PropertyDescriptor midColorPropertyDesc = propertyDescriptorCollection["MidColor"];
            PropertyDescriptor midLineWidthPropertyDesc = propertyDescriptorCollection["MidLineWidth"];
            PropertyDescriptor midLineStylePropertyDesc = propertyDescriptorCollection["MidLineStyle"];
            propertyDescriptorCollection.Remove(midColorPropertyDesc);
            propertyDescriptorCollection.Remove(midLineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(midLineStylePropertyDesc);
            if (indicator.ShowMidLevels)
            {
                propertyDescriptorCollection.Add(midColorPropertyDesc);
                propertyDescriptorCollection.Add(midLineWidthPropertyDesc);
                propertyDescriptorCollection.Add(midLineStylePropertyDesc);
            }


            // Show/hide properties related to HighlightORBTimeframe
            PropertyDescriptor highlightORBTimeframeColorPropertyDesc = propertyDescriptorCollection["HighlightORBTimeframeColor"];
            PropertyDescriptor highlightORBTimeframeOpacityPropertyDesc = propertyDescriptorCollection["HighlightORBTimeframeOpacity"];
            propertyDescriptorCollection.Remove(highlightORBTimeframeColorPropertyDesc);
            propertyDescriptorCollection.Remove(highlightORBTimeframeOpacityPropertyDesc);
            if (indicator.HighlightORBTimeframe)
            {
                propertyDescriptorCollection.Add(highlightORBTimeframeColorPropertyDesc);
                propertyDescriptorCollection.Add(highlightORBTimeframeOpacityPropertyDesc);
            }


            // Show/hide properties related to HighlightORBRegion
            PropertyDescriptor highlightORBRegionColorPropertyDesc = propertyDescriptorCollection["HighlightORBRegionColor"];
            PropertyDescriptor highlightORBRegionOpacityPropertyDesc = propertyDescriptorCollection["HighlightORBRegionOpacity"];
            propertyDescriptorCollection.Remove(highlightORBRegionColorPropertyDesc);
            propertyDescriptorCollection.Remove(highlightORBRegionOpacityPropertyDesc);
            if (indicator.HighlightORBRegion)
            {
                propertyDescriptorCollection.Add(highlightORBRegionColorPropertyDesc);
                propertyDescriptorCollection.Add(highlightORBRegionOpacityPropertyDesc);
            }


            // Show/hide properties related to HighlightORBRange
            PropertyDescriptor highlightORBRangeColorPropertyDesc = propertyDescriptorCollection["HighlightORBRangeColor"];
            PropertyDescriptor highlightORBRangeOpacityPropertyDesc = propertyDescriptorCollection["HighlightORBRangeOpacity"];
            propertyDescriptorCollection.Remove(highlightORBRangeColorPropertyDesc);
            propertyDescriptorCollection.Remove(highlightORBRangeOpacityPropertyDesc);
            if (indicator.HighlightORBRange)
            {
                propertyDescriptorCollection.Add(highlightORBRangeColorPropertyDesc);
                propertyDescriptorCollection.Add(highlightORBRangeOpacityPropertyDesc);
            }


            // Show/hide properties related to ShowLabels
            PropertyDescriptor labelSizePropertyDesc = propertyDescriptorCollection["LabelSize"];
            PropertyDescriptor labelBoldPropertyDesc = propertyDescriptorCollection["LabelBold"];
            propertyDescriptorCollection.Remove(labelSizePropertyDesc);
            propertyDescriptorCollection.Remove(labelBoldPropertyDesc);
            if (indicator.ShowLabels)
            {
                propertyDescriptorCollection.Add(labelSizePropertyDesc);
                propertyDescriptorCollection.Add(labelBoldPropertyDesc);
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
		private LunarTick.LunarTickORB[] cacheLunarTickORB;
		public LunarTick.LunarTickORB LunarTickORB(DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.ORBEnums.ORBExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return LunarTickORB(Input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public LunarTick.LunarTickORB LunarTickORB(ISeries<double> input, DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.ORBEnums.ORBExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			if (cacheLunarTickORB != null)
				for (int idx = 0; idx < cacheLunarTickORB.Length; idx++)
					if (cacheLunarTickORB[idx] != null && cacheLunarTickORB[idx].SessionStartTime == sessionStartTime && cacheLunarTickORB[idx].SessionEndTime == sessionEndTime && cacheLunarTickORB[idx].OpeningRangeDurationMinutes == openingRangeDurationMinutes && cacheLunarTickORB[idx].ExtensionLevels == extensionLevels && cacheLunarTickORB[idx].ExtLevel1Multiplier == extLevel1Multiplier && cacheLunarTickORB[idx].ExtLevel2Multiplier == extLevel2Multiplier && cacheLunarTickORB[idx].ExtLevel3Multiplier == extLevel3Multiplier && cacheLunarTickORB[idx].EqualsInput(input))
						return cacheLunarTickORB[idx];
			return CacheIndicator<LunarTick.LunarTickORB>(new LunarTick.LunarTickORB(){ SessionStartTime = sessionStartTime, SessionEndTime = sessionEndTime, OpeningRangeDurationMinutes = openingRangeDurationMinutes, ExtensionLevels = extensionLevels, ExtLevel1Multiplier = extLevel1Multiplier, ExtLevel2Multiplier = extLevel2Multiplier, ExtLevel3Multiplier = extLevel3Multiplier }, input, ref cacheLunarTickORB);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.LunarTickORB LunarTickORB(DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.ORBEnums.ORBExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.LunarTickORB(Input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public Indicators.LunarTick.LunarTickORB LunarTickORB(ISeries<double> input , DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.ORBEnums.ORBExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.LunarTickORB(input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.LunarTickORB LunarTickORB(DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.ORBEnums.ORBExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.LunarTickORB(Input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public Indicators.LunarTick.LunarTickORB LunarTickORB(ISeries<double> input , DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.ORBEnums.ORBExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.LunarTickORB(input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}
	}
}

#endregion

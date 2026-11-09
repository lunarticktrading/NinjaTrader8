#region Using declarations
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums;
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
    namespace InitialBalanceEnums
    {
        public enum InitialBalanceExtensionLevels
        {
            None,
            One,
            Two,
            Three
        }

        public enum InitialBalanceLevels
        {
            IBHighExt3,
            IBHighExt3Mid,
            IBHighExt2,
            IBHighExt2Mid,
            IBHighExt1,
            IBHighExt1Mid,
            IBHigh,
            IBMid,
            IBLow,
            IBLowExt1Mid,
            IBLowExt1,
            IBLowExt2Mid,
            IBLowExt2,
            IBLowExt3Mid,
            IBLowExt3
        }
    }

    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Display", 2)]
    [Gui.CategoryOrder("Developer", 3)]
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickInitialBalanceTypeConverter")]
    public class LunarTickInitialBalance : Indicator
    {
        public class InitialBalanceRegion
        {
            public InitialBalanceRegion(MasterInstrument instrument, DateTime startTime, DateTime endTime, DateTime sessionEndTime)
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

            public double GetIBLevel(InitialBalanceLevels level)
            {
                if (!(IsUpdated && IsConfirmed))
                    return 0;

                double delta = High.Value - Low.Value;

                switch (level)
                {
                    case InitialBalanceLevels.IBHighExt3:
                        return High.Value + (3 * delta);
                    case InitialBalanceLevels.IBHighExt3Mid:
                        return Mid.Value + (3 * delta);
                    case InitialBalanceLevels.IBHighExt2:
                        return High.Value + (2 * delta);
                    case InitialBalanceLevels.IBHighExt2Mid:
                        return Mid.Value + (2 * delta);
                    case InitialBalanceLevels.IBHighExt1:
                        return High.Value + delta;
                    case InitialBalanceLevels.IBHighExt1Mid:
                        return Mid.Value + delta;
                    case InitialBalanceLevels.IBHigh:
                        return High.Value;
                    case InitialBalanceLevels.IBMid:
                        return Mid.Value;
                    case InitialBalanceLevels.IBLow:
                        return Low.Value;
                    case InitialBalanceLevels.IBLowExt1Mid:
                        return Mid.Value - delta;
                    case InitialBalanceLevels.IBLowExt1:
                        return Low.Value - delta;
                    case InitialBalanceLevels.IBLowExt2Mid:
                        return Mid.Value - (2 * delta);
                    case InitialBalanceLevels.IBLowExt2:
                        return Low.Value - (2 * delta);
                    case InitialBalanceLevels.IBLowExt3Mid:
                        return Mid.Value - (3 * delta);
                    case InitialBalanceLevels.IBLowExt3:
                        return Low.Value - (3 * delta);
                }

                return 0;
            }

            private bool _isConfirmed = false;
        }

        #region Constants

        public const string Version = "1.3.2";
        public const int InitialBalanceDurationMinutes = 60;

        #endregion

        #region Members

        private List<InitialBalanceRegion> _ibRegions = new List<InitialBalanceRegion>();
        private SimpleFont _labelFont;
        private int _decimalPlaces = 0;
        private int _lastBarUpdated = -1;

        private Series<double> _ibHighExt3;
        private Series<double> _ibHighExt3Mid;
        private Series<double> _ibHighExt2;
        private Series<double> _ibHighExt2Mid;
        private Series<double> _ibHighExt1;
        private Series<double> _ibHighExt1Mid;
        private Series<double> _ibHigh;
        private Series<double> _ibMid;
        private Series<double> _ibLow;
        private Series<double> _ibLowExt1Mid;
        private Series<double> _ibLowExt1;
        private Series<double> _ibLowExt2Mid;
        private Series<double> _ibLowExt2;
        private Series<double> _ibLowExt3Mid;
        private Series<double> _ibLowExt3;

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

        [RefreshProperties(RefreshProperties.All)]
        [NinjaScriptProperty]
        [Display(Name = "Extension Levels", Order = 3, GroupName = "Parameters")]
        public NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels ExtensionLevels
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ext Level 1 Multiplier", Order = 4, GroupName = "Parameters")]
        public double ExtLevel1Multiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ext Level 2 Multiplier", Order = 5, GroupName = "Parameters")]
        public double ExtLevel2Multiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ext Level 3 Multiplier", Order = 6, GroupName = "Parameters")]
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
        [Display(Name = "Highlight IB Region", Order = 12, GroupName = "Display")]
        public bool HighlightIBRegion
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Highlight IB Region Color", Order = 13, GroupName = "Display")]
        public Brush HighlightIBRegionColor
        { get; set; }

        [Browsable(false)]
        public string HighlightIBRegionColorSerializable
        {
            get { return Serialize.BrushToString(HighlightIBRegionColor); }
            set { HighlightIBRegionColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Highlight IB Region Opacity", Order = 14, GroupName = "Display")]
        public int HighlightIBRegionOpacity
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight IB Range", Order = 15, GroupName = "Display")]
        public bool HighlightIBRange
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Highlight IB Range Color", Order = 16, GroupName = "Display")]
        public Brush HighlightIBRangeColor
        { get; set; }

        [Browsable(false)]
        public string HighlightIBRangeColorSerializable
        {
            get { return Serialize.BrushToString(HighlightIBRangeColor); }
            set { HighlightIBRangeColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Highlight IB Range Opacity", Order = 17, GroupName = "Display")]
        public int HighlightIBRangeOpacity
        { get; set; }

        [Display(Name = "Show Developing Region", Order = 18, GroupName = "Display")]
        public bool ShowDevelopingRegion
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Labels", Order = 19, GroupName = "Display")]
        public bool ShowLabels
        { get; set; }

        [Range(1, 100)]
        [Display(Name = "Label Size", Order = 20, GroupName = "Display")]
        public int LabelSize
        { get; set; }

        [Display(Name = "Label Bold", Order = 21, GroupName = "Display")]
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
        public Series<double> IBHighExt3
        {
            get { Update(); return _ibHighExt3; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt3Mid
        {
            get { Update(); return _ibHighExt3Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt2
        {
            get { Update(); return _ibHighExt2; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt2Mid
        {
            get { Update(); return _ibHighExt2Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt1
        {
            get { Update(); return _ibHighExt1; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt1Mid
        {
            get { Update(); return _ibHighExt1Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHigh
        {
            get { Update(); return _ibHigh; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBMid
        {
            get { Update(); return _ibMid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLow
        {
            get { Update(); return _ibLow; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt1Mid
        {
            get { Update(); return _ibLowExt1Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt1
        {
            get { Update(); return _ibLowExt1; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt2Mid
        {
            get { Update(); return _ibLowExt2Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt2
        {
            get { Update(); return _ibLowExt2; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt3Mid
        {
            get { Update(); return _ibLowExt3Mid; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt3
        {
            get { Update(); return _ibLowExt3; }
        }

        #endregion

        #region Indicator methods

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                return Name + "(" + SessionStartTime.ToString("HH:mm") + "," + SessionEndTime.ToString("HH:mm") + "," + ExtensionLevels + "," + ExtLevel1Multiplier + "," + ExtLevel2Multiplier + "," + ExtLevel3Multiplier + ")";
            }
        }

        protected override void OnStateChange()
        {
            DebugPrint($"OnStateChange({State})");

            if (State == State.SetDefaults)
            {
                Description = @"An Initial Balance indicator, supporting extension levels and mid levels.";
                Name = "LunarTick Initial Balance";
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
                ExtensionLevels = InitialBalanceEnums.InitialBalanceExtensionLevels.None;
                ExtLevel1Multiplier = 1;
                ExtLevel2Multiplier = 2;
                ExtLevel3Multiplier = 3;
                NumDays = 5;
                HighColor = Brushes.Green;
                HighLineWidth = 2;
                HighLineStyle = DashStyleHelper.Solid;
                ShowMidLevels = true;
                MidColor = Brushes.Orange;
                MidLineWidth = 1;
                MidLineStyle = DashStyleHelper.Dash;
                LowColor = Brushes.Red;
                LowLineWidth = 2;
                LowLineStyle = DashStyleHelper.Solid;
                HighlightIBRegion = true;
                HighlightIBRegionColor = Brushes.Cyan;
                HighlightIBRegionOpacity = 20;
                HighlightIBRange = true;
                HighlightIBRangeColor = Brushes.Cyan;
                HighlightIBRangeOpacity = 10;
                ShowDevelopingRegion = true;
                ShowLabels = true;
                LabelSize = 11;
                LabelBold = true;

                VersionInformation = $"{Version} - {Assembly.GetAssembly(typeof(LunarTickInitialBalance)).GetName().Version}";
                Debug = false;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Minute, 1);
            }
            else if (State == State.DataLoaded)
            {
                // Detect number of decimal places.
                var tickSize = Instrument.MasterInstrument.TickSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
                tickSize.LastIndexOf('.');
                _decimalPlaces = tickSize.Contains(".") ? tickSize.Split('.')[1].Length : 0;
                _lastBarUpdated = -1;

                _ibRegions.Clear();
                _labelFont = new SimpleFont() { Family = ChartControl.Properties.LabelFont.Family, Size = LabelSize, Bold = LabelBold };

                _ibHighExt3 = new Series<double>(this);
                _ibHighExt3Mid = new Series<double>(this);
                _ibHighExt2 = new Series<double>(this);
                _ibHighExt2Mid = new Series<double>(this);
                _ibHighExt1 = new Series<double>(this);
                _ibHighExt1Mid = new Series<double>(this);
                _ibHigh = new Series<double>(this);
                _ibMid = new Series<double>(this);
                _ibLow = new Series<double>(this);
                _ibLowExt1Mid = new Series<double>(this);
                _ibLowExt1 = new Series<double>(this);
                _ibLowExt2Mid = new Series<double>(this);
                _ibLowExt2 = new Series<double>(this);
                _ibLowExt3Mid = new Series<double>(this);
                _ibLowExt3 = new Series<double>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            if (!Bars.BarsType.IsIntraday)
                return;
            if (CurrentBar < 1)
                return;

            int ibStartTime = ToTime(SessionStartTime);
            int ibEndTime = ToTime(SessionStartTime.AddMinutes(InitialBalanceDurationMinutes));
            int sessionEndTime = ToTime(SessionEndTime);
            int prevBarEndTime = ToTime(Time[1]);
            int currentBarEndTime = ToTime(Time[0]);
            int currentBarStartTime = prevBarEndTime;

            if (BarsInProgress == 0)
            {
                if (_ibRegions.Count == 0)
                    return;

                if (NumDays > 0 && _ibRegions.Count > NumDays)
                {
                    // Need to trim oldest IB region.
                    var oldestIBRegion = _ibRegions[0];
                    _ibRegions.RemoveAt(0);
                    CleanupIBRegion(oldestIBRegion);
                }

                var currentIBRegion = _ibRegions.Last();

                if (currentIBRegion.StartBarIndex == -1)
                {
                    DateTime regionStartTime = currentIBRegion.StartTime;
                    int regionStartBar = Bars.GetBar(regionStartTime);
                    if (ToTime(Time[CurrentBar - regionStartBar]) == ibStartTime)
                        regionStartBar++; // We found the bar that ENDS at the IB start time, but we want to draw region from the start bar onwards, so start from next bar on chart.
                    currentIBRegion.StartBarIndex = regionStartBar;
                }

                // Highlight region
                if (HighlightIBRegion)
                {
                    if ((currentIBRegion.IsConfirmed && string.IsNullOrEmpty(currentIBRegion.RegionTag)) ||
                        (!currentIBRegion.IsConfirmed && ShowDevelopingRegion && (string.IsNullOrEmpty(currentIBRegion.RegionTag) || IsFirstTickOfBar)))
                    {
                        currentIBRegion.RegionTag = $"LunarTickInitialBalance-{currentIBRegion.StartTime.ToString("yyyyMMdd-HHmm")}-Region";
                        DateTime regionEndTime = Time[0] < currentIBRegion.EndTime ? Time[0] : currentIBRegion.EndTime;
                        int regionEndBar = Bars.GetBar(regionEndTime);
                        currentIBRegion.EndBarIndex = regionEndBar;
                        Draw.Rectangle(this, currentIBRegion.RegionTag, false, CurrentBar - currentIBRegion.StartBarIndex, currentIBRegion.High.Value, CurrentBar - currentIBRegion.EndBarIndex, currentIBRegion.Low.Value, Brushes.Transparent, HighlightIBRegionColor, HighlightIBRegionOpacity);
                    }
                }

                // Draw lines and (optional) range (only after IB region is confirmed).
                if (currentIBRegion.IsConfirmed && (Time[0] > currentIBRegion.StartTime) && !currentIBRegion.IsDisplayed)
                {
                    DisplayInitialBalance(currentIBRegion);
                }

                // Populate data series (only after IB region is confirmed).
                if (currentIBRegion.IsConfirmed && (Time[0] > currentIBRegion.StartTime) && (CurrentBar > _lastBarUpdated))
                {
                    PopulateDataSeries(currentIBRegion, 0);
                    _lastBarUpdated = CurrentBar;
                }

                // Cleanup labels after end of IB range.
                if (IsFirstTickOfBar && (Time[1] <= currentIBRegion.SessionEndTime) && (Time[0] > currentIBRegion.SessionEndTime))
                {
                    CleanupIBLabels();
                }
            }
            else if (BarsInProgress == 1)
            {
                InitialBalanceRegion? currentIBRegion = null;

                // Check for start of new IB region.
                if (IsFirstTickOfBar && currentBarStartTime == ibStartTime)
                {
                    DateTime dtStartTime = Time[1];
                    DateTime dtEndTime = dtStartTime.AddMinutes(InitialBalanceDurationMinutes);
                    DateTime dtSessionEndTime = new DateTime(dtStartTime.Year, dtStartTime.Month, dtStartTime.Day, SessionEndTime.Hour, SessionEndTime.Minute, 0, dtStartTime.Kind);
                    if (sessionEndTime < ibStartTime)
                        dtSessionEndTime = dtSessionEndTime.AddDays(1);
                    currentIBRegion = new InitialBalanceRegion(Instrument.MasterInstrument, dtStartTime, dtEndTime, dtSessionEndTime);
                    _ibRegions.Add(currentIBRegion);
                }
                else if (_ibRegions.Count > 0)
                {
                    currentIBRegion = _ibRegions.Last();
                }

                if (currentIBRegion == null)
                    return;

                if (!currentIBRegion.IsConfirmed)
                {
                    bool regionChanged = false;

                    if (Time[1] >= currentIBRegion.EndTime)
                    {
                        // IB region is now confirmed.
                        currentIBRegion.IsConfirmed = true;
                        currentIBRegion.RegionTag = null;
                    }
                    else
                    {
                        regionChanged = currentIBRegion.UpdateRange(Low[0], High[0]);
                        if (regionChanged)
                            currentIBRegion.RegionTag = null;
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private void PopulateDataSeries(InitialBalanceRegion ibRegion, int barsAgo)
        {
            if (ibRegion == null)
                return;
            if (!ibRegion.IsConfirmed)
                return;
            if (!ibRegion.IsDisplayed)
                return;

            int barIdx = CurrentBar - barsAgo;
            int rangeEndBar = Bars.GetBar(ibRegion.SessionEndTime);

            if (barIdx < ibRegion.StartBarIndex || barIdx > rangeEndBar)
                return;

            bool showExt1 = ExtensionLevels != InitialBalanceEnums.InitialBalanceExtensionLevels.None;
            bool showExt2 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Two || ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;


            IBHigh[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBHigh);
            IBLow[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBLow);
            if (showExt1)
            {
                IBHighExt1[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt1);
                IBLowExt1[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt1);
            }
            if (showExt2)
            {
                IBHighExt2[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt2);
                IBLowExt2[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt2);
            }
            if (showExt3)
            {
                IBHighExt3[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt3);
                IBLowExt3[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt3);
            }
            if (ShowMidLevels)
            {
                IBMid[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBMid);
                if (showExt1)
                {
                    IBHighExt1Mid[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt1Mid);
                    IBLowExt1Mid[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt1Mid);
                }
                if (showExt2)
                {
                    IBHighExt2Mid[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt2Mid);
                    IBLowExt2Mid[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt2Mid);
                }
                if (showExt3)
                {
                    IBHighExt3Mid[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt3Mid);
                    IBLowExt3Mid[barsAgo] = ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt3Mid);
                }
            }
        }

        private void DisplayInitialBalance(InitialBalanceRegion ibRegion)
        {
            if (ibRegion == null)
                return;
            if (!ibRegion.IsConfirmed)
                return;
            if (ibRegion.IsDisplayed)
                return;

            int rangeEndBar = Bars.GetBar(ibRegion.SessionEndTime);

            // Draw range.
            if (HighlightIBRange && string.IsNullOrEmpty(ibRegion.RangeTag))
            {
                ibRegion.RangeTag = $"LunarTickInitialBalance-{ibRegion.StartTime.ToString("yyyyMMdd-HHmm")}-Range";
                Draw.Rectangle(this, ibRegion.RangeTag, false, Time[CurrentBar - ibRegion.StartBarIndex], ibRegion.High.Value, ibRegion.SessionEndTime, ibRegion.Low.Value, Brushes.Transparent, HighlightIBRangeColor, HighlightIBRangeOpacity);
            }

            // Draw lines.
            bool showExt1 = ExtensionLevels != InitialBalanceEnums.InitialBalanceExtensionLevels.None;
            bool showExt2 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Two || ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            DebugPrint($"ibRegion.StartTime: {ibRegion.StartTime.ToString("yyyyMMdd-HHmm")}");
            DebugPrint($"Time[CurrentBar - ibRegion.StartBarIndex]: {Time[CurrentBar - ibRegion.StartBarIndex].ToString("yyyyMMdd-HHmm")}");
            UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBHigh", ibRegion.GetIBLevel(InitialBalanceLevels.IBHigh), HighColor, HighLineStyle, HighLineWidth, "IB High", _labelFont);
            UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBLow", ibRegion.GetIBLevel(InitialBalanceLevels.IBLow), LowColor, LowLineStyle, LowLineWidth, "IB Low", _labelFont);
            if (showExt1)
            {
                UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBHighExt1", ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt1), HighColor, HighLineStyle, HighLineWidth, "IB High Ext 1", _labelFont);
                UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBLowExt1", ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt1), LowColor, LowLineStyle, LowLineWidth, "IB Low Ext 1", _labelFont);
            }
            if (showExt2)
            {
                UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBHighExt2", ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt2), HighColor, HighLineStyle, HighLineWidth, "IB High Ext 2", _labelFont);
                UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBLowExt2", ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt2), LowColor, LowLineStyle, LowLineWidth, "IB Low Ext 2", _labelFont);
            }
            if (showExt3)
            {
                UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBHighExt3", ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt3), HighColor, HighLineStyle, HighLineWidth, "IB High Ext 3", _labelFont);
                UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBLowExt3", ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt3), LowColor, LowLineStyle, LowLineWidth, "IB Low Ext 3", _labelFont);
            }
            if (ShowMidLevels)
            {
                UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBMid", ibRegion.Mid.Value, MidColor, MidLineStyle, MidLineWidth, "IB Mid", _labelFont);
                if (showExt1)
                {
                    UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBHighExt1Mid", ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt1Mid), MidColor, MidLineStyle, MidLineWidth, "IB High Ext 1 Mid", _labelFont);
                    UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBLowExt1Mid", ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt1Mid), MidColor, MidLineStyle, MidLineWidth, "IB Low Ext 1 Mid", _labelFont);
                }
                if (showExt2)
                {
                    UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBHighExt2Mid", ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt2Mid), MidColor, MidLineStyle, MidLineWidth, "IB High Ext 2 Mid", _labelFont);
                    UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBLowExt2Mid", ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt2Mid), MidColor, MidLineStyle, MidLineWidth, "IB Low Ext 2 Mid", _labelFont);
                }
                if (showExt3)
                {
                    UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBHighExt3Mid", ibRegion.GetIBLevel(InitialBalanceLevels.IBHighExt3Mid), MidColor, MidLineStyle, MidLineWidth, "IB High Ext 3 Mid", _labelFont);
                    UpdateLineAndLabel(ibRegion, "LunarTickInitialBalance", "IBLowExt3Mid", ibRegion.GetIBLevel(InitialBalanceLevels.IBLowExt3Mid), MidColor, MidLineStyle, MidLineWidth, "IB Low Ext 3 Mid", _labelFont);
                }
            }

            ibRegion.IsDisplayed = true;
        }

        private void UpdateLineAndLabel(InitialBalanceRegion ibRegion, string tagPrefix, string name, double yValue, Brush lineBrush, DashStyleHelper lineStyle, int lineWidth, string labelText, SimpleFont labelFont)
        {
            DateTime dtStartTime = Time[CurrentBar - ibRegion.StartBarIndex];
            DateTime dtEndTime = ibRegion.SessionEndTime;
            Draw.Line(this, $"{tagPrefix}-{ibRegion.StartTime.ToString("yyyyMMdd-HHmm")}-{name}-Line", false, dtStartTime, yValue, dtEndTime, yValue, lineBrush, lineStyle, lineWidth);
            if (ShowLabels)
            {
                Draw.Text(this, $"{tagPrefix}-{name}-Label", false, $" {yValue.ToString($"F{_decimalPlaces}")} {labelText}", dtEndTime, yValue, 0, lineBrush, labelFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }
        }

        private void CleanupIBRegion(InitialBalanceRegion deadIBRegion)
        {
            var prefix = $"LunarTickInitialBalance-{deadIBRegion.StartTime.ToString("yyyyMMdd-HHmm")}";
            DebugPrint($"Removing {prefix}");
            RemoveDrawObject($"{prefix}-Region");
            RemoveDrawObject($"{prefix}-Range");
            bool showExt1 = ExtensionLevels != InitialBalanceEnums.InitialBalanceExtensionLevels.None;
            bool showExt2 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Two || ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            RemoveDrawObject($"{prefix}-IBHigh-Line");
            RemoveDrawObject($"{prefix}-IBLow-Line");
            if (showExt1)
            {
                RemoveDrawObject($"{prefix}-IBHighExt1-Line");
                RemoveDrawObject($"{prefix}-IBLowExt1-Line");
            }
            if (showExt2)
            {
                RemoveDrawObject($"{prefix}-IBHighExt2-Line");
                RemoveDrawObject($"{prefix}-IBLowExt2-Line");
            }
            if (showExt3)
            {
                RemoveDrawObject($"{prefix}-IBHighExt3-Line");
                RemoveDrawObject($"{prefix}-IBLowExt3-Line");
            }
            if (ShowMidLevels)
            {
                RemoveDrawObject($"{prefix}-IBMid-Line");
                if (showExt1)
                {
                    RemoveDrawObject($"{prefix}-IBHighExt1Mid-Line");
                    RemoveDrawObject($"{prefix}-IBLowExt1Mid-Line");
                }
                if (showExt2)
                {
                    RemoveDrawObject($"{prefix}-IBHighExt2Mid-Line");
                    RemoveDrawObject($"{prefix}-IBLowExt2Mid-Line");
                }
                if (showExt3)
                {
                    RemoveDrawObject($"{prefix}-IBHighExt3Mid-Line");
                    RemoveDrawObject($"{prefix}-IBLowExt3Mid-Line");
                }
            }
        }

        private void CleanupIBLabels()
        {
            DebugPrint("Removing InitialBalance labels");
            bool showExt1 = ExtensionLevels != InitialBalanceEnums.InitialBalanceExtensionLevels.None;
            bool showExt2 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Two || ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            RemoveDrawObject("LunarTickInitialBalance-IBHigh-Label");
            RemoveDrawObject("LunarTickInitialBalance-IBLow-Label");
            if (showExt1)
            {
                RemoveDrawObject("LunarTickInitialBalance-IBHighExt1-Label");
                RemoveDrawObject("LunarTickInitialBalance-IBLowExt1-Label");
            }
            if (showExt2)
            {
                RemoveDrawObject("LunarTickInitialBalance-IBHighExt2-Label");
                RemoveDrawObject("LunarTickInitialBalance-IBLowExt2-Label");
            }
            if (showExt3)
            {
                RemoveDrawObject("LunarTickInitialBalance-IBHighExt3-Label");
                RemoveDrawObject("LunarTickInitialBalance-IBLowExt3-Label");
            }
            if (ShowMidLevels)
            {
                RemoveDrawObject("LunarTickInitialBalance-IBMid-Label");
                if (showExt1)
                {
                    RemoveDrawObject("LunarTickInitialBalance-IBHighExt1Mid-Label");
                    RemoveDrawObject("LunarTickInitialBalance-IBLowExt1Mid-Label");
                }
                if (showExt2)
                {
                    RemoveDrawObject("LunarTickInitialBalance-IBHighExt2Mid-Label");
                    RemoveDrawObject("LunarTickInitialBalance-IBLowExt2Mid-Label");
                }
                if (showExt3)
                {
                    RemoveDrawObject("LunarTickInitialBalance-IBHighExt3Mid-Label");
                    RemoveDrawObject("LunarTickInitialBalance-IBLowExt3Mid-Label");
                }
            }
        }

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"LunarTickInitialBalance[{Instrument.FullName}]: {msg}");
                else
                    Print($"LunarTickInitialBalance: {msg}");
            }
        }

        #endregion
    }

    public class LunarTickInitialBalanceTypeConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            // We need the indicator instance which actually exists on the grid
            LunarTickInitialBalance indicator = component as LunarTickInitialBalance;

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

            if (indicator.ExtensionLevels == InitialBalanceExtensionLevels.One)
            {
                propertyDescriptorCollection.Add(extLevel1MultiplierPropertyDesc);
            }
            else if (indicator.ExtensionLevels == InitialBalanceExtensionLevels.Two)
            {
                propertyDescriptorCollection.Add(extLevel1MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(extLevel2MultiplierPropertyDesc);
            }
            else if (indicator.ExtensionLevels == InitialBalanceExtensionLevels.Three)
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


            // Show/hide properties related to HighlightIBRegion
            PropertyDescriptor highlightIBRegionColorPropertyDesc = propertyDescriptorCollection["HighlightIBRegionColor"];
            PropertyDescriptor highlightIBRegionOpacityPropertyDesc = propertyDescriptorCollection["HighlightIBRegionOpacity"];
            propertyDescriptorCollection.Remove(highlightIBRegionColorPropertyDesc);
            propertyDescriptorCollection.Remove(highlightIBRegionOpacityPropertyDesc);
            if (indicator.HighlightIBRegion)
            {
                propertyDescriptorCollection.Add(highlightIBRegionColorPropertyDesc);
                propertyDescriptorCollection.Add(highlightIBRegionOpacityPropertyDesc);
            }


            // Show/hide properties related to HighlightIBRange
            PropertyDescriptor highlightIBRangeColorPropertyDesc = propertyDescriptorCollection["HighlightIBRangeColor"];
            PropertyDescriptor highlightIBRangeOpacityPropertyDesc = propertyDescriptorCollection["HighlightIBRangeOpacity"];
            propertyDescriptorCollection.Remove(highlightIBRangeColorPropertyDesc);
            propertyDescriptorCollection.Remove(highlightIBRangeOpacityPropertyDesc);
            if (indicator.HighlightIBRange)
            {
                propertyDescriptorCollection.Add(highlightIBRangeColorPropertyDesc);
                propertyDescriptorCollection.Add(highlightIBRangeOpacityPropertyDesc);
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
		private LunarTick.LunarTickInitialBalance[] cacheLunarTickInitialBalance;
		public LunarTick.LunarTickInitialBalance LunarTickInitialBalance(DateTime sessionStartTime, DateTime sessionEndTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return LunarTickInitialBalance(Input, sessionStartTime, sessionEndTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public LunarTick.LunarTickInitialBalance LunarTickInitialBalance(ISeries<double> input, DateTime sessionStartTime, DateTime sessionEndTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			if (cacheLunarTickInitialBalance != null)
				for (int idx = 0; idx < cacheLunarTickInitialBalance.Length; idx++)
					if (cacheLunarTickInitialBalance[idx] != null && cacheLunarTickInitialBalance[idx].SessionStartTime == sessionStartTime && cacheLunarTickInitialBalance[idx].SessionEndTime == sessionEndTime && cacheLunarTickInitialBalance[idx].ExtensionLevels == extensionLevels && cacheLunarTickInitialBalance[idx].ExtLevel1Multiplier == extLevel1Multiplier && cacheLunarTickInitialBalance[idx].ExtLevel2Multiplier == extLevel2Multiplier && cacheLunarTickInitialBalance[idx].ExtLevel3Multiplier == extLevel3Multiplier && cacheLunarTickInitialBalance[idx].EqualsInput(input))
						return cacheLunarTickInitialBalance[idx];
			return CacheIndicator<LunarTick.LunarTickInitialBalance>(new LunarTick.LunarTickInitialBalance(){ SessionStartTime = sessionStartTime, SessionEndTime = sessionEndTime, ExtensionLevels = extensionLevels, ExtLevel1Multiplier = extLevel1Multiplier, ExtLevel2Multiplier = extLevel2Multiplier, ExtLevel3Multiplier = extLevel3Multiplier }, input, ref cacheLunarTickInitialBalance);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.LunarTickInitialBalance LunarTickInitialBalance(DateTime sessionStartTime, DateTime sessionEndTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.LunarTickInitialBalance(Input, sessionStartTime, sessionEndTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public Indicators.LunarTick.LunarTickInitialBalance LunarTickInitialBalance(ISeries<double> input , DateTime sessionStartTime, DateTime sessionEndTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.LunarTickInitialBalance(input, sessionStartTime, sessionEndTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.LunarTickInitialBalance LunarTickInitialBalance(DateTime sessionStartTime, DateTime sessionEndTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.LunarTickInitialBalance(Input, sessionStartTime, sessionEndTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public Indicators.LunarTick.LunarTickInitialBalance LunarTickInitialBalance(ISeries<double> input , DateTime sessionStartTime, DateTime sessionEndTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.LunarTickInitialBalance(input, sessionStartTime, sessionEndTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}
	}
}

#endregion

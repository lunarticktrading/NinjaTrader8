#region Using declarations
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeEnums;
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
    namespace OpeningRangeEnums
    {
        public enum ExtendPlots
        {
            EndOfSession,
            NextOpeningRange
        }

        public enum OpeningRangeExtensionLevels
        {
            None,
            One,
            Two,
            Three
        }

        public enum OpeningRangePlots
        {
            ORHighExt3,
            ORHighExt3Mid,
            ORHighExt2,
            ORHighExt2Mid,
            ORHighExt1,
            ORHighExt1Mid,
            ORHigh,
            ORMid,
            ORLow,
            ORLowExt1Mid,
            ORLowExt1,
            ORLowExt2Mid,
            ORLowExt2,
            ORLowExt3Mid,
            ORLowExt3
        }
    }

    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Display", 2)]
    [Gui.CategoryOrder("Developer", 3)]
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeTypeConverter")]
    public class OpeningRange : Indicator
    {
        public class OpeningRangeRegion
        {
            public OpeningRangeRegion(MasterInstrument instrument, DateTime startTime, DateTime endTime, DateTime sessionEndTime)
            {
                Instrument = instrument;
                StartTime = startTime;
                EndTime = endTime;
                SessionEndTime = sessionEndTime;
                RegionTag = null;
                High = null;
                Low = null;
                IsConfirmed = false;
                IsBackFilled = false;
                StartBarIndex = -1;
                EndBarIndex = -1;
                LastPlotBarIndex = -1;
            }

            public MasterInstrument Instrument { get; }
            public DateTime StartTime { get; }
            public DateTime EndTime { get; }
            public DateTime SessionEndTime { get; }
            public string? RegionTag { get; set; }
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
            public bool IsBackFilled { get; set; }
            public int StartBarIndex { get; set; }
            public int EndBarIndex { get; set; }
            public int LastPlotBarIndex { get; set; }

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

            private bool _isConfirmed = false;
        }

        #region Constants

        public const string Version = "1.0.0";

        #endregion

        #region Members

        private List<OpeningRangeRegion> _ibRegions = new List<OpeningRangeRegion>();
        private Brush _highlightTimeframeBrush;
        private int _lastBarPlotted = -1;

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
        public NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeEnums.OpeningRangeExtensionLevels ExtensionLevels
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

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight Timeframe", Order = 2, GroupName = "Display")]
        public bool HighlightTimeframe
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Highlight Timeframe Color", Order = 3, GroupName = "Display")]
        public Brush HighlightTimeframeColor
        { get; set; }

        [Browsable(false)]
        public string HighlightTimeframeColorSerializable
        {
            get { return Serialize.BrushToString(HighlightTimeframeColor); }
            set { HighlightTimeframeColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Highlight Timeframe Opacity", Order = 4, GroupName = "Display")]
        public int HighlightTimeframeOpacity
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight Region", Order = 5, GroupName = "Display")]
        public bool HighlightRegion
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Highlight Region Color", Order = 6, GroupName = "Display")]
        public Brush HighlightRegionColor
        { get; set; }

        [Browsable(false)]
        public string HighlightRegionColorSerializable
        {
            get { return Serialize.BrushToString(HighlightRegionColor); }
            set { HighlightRegionColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Highlight Region Opacity", Order = 7, GroupName = "Display")]
        public int HighlightRegionOpacity
        { get; set; }

        [Display(Name = "Extend Levels Until", Order = 8, GroupName = "Display")]
        public NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeEnums.ExtendPlots ExtendLevelsUntil
        { get; set; }

        [Display(Name = "Show 50% Levels", Order = 9, GroupName = "Display")]
        public bool ShowMidLevels
        { get; set; }

        [Display(Name = "Show Developing Region", Order = 10, GroupName = "Display")]
        public bool ShowDevelopingRegion
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Labels", Order = 11, GroupName = "Display")]
        public bool ShowLabels
        { get; set; }

        [Range(0, 100)]
        [Display(Name = "Label Offset", Order = 12, GroupName = "Display")]
        public int LabelOffset
        { get; set; }

        [Display(Name = "Hide Joins", Order = 13, GroupName = "Display")]
        public bool HideJoins
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
        public Series<double> ORHighExt3
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORHighExt3Mid
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORHighExt2
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORHighExt2Mid
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORHighExt1
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORHighExt1Mid
        {
            get { return Values[5]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORHigh
        {
            get { return Values[6]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORMid
        {
            get { return Values[7]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORLow
        {
            get { return Values[8]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORLowExt1Mid
        {
            get { return Values[9]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORLowExt1
        {
            get { return Values[10]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORLowExt2Mid
        {
            get { return Values[11]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORLowExt2
        {
            get { return Values[12]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORLowExt3Mid
        {
            get { return Values[13]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORLowExt3
        {
            get { return Values[14]; }
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
                Description = @"An Opening Range indicator, supporting extension levels and 50% levels.";
                Name = "Opening Range";
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
                SessionEndTime = DateTime.Parse("17:00:00");
                OpeningRangeDurationMinutes = 15;
                ExtensionLevels = OpeningRangeEnums.OpeningRangeExtensionLevels.None;
                ExtLevel1Multiplier = 1;
                ExtLevel2Multiplier = 2;
                ExtLevel3Multiplier = 3;
                NumDays = 5;
                HighlightTimeframe = false;
                HighlightTimeframeColor = Brushes.Purple;
                HighlightTimeframeOpacity = 20;
                HighlightRegion = true;
                HighlightRegionColor = Brushes.Magenta;
                HighlightRegionOpacity = 20;
                ExtendLevelsUntil = ExtendPlots.EndOfSession;
                ShowMidLevels = true;
                ShowDevelopingRegion = true;
                ShowLabels = true;
                LabelOffset = 3;
                HideJoins = true;

                VersionInformation = $"{Version} - {Assembly.GetAssembly(typeof(OpeningRange)).GetName().Version}";
                Debug = false;

                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "OR High Ext3");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "OR High Ext3 50%");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "OR High Ext2");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "OR High Ext2 50%");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "OR High Ext1");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "OR High Ext1 50%");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "OR High");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "OR 50%");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "OR Low");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "OR Low Ext1 50%");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "OR Low Ext1");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "OR Low Ext2 50%");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "OR Low Ext2");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "OR Low Ext3 50%");
                AddPlot(new Stroke(Brushes.White, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "OR Low Ext3");
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Minute, 1);

                SolidColorBrush highlightTimeframeBrush = HighlightTimeframeColor as SolidColorBrush;
                _highlightTimeframeBrush = new SolidColorBrush(Color.FromArgb((byte)((HighlightTimeframeOpacity / 100.0) * 255.0), highlightTimeframeBrush.Color.R, highlightTimeframeBrush.Color.G, highlightTimeframeBrush.Color.B));
                _highlightTimeframeBrush.Freeze();
            }
            else if (State == State.DataLoaded)
            {
                _ibRegions.Clear();
                _lastBarPlotted = -1;
            }
        }

        protected override void OnBarUpdate()
        {
            if (!Bars.BarsType.IsIntraday)
                return;
            if (CurrentBar < 1)
                return;

            int ibStartTime = ToTime(SessionStartTime);
            int ibEndTime = ToTime(SessionStartTime.AddMinutes(OpeningRangeDurationMinutes));
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
                    // Need to trim oldest OR region.
                    var oldestORRegion = _ibRegions[0];
                    _ibRegions.RemoveAt(0);
                    CleanupRegion(oldestORRegion);
                }

                var currentORRegion = _ibRegions.Last();

                if (currentORRegion.StartBarIndex == -1)
                {
                    DateTime regionStartTime = currentORRegion.StartTime;
                    int regionStartBar = Bars.GetBar(regionStartTime);
                    if (ToTime(Time[CurrentBar - regionStartBar]) == ibStartTime)
                        regionStartBar++; // We found the bar that ENDS at the OR start time, but we want to draw region from the start bar onwards, so start from next bar on chart.
                    currentORRegion.StartBarIndex = regionStartBar;
                }

                // Highlight timeframe
                if (HighlightTimeframe)
                {
                    if (Time[1] >= currentORRegion.StartTime && Time[0] <= currentORRegion.EndTime)
                    {
                        BackBrush = _highlightTimeframeBrush;
                    }
                }

                // Highlight region
                if (HighlightRegion)
                {
                    if ((currentORRegion.IsConfirmed && string.IsNullOrEmpty(currentORRegion.RegionTag)) ||
                        (!currentORRegion.IsConfirmed && ShowDevelopingRegion && (string.IsNullOrEmpty(currentORRegion.RegionTag) || IsFirstTickOfBar)))
                    {
                        currentORRegion.RegionTag = $"OpeningRange-{currentORRegion.StartTime.ToString("yyyyMMdd-HHmm")}";
                        DateTime regionEndTime = Time[0] < currentORRegion.EndTime ? Time[0] : currentORRegion.EndTime;
                        int regionEndBar = Bars.GetBar(regionEndTime);
                        currentORRegion.EndBarIndex = regionEndBar;
                        Draw.Rectangle(this, currentORRegion.RegionTag, false, CurrentBar - currentORRegion.StartBarIndex, currentORRegion.High.Value, CurrentBar - currentORRegion.EndBarIndex, currentORRegion.Low.Value, Brushes.Transparent, HighlightRegionColor, HighlightRegionOpacity);
                    }
                }

                // Populate plots (only after OR region is confirmed).
                if (currentORRegion.IsConfirmed && (Time[0] > currentORRegion.StartTime) && (CurrentBar > _lastBarPlotted))
                {
                    int startBarsAgo = 0;
                    if (!currentORRegion.IsBackFilled)
                    {
                        if (HideJoins)
                            ClearPlots((CurrentBar - currentORRegion.StartBarIndex) + 1);

                        for (int i = CurrentBar - currentORRegion.StartBarIndex; i >= 0; i--)
                        {
                            PopulatePlots(currentORRegion, i, false);
                        }
                        currentORRegion.IsBackFilled = true;
                    }

                    PopulatePlots(currentORRegion, 0, ShowLabels);
                    _lastBarPlotted = CurrentBar;
                }
            }
            else if (BarsInProgress == 1)
            {
                OpeningRangeRegion? currentORRegion = null;

                // Check for start of new OR region.
                if (IsFirstTickOfBar && currentBarStartTime == ibStartTime)
                {
                    DateTime dtStartTime = Time[1];
                    DateTime dtEndTime = dtStartTime.AddMinutes(OpeningRangeDurationMinutes);
                    DateTime dtSessionEndTime = new DateTime(dtStartTime.Year, dtStartTime.Month, dtStartTime.Day, SessionEndTime.Hour, SessionEndTime.Minute, 0, dtStartTime.Kind);
                    if (sessionEndTime < ibStartTime)
                        dtSessionEndTime = dtSessionEndTime.AddDays(1);
                    currentORRegion = new OpeningRangeRegion(Instrument.MasterInstrument, dtStartTime, dtEndTime, dtSessionEndTime);
                    _ibRegions.Add(currentORRegion);
                }
                else if (_ibRegions.Count > 0)
                {
                    currentORRegion = _ibRegions.Last();
                }

                if (currentORRegion == null)
                    return;

                if (!currentORRegion.IsConfirmed)
                {
                    bool regionChanged = false;

                    if (Time[1] >= currentORRegion.EndTime)
                    {
                        // OR region is now confirmed.
                        currentORRegion.IsConfirmed = true;
                        currentORRegion.RegionTag = null;
                    }
                    else
                    {
                        regionChanged = currentORRegion.UpdateRange(Low[0], High[0]);
                        if (regionChanged)
                            currentORRegion.RegionTag = null;
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private void PopulatePlots(OpeningRangeRegion ibRegion, int barsAgo, bool showLabels)
        {
            if (barsAgo >= ORHigh.Count)
                return;

            if (ExtendLevelsUntil == ExtendPlots.EndOfSession)
            {
                var barStartTime = Time[barsAgo + 1]; // Use end time of previous bar
                if (barStartTime >= ibRegion.SessionEndTime)
                    return;
            }

            bool showExt1 = ExtensionLevels != OpeningRangeEnums.OpeningRangeExtensionLevels.None;
            bool showExt2 = ExtensionLevels == OpeningRangeEnums.OpeningRangeExtensionLevels.Two || ExtensionLevels == OpeningRangeEnums.OpeningRangeExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == OpeningRangeEnums.OpeningRangeExtensionLevels.Three;
            double delta = ibRegion.High.Value - ibRegion.Low.Value;
            SimpleFont font = (SimpleFont)ChartControl.Properties.LabelFont.Clone();
            font.Size = 14;


            ORHigh[barsAgo] = ibRegion.High.Value;
            ORLow[barsAgo] = ibRegion.Low.Value;
            if (showLabels)
            {
                Draw.Text(this, "ORHighLabel", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORHigh].Name}", -LabelOffset, ORHigh[barsAgo], 0, Plots[(int)OpeningRangePlots.ORHigh].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                Draw.Text(this, "ORLowLabel", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORLow].Name}", -LabelOffset, ORLow[barsAgo], 0, Plots[(int)OpeningRangePlots.ORLow].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (showExt1)
            {
                ORHighExt1[barsAgo] = ibRegion.High.Value + delta;
                ORLowExt1[barsAgo] = ibRegion.Low.Value - delta;
                if (showLabels)
                {
                    Draw.Text(this, "ORHighExt1Label", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORHighExt1].Name}", -LabelOffset, ORHighExt1[barsAgo], 0, Plots[(int)OpeningRangePlots.ORHighExt1].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    Draw.Text(this, "ORLowExt1Label", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORLowExt1].Name}", -LabelOffset, ORLowExt1[barsAgo], 0, Plots[(int)OpeningRangePlots.ORLowExt1].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                }
            }
            if (showExt2)
            {
                ORHighExt2[barsAgo] = ibRegion.High.Value + (2 * delta);
                ORLowExt2[barsAgo] = ibRegion.Low.Value - (2 * delta);
                if (showLabels)
                {
                    Draw.Text(this, "ORHighExt2Label", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORHighExt2].Name}", -LabelOffset, ORHighExt2[barsAgo], 0, Plots[(int)OpeningRangePlots.ORHighExt2].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    Draw.Text(this, "ORLowExt2Label", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORLowExt2].Name}", -LabelOffset, ORLowExt2[barsAgo], 0, Plots[(int)OpeningRangePlots.ORLowExt2].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                }
            }
            if (showExt3)
            {
                ORHighExt3[barsAgo] = ibRegion.High.Value + (3 * delta);
                ORLowExt3[barsAgo] = ibRegion.Low.Value - (3 * delta);
                if (showLabels)
                {
                    Draw.Text(this, "ORHighExt3Label", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORHighExt3].Name}", -LabelOffset, ORHighExt3[barsAgo], 0, Plots[(int)OpeningRangePlots.ORHighExt3].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    Draw.Text(this, "ORLowExt3Label", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORLowExt3].Name}", -LabelOffset, ORLowExt3[barsAgo], 0, Plots[(int)OpeningRangePlots.ORLowExt3].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                }
            }

            if (ShowMidLevels)
            {
                ORMid[barsAgo] = ibRegion.Mid.Value;
                if (showLabels)
                {
                    Draw.Text(this, "ORMidLabel", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORMid].Name}", -LabelOffset, ORMid[barsAgo], 0, Plots[(int)OpeningRangePlots.ORMid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                }

                if (showExt1)
                {
                    ORHighExt1Mid[barsAgo] = ibRegion.Mid.Value + delta;
                    ORLowExt1Mid[barsAgo] = ibRegion.Mid.Value - delta;
                    if (showLabels)
                    {
                        Draw.Text(this, "ORHighExt1MidLabel", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORHighExt1Mid].Name}", -LabelOffset, ORHighExt1Mid[barsAgo], 0, Plots[(int)OpeningRangePlots.ORHighExt1Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                        Draw.Text(this, "ORLowExt1MidLabel", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORLowExt1Mid].Name}", -LabelOffset, ORLowExt1Mid[barsAgo], 0, Plots[(int)OpeningRangePlots.ORLowExt1Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                }
                if (showExt2)
                {
                    ORHighExt2Mid[barsAgo] = ibRegion.Mid.Value + (2 * delta);
                    ORLowExt2Mid[barsAgo] = ibRegion.Mid.Value - (2 * delta);
                    if (showLabels)
                    {
                        Draw.Text(this, "ORHighExt2MidLabel", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORHighExt2Mid].Name}", -LabelOffset, ORHighExt2Mid[barsAgo], 0, Plots[(int)OpeningRangePlots.ORHighExt2Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                        Draw.Text(this, "ORLowExt2MidLabel", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORLowExt2Mid].Name}", -LabelOffset, ORLowExt2Mid[barsAgo], 0, Plots[(int)OpeningRangePlots.ORLowExt2Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                }
                if (showExt3)
                {
                    ORHighExt3Mid[barsAgo] = ibRegion.Mid.Value + (3 * delta);
                    ORLowExt3Mid[barsAgo] = ibRegion.Mid.Value - (3 * delta);
                    if (showLabels)
                    {
                        Draw.Text(this, "ORHighExt3MidLabel", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORHighExt3Mid].Name}", -LabelOffset, ORHighExt3Mid[barsAgo], 0, Plots[(int)OpeningRangePlots.ORHighExt3Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                        Draw.Text(this, "ORLowExt3MidLabel", false, $"{OpeningRangeDurationMinutes}m {Plots[(int)OpeningRangePlots.ORLowExt3Mid].Name}", -LabelOffset, ORLowExt3Mid[barsAgo], 0, Plots[(int)OpeningRangePlots.ORLowExt3Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                }
            }

            ibRegion.LastPlotBarIndex = Math.Max(ibRegion.LastPlotBarIndex, CurrentBar - barsAgo);
        }

        private void ClearPlots(int barsAgo = 0)
        {
            bool showExt1 = ExtensionLevels != OpeningRangeEnums.OpeningRangeExtensionLevels.None;
            bool showExt2 = ExtensionLevels == OpeningRangeEnums.OpeningRangeExtensionLevels.Two || ExtensionLevels == OpeningRangeEnums.OpeningRangeExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == OpeningRangeEnums.OpeningRangeExtensionLevels.Three;

            ORHigh.Reset(barsAgo);
            ORLow.Reset(barsAgo);

            if (showExt1)
            {
                ORHighExt1.Reset(barsAgo);
                ORLowExt1.Reset(barsAgo);
            }
            if (showExt2)
            {
                ORHighExt2.Reset(barsAgo);
                ORLowExt2.Reset(barsAgo);
            }
            if (showExt3)
            {
                ORHighExt3.Reset(barsAgo);
                ORLowExt3.Reset(barsAgo);
            }

            if (ShowMidLevels)
            {
                ORMid.Reset(barsAgo);
                if (showExt1)
                {
                    ORHighExt1Mid.Reset(barsAgo);
                    ORLowExt1Mid.Reset(barsAgo);
                }
                if (showExt2)
                {
                    ORHighExt2Mid.Reset(barsAgo);
                    ORLowExt2Mid.Reset(barsAgo);
                }
                if (showExt3)
                {
                    ORHighExt3Mid.Reset(barsAgo);
                    ORLowExt3Mid.Reset(barsAgo);
                }
            }
        }

        private void CleanupRegion(OpeningRangeRegion deadRegion)
        {
            // Remove the region.
            DebugPrint($"Cleaning up region tag: {deadRegion.RegionTag}");
            RemoveDrawObject(deadRegion.RegionTag);

            // Clear the plots.
            if (deadRegion.StartBarIndex >= 0 && deadRegion.LastPlotBarIndex >= 0 && deadRegion.LastPlotBarIndex >= deadRegion.StartBarIndex)
            {
                DebugPrint($"Cleaning up region plots from bar index {deadRegion.StartBarIndex} to {deadRegion.LastPlotBarIndex} (CurrentBar = {CurrentBar})");
                for (int i = deadRegion.StartBarIndex; i <= deadRegion.LastPlotBarIndex; i++)
                {
                    ClearPlots(CurrentBar - i);
                }
            }
        }

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"OpeningRange[{Instrument.FullName}]: {msg}");
                else
                    Print($"OpeningRange: {msg}");
            }
        }

        #endregion
    }

    public class OpeningRangeTypeConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            // We need the indicator instance which actually exists on the grid
            OpeningRange indicator = component as OpeningRange;

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

            if (indicator.ExtensionLevels == OpeningRangeExtensionLevels.One)
            {
                propertyDescriptorCollection.Add(extLevel1MultiplierPropertyDesc);
            }
            else if (indicator.ExtensionLevels == OpeningRangeExtensionLevels.Two)
            {
                propertyDescriptorCollection.Add(extLevel1MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(extLevel2MultiplierPropertyDesc);
            }
            else if (indicator.ExtensionLevels == OpeningRangeExtensionLevels.Three)
            {
                propertyDescriptorCollection.Add(extLevel1MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(extLevel2MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(extLevel3MultiplierPropertyDesc);
            }


            // Show/hide properties related to HighlightTimeframe
            PropertyDescriptor highlightTimeframeColorPropertyDesc = propertyDescriptorCollection["HighlightTimeframeColor"];
            PropertyDescriptor highlightTimeframeOpacityPropertyDesc = propertyDescriptorCollection["HighlightTimeframeOpacity"];
            propertyDescriptorCollection.Remove(highlightTimeframeColorPropertyDesc);
            propertyDescriptorCollection.Remove(highlightTimeframeOpacityPropertyDesc);
            if (indicator.HighlightTimeframe)
            {
                propertyDescriptorCollection.Add(highlightTimeframeColorPropertyDesc);
                propertyDescriptorCollection.Add(highlightTimeframeOpacityPropertyDesc);
            }


            // Show/hide properties related to HighlightRegion
            PropertyDescriptor highlightRegionColorPropertyDesc = propertyDescriptorCollection["HighlightRegionColor"];
            PropertyDescriptor highlightRegionOpacityPropertyDesc = propertyDescriptorCollection["HighlightRegionOpacity"];
            propertyDescriptorCollection.Remove(highlightRegionColorPropertyDesc);
            propertyDescriptorCollection.Remove(highlightRegionOpacityPropertyDesc);
            if (indicator.HighlightRegion)
            {
                propertyDescriptorCollection.Add(highlightRegionColorPropertyDesc);
                propertyDescriptorCollection.Add(highlightRegionOpacityPropertyDesc);
            }


            // Show/hide properties related to ShowLabels
            PropertyDescriptor labelOffsetPropertyDesc = propertyDescriptorCollection["LabelOffset"];
            propertyDescriptorCollection.Remove(labelOffsetPropertyDesc);
            if (indicator.ShowLabels)
            {
                propertyDescriptorCollection.Add(labelOffsetPropertyDesc);
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
		private LunarTick.OpeningRange[] cacheOpeningRange;
		public LunarTick.OpeningRange OpeningRange(DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeEnums.OpeningRangeExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return OpeningRange(Input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public LunarTick.OpeningRange OpeningRange(ISeries<double> input, DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeEnums.OpeningRangeExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			if (cacheOpeningRange != null)
				for (int idx = 0; idx < cacheOpeningRange.Length; idx++)
					if (cacheOpeningRange[idx] != null && cacheOpeningRange[idx].SessionStartTime == sessionStartTime && cacheOpeningRange[idx].SessionEndTime == sessionEndTime && cacheOpeningRange[idx].OpeningRangeDurationMinutes == openingRangeDurationMinutes && cacheOpeningRange[idx].ExtensionLevels == extensionLevels && cacheOpeningRange[idx].ExtLevel1Multiplier == extLevel1Multiplier && cacheOpeningRange[idx].ExtLevel2Multiplier == extLevel2Multiplier && cacheOpeningRange[idx].ExtLevel3Multiplier == extLevel3Multiplier && cacheOpeningRange[idx].EqualsInput(input))
						return cacheOpeningRange[idx];
			return CacheIndicator<LunarTick.OpeningRange>(new LunarTick.OpeningRange(){ SessionStartTime = sessionStartTime, SessionEndTime = sessionEndTime, OpeningRangeDurationMinutes = openingRangeDurationMinutes, ExtensionLevels = extensionLevels, ExtLevel1Multiplier = extLevel1Multiplier, ExtLevel2Multiplier = extLevel2Multiplier, ExtLevel3Multiplier = extLevel3Multiplier }, input, ref cacheOpeningRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.OpeningRange OpeningRange(DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeEnums.OpeningRangeExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.OpeningRange(Input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public Indicators.LunarTick.OpeningRange OpeningRange(ISeries<double> input , DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeEnums.OpeningRangeExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.OpeningRange(input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.OpeningRange OpeningRange(DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeEnums.OpeningRangeExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.OpeningRange(Input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public Indicators.LunarTick.OpeningRange OpeningRange(ISeries<double> input , DateTime sessionStartTime, DateTime sessionEndTime, int openingRangeDurationMinutes, NinjaTrader.NinjaScript.Indicators.LunarTick.OpeningRangeEnums.OpeningRangeExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.OpeningRange(input, sessionStartTime, sessionEndTime, openingRangeDurationMinutes, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}
	}
}

#endregion

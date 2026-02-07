#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Reflection;
using NinjaTrader.NinjaScript.MarketAnalyzerColumns;
using NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums;
using NinjaTrader.NinjaScript.Indicators.LunarTick;
using NinjaTrader.NinjaScript;
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

        public enum InitialBalancePlots
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

    [Gui.CategoryOrder("[01] Parameters", 1)]
    [Gui.CategoryOrder("[02] Display", 2)]
    [Gui.CategoryOrder("[03] Developer", 3)]
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceTypeConverter")]
    public class InitialBalance : Indicator
	{
        public class InitialBalanceRegion
        {
            public InitialBalanceRegion(MasterInstrument instrument, DateTime startTime, DateTime endTime)
            {
                Instrument = instrument;
                StartTime = startTime;
                EndTime = endTime;
                RegionTag = null;
                High = null;
                Low = null;
                IsConfirmed = false;
                IsBackFilled = false;
                StartBarIndex = -1;
                EndBarIndex = -1;
            }

            public MasterInstrument Instrument { get; }
            public DateTime StartTime { get; }
            public DateTime EndTime { get; }
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

        private List<InitialBalanceRegion> _ibRegions = new List<InitialBalanceRegion>();
        private Brush _highlightTimeframeBrush;
        private int _lastBarPlotted = -1;

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Start Time", Order = 1, GroupName = "[01] Parameters")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime StartTime
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "End Time", Order = 2, GroupName = "[01] Parameters")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime EndTime
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [NinjaScriptProperty]
        [Display(Name = "Extension Levels", Order = 3, GroupName = "[01] Parameters")]
        public NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels ExtensionLevels
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ext Level 1 multiplier", Order = 4, GroupName = "[01] Parameters")]
        public double ExtLevel1Multiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ext Level 2 multiplier", Order = 5, GroupName = "[01] Parameters")]
        public double ExtLevel2Multiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ext Level 3 multiplier", Order = 6, GroupName = "[01] Parameters")]
        public double ExtLevel3Multiplier
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight Timeframe", Order = 1, GroupName = "[02] Display")]
        public bool HighlightTimeframe
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Highlight Timeframe color", Order = 2, GroupName = "[02] Display")]
        public Brush HighlightTimeframeColor
        { get; set; }

        [Browsable(false)]
        public string HighlightTimeframeColorSerializable
        {
            get { return Serialize.BrushToString(HighlightTimeframeColor); }
            set { HighlightTimeframeColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Highlight Timeframe opacity", Order = 3, GroupName = "[02] Display")]
        public int HighlightTimeframeOpacity
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight Region", Order = 4, GroupName = "[02] Display")]
        public bool HighlightRegion
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Highlight Region color", Order = 5, GroupName = "[02] Display")]
        public Brush HighlightRegionColor
        { get; set; }

        [Browsable(false)]
        public string HighlightRegionColorSerializable
        {
            get { return Serialize.BrushToString(HighlightRegionColor); }
            set { HighlightRegionColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Highlight Region opacity", Order = 6, GroupName = "[02] Display")]
        public int HighlightRegionOpacity
        { get; set; }

        [Display(Name = "Show 50% Levels", Order = 7, GroupName = "[02] Display")]
        public bool ShowMidLevels
        { get; set; }

        [Display(Name = "Show Developing Region", Order = 8, GroupName = "[02] Display")]
        public bool ShowDevelopingRegion
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Labels", Order = 9, GroupName = "[02] Display")]
        public bool ShowLabels
        { get; set; }

        [Range(0, 100)]
        [Display(Name = "Label Offset", Order = 10, GroupName = "[02] Display")]
        public int LabelOffset
        { get; set; }

        [Display(Name = "Hide Joins", Order = 11, GroupName = "[02] Display")]
        public bool HideJoins
        { get; set; }

        [ReadOnly(true)]
        [XmlIgnore]
        [Display(Name = "Version", Description = "Version information.", Order = 1, GroupName = "[03] Developer")]
        public string VersionInformation
        { get; set; }

        [Display(Name = "Debug", Description = "Toggle debug logging.", Order = 2, GroupName = "[03] Developer")]
        public bool Debug
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt3
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt3Mid
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt2
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt2Mid
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt1
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHighExt1Mid
        {
            get { return Values[5]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBHigh
        {
            get { return Values[6]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBMid
        {
            get { return Values[7]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLow
        {
            get { return Values[8]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt1Mid
        {
            get { return Values[9]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt1
        {
            get { return Values[10]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt2Mid
        {
            get { return Values[11]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt2
        {
            get { return Values[12]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt3Mid
        {
            get { return Values[13]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IBLowExt3
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

                return Name + "(" + StartTime.ToString("HH:mm") + "," + EndTime.ToString("HH:mm") + "," + ExtensionLevels + "," + ExtLevel1Multiplier + "," + ExtLevel2Multiplier + "," + ExtLevel3Multiplier + ")";
            }
        }

        protected override void OnStateChange()
		{
            DebugPrint($"OnStateChange({State})");

            if (State == State.SetDefaults)
			{
				Description									= @"An Initial Balance indicator, supporting extension levels and 50% levels.";
				Name										= "Initial Balance";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsAutoScale                                 = false;
                ZOrder                                      = 0;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

                StartTime                                   = DateTime.Parse("09:30:00");
                EndTime                                     = DateTime.Parse("10:30:00");
                ExtensionLevels                             = InitialBalanceEnums.InitialBalanceExtensionLevels.None;
                ExtLevel1Multiplier                         = 1;
                ExtLevel2Multiplier                         = 2;
                ExtLevel3Multiplier                         = 3;
                HighlightTimeframe                          = false;
                HighlightTimeframeColor                     = Brushes.Purple;
                HighlightTimeframeOpacity                   = 20;
                HighlightRegion                             = true;
                HighlightRegionColor                        = Brushes.Yellow;
                HighlightRegionOpacity                      = 20;
                ShowMidLevels                               = true;
                ShowDevelopingRegion                        = true;
                ShowLabels                                  = true;
                LabelOffset                                 = 3;
                HideJoins                                   = true;

                VersionInformation                          = $"{Version} - {Assembly.GetAssembly(typeof(InitialBalance)).GetName().Version}";
                Debug                                       = false;

                AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "IB High Ext3");
                AddPlot(new Stroke(Brushes.Goldenrod, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "IB High Ext3 50%");
                AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "IB High Ext2");
                AddPlot(new Stroke(Brushes.Goldenrod, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "IB High Ext2 50%");
                AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "IB High Ext1");
                AddPlot(new Stroke(Brushes.Goldenrod, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "IB High Ext1 50%");
                AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "IB High");
                AddPlot(new Stroke(Brushes.Goldenrod, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "IB 50%");
                AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "IB Low");
                AddPlot(new Stroke(Brushes.Goldenrod, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "IB Low Ext1 50%");
                AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "IB Low Ext1");
                AddPlot(new Stroke(Brushes.Goldenrod, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "IB Low Ext2 50%");
                AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "IB Low Ext2");
                AddPlot(new Stroke(Brushes.Goldenrod, DashStyleHelper.Solid, 1.0f), PlotStyle.Hash, "IB Low Ext3 50%");
                AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 2.0f), PlotStyle.Line, "IB Low Ext3");
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

            int ibStartTime = ToTime(StartTime);
            int ibEndTime = ToTime(EndTime);
            int prevBarEndTime = ToTime(Time[1]);
            int currentBarEndTime = ToTime(Time[0]);
            int currentBarStartTime = prevBarEndTime;

            if (BarsInProgress == 0)
			{
                if (_ibRegions.Count == 0)
                    return;

                var currentIBRegion = _ibRegions.Last();

                if (currentIBRegion.StartBarIndex == -1)
                {
                    DateTime regionStartTime = currentIBRegion.StartTime;
                    int regionStartBar = Bars.GetBar(regionStartTime);
                    if (ToTime(Time[CurrentBar - regionStartBar]) == ibStartTime)
                        regionStartBar++; // We found the bar that ENDS at the IB start time, but we want to draw region from the start bar onwards, so start from next bar on chart.
                    currentIBRegion.StartBarIndex = regionStartBar;
                }

                // Highlight timeframe
                if (HighlightTimeframe)
                {
                    if (Time[1] >= currentIBRegion.StartTime && Time[0] <= currentIBRegion.EndTime)
                    {
                        BackBrush = _highlightTimeframeBrush;
                    }
                }

                // Highlight region
                if (HighlightRegion)
                {
                    if ((currentIBRegion.IsConfirmed && string.IsNullOrEmpty(currentIBRegion.RegionTag)) ||
                        (!currentIBRegion.IsConfirmed && ShowDevelopingRegion && (string.IsNullOrEmpty(currentIBRegion.RegionTag) || IsFirstTickOfBar)))
                    {
                        currentIBRegion.RegionTag = $"InitialBalance-{currentIBRegion.StartTime.ToString("yyyyMMdd-HHmm")}";
                        DateTime regionEndTime = Time[0] < currentIBRegion.EndTime ? Time[0] : currentIBRegion.EndTime;
                        int regionEndBar = Bars.GetBar(regionEndTime);
                        currentIBRegion.EndBarIndex = regionEndBar;
                        Draw.Rectangle(this, currentIBRegion.RegionTag, false, CurrentBar - currentIBRegion.StartBarIndex, currentIBRegion.High.Value, CurrentBar - currentIBRegion.EndBarIndex, currentIBRegion.Low.Value, Brushes.Transparent, HighlightRegionColor, HighlightRegionOpacity);
                    }
                }

                // Populate plots (only after IB region is confirmed).
                if (currentIBRegion.IsConfirmed && (Time[0] > currentIBRegion.StartTime) && (Time[0] <= currentIBRegion.StartTime.AddDays(1)) && (CurrentBar > _lastBarPlotted))
                {
                    int startBarsAgo = 0;
                    if (!currentIBRegion.IsBackFilled)
                    {
                        if (HideJoins)
                            ClearPlots((CurrentBar - currentIBRegion.StartBarIndex) + 1);

                        for (int i = CurrentBar - currentIBRegion.StartBarIndex; i >= 0; i--)
                        {
                            PopulatePlots(currentIBRegion, i, false);
                        }
                        currentIBRegion.IsBackFilled = true;
                    }

                    PopulatePlots(currentIBRegion, 0, ShowLabels);
                    _lastBarPlotted = CurrentBar;
                }
            }
            else if (BarsInProgress == 1)
            {
                InitialBalanceRegion? currentIBRegion = null;

                // Check for start of new IB region.
                if (IsFirstTickOfBar && currentBarStartTime == ibStartTime)
                {
                    DateTime startTime = Time[1];
                    DateTime endTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, EndTime.Hour, EndTime.Minute, 0, startTime.Kind);
                    if (ibEndTime < ibStartTime)
                        endTime = endTime.AddDays(1);
                    currentIBRegion = new InitialBalanceRegion(Instrument.MasterInstrument, startTime, endTime);
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

        private void PopulatePlots(InitialBalanceRegion ibRegion, int barsAgo, bool showLabels)
        {
            if (barsAgo >= IBHigh.Count)
                return;

            bool showExt1 = ExtensionLevels != InitialBalanceEnums.InitialBalanceExtensionLevels.None;
            bool showExt2 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Two || ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            double delta = ibRegion.High.Value - ibRegion.Low.Value;
            SimpleFont font = (SimpleFont)ChartControl.Properties.LabelFont.Clone();
            font.Size = 14;


            IBHigh[barsAgo] = ibRegion.High.Value;
            IBLow[barsAgo] = ibRegion.Low.Value;
            if (showLabels)
            {
                Draw.Text(this, "IBHighLabel", false, Plots[(int)InitialBalancePlots.IBHigh].Name, -LabelOffset, IBHigh[barsAgo], 0, Plots[(int)InitialBalancePlots.IBHigh].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                Draw.Text(this, "IBLowLabel", false, Plots[(int)InitialBalancePlots.IBLow].Name, -LabelOffset, IBLow[barsAgo], 0, Plots[(int)InitialBalancePlots.IBLow].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (showExt1)
            {
                IBHighExt1[barsAgo] = ibRegion.High.Value + delta;
                IBLowExt1[barsAgo] = ibRegion.Low.Value - delta;
                if (showLabels)
                {
                    Draw.Text(this, "IBHighExt1Label", false, Plots[(int)InitialBalancePlots.IBHighExt1].Name, -LabelOffset, IBHighExt1[barsAgo], 0, Plots[(int)InitialBalancePlots.IBHighExt1].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    Draw.Text(this, "IBLowExt1Label", false, Plots[(int)InitialBalancePlots.IBLowExt1].Name, -LabelOffset, IBLowExt1[barsAgo], 0, Plots[(int)InitialBalancePlots.IBLowExt1].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                }
            }
            if (showExt2)
            {
                IBHighExt2[barsAgo] = ibRegion.High.Value + (2 * delta);
                IBLowExt2[barsAgo] = ibRegion.Low.Value - (2 * delta);
                if (showLabels)
                {
                    Draw.Text(this, "IBHighExt2Label", false, Plots[(int)InitialBalancePlots.IBHighExt2].Name, -LabelOffset, IBHighExt2[barsAgo], 0, Plots[(int)InitialBalancePlots.IBHighExt2].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    Draw.Text(this, "IBLowExt2Label", false, Plots[(int)InitialBalancePlots.IBLowExt2].Name, -LabelOffset, IBLowExt2[barsAgo], 0, Plots[(int)InitialBalancePlots.IBLowExt2].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                }
            }
            if (showExt3)
            {
                IBHighExt3[barsAgo] = ibRegion.High.Value + (3 * delta);
                IBLowExt3[barsAgo] = ibRegion.Low.Value - (3 * delta);
                if (showLabels)
                {
                    Draw.Text(this, "IBHighExt3Label", false, Plots[(int)InitialBalancePlots.IBHighExt3].Name, -LabelOffset, IBHighExt3[barsAgo], 0, Plots[(int)InitialBalancePlots.IBHighExt3].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    Draw.Text(this, "IBLowExt3Label", false, Plots[(int)InitialBalancePlots.IBLowExt3].Name, -LabelOffset, IBLowExt3[barsAgo], 0, Plots[(int)InitialBalancePlots.IBLowExt3].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                }
            }

            if (ShowMidLevels)
            {
                IBMid[barsAgo] = ibRegion.Mid.Value;
                if (showLabels)
                {
                    Draw.Text(this, "IBMidLabel", false, Plots[(int)InitialBalancePlots.IBMid].Name, -LabelOffset, IBMid[barsAgo], 0, Plots[(int)InitialBalancePlots.IBMid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                }

                if (showExt1)
                {
                    IBHighExt1Mid[barsAgo] = ibRegion.Mid.Value + delta;
                    IBLowExt1Mid[barsAgo] = ibRegion.Mid.Value - delta;
                    if (showLabels)
                    {
                        Draw.Text(this, "IBHighExt1MidLabel", false, Plots[(int)InitialBalancePlots.IBHighExt1Mid].Name, -LabelOffset, IBHighExt1Mid[barsAgo], 0, Plots[(int)InitialBalancePlots.IBHighExt1Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                        Draw.Text(this, "IBLowExt1MidLabel", false, Plots[(int)InitialBalancePlots.IBLowExt1Mid].Name, -LabelOffset, IBLowExt1Mid[barsAgo], 0, Plots[(int)InitialBalancePlots.IBLowExt1Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                }
                if (showExt2)
                {
                    IBHighExt2Mid[barsAgo] = ibRegion.Mid.Value + (2 * delta);
                    IBLowExt2Mid[barsAgo] = ibRegion.Mid.Value - (2 * delta);
                    if (showLabels)
                    {
                        Draw.Text(this, "IBHighExt2MidLabel", false, Plots[(int)InitialBalancePlots.IBHighExt2Mid].Name, -LabelOffset, IBHighExt2Mid[barsAgo], 0, Plots[(int)InitialBalancePlots.IBHighExt2Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                        Draw.Text(this, "IBLowExt2MidLabel", false, Plots[(int)InitialBalancePlots.IBLowExt2Mid].Name, -LabelOffset, IBLowExt2Mid[barsAgo], 0, Plots[(int)InitialBalancePlots.IBLowExt2Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                }
                if (showExt3)
                {
                    IBHighExt3Mid[barsAgo] = ibRegion.Mid.Value + (3 * delta);
                    IBLowExt3Mid[barsAgo] = ibRegion.Mid.Value - (3 * delta);
                    if (showLabels)
                    {
                        Draw.Text(this, "IBHighExt3MidLabel", false, Plots[(int)InitialBalancePlots.IBHighExt3Mid].Name, -LabelOffset, IBHighExt3Mid[barsAgo], 0, Plots[(int)InitialBalancePlots.IBHighExt3Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                        Draw.Text(this, "IBLowExt3MidLabel", false, Plots[(int)InitialBalancePlots.IBLowExt3Mid].Name, -LabelOffset, IBLowExt3Mid[barsAgo], 0, Plots[(int)InitialBalancePlots.IBLowExt3Mid].Brush, font, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                }
            }
        }

        private void ClearPlots(int barsAgo = 0)
        {
            bool showExt1 = ExtensionLevels != InitialBalanceEnums.InitialBalanceExtensionLevels.None;
            bool showExt2 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Two || ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;
            bool showExt3 = ExtensionLevels == InitialBalanceEnums.InitialBalanceExtensionLevels.Three;

            IBHigh.Reset(barsAgo);
            IBLow.Reset(barsAgo);

            if (showExt1)
            {
                IBHighExt1.Reset(barsAgo);
                IBLowExt1.Reset(barsAgo);
            }
            if (showExt2)
            {
                IBHighExt2.Reset(barsAgo);
                IBLowExt2.Reset(barsAgo);
            }
            if (showExt3)
            {
                IBHighExt3.Reset(barsAgo);
                IBLowExt3.Reset(barsAgo);
            }

            if (ShowMidLevels)
            {
                IBMid.Reset(barsAgo);
                if (showExt1)
                {
                    IBHighExt1Mid.Reset(barsAgo);
                    IBLowExt1Mid.Reset(barsAgo);
                }
                if (showExt2)
                {
                    IBHighExt2Mid.Reset(barsAgo);
                    IBLowExt2Mid.Reset(barsAgo);
                }
                if (showExt3)
                {
                    IBHighExt3Mid.Reset(barsAgo);
                    IBLowExt3Mid.Reset(barsAgo);
                }
            }
        }

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"InitialBalance[{Instrument.FullName}]: {msg}");
                else
                    Print($"InitialBalance: {msg}");
            }
        }

        #endregion
    }

    public class InitialBalanceTypeConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            // We need the indicator instance which actually exists on the grid
            InitialBalance indicator = component as InitialBalance;

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
		private LunarTick.InitialBalance[] cacheInitialBalance;
		public LunarTick.InitialBalance InitialBalance(DateTime startTime, DateTime endTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return InitialBalance(Input, startTime, endTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public LunarTick.InitialBalance InitialBalance(ISeries<double> input, DateTime startTime, DateTime endTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			if (cacheInitialBalance != null)
				for (int idx = 0; idx < cacheInitialBalance.Length; idx++)
					if (cacheInitialBalance[idx] != null && cacheInitialBalance[idx].StartTime == startTime && cacheInitialBalance[idx].EndTime == endTime && cacheInitialBalance[idx].ExtensionLevels == extensionLevels && cacheInitialBalance[idx].ExtLevel1Multiplier == extLevel1Multiplier && cacheInitialBalance[idx].ExtLevel2Multiplier == extLevel2Multiplier && cacheInitialBalance[idx].ExtLevel3Multiplier == extLevel3Multiplier && cacheInitialBalance[idx].EqualsInput(input))
						return cacheInitialBalance[idx];
			return CacheIndicator<LunarTick.InitialBalance>(new LunarTick.InitialBalance(){ StartTime = startTime, EndTime = endTime, ExtensionLevels = extensionLevels, ExtLevel1Multiplier = extLevel1Multiplier, ExtLevel2Multiplier = extLevel2Multiplier, ExtLevel3Multiplier = extLevel3Multiplier }, input, ref cacheInitialBalance);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.InitialBalance InitialBalance(DateTime startTime, DateTime endTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.InitialBalance(Input, startTime, endTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public Indicators.LunarTick.InitialBalance InitialBalance(ISeries<double> input , DateTime startTime, DateTime endTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.InitialBalance(input, startTime, endTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.InitialBalance InitialBalance(DateTime startTime, DateTime endTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.InitialBalance(Input, startTime, endTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}

		public Indicators.LunarTick.InitialBalance InitialBalance(ISeries<double> input , DateTime startTime, DateTime endTime, NinjaTrader.NinjaScript.Indicators.LunarTick.InitialBalanceEnums.InitialBalanceExtensionLevels extensionLevels, double extLevel1Multiplier, double extLevel2Multiplier, double extLevel3Multiplier)
		{
			return indicator.InitialBalance(input, startTime, endTime, extensionLevels, extLevel1Multiplier, extLevel2Multiplier, extLevel3Multiplier);
		}
	}
}

#endregion

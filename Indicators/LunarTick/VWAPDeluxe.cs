#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Reflection;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("[01] Parameters", 1)]
    [Gui.CategoryOrder("[02] Display", 2)]
    [Gui.CategoryOrder("[03] Bars", 3)]
    [Gui.CategoryOrder("[04] Developer", 4)]
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.LunarTick.VWAPDeluxeTypeConverter")]
    public class VWAPDeluxe : Indicator
    {
        #region Constants

        public const string Version = "1.1.1";

        #endregion

        #region Members

        NinjaTrader.NinjaScript.Indicators.VWAPStandardDeviations _numStandardDeviations = VWAPStandardDeviations.None;
        OrderFlowVWAP? _orderFlowVWAP = null;

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Reset interval", Order = 1, GroupName = "[01] Parameters")]
        public NinjaTrader.NinjaScript.Indicators.VWAPResetInterval ResetInterval
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Resolution", Order = 2, GroupName = "[01] Parameters")]
        public NinjaTrader.NinjaScript.Indicators.VWAPResolution Resolution
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [NinjaScriptProperty]
        [Display(Name = "Std Dev bands", Order = 3, GroupName = "[01] Parameters")]
        public NinjaTrader.NinjaScript.Indicators.VWAPStandardDeviations NumStandardDeviations
        {
            get => _numStandardDeviations;

            set
            {
                _numStandardDeviations = value;
                if (_numStandardDeviations == VWAPStandardDeviations.None)
                {
                    // Disable options that require at least one StdDev band.
                    HighlightBarsInBalanceRegion = false;
                    HighlightBarsInBullishImbalanceRegion = false;
                    HighlightBarsInBearishImbalanceRegion = false;
                }
            }
        }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Std Dev 1 multiplier", Order = 4, GroupName = "[01] Parameters")]
        public double StdDev1Multiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Std Dev 2 multiplier", Order = 5, GroupName = "[01] Parameters")]
        public double StdDev2Multiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Std Dev 3 multiplier", Order = 6, GroupName = "[01] Parameters")]
        public double StdDev3Multiplier
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Std Dev 1 upper area color", Order = 1, GroupName = "[02] Display")]
        public Brush StdDev1UpperAreaColor
        { get; set; }

        [Browsable(false)]
        public string StdDev1UpperAreaColorSerializable
        {
            get { return Serialize.BrushToString(StdDev1UpperAreaColor); }
            set { StdDev1UpperAreaColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Std Dev 1 upper area opacity", Order = 2, GroupName = "[02] Display")]
        public int StdDev1UpperAreaOpacity
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Std Dev 1 lower area color", Order = 3, GroupName = "[02] Display")]
        public Brush StdDev1LowerAreaColor
        { get; set; }

        [Browsable(false)]
        public string StdDev1LowerAreaColorSerializable
        {
            get { return Serialize.BrushToString(StdDev1LowerAreaColor); }
            set { StdDev1LowerAreaColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Std Dev 1 lower area opacity", Order = 4, GroupName = "[02] Display")]
        public int StdDev1LowerAreaOpacity
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Std Dev 2 upper area color", Order = 5, GroupName = "[02] Display")]
        public Brush StdDev2UpperAreaColor
        { get; set; }

        [Browsable(false)]
        public string StdDev2UpperAreaColorSerializable
        {
            get { return Serialize.BrushToString(StdDev2UpperAreaColor); }
            set { StdDev2UpperAreaColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Std Dev 2 upper area opacity", Order = 6, GroupName = "[02] Display")]
        public int StdDev2UpperAreaOpacity
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Std Dev 2 lower area color", Order = 7, GroupName = "[02] Display")]
        public Brush StdDev2LowerAreaColor
        { get; set; }

        [Browsable(false)]
        public string StdDev2LowerAreaColorSerializable
        {
            get { return Serialize.BrushToString(StdDev2LowerAreaColor); }
            set { StdDev2LowerAreaColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Std Dev 2 lower area opacity", Order = 8, GroupName = "[02] Display")]
        public int StdDev2LowerAreaOpacity
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Std Dev 3 upper area color", Order = 9, GroupName = "[02] Display")]
        public Brush StdDev3UpperAreaColor
        { get; set; }

        [Browsable(false)]
        public string StdDev3UpperAreaColorSerializable
        {
            get { return Serialize.BrushToString(StdDev3UpperAreaColor); }
            set { StdDev3UpperAreaColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Std Dev 3 upper area opacity", Order = 10, GroupName = "[02] Display")]
        public int StdDev3UpperAreaOpacity
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Std Dev 3 lower area color", Order = 11, GroupName = "[02] Display")]
        public Brush StdDev3LowerAreaColor
        { get; set; }

        [Browsable(false)]
        public string StdDev3LowerAreaColorSerializable
        {
            get { return Serialize.BrushToString(StdDev3LowerAreaColor); }
            set { StdDev3LowerAreaColor = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(Name = "Std Dev 3 lower area opacity", Order = 12, GroupName = "[02] Display")]
        public int StdDev3LowerAreaOpacity
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Color VWAP by price", Order = 13, GroupName = "[02] Display")]
        public bool ColorVWAPByPrice
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Color for above price", Order = 14, GroupName = "[02] Display")]
        public Brush AbovePriceColor
        { get; set; }

        [Browsable(false)]
        public string AbovePriceColorSerializable
        {
            get { return Serialize.BrushToString(AbovePriceColor); }
            set { AbovePriceColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Color for below price", Order = 15, GroupName = "[02] Display")]
        public Brush BelowPriceColor
        { get; set; }

        [Browsable(false)]
        public string BelowPriceColorSerializable
        {
            get { return Serialize.BrushToString(BelowPriceColor); }
            set { BelowPriceColor = Serialize.StringToBrush(value); }
        }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight bars in balance region", Description = "Highlight the bars closing inside the Std Deviation 1 bands.", Order = 1, GroupName = "[03] Bars")]
        public bool HighlightBarsInBalanceRegion
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Balance region up bars", Order = 2, GroupName = "[03] Bars")]
        public Brush BalanceRegionUpBarsColor
        { get; set; }

        [Browsable(false)]
        public string BalanceRegionUpBarsColorSerializable
        {
            get { return Serialize.BrushToString(BalanceRegionUpBarsColor); }
            set { BalanceRegionUpBarsColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Balance region down bars", Order = 3, GroupName = "[03] Bars")]
        public Brush BalanceRegionDownBarsColor
        { get; set; }

        [Browsable(false)]
        public string BalanceRegionDownBarsColorSerializable
        {
            get { return Serialize.BrushToString(BalanceRegionDownBarsColor); }
            set { BalanceRegionDownBarsColor = Serialize.StringToBrush(value); }
        }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight bars in bullish imbalance region", Description = "Highlight the bars closing above the Std Deviation 1 Upper band.", Order = 4, GroupName = "[03] Bars")]
        public bool HighlightBarsInBullishImbalanceRegion
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Bullish imbalance region up bars", Order = 5, GroupName = "[03] Bars")]
        public Brush BullishImbalanceRegionUpBarsColor
        { get; set; }

        [Browsable(false)]
        public string BullishImbalanceRegionUpBarsColorSerializable
        {
            get { return Serialize.BrushToString(BullishImbalanceRegionUpBarsColor); }
            set { BullishImbalanceRegionUpBarsColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bullish imbalance region down bars", Order = 6, GroupName = "[03] Bars")]
        public Brush BullishImbalanceRegionDownBarsColor
        { get; set; }

        [Browsable(false)]
        public string BullishImbalanceRegionDownBarsColorSerializable
        {
            get { return Serialize.BrushToString(BullishImbalanceRegionDownBarsColor); }
            set { BullishImbalanceRegionDownBarsColor = Serialize.StringToBrush(value); }
        }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight bars in bearish imbalance region", Description = "Highlight the bars closing below the Std Deviation 1 Lower band.", Order = 7, GroupName = "[03] Bars")]
        public bool HighlightBarsInBearishImbalanceRegion
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Bearish imbalance region up bars", Order = 8, GroupName = "[03] Bars")]
        public Brush BearishImbalanceRegionUpBarsColor
        { get; set; }

        [Browsable(false)]
        public string BearishImbalanceRegionUpBarsColorSerializable
        {
            get { return Serialize.BrushToString(BearishImbalanceRegionUpBarsColor); }
            set { BearishImbalanceRegionUpBarsColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bearish imbalance region down bars", Order = 9, GroupName = "[03] Bars")]
        public Brush BearishImbalanceRegionDownBarsColor
        { get; set; }

        [Browsable(false)]
        public string BearishImbalanceRegionDownBarsColorSerializable
        {
            get { return Serialize.BrushToString(BearishImbalanceRegionDownBarsColor); }
            set { BearishImbalanceRegionDownBarsColor = Serialize.StringToBrush(value); }
        }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Highlight bar/VWAP interactions", Description = "Highlight the bars touching VWAP or any Std Deviation band.", Order = 10, GroupName = "[03] Bars")]
        public bool HighlightBarVWAPInteractions
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Bar/VWAP interaction up bars", Order = 11, GroupName = "[03] Bars")]
        public Brush BarVWAPInteractionUpBarsColor
        { get; set; }

        [Browsable(false)]
        public string BarVWAPInteractionUpBarsColorSerializable
        {
            get { return Serialize.BrushToString(BarVWAPInteractionUpBarsColor); }
            set { BarVWAPInteractionUpBarsColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bar/VWAP interaction down bars", Order = 12, GroupName = "[03] Bars")]
        public Brush BarVWAPInteractionDownBarsColor
        { get; set; }

        [Browsable(false)]
        public string BarVWAPInteractionDownBarsColorSerializable
        {
            get { return Serialize.BrushToString(BarVWAPInteractionDownBarsColor); }
            set { BarVWAPInteractionDownBarsColor = Serialize.StringToBrush(value); }
        }

        [ReadOnly(true)]
        [XmlIgnore]
        [Display(Name = "Version", Description = "Version information.", Order = 1, GroupName = "[04] Developer")]
        public string VersionInformation
        { get; set; }

        [Display(Name = "Debug", Description = "Toggle debug logging.", Order = 2, GroupName = "[04] Developer")]
        public bool Debug
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> VWAP
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StdDev1Upper
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StdDev1Lower
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StdDev2Upper
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StdDev2Lower
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StdDev3Upper
        {
            get { return Values[5]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StdDev3Lower
        {
            get { return Values[6]; }
        }

        #endregion

        #region Indicator methods

        protected override void OnStateChange()
		{
            DebugPrint($"OnStateChange({State})");

            if (State == State.SetDefaults)
			{
				Description									= @"VWAP and standard deviation bands supporting bar/band coloring options.\n\nNOTE: Requires access to OrderFlow VWAP.";
				Name										= "VWAP Deluxe";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsAutoScale                                 = false;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

                ResetInterval                               = VWAPResetInterval.Session;
				Resolution                                  = VWAPResolution.Standard;
				NumStandardDeviations                       = VWAPStandardDeviations.Three;
				StdDev1Multiplier                           = 1;
                StdDev2Multiplier                           = 2;
                StdDev3Multiplier                           = 3;

                StdDev1UpperAreaColor                       = Brushes.CornflowerBlue;
                StdDev1UpperAreaOpacity                     = 50;
                StdDev1LowerAreaColor                       = Brushes.CornflowerBlue;
                StdDev1LowerAreaOpacity                     = 50;

                StdDev2UpperAreaColor                       = Brushes.CornflowerBlue;
                StdDev2UpperAreaOpacity                     = 40;
                StdDev2LowerAreaColor                       = Brushes.CornflowerBlue;
                StdDev2LowerAreaOpacity                     = 40;

                StdDev3UpperAreaColor                       = Brushes.CornflowerBlue;
                StdDev3UpperAreaOpacity                     = 30;
                StdDev3LowerAreaColor                       = Brushes.CornflowerBlue;
                StdDev3LowerAreaOpacity                     = 30;

                ColorVWAPByPrice                            = false;
                AbovePriceColor                             = Brushes.Red;
                BelowPriceColor                             = Brushes.LimeGreen;

                HighlightBarsInBalanceRegion                = false;
                BalanceRegionUpBarsColor                    = Brushes.LightGray;
                BalanceRegionDownBarsColor                  = Brushes.DimGray;

                HighlightBarsInBullishImbalanceRegion       = false;
                BullishImbalanceRegionUpBarsColor           = Brushes.LimeGreen;
                BullishImbalanceRegionDownBarsColor         = Brushes.DarkGreen;

                HighlightBarsInBearishImbalanceRegion       = false;
                BearishImbalanceRegionUpBarsColor           = Brushes.Red;
                BearishImbalanceRegionDownBarsColor         = Brushes.DarkRed;

                HighlightBarVWAPInteractions                = false;
                BarVWAPInteractionUpBarsColor               = Brushes.Yellow;
                BarVWAPInteractionDownBarsColor             = Brushes.Goldenrod;

                VersionInformation                          = $"{Version} - {Assembly.GetAssembly(typeof(VWAPDeluxe)).GetName().Version}";
                Debug										= false;

                AddPlot(new Stroke(Brushes.Cyan, 3), PlotStyle.Line, "VWAP");

                AddPlot(new Stroke(Brushes.DodgerBlue, 3), PlotStyle.Line, "Std Dev 1 Upper");
                AddPlot(new Stroke(Brushes.DodgerBlue, 3), PlotStyle.Line, "Std Dev 1 Lower");

                AddPlot(new Stroke(Brushes.DodgerBlue, 3), PlotStyle.Line, "Std Dev 2 Upper");
                AddPlot(new Stroke(Brushes.DodgerBlue, 3), PlotStyle.Line, "Std Dev 2 Lower");

                AddPlot(new Stroke(Brushes.DodgerBlue, 3), PlotStyle.Line, "Std Dev 3 Upper");
                AddPlot(new Stroke(Brushes.DodgerBlue, 3), PlotStyle.Line, "Std Dev 3 Lower");
            }
            else if (State == State.Configure)
            {
                if (Resolution == VWAPResolution.Tick)
                {
                    AddDataSeries(Data.BarsPeriodType.Tick, 1);
                }
            }
            else if (State == State.DataLoaded)
			{
                _orderFlowVWAP = OrderFlowVWAP(Resolution, Bars.TradingHours, NumStandardDeviations, StdDev1Multiplier, StdDev2Multiplier, StdDev3Multiplier);
                if (_orderFlowVWAP != null)
                {
                    _orderFlowVWAP.ResetInterval = ResetInterval;
                }
                else
                {
                    DebugPrint("Failed to create instance of OrderFlowVWAP!");
                }
            }
        }

		protected override void OnBarUpdate()
		{
            if (CurrentBar < 1)
                return;
            if (_orderFlowVWAP == null)
                return;

            if (BarsInProgress == 0)
            {
                _orderFlowVWAP?.Update();
                VWAP[0] = _orderFlowVWAP.VWAP[0];
                StdDev1Upper[0] = _orderFlowVWAP.StdDev1Upper[0];
                StdDev1Lower[0] = _orderFlowVWAP.StdDev1Lower[0];
                StdDev2Upper[0] = _orderFlowVWAP.StdDev2Upper[0];
                StdDev2Lower[0] = _orderFlowVWAP.StdDev2Lower[0];
                StdDev3Upper[0] = _orderFlowVWAP.StdDev3Upper[0];
                StdDev3Lower[0] = _orderFlowVWAP.StdDev3Lower[0];

                // Color the VWAP line above/below price.
                if (ColorVWAPByPrice)
                {
                    if (VWAP[0] > Close[0])
                        PlotBrushes[0][0] = AbovePriceColor;
                    else if (VWAP[0] < Close[0])
                        PlotBrushes[0][0] = BelowPriceColor;
                    else
                        PlotBrushes[0][0] = PlotBrushes[0][1];
                }

                // Bands fills.
                if (NumStandardDeviations == VWAPStandardDeviations.One || NumStandardDeviations == VWAPStandardDeviations.Two || NumStandardDeviations == VWAPStandardDeviations.Three)
                {
                    if (StdDev1UpperAreaOpacity > 0)
                    {
                        Draw.Region(this, "StdDev1UpperRegion", CurrentBar, 0, StdDev1Upper, VWAP, null, StdDev1UpperAreaColor, StdDev1UpperAreaOpacity);
                    }
                    if (StdDev1LowerAreaOpacity > 0)
                    {
                        Draw.Region(this, "StdDev1LowerRegion", CurrentBar, 0, StdDev1Lower, VWAP, null, StdDev1LowerAreaColor, StdDev1LowerAreaOpacity);
                    }
                }

                if (NumStandardDeviations == VWAPStandardDeviations.Two || NumStandardDeviations == VWAPStandardDeviations.Three)
                {
                    if (StdDev2UpperAreaOpacity > 0)
                    {
                        Draw.Region(this, "StdDev2UpperRegion", CurrentBar, 0, StdDev2Upper, StdDev1Upper, null, StdDev2UpperAreaColor, StdDev2UpperAreaOpacity);
                    }
                    if (StdDev2LowerAreaOpacity > 0)
                    {
                        Draw.Region(this, "StdDev2LowerRegion", CurrentBar, 0, StdDev2Lower, StdDev1Lower, null, StdDev2LowerAreaColor, StdDev2LowerAreaOpacity);
                    }
                }

                if (NumStandardDeviations == VWAPStandardDeviations.Three)
                {
                    if (StdDev3UpperAreaOpacity > 0)
                    {
                        Draw.Region(this, "StdDev3UpperRegion", CurrentBar, 0, StdDev3Upper, StdDev2Upper, null, StdDev3UpperAreaColor, StdDev3UpperAreaOpacity);
                    }
                    if (StdDev3LowerAreaOpacity > 0)
                    {
                        Draw.Region(this, "StdDev3LowerRegion", CurrentBar, 0, StdDev3Lower, StdDev2Lower, null, StdDev3LowerAreaColor, StdDev3LowerAreaOpacity);
                    }
                }

                // Region bar colors (requires at least 1 StdDev).
                if ((NumStandardDeviations != VWAPStandardDeviations.None) && (HighlightBarsInBalanceRegion || HighlightBarsInBullishImbalanceRegion || HighlightBarsInBearishImbalanceRegion))
                {
                    if (HighlightBarsInBullishImbalanceRegion && (Close[0] > _orderFlowVWAP.StdDev1Upper[0]))
                    {
                        BarBrushes[0] = (Close[0] > Open[0]) ? BullishImbalanceRegionUpBarsColor : BullishImbalanceRegionDownBarsColor;
                    }
                    else if (HighlightBarsInBearishImbalanceRegion && (Close[0] < _orderFlowVWAP.StdDev1Lower[0]))
                    {
                        BarBrushes[0] = (Close[0] > Open[0]) ? BearishImbalanceRegionUpBarsColor : BearishImbalanceRegionDownBarsColor;
                    }
                    else if (HighlightBarsInBalanceRegion && (Close[0] <= _orderFlowVWAP.StdDev1Upper[0]) && (Close[0] >= _orderFlowVWAP.StdDev1Lower[0]))
                    {
                        BarBrushes[0] = (Close[0] > Open[0]) ? BalanceRegionUpBarsColor : BalanceRegionDownBarsColor;
                    }
                }
                if (HighlightBarVWAPInteractions)
                {
                    bool checkStdDev1 = NumStandardDeviations != VWAPStandardDeviations.None;
                    bool checkStdDev2 = NumStandardDeviations == VWAPStandardDeviations.Two || NumStandardDeviations == VWAPStandardDeviations.Three;
                    bool checkStdDev3 = NumStandardDeviations == VWAPStandardDeviations.Three;
                    bool barVWAPInteraction = false;


                    if (!barVWAPInteraction && _orderFlowVWAP.VWAP[0] <= High[0] && _orderFlowVWAP.VWAP[0] >= Low[0])
                        barVWAPInteraction = true;

                    if (!barVWAPInteraction && checkStdDev1 && _orderFlowVWAP.StdDev1Upper[0] <= High[0] && _orderFlowVWAP.StdDev1Upper[0] >= Low[0])
                        barVWAPInteraction = true;
                    if (!barVWAPInteraction && checkStdDev1 && _orderFlowVWAP.StdDev1Lower[0] <= High[0] && _orderFlowVWAP.StdDev1Lower[0] >= Low[0])
                        barVWAPInteraction = true;

                    if (!barVWAPInteraction && checkStdDev2 && _orderFlowVWAP.StdDev2Upper[0] <= High[0] && _orderFlowVWAP.StdDev2Upper[0] >= Low[0])
                        barVWAPInteraction = true;
                    if (!barVWAPInteraction && checkStdDev2 && _orderFlowVWAP.StdDev2Lower[0] <= High[0] && _orderFlowVWAP.StdDev2Lower[0] >= Low[0])
                        barVWAPInteraction = true;

                    if (!barVWAPInteraction && checkStdDev3 && _orderFlowVWAP.StdDev3Upper[0] <= High[0] && _orderFlowVWAP.StdDev3Upper[0] >= Low[0])
                        barVWAPInteraction = true;
                    if (!barVWAPInteraction && checkStdDev3 && _orderFlowVWAP.StdDev3Lower[0] <= High[0] && _orderFlowVWAP.StdDev3Lower[0] >= Low[0])
                        barVWAPInteraction = true;

                    if (barVWAPInteraction)
                    {
                        BarBrushes[0] = (Close[0] > Open[0]) ? BarVWAPInteractionUpBarsColor : BarVWAPInteractionDownBarsColor;
                    }
                }
            }
            else if (BarsInProgress == 1)
            {
                // We have to update the secondary tick series of the cached indicator using Tick Resolution to make sure the values we get in BarsInProgress == 0 are in sync
                OrderFlowVWAP(BarsArray[0], VWAPResolution.Tick, BarsArray[0].TradingHours, NumStandardDeviations, StdDev1Multiplier, StdDev2Multiplier, StdDev3Multiplier).Update(OrderFlowVWAP(BarsArray[0], VWAPResolution.Tick, BarsArray[0].TradingHours, NumStandardDeviations, StdDev1Multiplier, StdDev2Multiplier, StdDev3Multiplier).BarsArray[1].Count - 1, 1);
            }
        }

        #endregion

        #region Private methods

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"VWAPDeluxe[{Instrument.FullName}]: {msg}");
                else
                    Print($"VWAPDeluxe: {msg}");
            }
        }

        #endregion
    }

    public class VWAPDeluxeTypeConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            // We need the indicator instance which actually exists on the grid
            VWAPDeluxe indicator = component as VWAPDeluxe;

            // base.GetProperties ensures we have all the properties (and associated property grid editors)
            // NinjaTrader internal logic determines for a given indicator
            PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context)
                                                                        ? base.GetProperties(context, component, attrs)
                                                                        : TypeDescriptor.GetProperties(component, attrs);

            if (indicator == null || propertyDescriptorCollection == null)
                return propertyDescriptorCollection;


            // Show/hide properties related to NumStandardDeviations
            PropertyDescriptor stdDev1MultiplierPropertyDesc = propertyDescriptorCollection["StdDev1Multiplier"];
            PropertyDescriptor stdDev2MultiplierPropertyDesc = propertyDescriptorCollection["StdDev2Multiplier"];
            PropertyDescriptor stdDev3MultiplierPropertyDesc = propertyDescriptorCollection["StdDev3Multiplier"];

            PropertyDescriptor stdDev1UpperAreaColorPropertyDesc = propertyDescriptorCollection["StdDev1UpperAreaColor"];
            PropertyDescriptor stdDev2UpperAreaColorPropertyDesc = propertyDescriptorCollection["StdDev2UpperAreaColor"];
            PropertyDescriptor stdDev3UpperAreaColorPropertyDesc = propertyDescriptorCollection["StdDev3UpperAreaColor"];

            PropertyDescriptor stdDev1UpperAreaOpacityPropertyDesc = propertyDescriptorCollection["StdDev1UpperAreaOpacity"];
            PropertyDescriptor stdDev2UpperAreaOpacityPropertyDesc = propertyDescriptorCollection["StdDev2UpperAreaOpacity"];
            PropertyDescriptor stdDev3UpperAreaOpacityPropertyDesc = propertyDescriptorCollection["StdDev3UpperAreaOpacity"];

            PropertyDescriptor stdDev1LowerAreaColorPropertyDesc = propertyDescriptorCollection["StdDev1LowerAreaColor"];
            PropertyDescriptor stdDev2LowerAreaColorPropertyDesc = propertyDescriptorCollection["StdDev2LowerAreaColor"];
            PropertyDescriptor stdDev3LowerAreaColorPropertyDesc = propertyDescriptorCollection["StdDev3LowerAreaColor"];

            PropertyDescriptor stdDev1LowerAreaOpacityPropertyDesc = propertyDescriptorCollection["StdDev1LowerAreaOpacity"];
            PropertyDescriptor stdDev2LowerAreaOpacityPropertyDesc = propertyDescriptorCollection["StdDev2LowerAreaOpacity"];
            PropertyDescriptor stdDev3LowerAreaOpacityPropertyDesc = propertyDescriptorCollection["StdDev3LowerAreaOpacity"];

            propertyDescriptorCollection.Remove(stdDev1MultiplierPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev2MultiplierPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev3MultiplierPropertyDesc);

            propertyDescriptorCollection.Remove(stdDev1UpperAreaColorPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev2UpperAreaColorPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev3UpperAreaColorPropertyDesc);

            propertyDescriptorCollection.Remove(stdDev1UpperAreaOpacityPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev2UpperAreaOpacityPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev3UpperAreaOpacityPropertyDesc);

            propertyDescriptorCollection.Remove(stdDev1LowerAreaColorPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev2LowerAreaColorPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev3LowerAreaColorPropertyDesc);

            propertyDescriptorCollection.Remove(stdDev1LowerAreaOpacityPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev2LowerAreaOpacityPropertyDesc);
            propertyDescriptorCollection.Remove(stdDev3LowerAreaOpacityPropertyDesc);

            if (indicator.NumStandardDeviations == VWAPStandardDeviations.One)
            {
                propertyDescriptorCollection.Add(stdDev1MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1UpperAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1UpperAreaOpacityPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1LowerAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1LowerAreaOpacityPropertyDesc);
            }
            else if (indicator.NumStandardDeviations == VWAPStandardDeviations.Two)
            {
                propertyDescriptorCollection.Add(stdDev1MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1UpperAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1UpperAreaOpacityPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1LowerAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1LowerAreaOpacityPropertyDesc);

                propertyDescriptorCollection.Add(stdDev2MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(stdDev2UpperAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev2UpperAreaOpacityPropertyDesc);
                propertyDescriptorCollection.Add(stdDev2LowerAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev2LowerAreaOpacityPropertyDesc);
            }
            else if (indicator.NumStandardDeviations == VWAPStandardDeviations.Three)
            {
                propertyDescriptorCollection.Add(stdDev1MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1UpperAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1UpperAreaOpacityPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1LowerAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev1LowerAreaOpacityPropertyDesc);

                propertyDescriptorCollection.Add(stdDev2MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(stdDev2UpperAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev2UpperAreaOpacityPropertyDesc);
                propertyDescriptorCollection.Add(stdDev2LowerAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev2LowerAreaOpacityPropertyDesc);

                propertyDescriptorCollection.Add(stdDev3MultiplierPropertyDesc);
                propertyDescriptorCollection.Add(stdDev3UpperAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev3UpperAreaOpacityPropertyDesc);
                propertyDescriptorCollection.Add(stdDev3LowerAreaColorPropertyDesc);
                propertyDescriptorCollection.Add(stdDev3LowerAreaOpacityPropertyDesc);
            }


            // Show/hide properties related to ColorVWAPByPrice
            PropertyDescriptor abovePriceColorPropertyDesc = propertyDescriptorCollection["AbovePriceColor"];
            PropertyDescriptor belowPriceColorPropertyDesc = propertyDescriptorCollection["BelowPriceColor"];
            propertyDescriptorCollection.Remove(abovePriceColorPropertyDesc);
            propertyDescriptorCollection.Remove(belowPriceColorPropertyDesc);
            if (indicator.ColorVWAPByPrice)
            {
                propertyDescriptorCollection.Add(abovePriceColorPropertyDesc);
                propertyDescriptorCollection.Add(belowPriceColorPropertyDesc);
            }


            // Enable/disable some HighlightBars checkboxes if no StdDev bands are showing. 
            PropertyDescriptor highlightBarsInBalanceRegionPropertyDesc = propertyDescriptorCollection["HighlightBarsInBalanceRegion"];
            PropertyDescriptor highlightBarsInBullishImbalanceRegionPropertyDesc = propertyDescriptorCollection["HighlightBarsInBullishImbalanceRegion"];
            PropertyDescriptor highlightBarsInBearishImbalanceRegionPropertyDesc = propertyDescriptorCollection["HighlightBarsInBearishImbalanceRegion"];
            propertyDescriptorCollection.Remove(highlightBarsInBalanceRegionPropertyDesc);
            propertyDescriptorCollection.Remove(highlightBarsInBullishImbalanceRegionPropertyDesc);
            propertyDescriptorCollection.Remove(highlightBarsInBearishImbalanceRegionPropertyDesc);
            highlightBarsInBalanceRegionPropertyDesc = new BoolRequiringStdDeviationsPropertyDescriptor(indicator, highlightBarsInBalanceRegionPropertyDesc);
            highlightBarsInBullishImbalanceRegionPropertyDesc = new BoolRequiringStdDeviationsPropertyDescriptor(indicator, highlightBarsInBullishImbalanceRegionPropertyDesc);
            highlightBarsInBearishImbalanceRegionPropertyDesc = new BoolRequiringStdDeviationsPropertyDescriptor(indicator, highlightBarsInBearishImbalanceRegionPropertyDesc);
            propertyDescriptorCollection.Add(highlightBarsInBalanceRegionPropertyDesc);
            propertyDescriptorCollection.Add(highlightBarsInBullishImbalanceRegionPropertyDesc);
            propertyDescriptorCollection.Add(highlightBarsInBearishImbalanceRegionPropertyDesc);


            // Show/hide properties related to HighlightBarsInBalanceRegion
            PropertyDescriptor balanceRegionUpBarsColorPropertyDesc = propertyDescriptorCollection["BalanceRegionUpBarsColor"];
            PropertyDescriptor balanceRegionDownBarsColorPropertyDesc = propertyDescriptorCollection["BalanceRegionDownBarsColor"];
            propertyDescriptorCollection.Remove(balanceRegionUpBarsColorPropertyDesc);
            propertyDescriptorCollection.Remove(balanceRegionDownBarsColorPropertyDesc);
            if (indicator.HighlightBarsInBalanceRegion)
            {
                propertyDescriptorCollection.Add(balanceRegionUpBarsColorPropertyDesc);
                propertyDescriptorCollection.Add(balanceRegionDownBarsColorPropertyDesc);
            }


            // Show/hide properties related to HighlightBarsInBullishImbalanceRegion
            PropertyDescriptor bullishImbalanceRegionUpBarsColorPropertyDesc = propertyDescriptorCollection["BullishImbalanceRegionUpBarsColor"];
            PropertyDescriptor bullishImbalanceRegionDownBarsColorPropertyDesc = propertyDescriptorCollection["BullishImbalanceRegionDownBarsColor"];
            propertyDescriptorCollection.Remove(bullishImbalanceRegionUpBarsColorPropertyDesc);
            propertyDescriptorCollection.Remove(bullishImbalanceRegionDownBarsColorPropertyDesc);
            if (indicator.HighlightBarsInBullishImbalanceRegion)
            {
                propertyDescriptorCollection.Add(bullishImbalanceRegionUpBarsColorPropertyDesc);
                propertyDescriptorCollection.Add(bullishImbalanceRegionDownBarsColorPropertyDesc);
            }


            // Show/hide properties related to HighlightBarsInBearishImbalanceRegion
            PropertyDescriptor bearishImbalanceRegionUpBarsColorPropertyDesc = propertyDescriptorCollection["BearishImbalanceRegionUpBarsColor"];
            PropertyDescriptor bearishImbalanceRegionDownBarsColorPropertyDesc = propertyDescriptorCollection["BearishImbalanceRegionDownBarsColor"];
            propertyDescriptorCollection.Remove(bearishImbalanceRegionUpBarsColorPropertyDesc);
            propertyDescriptorCollection.Remove(bearishImbalanceRegionDownBarsColorPropertyDesc);
            if (indicator.HighlightBarsInBearishImbalanceRegion)
            {
                propertyDescriptorCollection.Add(bearishImbalanceRegionUpBarsColorPropertyDesc);
                propertyDescriptorCollection.Add(bearishImbalanceRegionDownBarsColorPropertyDesc);
            }


            // Show/hide properties related to HighlightBarVWAPInteractions
            PropertyDescriptor barVWAPInteractionUpBarsColorPropertyDesc = propertyDescriptorCollection["BarVWAPInteractionUpBarsColor"];
            PropertyDescriptor barVWAPInteractionDownBarsColorPropertyDesc = propertyDescriptorCollection["BarVWAPInteractionDownBarsColor"];
            propertyDescriptorCollection.Remove(barVWAPInteractionUpBarsColorPropertyDesc);
            propertyDescriptorCollection.Remove(barVWAPInteractionDownBarsColorPropertyDesc);
            if (indicator.HighlightBarVWAPInteractions)
            {
                propertyDescriptorCollection.Add(barVWAPInteractionUpBarsColorPropertyDesc);
                propertyDescriptorCollection.Add(barVWAPInteractionDownBarsColorPropertyDesc);
            }

            return propertyDescriptorCollection;
        }

        // Important: This must return true otherwise the type convetor will not be called
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        { return true; }
    }

    public class BoolRequiringStdDeviationsPropertyDescriptor : PropertyDescriptor
    {
        // Need the instance on the property grid to check the show/hide toggle value
        private VWAPDeluxe indicatorInstance;

        private PropertyDescriptor property;

        // The base instance constructor helps store the default Name and Attributes (Such as DisplayAttribute.Name, .GroupName, .Order)
        // Otherwise those details would be lost when we converted the PropertyDescriptor to the new custom ReadOnlyDescriptor
        public BoolRequiringStdDeviationsPropertyDescriptor(VWAPDeluxe indicator, PropertyDescriptor propertyDescriptor) : base(propertyDescriptor.Name, propertyDescriptor.Attributes.OfType<Attribute>().ToArray())
        {
            indicatorInstance = indicator;
            property = propertyDescriptor;
        }

        // Stores the current value of the property on the indicator
        public override object GetValue(object component)
        {
            VWAPDeluxe targetInstance = component as VWAPDeluxe;

            if (targetInstance == null)
                return null;

            switch (property.Name)
            {
                case "HighlightBarsInBalanceRegion":
                    return targetInstance.HighlightBarsInBalanceRegion;
                case "HighlightBarsInBullishImbalanceRegion":
                    return targetInstance.HighlightBarsInBullishImbalanceRegion;
                case "HighlightBarsInBearishImbalanceRegion":
                    return targetInstance.HighlightBarsInBearishImbalanceRegion;
            }
            return null;
        }

        // Updates the current value of the property on the indicator
        public override void SetValue(object component, object value)
        {
            VWAPDeluxe targetInstance = component as VWAPDeluxe;

            if (targetInstance == null)
                return;

            switch (property.Name)
            {
                case "HighlightBarsInBalanceRegion":
                    targetInstance.HighlightBarsInBalanceRegion = (bool)value;
                    break;
                case "HighlightBarsInBullishImbalanceRegion":
                    targetInstance.HighlightBarsInBullishImbalanceRegion = (bool)value;
                    break;
                case "HighlightBarsInBearishImbalanceRegion":
                    targetInstance.HighlightBarsInBearishImbalanceRegion = (bool)value;
                    break;
            }
        }

        // set the PropertyDescriptor to "read only" based on the indicator instance input
        public override bool IsReadOnly
        { get { return indicatorInstance.NumStandardDeviations == VWAPStandardDeviations.None; } }

        // IsReadOnly is the relevant interface member we need to use to obtain our desired custom behavior
        // but applying a custom property descriptor requires having to handle a bunch of other operations as well.
        // I.e., the below methods and properties are required to be implemented, otherwise it won't compile.
        public override bool CanResetValue(object component)
        { return true; }

        public override Type ComponentType
        { get { return typeof(VWAPDeluxeTypeConverter); } }

        public override Type PropertyType
        { get { return typeof(bool); } }

        public override void ResetValue(object component)
        { }

        public override bool ShouldSerializeValue(object component)
        { return true; }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LunarTick.VWAPDeluxe[] cacheVWAPDeluxe;
		public LunarTick.VWAPDeluxe VWAPDeluxe(NinjaTrader.NinjaScript.Indicators.VWAPResetInterval resetInterval, NinjaTrader.NinjaScript.Indicators.VWAPResolution resolution, NinjaTrader.NinjaScript.Indicators.VWAPStandardDeviations numStandardDeviations, double stdDev1Multiplier, double stdDev2Multiplier, double stdDev3Multiplier)
		{
			return VWAPDeluxe(Input, resetInterval, resolution, numStandardDeviations, stdDev1Multiplier, stdDev2Multiplier, stdDev3Multiplier);
		}

		public LunarTick.VWAPDeluxe VWAPDeluxe(ISeries<double> input, NinjaTrader.NinjaScript.Indicators.VWAPResetInterval resetInterval, NinjaTrader.NinjaScript.Indicators.VWAPResolution resolution, NinjaTrader.NinjaScript.Indicators.VWAPStandardDeviations numStandardDeviations, double stdDev1Multiplier, double stdDev2Multiplier, double stdDev3Multiplier)
		{
			if (cacheVWAPDeluxe != null)
				for (int idx = 0; idx < cacheVWAPDeluxe.Length; idx++)
					if (cacheVWAPDeluxe[idx] != null && cacheVWAPDeluxe[idx].ResetInterval == resetInterval && cacheVWAPDeluxe[idx].Resolution == resolution && cacheVWAPDeluxe[idx].NumStandardDeviations == numStandardDeviations && cacheVWAPDeluxe[idx].StdDev1Multiplier == stdDev1Multiplier && cacheVWAPDeluxe[idx].StdDev2Multiplier == stdDev2Multiplier && cacheVWAPDeluxe[idx].StdDev3Multiplier == stdDev3Multiplier && cacheVWAPDeluxe[idx].EqualsInput(input))
						return cacheVWAPDeluxe[idx];
			return CacheIndicator<LunarTick.VWAPDeluxe>(new LunarTick.VWAPDeluxe(){ ResetInterval = resetInterval, Resolution = resolution, NumStandardDeviations = numStandardDeviations, StdDev1Multiplier = stdDev1Multiplier, StdDev2Multiplier = stdDev2Multiplier, StdDev3Multiplier = stdDev3Multiplier }, input, ref cacheVWAPDeluxe);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.VWAPDeluxe VWAPDeluxe(NinjaTrader.NinjaScript.Indicators.VWAPResetInterval resetInterval, NinjaTrader.NinjaScript.Indicators.VWAPResolution resolution, NinjaTrader.NinjaScript.Indicators.VWAPStandardDeviations numStandardDeviations, double stdDev1Multiplier, double stdDev2Multiplier, double stdDev3Multiplier)
		{
			return indicator.VWAPDeluxe(Input, resetInterval, resolution, numStandardDeviations, stdDev1Multiplier, stdDev2Multiplier, stdDev3Multiplier);
		}

		public Indicators.LunarTick.VWAPDeluxe VWAPDeluxe(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.VWAPResetInterval resetInterval, NinjaTrader.NinjaScript.Indicators.VWAPResolution resolution, NinjaTrader.NinjaScript.Indicators.VWAPStandardDeviations numStandardDeviations, double stdDev1Multiplier, double stdDev2Multiplier, double stdDev3Multiplier)
		{
			return indicator.VWAPDeluxe(input, resetInterval, resolution, numStandardDeviations, stdDev1Multiplier, stdDev2Multiplier, stdDev3Multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.VWAPDeluxe VWAPDeluxe(NinjaTrader.NinjaScript.Indicators.VWAPResetInterval resetInterval, NinjaTrader.NinjaScript.Indicators.VWAPResolution resolution, NinjaTrader.NinjaScript.Indicators.VWAPStandardDeviations numStandardDeviations, double stdDev1Multiplier, double stdDev2Multiplier, double stdDev3Multiplier)
		{
			return indicator.VWAPDeluxe(Input, resetInterval, resolution, numStandardDeviations, stdDev1Multiplier, stdDev2Multiplier, stdDev3Multiplier);
		}

		public Indicators.LunarTick.VWAPDeluxe VWAPDeluxe(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.VWAPResetInterval resetInterval, NinjaTrader.NinjaScript.Indicators.VWAPResolution resolution, NinjaTrader.NinjaScript.Indicators.VWAPStandardDeviations numStandardDeviations, double stdDev1Multiplier, double stdDev2Multiplier, double stdDev3Multiplier)
		{
			return indicator.VWAPDeluxe(input, resetInterval, resolution, numStandardDeviations, stdDev1Multiplier, stdDev2Multiplier, stdDev3Multiplier);
		}
	}
}

#endregion

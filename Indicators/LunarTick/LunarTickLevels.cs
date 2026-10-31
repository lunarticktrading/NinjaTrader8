#region Using declarations
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.MarketAnalyzerColumns;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LunarTick
{
    [Gui.CategoryOrder("Daily Levels", 1)]
    [Gui.CategoryOrder("1H Moving Averages", 2)]
    [Gui.CategoryOrder("4H Moving Averages", 3)]
    [Gui.CategoryOrder("Daily Moving Averages", 4)]
    [Gui.CategoryOrder("Weekly Moving Averages", 5)]
    [Gui.CategoryOrder("Monthly Moving Averages", 6)]
    [Gui.CategoryOrder("Sessions", 7)]
    [Gui.CategoryOrder("Display", 8)]
    [Gui.CategoryOrder("Developer", 9)]
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.LunarTick.LunarTickLevelsTypeConverter")]
    public class LunarTickLevels : Indicator
	{
        #region Constants

        public const string Version = "1.0.0";

		#endregion

		#region Members

		private double _opValue = 0.0;
        private DateTime? _opBarDateTime = null;

        private double _pdhValue = 0.0;
        private DateTime? _pdhBarDateTime = null;
        private double _pdlValue = 0.0;
        private DateTime? _pdlBarDateTime = null;

        private double _cdhValue = 0.0;
        private DateTime? _cdhBarDateTime = null;
        private double _cdlValue = 0.0;
        private DateTime? _cdlBarDateTime = null;

        private double _conhValue = 0.0;
        private DateTime? _conhBarDateTime = null;
        private double _conlValue = 0.0;
        private DateTime? _conlBarDateTime = null;

        private double _ponhValue = 0.0;
        private DateTime? _ponhBarDateTime = null;
        private double _ponlValue = 0.0;
        private DateTime? _ponlBarDateTime = null;

        private SMA _oneHourSMA1;
        private SMA _oneHourSMA2;
        private SMA _oneHourSMA3;
        private SMA _oneHourSMA4;

        private SMA _fourHourSMA1;
        private SMA _fourHourSMA2;
        private SMA _fourHourSMA3;
        private SMA _fourHourSMA4;

        private SMA _dailySMA1;
        private SMA _dailySMA2;
        private SMA _dailySMA3;
        private SMA _dailySMA4;

        private SMA _weeklySMA1;
        private SMA _weeklySMA2;
        private SMA _weeklySMA3;
        private SMA _weeklySMA4;

        private SMA _monthlySMA1;
        private SMA _monthlySMA2;
        private SMA _monthlySMA3;
        private SMA _monthlySMA4;

        private int _oneHourDataSeriesIndex = 0;
        private int _fourHourDataSeriesIndex = 0;
        private int _dailyDataSeriesIndex = 0;
        private int _weeklyDataSeriesIndex = 0;
        private int _monthlyDataSeriesIndex = 0;

        private SimpleFont _labelFont;

        private Brush _ethBackgroundBrush;

        private int _decimalPlaces = 0;

        #endregion

        #region Properties

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Opening Print", Order = 1, GroupName = "Daily Levels")]
        public bool ShowOpeningPrint
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Opening Print Color", Order = 2, GroupName = "Daily Levels")]
        public Brush OpeningPrintColor
        { get; set; }

        [Browsable(false)]
        public string OpeningPrintColorSerializable
        {
            get { return Serialize.BrushToString(OpeningPrintColor); }
            set { OpeningPrintColor = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Opening Print Line Width", Order = 3, GroupName = "Daily Levels")]
        public int OpeningPrintLineWidth
        { get; set; }

        [Display(Name = "Opening Print Line Style", Order = 4, GroupName = "Daily Levels")]
        public DashStyleHelper OpeningPrintLineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Previous Daily High", Order = 5, GroupName = "Daily Levels")]
        public bool ShowPreviousDailyHigh
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Previous Daily High Color", Order = 6, GroupName = "Daily Levels")]
        public Brush PreviousDailyHighColor
        { get; set; }

        [Browsable(false)]
        public string PreviousDailyHighColorSerializable
        {
            get { return Serialize.BrushToString(PreviousDailyHighColor); }
            set { PreviousDailyHighColor = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Previous Daily High Line Width", Order = 7, GroupName = "Daily Levels")]
        public int PreviousDailyHighLineWidth
        { get; set; }

        [Display(Name = "Previous Daily High Line Style", Order = 8, GroupName = "Daily Levels")]
        public DashStyleHelper PreviousDailyHighLineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Previous Daily Low", Order = 9, GroupName = "Daily Levels")]
        public bool ShowPreviousDailyLow
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Previous Daily Low Color", Order = 10, GroupName = "Daily Levels")]
        public Brush PreviousDailyLowColor
        { get; set; }

        [Browsable(false)]
        public string PreviousDailyLowColorSerializable
        {
            get { return Serialize.BrushToString(PreviousDailyLowColor); }
            set { PreviousDailyLowColor = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Previous Daily Low Line Width", Order = 11, GroupName = "Daily Levels")]
        public int PreviousDailyLowLineWidth
        { get; set; }

        [Display(Name = "Previous Daily Low Line Style", Order = 12, GroupName = "Daily Levels")]
        public DashStyleHelper PreviousDailyLowLineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Overnight High", Order = 13, GroupName = "Daily Levels")]
        public bool ShowOvernightHigh
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Overnight High Color", Order = 14, GroupName = "Daily Levels")]
        public Brush OvernightHighColor
        { get; set; }

        [Browsable(false)]
        public string OvernightHighColorSerializable
        {
            get { return Serialize.BrushToString(OvernightHighColor); }
            set { OvernightHighColor = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Overnight High Line Width", Order = 15, GroupName = "Daily Levels")]
        public int OvernightHighLineWidth
        { get; set; }

        [Display(Name = "Overnight High Line Style", Order = 16, GroupName = "Daily Levels")]
        public DashStyleHelper OvernightHighLineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Overnight Low", Order = 17, GroupName = "Daily Levels")]
        public bool ShowOvernightLow
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Overnight Low Color", Order = 18, GroupName = "Daily Levels")]
        public Brush OvernightLowColor
        { get; set; }

        [Browsable(false)]
        public string OvernightLowColorSerializable
        {
            get { return Serialize.BrushToString(OvernightLowColor); }
            set { OvernightLowColor = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Overnight Low Line Width", Order = 19, GroupName = "Daily Levels")]
        public int OvernightLowLineWidth
        { get; set; }

        [Display(Name = "Overnight Low Line Style", Order = 20, GroupName = "Daily Levels")]
        public DashStyleHelper OvernightLowLineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show 1H SMA1", Order = 1, GroupName = "1H Moving Averages")]
        public bool ShowOneHourSMA1
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "1H SMA1 Period", Order = 2, GroupName = "1H Moving Averages")]
        public int OneHourSMA1Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "1H SMA1 Color", Order = 3, GroupName = "1H Moving Averages")]
        public Brush OneHourSMA1Color
        { get; set; }

        [Browsable(false)]
        public string OneHourSMA1ColorSerializable
        {
            get { return Serialize.BrushToString(OneHourSMA1Color); }
            set { OneHourSMA1Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "1H SMA1 Line Width", Order = 4, GroupName = "1H Moving Averages")]
        public int OneHourSMA1LineWidth
        { get; set; }

        [Display(Name = "1H SMA1 Line Style", Order = 5, GroupName = "1H Moving Averages")]
        public DashStyleHelper OneHourSMA1LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show 1H SMA2", Order = 6, GroupName = "1H Moving Averages")]
        public bool ShowOneHourSMA2
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "1H SMA2 Period", Order = 7, GroupName = "1H Moving Averages")]
        public int OneHourSMA2Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "1H SMA2 Color", Order = 8, GroupName = "1H Moving Averages")]
        public Brush OneHourSMA2Color
        { get; set; }

        [Browsable(false)]
        public string OneHourSMA2ColorSerializable
        {
            get { return Serialize.BrushToString(OneHourSMA2Color); }
            set { OneHourSMA2Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "1H SMA2 Line Width", Order = 9, GroupName = "1H Moving Averages")]
        public int OneHourSMA2LineWidth
        { get; set; }

        [Display(Name = "1H SMA2 Line Style", Order = 10, GroupName = "1H Moving Averages")]
        public DashStyleHelper OneHourSMA2LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show 1H SMA3", Order = 11, GroupName = "1H Moving Averages")]
        public bool ShowOneHourSMA3
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "1H SMA3 Period", Order = 12, GroupName = "1H Moving Averages")]
        public int OneHourSMA3Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "1H SMA3 Color", Order = 13, GroupName = "1H Moving Averages")]
        public Brush OneHourSMA3Color
        { get; set; }

        [Browsable(false)]
        public string OneHourSMA3ColorSerializable
        {
            get { return Serialize.BrushToString(OneHourSMA3Color); }
            set { OneHourSMA3Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "1H SMA3 Line Width", Order = 14, GroupName = "1H Moving Averages")]
        public int OneHourSMA3LineWidth
        { get; set; }

        [Display(Name = "1H SMA3 Line Style", Order = 15, GroupName = "1H Moving Averages")]
        public DashStyleHelper OneHourSMA3LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show 1H SMA4", Order = 16, GroupName = "1H Moving Averages")]
        public bool ShowOneHourSMA4
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "1H SMA4 Period", Order = 17, GroupName = "1H Moving Averages")]
        public int OneHourSMA4Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "1H SMA4 Color", Order = 18, GroupName = "1H Moving Averages")]
        public Brush OneHourSMA4Color
        { get; set; }

        [Browsable(false)]
        public string OneHourSMA4ColorSerializable
        {
            get { return Serialize.BrushToString(OneHourSMA4Color); }
            set { OneHourSMA4Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "1H SMA4 Line Width", Order = 19, GroupName = "1H Moving Averages")]
        public int OneHourSMA4LineWidth
        { get; set; }

        [Display(Name = "1H SMA4 Line Style", Order = 20, GroupName = "1H Moving Averages")]
        public DashStyleHelper OneHourSMA4LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show 4H SMA1", Order = 1, GroupName = "4H Moving Averages")]
        public bool ShowFourHourSMA1
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "4H SMA1 Period", Order = 2, GroupName = "4H Moving Averages")]
        public int FourHourSMA1Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "4H SMA1 Color", Order = 3, GroupName = "4H Moving Averages")]
        public Brush FourHourSMA1Color
        { get; set; }

        [Browsable(false)]
        public string FourHourSMA1ColorSerializable
        {
            get { return Serialize.BrushToString(FourHourSMA1Color); }
            set { FourHourSMA1Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "4H SMA1 Line Width", Order = 4, GroupName = "4H Moving Averages")]
        public int FourHourSMA1LineWidth
        { get; set; }

        [Display(Name = "4H SMA1 Line Style", Order = 5, GroupName = "4H Moving Averages")]
        public DashStyleHelper FourHourSMA1LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show 4H SMA2", Order = 6, GroupName = "4H Moving Averages")]
        public bool ShowFourHourSMA2
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "4H SMA2 Period", Order = 7, GroupName = "4H Moving Averages")]
        public int FourHourSMA2Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "4H SMA2 Color", Order = 8, GroupName = "4H Moving Averages")]
        public Brush FourHourSMA2Color
        { get; set; }

        [Browsable(false)]
        public string FourHourSMA2ColorSerializable
        {
            get { return Serialize.BrushToString(FourHourSMA2Color); }
            set { FourHourSMA2Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "4H SMA2 Line Width", Order = 9, GroupName = "4H Moving Averages")]
        public int FourHourSMA2LineWidth
        { get; set; }

        [Display(Name = "4H SMA2 Line Style", Order = 10, GroupName = "4H Moving Averages")]
        public DashStyleHelper FourHourSMA2LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show 4H SMA3", Order = 11, GroupName = "4H Moving Averages")]
        public bool ShowFourHourSMA3
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "4H SMA3 Period", Order = 12, GroupName = "4H Moving Averages")]
        public int FourHourSMA3Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "4H SMA3 Color", Order = 13, GroupName = "4H Moving Averages")]
        public Brush FourHourSMA3Color
        { get; set; }

        [Browsable(false)]
        public string FourHourSMA3ColorSerializable
        {
            get { return Serialize.BrushToString(FourHourSMA3Color); }
            set { FourHourSMA3Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "4H SMA3 Line Width", Order = 14, GroupName = "4H Moving Averages")]
        public int FourHourSMA3LineWidth
        { get; set; }

        [Display(Name = "4H SMA3 Line Style", Order = 15, GroupName = "4H Moving Averages")]
        public DashStyleHelper FourHourSMA3LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show 4H SMA4", Order = 16, GroupName = "4H Moving Averages")]
        public bool ShowFourHourSMA4
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "4H SMA4 Period", Order = 17, GroupName = "4H Moving Averages")]
        public int FourHourSMA4Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "4H SMA4 Color", Order = 18, GroupName = "4H Moving Averages")]
        public Brush FourHourSMA4Color
        { get; set; }

        [Browsable(false)]
        public string FourHourSMA4ColorSerializable
        {
            get { return Serialize.BrushToString(FourHourSMA4Color); }
            set { FourHourSMA4Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "4H SMA4 Line Width", Order = 19, GroupName = "4H Moving Averages")]
        public int FourHourSMA4LineWidth
        { get; set; }

        [Display(Name = "4H SMA4 Line Style", Order = 20, GroupName = "4H Moving Averages")]
        public DashStyleHelper FourHourSMA4LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Daily SMA1", Order = 1, GroupName = "Daily Moving Averages")]
        public bool ShowDailySMA1
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Daily SMA1 Period", Order = 2, GroupName = "Daily Moving Averages")]
        public int DailySMA1Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Daily SMA1 Color", Order = 3, GroupName = "Daily Moving Averages")]
        public Brush DailySMA1Color
        { get; set; }

        [Browsable(false)]
        public string DailySMA1ColorSerializable
        {
            get { return Serialize.BrushToString(DailySMA1Color); }
            set { DailySMA1Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Daily SMA1 Line Width", Order = 4, GroupName = "Daily Moving Averages")]
        public int DailySMA1LineWidth
        { get; set; }

        [Display(Name = "Daily SMA1 Line Style", Order = 5, GroupName = "Daily Moving Averages")]
        public DashStyleHelper DailySMA1LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Daily SMA2", Order = 6, GroupName = "Daily Moving Averages")]
        public bool ShowDailySMA2
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Daily SMA2 Period", Order = 7, GroupName = "Daily Moving Averages")]
        public int DailySMA2Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Daily SMA2 Color", Order = 8, GroupName = "Daily Moving Averages")]
        public Brush DailySMA2Color
        { get; set; }

        [Browsable(false)]
        public string DailySMA2ColorSerializable
        {
            get { return Serialize.BrushToString(DailySMA2Color); }
            set { DailySMA2Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Daily SMA2 Line Width", Order = 9, GroupName = "Daily Moving Averages")]
        public int DailySMA2LineWidth
        { get; set; }

        [Display(Name = "Daily SMA2 Line Style", Order = 10, GroupName = "Daily Moving Averages")]
        public DashStyleHelper DailySMA2LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Daily SMA3", Order = 11, GroupName = "Daily Moving Averages")]
        public bool ShowDailySMA3
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Daily SMA3 Period", Order = 12, GroupName = "Daily Moving Averages")]
        public int DailySMA3Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Daily SMA3 Color", Order = 13, GroupName = "Daily Moving Averages")]
        public Brush DailySMA3Color
        { get; set; }

        [Browsable(false)]
        public string DailySMA3ColorSerializable
        {
            get { return Serialize.BrushToString(DailySMA3Color); }
            set { DailySMA3Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Daily SMA3 Line Width", Order = 14, GroupName = "Daily Moving Averages")]
        public int DailySMA3LineWidth
        { get; set; }

        [Display(Name = "Daily SMA3 Line Style", Order = 15, GroupName = "Daily Moving Averages")]
        public DashStyleHelper DailySMA3LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Daily SMA4", Order = 16, GroupName = "Daily Moving Averages")]
        public bool ShowDailySMA4
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Daily SMA4 Period", Order = 17, GroupName = "Daily Moving Averages")]
        public int DailySMA4Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Daily SMA4 Color", Order = 18, GroupName = "Daily Moving Averages")]
        public Brush DailySMA4Color
        { get; set; }

        [Browsable(false)]
        public string DailySMA4ColorSerializable
        {
            get { return Serialize.BrushToString(DailySMA4Color); }
            set { DailySMA4Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Daily SMA4 Line Width", Order = 19, GroupName = "Daily Moving Averages")]
        public int DailySMA4LineWidth
        { get; set; }

        [Display(Name = "Daily SMA4 Line Style", Order = 20, GroupName = "Daily Moving Averages")]
        public DashStyleHelper DailySMA4LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Weekly SMA1", Order = 1, GroupName = "Weekly Moving Averages")]
        public bool ShowWeeklySMA1
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Weekly SMA1 Period", Order = 2, GroupName = "Weekly Moving Averages")]
        public int WeeklySMA1Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Weekly SMA1 Color", Order = 3, GroupName = "Weekly Moving Averages")]
        public Brush WeeklySMA1Color
        { get; set; }

        [Browsable(false)]
        public string WeeklySMA1ColorSerializable
        {
            get { return Serialize.BrushToString(WeeklySMA1Color); }
            set { WeeklySMA1Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Weekly SMA1 Line Width", Order = 4, GroupName = "Weekly Moving Averages")]
        public int WeeklySMA1LineWidth
        { get; set; }

        [Display(Name = "Weekly SMA1 Line Style", Order = 5, GroupName = "Weekly Moving Averages")]
        public DashStyleHelper WeeklySMA1LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Weekly SMA2", Order = 6, GroupName = "Weekly Moving Averages")]
        public bool ShowWeeklySMA2
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Weekly SMA2 Period", Order = 7, GroupName = "Weekly Moving Averages")]
        public int WeeklySMA2Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Weekly SMA2 Color", Order = 8, GroupName = "Weekly Moving Averages")]
        public Brush WeeklySMA2Color
        { get; set; }

        [Browsable(false)]
        public string WeeklySMA2ColorSerializable
        {
            get { return Serialize.BrushToString(WeeklySMA2Color); }
            set { WeeklySMA2Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Weekly SMA2 Line Width", Order = 9, GroupName = "Weekly Moving Averages")]
        public int WeeklySMA2LineWidth
        { get; set; }

        [Display(Name = "Weekly SMA2 Line Style", Order = 10, GroupName = "Weekly Moving Averages")]
        public DashStyleHelper WeeklySMA2LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Weekly SMA3", Order = 11, GroupName = "Weekly Moving Averages")]
        public bool ShowWeeklySMA3
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Weekly SMA3 Period", Order = 12, GroupName = "Weekly Moving Averages")]
        public int WeeklySMA3Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Weekly SMA3 Color", Order = 13, GroupName = "Weekly Moving Averages")]
        public Brush WeeklySMA3Color
        { get; set; }

        [Browsable(false)]
        public string WeeklySMA3ColorSerializable
        {
            get { return Serialize.BrushToString(WeeklySMA3Color); }
            set { WeeklySMA3Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Weekly SMA3 Line Width", Order = 14, GroupName = "Weekly Moving Averages")]
        public int WeeklySMA3LineWidth
        { get; set; }

        [Display(Name = "Weekly SMA3 Line Style", Order = 15, GroupName = "Weekly Moving Averages")]
        public DashStyleHelper WeeklySMA3LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Weekly SMA4", Order = 16, GroupName = "Weekly Moving Averages")]
        public bool ShowWeeklySMA4
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Weekly SMA4 Period", Order = 17, GroupName = "Weekly Moving Averages")]
        public int WeeklySMA4Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Weekly SMA4 Color", Order = 18, GroupName = "Weekly Moving Averages")]
        public Brush WeeklySMA4Color
        { get; set; }

        [Browsable(false)]
        public string WeeklySMA4ColorSerializable
        {
            get { return Serialize.BrushToString(WeeklySMA4Color); }
            set { WeeklySMA4Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Weekly SMA4 Line Width", Order = 19, GroupName = "Weekly Moving Averages")]
        public int WeeklySMA4LineWidth
        { get; set; }

        [Display(Name = "Weekly SMA4 Line Style", Order = 20, GroupName = "Weekly Moving Averages")]
        public DashStyleHelper WeeklySMA4LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Monthly SMA1", Order = 1, GroupName = "Monthly Moving Averages")]
        public bool ShowMonthlySMA1
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Monthly SMA1 Period", Order = 2, GroupName = "Monthly Moving Averages")]
        public int MonthlySMA1Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Monthly SMA1 Color", Order = 3, GroupName = "Monthly Moving Averages")]
        public Brush MonthlySMA1Color
        { get; set; }

        [Browsable(false)]
        public string MonthlySMA1ColorSerializable
        {
            get { return Serialize.BrushToString(MonthlySMA1Color); }
            set { MonthlySMA1Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Monthly SMA1 Line Width", Order = 4, GroupName = "Monthly Moving Averages")]
        public int MonthlySMA1LineWidth
        { get; set; }

        [Display(Name = "Monthly SMA1 Line Style", Order = 5, GroupName = "Monthly Moving Averages")]
        public DashStyleHelper MonthlySMA1LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Monthly SMA2", Order = 6, GroupName = "Monthly Moving Averages")]
        public bool ShowMonthlySMA2
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Monthly SMA2 Period", Order = 7, GroupName = "Monthly Moving Averages")]
        public int MonthlySMA2Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Monthly SMA2 Color", Order = 8, GroupName = "Monthly Moving Averages")]
        public Brush MonthlySMA2Color
        { get; set; }

        [Browsable(false)]
        public string MonthlySMA2ColorSerializable
        {
            get { return Serialize.BrushToString(MonthlySMA2Color); }
            set { MonthlySMA2Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Monthly SMA2 Line Width", Order = 9, GroupName = "Monthly Moving Averages")]
        public int MonthlySMA2LineWidth
        { get; set; }

        [Display(Name = "Monthly SMA2 Line Style", Order = 10, GroupName = "Monthly Moving Averages")]
        public DashStyleHelper MonthlySMA2LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Monthly SMA3", Order = 11, GroupName = "Monthly Moving Averages")]
        public bool ShowMonthlySMA3
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Monthly SMA3 Period", Order = 12, GroupName = "Monthly Moving Averages")]
        public int MonthlySMA3Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Monthly SMA3 Color", Order = 13, GroupName = "Monthly Moving Averages")]
        public Brush MonthlySMA3Color
        { get; set; }

        [Browsable(false)]
        public string MonthlySMA3ColorSerializable
        {
            get { return Serialize.BrushToString(MonthlySMA3Color); }
            set { MonthlySMA3Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Monthly SMA3 Line Width", Order = 14, GroupName = "Monthly Moving Averages")]
        public int MonthlySMA3LineWidth
        { get; set; }

        [Display(Name = "Monthly SMA3 Line Style", Order = 15, GroupName = "Monthly Moving Averages")]
        public DashStyleHelper MonthlySMA3LineStyle
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Monthly SMA4", Order = 16, GroupName = "Monthly Moving Averages")]
        public bool ShowMonthlySMA4
        { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Monthly SMA4 Period", Order = 17, GroupName = "Monthly Moving Averages")]
        public int MonthlySMA4Period
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Monthly SMA4 Color", Order = 18, GroupName = "Monthly Moving Averages")]
        public Brush MonthlySMA4Color
        { get; set; }

        [Browsable(false)]
        public string MonthlySMA4ColorSerializable
        {
            get { return Serialize.BrushToString(MonthlySMA4Color); }
            set { MonthlySMA4Color = Serialize.StringToBrush(value); }
        }

        [Range(1, 10)]
        [Display(Name = "Monthly SMA4 Line Width", Order = 19, GroupName = "Monthly Moving Averages")]
        public int MonthlySMA4LineWidth
        { get; set; }

        [Display(Name = "Monthly SMA4 Line Style", Order = 20, GroupName = "Monthly Moving Averages")]
        public DashStyleHelper MonthlySMA4LineStyle
        { get; set; }

        [Display(Name = "RTH Start Time", Order = 1, GroupName = "Sessions")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime RTHStartTime
        { get; set; }

        [Display(Name = "RTH End Time", Order = 2, GroupName = "Sessions")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime RTHEndTime
        { get; set; }

        [Display(Name = "ETH Start Time", Order = 3, GroupName = "Sessions")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime ETHStartTime
        { get; set; }

        [Display(Name = "ETH End Time", Order = 4, GroupName = "Sessions")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime ETHEndTime
        { get; set; }

        [XmlIgnore]
        [Display(Name = "ETH Background Color", Order = 5, GroupName = "Sessions")]
        public Brush ETHBackgroundColor
        { get; set; }

        [Browsable(false)]
        public string ETHBackgroundColorSerializable
        {
            get { return Serialize.BrushToString(ETHBackgroundColor); }
            set { ETHBackgroundColor = Serialize.StringToBrush(value); }
        }

        [Range(1, 100)]
        [Display(Name = "ETH Background Opacity", Order = 6, GroupName = "Sessions")]
        public int ETHBackgroundOpacity
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Extend SMA Levels", Order = 1, GroupName = "Display")]
        public bool ExtendSMALevels
        { get; set; }

        [Range(1, 100)]
        [Display(Name = "SMA Level Length", Order = 2, GroupName = "Display")]
        public int SMALevelLength
        { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Show Labels", Order = 3, GroupName = "Display")]
        public bool ShowLabels
        { get; set; }

        [Range(1, 100)]
        [Display(Name = "Label Size", Order = 4, GroupName = "Display")]
        public int LabelSize
        { get; set; }

        [Display(Name = "Label Bold", Order = 5, GroupName = "Display")]
        public bool LabelBold
        { get; set; }

        [Range(0, 100)]
        [Display(Name = "Label Offset", Order = 6, GroupName = "Display")]
        public int LabelOffset
        { get; set; }

        [ReadOnly(true)]
        [XmlIgnore]
        [Display(Name = "Version", Description = "Version information.", Order = 1, GroupName = "Developer")]
        public string VersionInformation
        { get; set; }

        [Display(Name = "Debug", Description = "Toggle debug logging.", Order = 2, GroupName = "Developer")]
        public bool Debug
        { get; set; }

        #endregion

        #region Indicator methods

        protected override void OnStateChange()
		{
            DebugPrint($"OnStateChange({State})");
            
			if (State == State.SetDefaults)
			{
				Description									= @"Intraday trading levels.";
				Name										= "LunarTick Levels";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

				ShowOpeningPrint = true;
                OpeningPrintColor = Brushes.Purple;
                OpeningPrintLineWidth = 2;
                OpeningPrintLineStyle = DashStyleHelper.Dash;

                ShowPreviousDailyHigh = true;
                PreviousDailyHighColor = Brushes.Red;
                PreviousDailyHighLineWidth = 1;
                PreviousDailyHighLineStyle = DashStyleHelper.Solid;

                ShowPreviousDailyLow = true;
                PreviousDailyLowColor = Brushes.Green;
                PreviousDailyLowLineWidth = 1;
                PreviousDailyLowLineStyle = DashStyleHelper.Solid;

                ShowOvernightHigh = true;
                OvernightHighColor = Brushes.Gray;
                OvernightHighLineWidth = 1;
                OvernightHighLineStyle = DashStyleHelper.Solid;

                ShowOvernightLow = true;
                OvernightLowColor = Brushes.Gray;
                OvernightLowLineWidth = 1;
                OvernightLowLineStyle = DashStyleHelper.Solid;

                ShowOneHourSMA1 = false;
                OneHourSMA1Period = 20;
                OneHourSMA1Color = Brushes.Fuchsia;
                OneHourSMA1LineWidth = 1;
                OneHourSMA1LineStyle = DashStyleHelper.Solid;

                ShowOneHourSMA2 = true;
                OneHourSMA2Period = 50;
                OneHourSMA2Color = Brushes.Fuchsia;
                OneHourSMA2LineWidth = 1;
                OneHourSMA2LineStyle = DashStyleHelper.Solid;

                ShowOneHourSMA3 = true;
                OneHourSMA3Period = 100;
                OneHourSMA3Color = Brushes.Fuchsia;
                OneHourSMA3LineWidth = 1;
                OneHourSMA3LineStyle = DashStyleHelper.Solid;

                ShowOneHourSMA4 = true;
                OneHourSMA4Period = 200;
                OneHourSMA4Color = Brushes.Fuchsia;
                OneHourSMA4LineWidth = 1;
                OneHourSMA4LineStyle = DashStyleHelper.Solid;

                ShowFourHourSMA1 = false;
                FourHourSMA1Period = 20;
                FourHourSMA1Color = Brushes.Maroon;
                FourHourSMA1LineWidth = 1;
                FourHourSMA1LineStyle = DashStyleHelper.Solid;

                ShowFourHourSMA2 = true;
                FourHourSMA2Period = 50;
                FourHourSMA2Color = Brushes.Maroon;
                FourHourSMA2LineWidth = 1;
                FourHourSMA2LineStyle = DashStyleHelper.Solid;

                ShowFourHourSMA3 = true;
                FourHourSMA3Period = 100;
                FourHourSMA3Color = Brushes.Maroon;
                FourHourSMA3LineWidth = 1;
                FourHourSMA3LineStyle = DashStyleHelper.Solid;

                ShowFourHourSMA4 = true;
                FourHourSMA4Period = 200;
                FourHourSMA4Color = Brushes.Maroon;
                FourHourSMA4LineWidth = 1;
                FourHourSMA4LineStyle = DashStyleHelper.Solid;

                ShowDailySMA1 = false;
                DailySMA1Period = 20;
                DailySMA1Color = Brushes.Blue;
                DailySMA1LineWidth = 1;
                DailySMA1LineStyle = DashStyleHelper.Solid;

                ShowDailySMA2 = true;
                DailySMA2Period = 50;
                DailySMA2Color = Brushes.Blue;
                DailySMA2LineWidth = 1;
                DailySMA2LineStyle = DashStyleHelper.Solid;

                ShowDailySMA3 = true;
                DailySMA3Period = 100;
                DailySMA3Color = Brushes.Blue;
                DailySMA3LineWidth = 1;
                DailySMA3LineStyle = DashStyleHelper.Solid;

                ShowDailySMA4 = true;
                DailySMA4Period = 200;
                DailySMA4Color = Brushes.Blue;
                DailySMA4LineWidth = 1;
                DailySMA4LineStyle = DashStyleHelper.Solid;

                ShowWeeklySMA1 = false;
                WeeklySMA1Period = 20;
                WeeklySMA1Color = Brushes.Navy;
                WeeklySMA1LineWidth = 1;
                WeeklySMA1LineStyle = DashStyleHelper.Solid;

                ShowWeeklySMA2 = false;
                WeeklySMA2Period = 50;
                WeeklySMA2Color = Brushes.Navy;
                WeeklySMA2LineWidth = 1;
                WeeklySMA2LineStyle = DashStyleHelper.Solid;

                ShowWeeklySMA3 = false;
                WeeklySMA3Period = 100;
                WeeklySMA3Color = Brushes.Navy;
                WeeklySMA3LineWidth = 1;
                WeeklySMA3LineStyle = DashStyleHelper.Solid;

                ShowWeeklySMA4 = false;
                WeeklySMA4Period = 200;
                WeeklySMA4Color = Brushes.Navy;
                WeeklySMA4LineWidth = 1;
                WeeklySMA4LineStyle = DashStyleHelper.Solid;

                ShowMonthlySMA1 = false;
                MonthlySMA1Period = 20;
                MonthlySMA1Color = Brushes.Gray;
                MonthlySMA1LineWidth = 1;
                MonthlySMA1LineStyle = DashStyleHelper.Solid;

                ShowMonthlySMA2 = false;
                MonthlySMA2Period = 50;
                MonthlySMA2Color = Brushes.Gray;
                MonthlySMA2LineWidth = 1;
                MonthlySMA2LineStyle = DashStyleHelper.Solid;

                ShowMonthlySMA3 = false;
                MonthlySMA3Period = 100;
                MonthlySMA3Color = Brushes.Gray;
                MonthlySMA3LineWidth = 1;
                MonthlySMA3LineStyle = DashStyleHelper.Solid;

                ShowMonthlySMA4 = false;
                MonthlySMA4Period = 200;
                MonthlySMA4Color = Brushes.Gray;
                MonthlySMA4LineWidth = 1;
                MonthlySMA4LineStyle = DashStyleHelper.Solid;

                RTHStartTime = DateTime.Parse("09:30:00");
                RTHEndTime = DateTime.Parse("17:00:00");
                ETHStartTime = DateTime.Parse("18:00:00");
                ETHEndTime = DateTime.Parse("17:00:00");
                ETHBackgroundColor = Brushes.DarkBlue;
                ETHBackgroundOpacity = 10;

                ExtendSMALevels = false;
                SMALevelLength = 5;

				ShowLabels = true;
                LabelSize = 11;
                LabelBold = true;
                LabelOffset = 5;

                VersionInformation = $"{Version} - {Assembly.GetAssembly(typeof(LunarTickLevels)).GetName().Version}";
                Debug = false;
            }
            else if (State == State.Configure)
            {
                int additionalDataSeriesCount = 0;

                AddDataSeries(BarsPeriodType.Minute, 1);    // BarsInProgress == 1
                additionalDataSeriesCount++;

                if (ShowOneHourSMA1 || ShowOneHourSMA2 || ShowOneHourSMA3 || ShowOneHourSMA4)
                {
                    AddDataSeries(BarsPeriodType.Minute, 60);
                    additionalDataSeriesCount++;
                    _oneHourDataSeriesIndex = additionalDataSeriesCount;
                }

                if (ShowFourHourSMA1 || ShowFourHourSMA2 || ShowFourHourSMA3 || ShowFourHourSMA4)
                {
                    AddDataSeries(BarsPeriodType.Minute, 240);
                    additionalDataSeriesCount++;
                    _fourHourDataSeriesIndex = additionalDataSeriesCount;
                }

                if (ShowDailySMA1 || ShowDailySMA2 || ShowDailySMA3 || ShowDailySMA4)
                {
                    AddDataSeries(BarsPeriodType.Day, 1);
                    additionalDataSeriesCount++;
                    _dailyDataSeriesIndex = additionalDataSeriesCount;
                }

                if (ShowWeeklySMA1 || ShowWeeklySMA2 || ShowWeeklySMA3 || ShowWeeklySMA4)
                {
                    AddDataSeries(BarsPeriodType.Week, 1);
                    additionalDataSeriesCount++;
                    _weeklyDataSeriesIndex = additionalDataSeriesCount;
                }

                if (ShowMonthlySMA1 || ShowMonthlySMA2 || ShowMonthlySMA3 || ShowMonthlySMA4)
                {
                    AddDataSeries(BarsPeriodType.Month, 1);
                    additionalDataSeriesCount++;
                    _monthlyDataSeriesIndex = additionalDataSeriesCount;
                }
            }
            else if (State == State.DataLoaded)
			{
                _opBarDateTime = null;
                _pdhBarDateTime = null;
                _pdlBarDateTime = null;
                _conhBarDateTime = null;
                _conlBarDateTime = null;
                _ponhBarDateTime = null;
                _ponlBarDateTime = null;
                _cdhBarDateTime = null;
                _cdlBarDateTime = null;
                
                _labelFont = new SimpleFont() { Family = ChartControl.Properties.LabelFont.Family, Size = LabelSize, Bold = LabelBold };

                _ethBackgroundBrush = ETHBackgroundColor.Clone();
                _ethBackgroundBrush.Opacity = ETHBackgroundOpacity * 0.01;
                _ethBackgroundBrush.Freeze();

                // Detect number of decimal places.
                var tickSize = Instrument.MasterInstrument.TickSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
                tickSize.LastIndexOf('.');
                _decimalPlaces = tickSize.Contains(".") ? tickSize.Split('.')[1].Length : 0;

                if (ShowOneHourSMA1)
                    _oneHourSMA1 = SMA(BarsArray[_oneHourDataSeriesIndex], OneHourSMA1Period);
                if (ShowOneHourSMA2)
                    _oneHourSMA2 = SMA(BarsArray[_oneHourDataSeriesIndex], OneHourSMA2Period);
                if (ShowOneHourSMA3)
                    _oneHourSMA3 = SMA(BarsArray[_oneHourDataSeriesIndex], OneHourSMA3Period);
                if (ShowOneHourSMA4)
                    _oneHourSMA4 = SMA(BarsArray[_oneHourDataSeriesIndex], OneHourSMA4Period);

                if (ShowFourHourSMA1)
                    _fourHourSMA1 = SMA(BarsArray[_fourHourDataSeriesIndex], FourHourSMA1Period);
                if (ShowFourHourSMA2)
                    _fourHourSMA2 = SMA(BarsArray[_fourHourDataSeriesIndex], FourHourSMA2Period);
                if (ShowFourHourSMA3)
                    _fourHourSMA3 = SMA(BarsArray[_fourHourDataSeriesIndex], FourHourSMA3Period);
                if (ShowFourHourSMA4)
                    _fourHourSMA4 = SMA(BarsArray[_fourHourDataSeriesIndex], FourHourSMA4Period);

                if (ShowDailySMA1)
                    _dailySMA1 = SMA(BarsArray[_dailyDataSeriesIndex], DailySMA1Period);
                if (ShowDailySMA2)
                    _dailySMA2 = SMA(BarsArray[_dailyDataSeriesIndex], DailySMA2Period);
                if (ShowDailySMA3)
                    _dailySMA3 = SMA(BarsArray[_dailyDataSeriesIndex], DailySMA3Period);
                if (ShowDailySMA4)
                    _dailySMA4 = SMA(BarsArray[_dailyDataSeriesIndex], DailySMA4Period);

                if (ShowWeeklySMA1)
                    _weeklySMA1 = SMA(BarsArray[_weeklyDataSeriesIndex], WeeklySMA1Period);
                if (ShowWeeklySMA2)
                    _weeklySMA2 = SMA(BarsArray[_weeklyDataSeriesIndex], WeeklySMA2Period);
                if (ShowWeeklySMA3)
                    _weeklySMA3 = SMA(BarsArray[_weeklyDataSeriesIndex], WeeklySMA3Period);
                if (ShowWeeklySMA4)
                    _weeklySMA4 = SMA(BarsArray[_weeklyDataSeriesIndex], WeeklySMA4Period);

                if (ShowMonthlySMA1)
                    _monthlySMA1 = SMA(BarsArray[_monthlyDataSeriesIndex], MonthlySMA1Period);
                if (ShowMonthlySMA2)
                    _monthlySMA2 = SMA(BarsArray[_monthlyDataSeriesIndex], MonthlySMA2Period);
                if (ShowMonthlySMA3)
                    _monthlySMA3 = SMA(BarsArray[_monthlyDataSeriesIndex], MonthlySMA3Period);
                if (ShowMonthlySMA4)
                    _monthlySMA4 = SMA(BarsArray[_monthlyDataSeriesIndex], MonthlySMA4Period);
            }
        }

		protected override void OnBarUpdate()
		{
            if (!Bars.BarsType.IsIntraday)
                return;
			if (CurrentBar < 2)
				return;

            if (BarsInProgress == 1)
            {
                // 1-Minute bar series.
                DebugPrint($"BarsInProgress [{BarsInProgress}] :: CurrentBar [{CurrentBar}]");

                // FYI: Ninja bar times are end of bar.
                var currentBarStartTime = Time[1];
                var currentBarEndTime = Time[0];
                var previousBarStartTime = Time[2];
                var previousBarEndTime = Time[1];

                bool isCurrentBarInsideRTHSession = (ToTime(currentBarEndTime) > ToTime(currentBarStartTime)) && (ToTime(currentBarStartTime) >= ToTime(RTHStartTime)) && (ToTime(currentBarEndTime) <= ToTime(RTHEndTime));
                bool isPreviousBarInsideRTHSession = (ToTime(previousBarEndTime) > ToTime(previousBarStartTime)) && (ToTime(previousBarStartTime) >= ToTime(RTHStartTime)) && (ToTime(previousBarEndTime) <= ToTime(RTHEndTime));

                //////////////////////////////////////////////////
                // Opening Print
                //////////////////////////////////////////////////

                if (IsFirstTickOfBar && isCurrentBarInsideRTHSession && !isPreviousBarInsideRTHSession)
                {
                    // New RTH session started
                    //DebugPrint($"OP: Bar {CurrentBar} entered RTH: O={Open[0]}, H={High[0]}, L={Low[0]}, C={Close[0]}, T={Time[0]}");
                    _opValue = Open[0];
                    _opBarDateTime = Time[0];
                }

                //////////////////////////////////////////////////
                // Previous Daily High/Low
                //////////////////////////////////////////////////

                if (IsFirstTickOfBar && isCurrentBarInsideRTHSession && !isPreviousBarInsideRTHSession)
                {
                    // New RTH session started
                    //DebugPrint($"PDH/L: Bar {CurrentBar} entered RTH: O={Open[0]}, H={High[0]}, L={Low[0]}, C={Close[0]}, T={Time[0]}");
                    _cdhValue = High[0];
                    _cdhBarDateTime = Time[0];
                    _cdlValue = Low[0];
                    _cdlBarDateTime = Time[0];
                }

                if (IsFirstTickOfBar && !isCurrentBarInsideRTHSession && isPreviousBarInsideRTHSession)
                {
                    // Just left RTH session
                    //DebugPrint($"PDH/L: Bar {CurrentBar} left RTH: O={Open[0]}, H={High[0]}, L={Low[0]}, C={Close[0]}, T={Time[0]}");
                    _pdhValue = _cdhValue;
                    _pdhBarDateTime = _cdhBarDateTime;
                    _pdlValue = _cdlValue;
                    _pdlBarDateTime = _cdlBarDateTime;
                }

                if (isCurrentBarInsideRTHSession)
                {
                    // Inside RTH session
                    if (High[0] > _cdhValue)
                    {
                        //DebugPrint($"PDH/L: Bar {CurrentBar} made new High: O={Open[0]}, H={High[0]}, L={Low[0]}, C={Close[0]}, T={Time[0]}");
                        _cdhValue = High[0];
                        _cdhBarDateTime = Time[0];
                    }
                    if (Low[0] < _cdlValue)
                    {
                        //DebugPrint($"PDH/L: Bar {CurrentBar} made new Low: O={Open[0]}, H={High[0]}, L={Low[0]}, C={Close[0]}, T={Time[0]}");
                        _cdlValue = Low[0];
                        _cdlBarDateTime = Time[0];
                    }
                }

                //////////////////////////////////////////////////
                // Overnight High/Low
                //////////////////////////////////////////////////

                if (IsFirstTickOfBar && (ToTime(previousBarEndTime) <= ToTime(ETHStartTime)) && (ToTime(currentBarEndTime) > ToTime(ETHStartTime)))
                {
                    // New ETH session started
                    //DebugPrint($"ONH/L: Bar {CurrentBar} entered new ETH: O={Open[0]}, H={High[0]}, L={Low[0]}, C={Close[0]}, T={Time[0]}");
                    _conhValue = High[0];
                    _conhBarDateTime = Time[0];
                    _conlValue = Low[0];
                    _conlBarDateTime = Time[0];
                }

                if (IsFirstTickOfBar && isCurrentBarInsideRTHSession && !isPreviousBarInsideRTHSession)
                {
                    // Just entered RTH session
                    //DebugPrint($"ONH/L: Bar {CurrentBar} entered RTH: O={Open[0]}, H={High[0]}, L={Low[0]}, C={Close[0]}, T={Time[0]}");
                    _ponhValue = _conhValue;
                    _ponhBarDateTime = _conhBarDateTime;
                    _ponlValue = _conlValue;
                    _ponlBarDateTime = _conlBarDateTime;
                }

                if (!isCurrentBarInsideRTHSession)
                {
                    // Inside overnight session
                    if (High[0] > _conhValue)
                    {
                        //DebugPrint($"ONH/L: Bar {CurrentBar} made new High: O={Open[0]}, H={High[0]}, L={Low[0]}, C={Close[0]}, T={Time[0]}");
                        _conhValue = High[0];
                        _conhBarDateTime = Time[0];
                    }
                    if (Low[0] < _conlValue)
                    {
                        //DebugPrint($"ONH/L: Bar {CurrentBar} made new Low: O={Open[0]}, H={High[0]}, L={Low[0]}, C={Close[0]}, T={Time[0]}");
                        _conlValue = Low[0];
                        _conlBarDateTime = Time[0];
                    }
                }
            }
            else if (BarsInProgress == 0)
            {
                // Chart bar series.
                DebugPrint($"BarsInProgress [{BarsInProgress}] :: CurrentBar [{CurrentBar}]");

                // FYI: Ninja bar times are end of bar.
                var currentBarStartTime = Time[1];
                var currentBarEndTime = Time[0];
                var previousBarStartTime = Time[2];
                var previousBarEndTime = Time[1];

                bool isCurrentBarInsideRTHSession = (ToTime(currentBarEndTime) > ToTime(currentBarStartTime)) && (ToTime(currentBarStartTime) >= ToTime(RTHStartTime)) && (ToTime(currentBarEndTime) <= ToTime(RTHEndTime));
                bool isPreviousBarInsideRTHSession = (ToTime(previousBarEndTime) > ToTime(previousBarStartTime)) && (ToTime(previousBarStartTime) >= ToTime(RTHStartTime)) && (ToTime(previousBarEndTime) <= ToTime(RTHEndTime));

                //////////////////////////////////////////////////
                // Opening Print
                //////////////////////////////////////////////////

                if (ShowOpeningPrint && _opBarDateTime.HasValue)
                {
                    int startBarIndex = Bars.GetBar(_opBarDateTime.Value);
                    UpdateLineAndLabel("LunarTickLevels_OP", false, _opValue, CurrentBar - startBarIndex, -LabelOffset, OpeningPrintColor, OpeningPrintLineStyle, OpeningPrintLineWidth, $"{_opValue.ToString($"F{_decimalPlaces}")} OP");
                }

                //////////////////////////////////////////////////
                // Previous Daily High/Low
                //////////////////////////////////////////////////

                if (ShowPreviousDailyHigh && _pdhBarDateTime.HasValue)
                {
                    int startBarIndex = Bars.GetBar(_pdhBarDateTime.Value);
                    UpdateLineAndLabel("LunarTickLevels_PDH", false, _pdhValue, CurrentBar - startBarIndex, -LabelOffset, PreviousDailyHighColor, PreviousDailyHighLineStyle, PreviousDailyHighLineWidth, $"{_pdhValue.ToString($"F{_decimalPlaces}")} PDH");
                }
                if (ShowPreviousDailyLow && _pdlBarDateTime.HasValue)
                {
                    int startBarIndex = Bars.GetBar(_pdlBarDateTime.Value);
                    UpdateLineAndLabel("LunarTickLevels_PDL", false, _pdlValue, CurrentBar - startBarIndex, -LabelOffset, PreviousDailyLowColor, PreviousDailyLowLineStyle, PreviousDailyLowLineWidth, $"{_pdlValue.ToString($"F{_decimalPlaces}")} PDL");
                }

                //////////////////////////////////////////////////
                // Overnight High/Low
                //////////////////////////////////////////////////

                if (ShowOvernightHigh && _ponhBarDateTime.HasValue)
                {
                    int startBarIndex = Bars.GetBar(_ponhBarDateTime.Value);
                    UpdateLineAndLabel("LunarTickLevels_ONH", false, _ponhValue, CurrentBar - startBarIndex, -LabelOffset, OvernightHighColor, OvernightHighLineStyle, OvernightHighLineWidth, $"{_ponhValue.ToString($"F{_decimalPlaces}")} ONH");
                }
                if (ShowOvernightLow && _ponlBarDateTime.HasValue)
                {
                    int startBarIndex = Bars.GetBar(_ponlBarDateTime.Value);
                    UpdateLineAndLabel("LunarTickLevels_ONL", false, _ponlValue, CurrentBar - startBarIndex, -LabelOffset, OvernightLowColor, OvernightLowLineStyle, OvernightLowLineWidth, $"{_ponlValue.ToString($"F{_decimalPlaces}")} ONL");
                }

                //////////////////////////////////////////////////
                // 1H Moving Averages
                //////////////////////////////////////////////////

                if (ShowOneHourSMA1 && CurrentBars[_oneHourDataSeriesIndex] >= OneHourSMA1Period)
                {
                    double oneHourSMA1Value = _oneHourSMA1[0];
                    UpdateLineAndLabel("LunarTickLevels_1H_SMA1", false, oneHourSMA1Value, GetSMALevelsStartBarsAgo(), -LabelOffset, OneHourSMA1Color, OneHourSMA1LineStyle, OneHourSMA1LineWidth, $"{oneHourSMA1Value.ToString($"F{_decimalPlaces}")} 1H {OneHourSMA1Period}");
                }

                if (ShowOneHourSMA2 && CurrentBars[_oneHourDataSeriesIndex] >= OneHourSMA2Period)
                {
                    double oneHourSMA2Value = _oneHourSMA2[0];
                    UpdateLineAndLabel("LunarTickLevels_1H_SMA2", false, oneHourSMA2Value, GetSMALevelsStartBarsAgo(), -LabelOffset, OneHourSMA2Color, OneHourSMA2LineStyle, OneHourSMA2LineWidth, $"{oneHourSMA2Value.ToString($"F{_decimalPlaces}")} 1H {OneHourSMA2Period}");
                }

                if (ShowOneHourSMA3 && CurrentBars[_oneHourDataSeriesIndex] >= OneHourSMA3Period)
                {
                    double oneHourSMA3Value = _oneHourSMA3[0];
                    UpdateLineAndLabel("LunarTickLevels_1H_SMA3", false, oneHourSMA3Value, GetSMALevelsStartBarsAgo(), -LabelOffset, OneHourSMA3Color, OneHourSMA3LineStyle, OneHourSMA3LineWidth, $"{oneHourSMA3Value.ToString($"F{_decimalPlaces}")} 1H {OneHourSMA3Period}");
                }

                if (ShowOneHourSMA4 && CurrentBars[_oneHourDataSeriesIndex] >= OneHourSMA4Period)
                {
                    double oneHourSMA4Value = _oneHourSMA4[0];
                    UpdateLineAndLabel("LunarTickLevels_1H_SMA4", false, oneHourSMA4Value, GetSMALevelsStartBarsAgo(), -LabelOffset, OneHourSMA4Color, OneHourSMA4LineStyle, OneHourSMA4LineWidth, $"{oneHourSMA4Value.ToString($"F{_decimalPlaces}")} 1H {OneHourSMA4Period}");
                }

                //////////////////////////////////////////////////
                // 4H Moving Averages
                //////////////////////////////////////////////////

                if (ShowFourHourSMA1 && CurrentBars[_fourHourDataSeriesIndex] >= FourHourSMA1Period)
                {
                    double fourHourSMA1Value = _fourHourSMA1[0];
                    UpdateLineAndLabel("LunarTickLevels_4H_SMA1", false, fourHourSMA1Value, GetSMALevelsStartBarsAgo(), -LabelOffset, FourHourSMA1Color, FourHourSMA1LineStyle, FourHourSMA1LineWidth, $"{fourHourSMA1Value.ToString($"F{_decimalPlaces}")} 4H {FourHourSMA1Period}");
                }

                if (ShowFourHourSMA2 && CurrentBars[_fourHourDataSeriesIndex] >= FourHourSMA2Period)
                {
                    double fourHourSMA2Value = _fourHourSMA2[0];
                    UpdateLineAndLabel("LunarTickLevels_4H_SMA2", false, fourHourSMA2Value, GetSMALevelsStartBarsAgo(), -LabelOffset, FourHourSMA2Color, FourHourSMA2LineStyle, FourHourSMA2LineWidth, $"{fourHourSMA2Value.ToString($"F{_decimalPlaces}")} 4H {FourHourSMA2Period}");
                }

                if (ShowFourHourSMA3 && CurrentBars[_fourHourDataSeriesIndex] >= FourHourSMA3Period)
                {
                    double fourHourSMA3Value = _fourHourSMA3[0];
                    UpdateLineAndLabel("LunarTickLevels_4H_SMA3", false, fourHourSMA3Value, GetSMALevelsStartBarsAgo(), -LabelOffset, FourHourSMA3Color, FourHourSMA3LineStyle, FourHourSMA3LineWidth, $"{fourHourSMA3Value.ToString($"F{_decimalPlaces}")} 4H {FourHourSMA3Period}");
                }

                if (ShowFourHourSMA4 && CurrentBars[_fourHourDataSeriesIndex] >= FourHourSMA4Period)
                {
                    double fourHourSMA4Value = _fourHourSMA4[0];
                    UpdateLineAndLabel("LunarTickLevels_4H_SMA4", false, fourHourSMA4Value, GetSMALevelsStartBarsAgo(), -LabelOffset, FourHourSMA4Color, FourHourSMA4LineStyle, FourHourSMA4LineWidth, $"{fourHourSMA4Value.ToString($"F{_decimalPlaces}")} 4H {FourHourSMA4Period}");
                }

                //////////////////////////////////////////////////
                // Daily Moving Averages
                //////////////////////////////////////////////////

                if (ShowDailySMA1 && CurrentBars[_dailyDataSeriesIndex] >= DailySMA1Period)
                {
                    double dailySMA1Value = _dailySMA1[0];
                    UpdateLineAndLabel("LunarTickLevels_Daily_SMA1", false, dailySMA1Value, GetSMALevelsStartBarsAgo(), -LabelOffset, DailySMA1Color, DailySMA1LineStyle, DailySMA1LineWidth, $"{dailySMA1Value.ToString($"F{_decimalPlaces}")} Daily {DailySMA1Period}");
                }

                if (ShowDailySMA2 && CurrentBars[_dailyDataSeriesIndex] >= DailySMA2Period)
                {
                    double dailySMA2Value = _dailySMA2[0];
                    UpdateLineAndLabel("LunarTickLevels_Daily_SMA2", false, dailySMA2Value, GetSMALevelsStartBarsAgo(), -LabelOffset, DailySMA2Color, DailySMA2LineStyle, DailySMA2LineWidth, $"{dailySMA2Value.ToString($"F{_decimalPlaces}")} Daily {DailySMA2Period}");
                }

                if (ShowDailySMA3 && CurrentBars[_dailyDataSeriesIndex] >= DailySMA3Period)
                {
                    double dailySMA3Value = _dailySMA3[0];
                    UpdateLineAndLabel("LunarTickLevels_Daily_SMA3", false, dailySMA3Value, GetSMALevelsStartBarsAgo(), -LabelOffset, DailySMA3Color, DailySMA3LineStyle, DailySMA3LineWidth, $"{dailySMA3Value.ToString($"F{_decimalPlaces}")} Daily {DailySMA3Period}");
                }

                if (ShowDailySMA4 && CurrentBars[_dailyDataSeriesIndex] >= DailySMA4Period)
                {
                    double dailySMA4Value = _dailySMA4[0];
                    UpdateLineAndLabel("LunarTickLevels_Daily_SMA4", false, dailySMA4Value, GetSMALevelsStartBarsAgo(), -LabelOffset, DailySMA4Color, DailySMA4LineStyle, DailySMA4LineWidth, $"{dailySMA4Value.ToString($"F{_decimalPlaces}")} Daily {DailySMA4Period}");
                }

                //////////////////////////////////////////////////
                // Weekly Moving Averages
                //////////////////////////////////////////////////

                if (ShowWeeklySMA1 && CurrentBars[_weeklyDataSeriesIndex] >= WeeklySMA1Period)
                {
                    double weeklySMA1Value = _weeklySMA1[0];
                    UpdateLineAndLabel("LunarTickLevels_Weekly_SMA1", false, weeklySMA1Value, GetSMALevelsStartBarsAgo(), -LabelOffset, WeeklySMA1Color, WeeklySMA1LineStyle, WeeklySMA1LineWidth, $"{weeklySMA1Value.ToString($"F{_decimalPlaces}")} Weekly {WeeklySMA1Period}");
                }

                if (ShowWeeklySMA2 && CurrentBars[_weeklyDataSeriesIndex] >= WeeklySMA2Period)
                {
                    double weeklySMA2Value = _weeklySMA2[0];
                    UpdateLineAndLabel("LunarTickLevels_Weekly_SMA2", false, weeklySMA2Value, GetSMALevelsStartBarsAgo(), -LabelOffset, WeeklySMA2Color, WeeklySMA2LineStyle, WeeklySMA2LineWidth, $"{weeklySMA2Value.ToString($"F{_decimalPlaces}")} Weekly {WeeklySMA2Period}");
                }

                if (ShowWeeklySMA3 && CurrentBars[_weeklyDataSeriesIndex] >= WeeklySMA3Period)
                {
                    double weeklySMA3Value = _weeklySMA3[0];
                    UpdateLineAndLabel("LunarTickLevels_Weekly_SMA3", false, weeklySMA3Value, GetSMALevelsStartBarsAgo(), -LabelOffset, WeeklySMA3Color, WeeklySMA3LineStyle, WeeklySMA3LineWidth, $"{weeklySMA3Value.ToString($"F{_decimalPlaces}")} Weekly {WeeklySMA3Period}");
                }

                if (ShowWeeklySMA4 && CurrentBars[_weeklyDataSeriesIndex] >= WeeklySMA4Period)
                {
                    double weeklySMA4Value = _weeklySMA4[0];
                    UpdateLineAndLabel("LunarTickLevels_Weekly_SMA4", false, weeklySMA4Value, GetSMALevelsStartBarsAgo(), -LabelOffset, WeeklySMA4Color, WeeklySMA4LineStyle, WeeklySMA4LineWidth, $"{weeklySMA4Value.ToString($"F{_decimalPlaces}")} Weekly {WeeklySMA4Period}");
                }

                //////////////////////////////////////////////////
                // Monthly Moving Averages
                //////////////////////////////////////////////////

                if (ShowMonthlySMA1 && CurrentBars[_monthlyDataSeriesIndex] >= MonthlySMA1Period)
                {
                    double monthlySMA1Value = _monthlySMA1[0];
                    UpdateLineAndLabel("LunarTickLevels_Monthly_SMA1", false, monthlySMA1Value, GetSMALevelsStartBarsAgo(), -LabelOffset, MonthlySMA1Color, MonthlySMA1LineStyle, MonthlySMA1LineWidth, $"{monthlySMA1Value.ToString($"F{_decimalPlaces}")} Monthly {MonthlySMA1Period}");
                }

                if (ShowMonthlySMA2 && CurrentBars[_monthlyDataSeriesIndex] >= MonthlySMA2Period)
                {
                    double monthlySMA2Value = _monthlySMA2[0];
                    UpdateLineAndLabel("LunarTickLevels_Monthly_SMA2", false, monthlySMA2Value, GetSMALevelsStartBarsAgo(), -LabelOffset, MonthlySMA2Color, MonthlySMA2LineStyle, MonthlySMA2LineWidth, $"{monthlySMA2Value.ToString($"F{_decimalPlaces}")} Monthly {MonthlySMA2Period}");
                }

                if (ShowMonthlySMA3 && CurrentBars[_monthlyDataSeriesIndex] >= MonthlySMA3Period)
                {
                    double monthlySMA3Value = _monthlySMA3[0];
                    UpdateLineAndLabel("LunarTickLevels_Monthly_SMA3", false, monthlySMA3Value, GetSMALevelsStartBarsAgo(), -LabelOffset, MonthlySMA3Color, MonthlySMA3LineStyle, MonthlySMA3LineWidth, $"{monthlySMA3Value.ToString($"F{_decimalPlaces}")} Monthly {MonthlySMA3Period}");
                }

                if (ShowMonthlySMA4 && CurrentBars[_monthlyDataSeriesIndex] >= MonthlySMA4Period)
                {
                    double monthlySMA4Value = _monthlySMA4[0];
                    UpdateLineAndLabel("LunarTickLevels_Monthly_SMA4", false, monthlySMA4Value, GetSMALevelsStartBarsAgo(), -LabelOffset, MonthlySMA4Color, MonthlySMA4LineStyle, MonthlySMA4LineWidth, $"{monthlySMA4Value.ToString($"F{_decimalPlaces}")} Monthly {MonthlySMA4Period}");
                }

                //////////////////////////////////////////////////
                // ETH background
                //////////////////////////////////////////////////

                if (ETHBackgroundOpacity > 0)
                {
                    if (IsFirstTickOfBar && !isCurrentBarInsideRTHSession)
                    {
                        BackBrushAll = _ethBackgroundBrush;
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private int GetSMALevelsStartBarsAgo()
        {
            return ExtendSMALevels ? CurrentBar : SMALevelLength - 1;
        }

        private void UpdateLineAndLabel(string tagPrefix, bool autoscale, double yValue, int lineStartBarsAgo, int lineEndBarsAgo, Brush lineColor, DashStyleHelper lineStyle, int lineWidth, string labelText)
        {
            if (string.IsNullOrWhiteSpace(tagPrefix))
                return;
            if (lineWidth <= 0)
                return;

            Draw.Line(this, $"{tagPrefix}_Line", autoscale, Math.Min(lineStartBarsAgo, CurrentBar), yValue, Math.Min(lineEndBarsAgo, CurrentBar), yValue, lineColor, lineStyle, lineWidth);
            if (ShowLabels && !string.IsNullOrWhiteSpace(labelText))
            {
                Draw.Text(this, $"{tagPrefix}_Label", autoscale, $" {labelText}", Math.Min(lineEndBarsAgo, CurrentBar), yValue, 0, lineColor, _labelFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }
        }

        private void DebugPrint(string msg)
        {
            if (Debug)
            {
                if (Instrument != null && !string.IsNullOrWhiteSpace(Instrument.FullName))
                    Print($"LunarTickLevels[{Instrument.FullName}]: {msg}");
                else
                    Print($"LunarTickLevels: {msg}");
            }
        }

        #endregion
    }

    public class LunarTickLevelsTypeConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            // We need the indicator instance which actually exists on the grid
            LunarTickLevels indicator = component as LunarTickLevels;

            // base.GetProperties ensures we have all the properties (and associated property grid editors)
            // NinjaTrader internal logic determines for a given indicator
            PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context)
                                                                        ? base.GetProperties(context, component, attrs)
                                                                        : TypeDescriptor.GetProperties(component, attrs);

            if (indicator == null || propertyDescriptorCollection == null)
                return propertyDescriptorCollection;


            // Show/hide properties related to ShowOpeningPrint
            PropertyDescriptor openingPrintColorPropertyDesc = propertyDescriptorCollection["OpeningPrintColor"];
            PropertyDescriptor openingPrintLineWidthPropertyDesc = propertyDescriptorCollection["OpeningPrintLineWidth"];
            PropertyDescriptor openingPrintLineStylePropertyDesc = propertyDescriptorCollection["OpeningPrintLineStyle"];
            propertyDescriptorCollection.Remove(openingPrintColorPropertyDesc);
            propertyDescriptorCollection.Remove(openingPrintLineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(openingPrintLineStylePropertyDesc);
            if (indicator.ShowOpeningPrint)
            {
                propertyDescriptorCollection.Add(openingPrintColorPropertyDesc);
                propertyDescriptorCollection.Add(openingPrintLineWidthPropertyDesc);
                propertyDescriptorCollection.Add(openingPrintLineStylePropertyDesc);
            }

            // Show/hide properties related to ShowPreviousDailyHigh
            PropertyDescriptor previousDailyHighColorPropertyDesc = propertyDescriptorCollection["PreviousDailyHighColor"];
            PropertyDescriptor previousDailyHighLineWidthPropertyDesc = propertyDescriptorCollection["PreviousDailyHighLineWidth"];
            PropertyDescriptor previousDailyHighLineStylePropertyDesc = propertyDescriptorCollection["PreviousDailyHighLineStyle"];
            propertyDescriptorCollection.Remove(previousDailyHighColorPropertyDesc);
            propertyDescriptorCollection.Remove(previousDailyHighLineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(previousDailyHighLineStylePropertyDesc);
            if (indicator.ShowPreviousDailyHigh)
            {
                propertyDescriptorCollection.Add(previousDailyHighColorPropertyDesc);
                propertyDescriptorCollection.Add(previousDailyHighLineWidthPropertyDesc);
                propertyDescriptorCollection.Add(previousDailyHighLineStylePropertyDesc);
            }

            // Show/hide properties related to ShowPreviousDailyHigh
            PropertyDescriptor previousDailyLowColorPropertyDesc = propertyDescriptorCollection["PreviousDailyLowColor"];
            PropertyDescriptor previousDailyLowLineWidthPropertyDesc = propertyDescriptorCollection["PreviousDailyLowLineWidth"];
            PropertyDescriptor previousDailyLowLineStylePropertyDesc = propertyDescriptorCollection["PreviousDailyLowLineStyle"];
            propertyDescriptorCollection.Remove(previousDailyLowColorPropertyDesc);
            propertyDescriptorCollection.Remove(previousDailyLowLineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(previousDailyLowLineStylePropertyDesc);
            if (indicator.ShowPreviousDailyLow)
            {
                propertyDescriptorCollection.Add(previousDailyLowColorPropertyDesc);
                propertyDescriptorCollection.Add(previousDailyLowLineWidthPropertyDesc);
                propertyDescriptorCollection.Add(previousDailyLowLineStylePropertyDesc);
            }

            // Show/hide properties related to ShowOvernightHigh
            PropertyDescriptor overnightHighColorPropertyDesc = propertyDescriptorCollection["OvernightHighColor"];
            PropertyDescriptor overnightHighLineWidthPropertyDesc = propertyDescriptorCollection["OvernightHighLineWidth"];
            PropertyDescriptor overnightHighLineStylePropertyDesc = propertyDescriptorCollection["OvernightHighLineStyle"];
            propertyDescriptorCollection.Remove(overnightHighColorPropertyDesc);
            propertyDescriptorCollection.Remove(overnightHighLineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(overnightHighLineStylePropertyDesc);
            if (indicator.ShowOvernightHigh)
            {
                propertyDescriptorCollection.Add(overnightHighColorPropertyDesc);
                propertyDescriptorCollection.Add(overnightHighLineWidthPropertyDesc);
                propertyDescriptorCollection.Add(overnightHighLineStylePropertyDesc);
            }

            // Show/hide properties related to ShowOvernightHigh
            PropertyDescriptor overnightLowColorPropertyDesc = propertyDescriptorCollection["OvernightLowColor"];
            PropertyDescriptor overnightLowLineWidthPropertyDesc = propertyDescriptorCollection["OvernightLowLineWidth"];
            PropertyDescriptor overnightLowLineStylePropertyDesc = propertyDescriptorCollection["OvernightLowLineStyle"];
            propertyDescriptorCollection.Remove(overnightLowColorPropertyDesc);
            propertyDescriptorCollection.Remove(overnightLowLineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(overnightLowLineStylePropertyDesc);
            if (indicator.ShowOvernightLow)
            {
                propertyDescriptorCollection.Add(overnightLowColorPropertyDesc);
                propertyDescriptorCollection.Add(overnightLowLineWidthPropertyDesc);
                propertyDescriptorCollection.Add(overnightLowLineStylePropertyDesc);
            }

            // Show/hide properties related to ShowOneHourSMA1
            PropertyDescriptor oneHourSMA1PeriodPropertyDesc = propertyDescriptorCollection["OneHourSMA1Period"];
            PropertyDescriptor oneHourSMA1ColorPropertyDesc = propertyDescriptorCollection["OneHourSMA1Color"];
            PropertyDescriptor oneHourSMA1LineWidthPropertyDesc = propertyDescriptorCollection["OneHourSMA1LineWidth"];
            PropertyDescriptor oneHourSMA1LineStylePropertyDesc = propertyDescriptorCollection["OneHourSMA1LineStyle"];
            propertyDescriptorCollection.Remove(oneHourSMA1PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA1ColorPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA1LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA1LineStylePropertyDesc);
            if (indicator.ShowOneHourSMA1)
            {
                propertyDescriptorCollection.Add(oneHourSMA1PeriodPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA1ColorPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA1LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA1LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowOneHourSMA2
            PropertyDescriptor oneHourSMA2PeriodPropertyDesc = propertyDescriptorCollection["OneHourSMA2Period"];
            PropertyDescriptor oneHourSMA2ColorPropertyDesc = propertyDescriptorCollection["OneHourSMA2Color"];
            PropertyDescriptor oneHourSMA2LineWidthPropertyDesc = propertyDescriptorCollection["OneHourSMA2LineWidth"];
            PropertyDescriptor oneHourSMA2LineStylePropertyDesc = propertyDescriptorCollection["OneHourSMA2LineStyle"];
            propertyDescriptorCollection.Remove(oneHourSMA2PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA2ColorPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA2LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA2LineStylePropertyDesc);
            if (indicator.ShowOneHourSMA2)
            {
                propertyDescriptorCollection.Add(oneHourSMA2PeriodPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA2ColorPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA2LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA2LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowOneHourSMA3
            PropertyDescriptor oneHourSMA3PeriodPropertyDesc = propertyDescriptorCollection["OneHourSMA3Period"];
            PropertyDescriptor oneHourSMA3ColorPropertyDesc = propertyDescriptorCollection["OneHourSMA3Color"];
            PropertyDescriptor oneHourSMA3LineWidthPropertyDesc = propertyDescriptorCollection["OneHourSMA3LineWidth"];
            PropertyDescriptor oneHourSMA3LineStylePropertyDesc = propertyDescriptorCollection["OneHourSMA3LineStyle"];
            propertyDescriptorCollection.Remove(oneHourSMA3PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA3ColorPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA3LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA3LineStylePropertyDesc);
            if (indicator.ShowOneHourSMA3)
            {
                propertyDescriptorCollection.Add(oneHourSMA3PeriodPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA3ColorPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA3LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA3LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowOneHourSMA4
            PropertyDescriptor oneHourSMA4PeriodPropertyDesc = propertyDescriptorCollection["OneHourSMA4Period"];
            PropertyDescriptor oneHourSMA4ColorPropertyDesc = propertyDescriptorCollection["OneHourSMA4Color"];
            PropertyDescriptor oneHourSMA4LineWidthPropertyDesc = propertyDescriptorCollection["OneHourSMA4LineWidth"];
            PropertyDescriptor oneHourSMA4LineStylePropertyDesc = propertyDescriptorCollection["OneHourSMA4LineStyle"];
            propertyDescriptorCollection.Remove(oneHourSMA4PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA4ColorPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA4LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(oneHourSMA4LineStylePropertyDesc);
            if (indicator.ShowOneHourSMA4)
            {
                propertyDescriptorCollection.Add(oneHourSMA4PeriodPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA4ColorPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA4LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(oneHourSMA4LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowFourHourSMA1
            PropertyDescriptor fourHourSMA1PeriodPropertyDesc = propertyDescriptorCollection["FourHourSMA1Period"];
            PropertyDescriptor fourHourSMA1ColorPropertyDesc = propertyDescriptorCollection["FourHourSMA1Color"];
            PropertyDescriptor fourHourSMA1LineWidthPropertyDesc = propertyDescriptorCollection["FourHourSMA1LineWidth"];
            PropertyDescriptor fourHourSMA1LineStylePropertyDesc = propertyDescriptorCollection["FourHourSMA1LineStyle"];
            propertyDescriptorCollection.Remove(fourHourSMA1PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA1ColorPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA1LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA1LineStylePropertyDesc);
            if (indicator.ShowFourHourSMA1)
            {
                propertyDescriptorCollection.Add(fourHourSMA1PeriodPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA1ColorPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA1LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA1LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowFourHourSMA2
            PropertyDescriptor fourHourSMA2PeriodPropertyDesc = propertyDescriptorCollection["FourHourSMA2Period"];
            PropertyDescriptor fourHourSMA2ColorPropertyDesc = propertyDescriptorCollection["FourHourSMA2Color"];
            PropertyDescriptor fourHourSMA2LineWidthPropertyDesc = propertyDescriptorCollection["FourHourSMA2LineWidth"];
            PropertyDescriptor fourHourSMA2LineStylePropertyDesc = propertyDescriptorCollection["FourHourSMA2LineStyle"];
            propertyDescriptorCollection.Remove(fourHourSMA2PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA2ColorPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA2LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA2LineStylePropertyDesc);
            if (indicator.ShowFourHourSMA2)
            {
                propertyDescriptorCollection.Add(fourHourSMA2PeriodPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA2ColorPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA2LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA2LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowFourHourSMA3
            PropertyDescriptor fourHourSMA3PeriodPropertyDesc = propertyDescriptorCollection["FourHourSMA3Period"];
            PropertyDescriptor fourHourSMA3ColorPropertyDesc = propertyDescriptorCollection["FourHourSMA3Color"];
            PropertyDescriptor fourHourSMA3LineWidthPropertyDesc = propertyDescriptorCollection["FourHourSMA3LineWidth"];
            PropertyDescriptor fourHourSMA3LineStylePropertyDesc = propertyDescriptorCollection["FourHourSMA3LineStyle"];
            propertyDescriptorCollection.Remove(fourHourSMA3PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA3ColorPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA3LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA3LineStylePropertyDesc);
            if (indicator.ShowFourHourSMA3)
            {
                propertyDescriptorCollection.Add(fourHourSMA3PeriodPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA3ColorPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA3LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA3LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowFourHourSMA4
            PropertyDescriptor fourHourSMA4PeriodPropertyDesc = propertyDescriptorCollection["FourHourSMA4Period"];
            PropertyDescriptor fourHourSMA4ColorPropertyDesc = propertyDescriptorCollection["FourHourSMA4Color"];
            PropertyDescriptor fourHourSMA4LineWidthPropertyDesc = propertyDescriptorCollection["FourHourSMA4LineWidth"];
            PropertyDescriptor fourHourSMA4LineStylePropertyDesc = propertyDescriptorCollection["FourHourSMA4LineStyle"];
            propertyDescriptorCollection.Remove(fourHourSMA4PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA4ColorPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA4LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(fourHourSMA4LineStylePropertyDesc);
            if (indicator.ShowFourHourSMA4)
            {
                propertyDescriptorCollection.Add(fourHourSMA4PeriodPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA4ColorPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA4LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(fourHourSMA4LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowDailySMA1
            PropertyDescriptor dailySMA1PeriodPropertyDesc = propertyDescriptorCollection["DailySMA1Period"];
            PropertyDescriptor dailySMA1ColorPropertyDesc = propertyDescriptorCollection["DailySMA1Color"];
            PropertyDescriptor dailySMA1LineWidthPropertyDesc = propertyDescriptorCollection["DailySMA1LineWidth"];
            PropertyDescriptor dailySMA1LineStylePropertyDesc = propertyDescriptorCollection["DailySMA1LineStyle"];
            propertyDescriptorCollection.Remove(dailySMA1PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA1ColorPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA1LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA1LineStylePropertyDesc);
            if (indicator.ShowDailySMA1)
            {
                propertyDescriptorCollection.Add(dailySMA1PeriodPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA1ColorPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA1LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA1LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowDailySMA2
            PropertyDescriptor dailySMA2PeriodPropertyDesc = propertyDescriptorCollection["DailySMA2Period"];
            PropertyDescriptor dailySMA2ColorPropertyDesc = propertyDescriptorCollection["DailySMA2Color"];
            PropertyDescriptor dailySMA2LineWidthPropertyDesc = propertyDescriptorCollection["DailySMA2LineWidth"];
            PropertyDescriptor dailySMA2LineStylePropertyDesc = propertyDescriptorCollection["DailySMA2LineStyle"];
            propertyDescriptorCollection.Remove(dailySMA2PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA2ColorPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA2LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA2LineStylePropertyDesc);
            if (indicator.ShowDailySMA2)
            {
                propertyDescriptorCollection.Add(dailySMA2PeriodPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA2ColorPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA2LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA2LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowDailySMA3
            PropertyDescriptor dailySMA3PeriodPropertyDesc = propertyDescriptorCollection["DailySMA3Period"];
            PropertyDescriptor dailySMA3ColorPropertyDesc = propertyDescriptorCollection["DailySMA3Color"];
            PropertyDescriptor dailySMA3LineWidthPropertyDesc = propertyDescriptorCollection["DailySMA3LineWidth"];
            PropertyDescriptor dailySMA3LineStylePropertyDesc = propertyDescriptorCollection["DailySMA3LineStyle"];
            propertyDescriptorCollection.Remove(dailySMA3PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA3ColorPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA3LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA3LineStylePropertyDesc);
            if (indicator.ShowDailySMA3)
            {
                propertyDescriptorCollection.Add(dailySMA3PeriodPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA3ColorPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA3LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA3LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowDailySMA4
            PropertyDescriptor dailySMA4PeriodPropertyDesc = propertyDescriptorCollection["DailySMA4Period"];
            PropertyDescriptor dailySMA4ColorPropertyDesc = propertyDescriptorCollection["DailySMA4Color"];
            PropertyDescriptor dailySMA4LineWidthPropertyDesc = propertyDescriptorCollection["DailySMA4LineWidth"];
            PropertyDescriptor dailySMA4LineStylePropertyDesc = propertyDescriptorCollection["DailySMA4LineStyle"];
            propertyDescriptorCollection.Remove(dailySMA4PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA4ColorPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA4LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(dailySMA4LineStylePropertyDesc);
            if (indicator.ShowDailySMA4)
            {
                propertyDescriptorCollection.Add(dailySMA4PeriodPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA4ColorPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA4LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(dailySMA4LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowWeeklySMA1
            PropertyDescriptor weeklySMA1PeriodPropertyDesc = propertyDescriptorCollection["WeeklySMA1Period"];
            PropertyDescriptor weeklySMA1ColorPropertyDesc = propertyDescriptorCollection["WeeklySMA1Color"];
            PropertyDescriptor weeklySMA1LineWidthPropertyDesc = propertyDescriptorCollection["WeeklySMA1LineWidth"];
            PropertyDescriptor weeklySMA1LineStylePropertyDesc = propertyDescriptorCollection["WeeklySMA1LineStyle"];
            propertyDescriptorCollection.Remove(weeklySMA1PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA1ColorPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA1LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA1LineStylePropertyDesc);
            if (indicator.ShowWeeklySMA1)
            {
                propertyDescriptorCollection.Add(weeklySMA1PeriodPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA1ColorPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA1LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA1LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowWeeklySMA2
            PropertyDescriptor weeklySMA2PeriodPropertyDesc = propertyDescriptorCollection["WeeklySMA2Period"];
            PropertyDescriptor weeklySMA2ColorPropertyDesc = propertyDescriptorCollection["WeeklySMA2Color"];
            PropertyDescriptor weeklySMA2LineWidthPropertyDesc = propertyDescriptorCollection["WeeklySMA2LineWidth"];
            PropertyDescriptor weeklySMA2LineStylePropertyDesc = propertyDescriptorCollection["WeeklySMA2LineStyle"];
            propertyDescriptorCollection.Remove(weeklySMA2PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA2ColorPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA2LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA2LineStylePropertyDesc);
            if (indicator.ShowWeeklySMA2)
            {
                propertyDescriptorCollection.Add(weeklySMA2PeriodPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA2ColorPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA2LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA2LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowWeeklySMA3
            PropertyDescriptor weeklySMA3PeriodPropertyDesc = propertyDescriptorCollection["WeeklySMA3Period"];
            PropertyDescriptor weeklySMA3ColorPropertyDesc = propertyDescriptorCollection["WeeklySMA3Color"];
            PropertyDescriptor weeklySMA3LineWidthPropertyDesc = propertyDescriptorCollection["WeeklySMA3LineWidth"];
            PropertyDescriptor weeklySMA3LineStylePropertyDesc = propertyDescriptorCollection["WeeklySMA3LineStyle"];
            propertyDescriptorCollection.Remove(weeklySMA3PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA3ColorPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA3LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA3LineStylePropertyDesc);
            if (indicator.ShowWeeklySMA3)
            {
                propertyDescriptorCollection.Add(weeklySMA3PeriodPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA3ColorPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA3LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA3LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowWeeklySMA4
            PropertyDescriptor weeklySMA4PeriodPropertyDesc = propertyDescriptorCollection["WeeklySMA4Period"];
            PropertyDescriptor weeklySMA4ColorPropertyDesc = propertyDescriptorCollection["WeeklySMA4Color"];
            PropertyDescriptor weeklySMA4LineWidthPropertyDesc = propertyDescriptorCollection["WeeklySMA4LineWidth"];
            PropertyDescriptor weeklySMA4LineStylePropertyDesc = propertyDescriptorCollection["WeeklySMA4LineStyle"];
            propertyDescriptorCollection.Remove(weeklySMA4PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA4ColorPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA4LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(weeklySMA4LineStylePropertyDesc);
            if (indicator.ShowWeeklySMA4)
            {
                propertyDescriptorCollection.Add(weeklySMA4PeriodPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA4ColorPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA4LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(weeklySMA4LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowMonthlySMA1
            PropertyDescriptor monthlySMA1PeriodPropertyDesc = propertyDescriptorCollection["MonthlySMA1Period"];
            PropertyDescriptor monthlySMA1ColorPropertyDesc = propertyDescriptorCollection["MonthlySMA1Color"];
            PropertyDescriptor monthlySMA1LineWidthPropertyDesc = propertyDescriptorCollection["MonthlySMA1LineWidth"];
            PropertyDescriptor monthlySMA1LineStylePropertyDesc = propertyDescriptorCollection["MonthlySMA1LineStyle"];
            propertyDescriptorCollection.Remove(monthlySMA1PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA1ColorPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA1LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA1LineStylePropertyDesc);
            if (indicator.ShowMonthlySMA1)
            {
                propertyDescriptorCollection.Add(monthlySMA1PeriodPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA1ColorPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA1LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA1LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowMonthlySMA2
            PropertyDescriptor monthlySMA2PeriodPropertyDesc = propertyDescriptorCollection["MonthlySMA2Period"];
            PropertyDescriptor monthlySMA2ColorPropertyDesc = propertyDescriptorCollection["MonthlySMA2Color"];
            PropertyDescriptor monthlySMA2LineWidthPropertyDesc = propertyDescriptorCollection["MonthlySMA2LineWidth"];
            PropertyDescriptor monthlySMA2LineStylePropertyDesc = propertyDescriptorCollection["MonthlySMA2LineStyle"];
            propertyDescriptorCollection.Remove(monthlySMA2PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA2ColorPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA2LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA2LineStylePropertyDesc);
            if (indicator.ShowMonthlySMA2)
            {
                propertyDescriptorCollection.Add(monthlySMA2PeriodPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA2ColorPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA2LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA2LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowMonthlySMA3
            PropertyDescriptor monthlySMA3PeriodPropertyDesc = propertyDescriptorCollection["MonthlySMA3Period"];
            PropertyDescriptor monthlySMA3ColorPropertyDesc = propertyDescriptorCollection["MonthlySMA3Color"];
            PropertyDescriptor monthlySMA3LineWidthPropertyDesc = propertyDescriptorCollection["MonthlySMA3LineWidth"];
            PropertyDescriptor monthlySMA3LineStylePropertyDesc = propertyDescriptorCollection["MonthlySMA3LineStyle"];
            propertyDescriptorCollection.Remove(monthlySMA3PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA3ColorPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA3LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA3LineStylePropertyDesc);
            if (indicator.ShowMonthlySMA3)
            {
                propertyDescriptorCollection.Add(monthlySMA3PeriodPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA3ColorPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA3LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA3LineStylePropertyDesc);
            }

            // Show/hide properties related to ShowMonthlySMA4
            PropertyDescriptor monthlySMA4PeriodPropertyDesc = propertyDescriptorCollection["MonthlySMA4Period"];
            PropertyDescriptor monthlySMA4ColorPropertyDesc = propertyDescriptorCollection["MonthlySMA4Color"];
            PropertyDescriptor monthlySMA4LineWidthPropertyDesc = propertyDescriptorCollection["MonthlySMA4LineWidth"];
            PropertyDescriptor monthlySMA4LineStylePropertyDesc = propertyDescriptorCollection["MonthlySMA4LineStyle"];
            propertyDescriptorCollection.Remove(monthlySMA4PeriodPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA4ColorPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA4LineWidthPropertyDesc);
            propertyDescriptorCollection.Remove(monthlySMA4LineStylePropertyDesc);
            if (indicator.ShowMonthlySMA4)
            {
                propertyDescriptorCollection.Add(monthlySMA4PeriodPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA4ColorPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA4LineWidthPropertyDesc);
                propertyDescriptorCollection.Add(monthlySMA4LineStylePropertyDesc);
            }

            // Show/hide properties related to ExtendSMALevels
            PropertyDescriptor smaLevelLengthPropertyDesc = propertyDescriptorCollection["SMALevelLength"];
            propertyDescriptorCollection.Remove(smaLevelLengthPropertyDesc);
            if (!indicator.ExtendSMALevels)
            {
                propertyDescriptorCollection.Add(smaLevelLengthPropertyDesc);
            }

            // Show/hide properties related to ShowLabels
            PropertyDescriptor labelSizePropertyDesc = propertyDescriptorCollection["LabelSize"];
            PropertyDescriptor labelBoldPropertyDesc = propertyDescriptorCollection["LabelBold"];
            PropertyDescriptor labelOffsetPropertyDesc = propertyDescriptorCollection["LabelOffset"];
            propertyDescriptorCollection.Remove(labelSizePropertyDesc);
            propertyDescriptorCollection.Remove(labelBoldPropertyDesc);
            propertyDescriptorCollection.Remove(labelOffsetPropertyDesc);
            if (indicator.ShowLabels)
            {
                propertyDescriptorCollection.Add(labelSizePropertyDesc);
                propertyDescriptorCollection.Add(labelBoldPropertyDesc);
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
		private LunarTick.LunarTickLevels[] cacheLunarTickLevels;
		public LunarTick.LunarTickLevels LunarTickLevels()
		{
			return LunarTickLevels(Input);
		}

		public LunarTick.LunarTickLevels LunarTickLevels(ISeries<double> input)
		{
			if (cacheLunarTickLevels != null)
				for (int idx = 0; idx < cacheLunarTickLevels.Length; idx++)
					if (cacheLunarTickLevels[idx] != null &&  cacheLunarTickLevels[idx].EqualsInput(input))
						return cacheLunarTickLevels[idx];
			return CacheIndicator<LunarTick.LunarTickLevels>(new LunarTick.LunarTickLevels(), input, ref cacheLunarTickLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LunarTick.LunarTickLevels LunarTickLevels()
		{
			return indicator.LunarTickLevels(Input);
		}

		public Indicators.LunarTick.LunarTickLevels LunarTickLevels(ISeries<double> input )
		{
			return indicator.LunarTickLevels(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LunarTick.LunarTickLevels LunarTickLevels()
		{
			return indicator.LunarTickLevels(Input);
		}

		public Indicators.LunarTick.LunarTickLevels LunarTickLevels(ISeries<double> input )
		{
			return indicator.LunarTickLevels(input);
		}
	}
}

#endregion

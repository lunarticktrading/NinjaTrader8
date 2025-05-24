#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.gambcl
{
    public class RealOpenClose : Indicator
    {
        #region Members
        private SharpDX.Direct2D1.Brush _realOpenBrushDx;
        private SharpDX.Direct2D1.Brush _realCloseBrushDx;
        #endregion

        #region Indicator methods
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description									= @"Shows the real Open and Close levels for candles.";
                Name										= "RealOpenClose";
                Calculate									= Calculate.OnPriceChange;
                IsOverlay									= true;
                DisplayInDataBox							= true;
                DrawOnPricePanel							= true;
                DrawHorizontalGridLines						= true;
                DrawVerticalGridLines						= true;
                PaintPriceMarkers							= false;
                ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive					= true;
                ShowRealOpen								= true;
                ShowRealClose								= true;

                AddPlot(Brushes.White, "RealOpen");
                AddPlot(Brushes.Fuchsia, "RealClose");
            }
            else if (State == State.Configure)
            {
            }
        }

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return DefaultName;

                return Name + "(" + ShowRealOpen + "," + ShowRealClose + ")";
            }
        }

        protected override void OnBarUpdate()
        {
            RealOpen[0] = Open[0];
            RealClose[0] = Close[0];
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            if (!IsVisible)
                return;

            // NOTE: Do not call base.OnRender as we do not want the plots to appear on the chart, only in the DataBox.
            //base.OnRender(chartControl, chartScale);
            
            if (chartControl == null || chartScale == null || ChartBars == null || RenderTarget == null)
                return;

            for (int i = ChartBars.FromIndex; i <= ChartBars.ToIndex; i++)
            {
                var barX = chartControl.GetXByBarIndex(ChartBars, i);
                var nextBarX = chartControl.GetXByBarIndex(ChartBars, i + 1);
                var width = nextBarX - barX;
                barX -= (width / 2);
                
                if (ShowRealOpen && _realOpenBrushDx != null && !_realOpenBrushDx.IsDisposed)
                {
                    var realOpen = Bars.GetOpen(i);
                    var openY = chartScale.GetYByValue(realOpen);
                    RenderTarget.DrawLine(new SharpDX.Vector2(barX, openY), new SharpDX.Vector2(barX + width, openY), _realOpenBrushDx, Plots[0].Width);
                }

                if (ShowRealClose && _realCloseBrushDx != null && !_realCloseBrushDx.IsDisposed)
                {
                    var realClose = Bars.GetClose(i);
                    var closeY = chartScale.GetYByValue(realClose);
                    RenderTarget.DrawLine(new SharpDX.Vector2(barX, closeY), new SharpDX.Vector2(barX + width, closeY), _realCloseBrushDx, Plots[1].Width);
                }
            }
        }

        public override void OnRenderTargetChanged()
        {
            if (_realOpenBrushDx != null)
                _realOpenBrushDx.Dispose();

            if (_realCloseBrushDx != null)
                _realCloseBrushDx.Dispose();

            if (RenderTarget != null)
            {
                try
                {
                    _realOpenBrushDx = Plots[0].Brush.ToDxBrush(RenderTarget);
                    _realCloseBrushDx = Plots[1].Brush.ToDxBrush(RenderTarget);
                }
                catch (Exception e) { }
            }
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Display(Name="ShowRealOpen", Description="Show the real Open price of each candle, using the specified color.", Order=1, GroupName="Parameters")]
        public bool ShowRealOpen
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name="ShowRealClose", Description="Show the real Close price of each candle, using the specified color.", Order=3, GroupName="Parameters")]
        public bool ShowRealClose
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> RealOpen
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> RealClose
        {
            get { return Values[1]; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private gambcl.RealOpenClose[] cacheRealOpenClose;
		public gambcl.RealOpenClose RealOpenClose(bool showRealOpen, bool showRealClose)
		{
			return RealOpenClose(Input, showRealOpen, showRealClose);
		}

		public gambcl.RealOpenClose RealOpenClose(ISeries<double> input, bool showRealOpen, bool showRealClose)
		{
			if (cacheRealOpenClose != null)
				for (int idx = 0; idx < cacheRealOpenClose.Length; idx++)
					if (cacheRealOpenClose[idx] != null && cacheRealOpenClose[idx].ShowRealOpen == showRealOpen && cacheRealOpenClose[idx].ShowRealClose == showRealClose && cacheRealOpenClose[idx].EqualsInput(input))
						return cacheRealOpenClose[idx];
			return CacheIndicator<gambcl.RealOpenClose>(new gambcl.RealOpenClose(){ ShowRealOpen = showRealOpen, ShowRealClose = showRealClose }, input, ref cacheRealOpenClose);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.gambcl.RealOpenClose RealOpenClose(bool showRealOpen, bool showRealClose)
		{
			return indicator.RealOpenClose(Input, showRealOpen, showRealClose);
		}

		public Indicators.gambcl.RealOpenClose RealOpenClose(ISeries<double> input , bool showRealOpen, bool showRealClose)
		{
			return indicator.RealOpenClose(input, showRealOpen, showRealClose);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.gambcl.RealOpenClose RealOpenClose(bool showRealOpen, bool showRealClose)
		{
			return indicator.RealOpenClose(Input, showRealOpen, showRealClose);
		}

		public Indicators.gambcl.RealOpenClose RealOpenClose(ISeries<double> input , bool showRealOpen, bool showRealClose)
		{
			return indicator.RealOpenClose(input, showRealOpen, showRealClose);
		}
	}
}

#endregion

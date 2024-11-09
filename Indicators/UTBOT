using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo;

[Indicator(AccessRights = AccessRights.None, IsOverlay = true)]
public class UTBOT : Indicator {

    // parameters
    [Parameter("Sensitivity", DefaultValue = 1, MinValue = 1)]
    public double Sensitivity { get; set; }

    [Parameter("ATR Period", DefaultValue = 10, MinValue = 1)]
    public int ATRPeriod { get; set; }

    [Parameter("Show Signals", DefaultValue = true)]
    public bool ShowSignals { get; set; }

    // outputs    
    [Output("Long", PlotType = PlotType.Points, LineColor = "Cyan", Thickness = 5)]
    public IndicatorDataSeries Long { get; set; }
    
    [Output("Short", PlotType = PlotType.Points, LineColor = "LightPink", Thickness = 5)]
    public IndicatorDataSeries Short { get; set; }

    // variables
    private DataSeries src;
    private DataSeries atr;
    private DataSeries ema;
    private IndicatorDataSeries xATS;

    
    protected override void Initialize() {
        src = Bars.ClosePrices;
        xATS = CreateDataSeries();
        atr = Indicators.AverageTrueRange(ATRPeriod, MovingAverageType.Simple).Result;
        ema = Indicators.ExponentialMovingAverage(src, 1).Result;
    }

    public override void Calculate(int i) {
        double nLoss = Sensitivity * atr[i];
        double x = NZ(xATS[i-1]);
        xATS[i] = src[i] > x && src[i-1] > x ? Math.Max(x, src[i] - nLoss):
        (src[i] < x && src[i-1] < x ? Math.Min(x, src[i] + nLoss):
        (src[i] > x ? src[i] - nLoss : src[i] + nLoss));
        
        Long[i] = ema.HasCrossedAbove(xATS, 0) ? Bars.LowPrices[i] - Symbol.PipSize: double.NaN;
        Short[i] = xATS.HasCrossedAbove(ema, 0) ? Bars.HighPrices[i] + Symbol.PipSize : double.NaN;
        
        // draw arrows on chart
        if(ShowSignals) {
            if(!double.IsNaN(Long[i]))
                Chart.DrawIcon($"LONG {i}", ChartIconType.UpArrow, i, Bars.LowPrices[i] - Symbol.PipSize, Color.Cyan);
            if(!double.IsNaN(Short[i]))
                Chart.DrawIcon($"SHORT {i}", ChartIconType.DownArrow, i, Bars.HighPrices[i] + Symbol.PipSize, Color.LightPink);
        }
    }

    private static double NZ(double val) {
        return double.IsNaN(val) ? 0 : val;
    }
}

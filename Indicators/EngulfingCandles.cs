using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo;

[Indicator(AccessRights = AccessRights.None, IsOverlay = true)]
public class CandlestickPatterns : Indicator {

    // parameters
    [Parameter("Minimum Body Size", DefaultValue = 3, MinValue = 2)]
    public double MinBody { get; set; }
    
    [Parameter("Minimum Size Difference", DefaultValue = 1.5, MinValue = 1)]
    public double MinDiff { get; set; }
    
    [Parameter("Show Box", DefaultValue = true)]
    public bool ShowBox { get; set; }
    
    // outputs
    [Output("Bullish Engulfing", PlotType = PlotType.Points, LineColor = "Cyan", Thickness = 5)]
    public IndicatorDataSeries Bullish { get; set; }
    
    [Output("Bearish Engulfing", PlotType = PlotType.Points, LineColor = "LightPink", Thickness = 5)]
    public IndicatorDataSeries Bearish { get; set; }

    protected override void Initialize() {

    }

    public override void Calculate(int i) {
        if(i < 1) return;
        
        Bar current = Bars[i];
        Bar prev = Bars[i-1];
        double pip = Symbol.PipSize;
        bool size = Body(current) > MinBody && Body(prev) > MinBody;
        bool diff = Body(current) - Body(prev) > MinDiff;
        bool range = Math.Max(current.Open, current.Close) > Math.Max(prev.Open, prev.Close)
        && Math.Min(current.Open, current.Close) < Math.Min(prev.Open, prev.Close);
        
        bool bullish = IsBull(current) && IsBear(prev) && range && size && diff;
        bool bearish = IsBear(current) && IsBull(prev) && range && size && diff;
        
        Bullish[i] = bullish ? current.Low - pip: double.NaN;
        Bearish[i] = bearish ? current.High + pip : double.NaN;
        
        if(bullish) {
            Bullish[i] = current.Low - pip;
            if(ShowBox) Chart.DrawRectangle($"Box {i}", i + 1, current.Close + pip, i - 2, current.Open - pip, Color.Cyan, 3);
        }
        
        if(bearish) {
            Bearish[i] = current.High + pip;
            if(ShowBox) Chart.DrawRectangle($"Box {i}", i + 1, current.Close - pip, i - 2, current.Open + pip, Color.LightPink, 3);
        }
        
    }
    
    // utilities
    // get candle body size in pips
    private double Body(Bar bar) => Math.Abs(ToPips(bar.Close - bar.Open));
    
    // true when bullish candle
    private bool IsBull(Bar bar) => bar.Open < bar.Close;
    
    // true when not bullish candle
    private bool IsBear(Bar bar) => bar.Open > bar.Close;
    
    // turn price into pips (1 dp)
    private double ToPips(double a) => Math.Round(a / Symbol.PipSize, 1);
}

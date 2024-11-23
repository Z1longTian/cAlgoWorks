using System;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo;

[Cloud("BAND1TOP", "MA", FirstColor ="Blue", SecondColor = "Blue", Opacity = 0.1)]
[Cloud("BAND1BOTTOM","MA", FirstColor = "Blue", SecondColor = "Blue", Opacity = 0.1)]
[Cloud("BAND1TOP", "BAND2TOP", FirstColor = "Yellow", SecondColor = "Yellow", Opacity = 0.1)]
[Cloud("BAND2TOP", "BAND3TOP", FirstColor = "Red", SecondColor = "Red", Opacity = 0.1)]
[Cloud("BAND1BOTTOM", "BAND2BOTTOM", FirstColor = "Yellow", SecondColor = "Yellow", Opacity = 0.1)]
[Cloud("BAND2BOTTOM", "BAND3BOTTOM", FirstColor = "Red", SecondColor = "Red", Opacity = 0.1)]
[Indicator(AccessRights = AccessRights.None, IsOverlay = true)]
public class MBB : Indicator {

    // parameters
    [Parameter("MA Type", DefaultValue = MovingAverageType.Simple)]
    public MovingAverageType MAType { get; set; }
    
    [Parameter("MA Length", DefaultValue = 20)]
    public int MALength { get; set; }
    
    [Parameter("STD1", DefaultValue = 1)]
    public double STD1 { get; set; }
    
    [Parameter("STD2", DefaultValue = 2)]
    public double STD2 { get; set; }
    
    [Parameter("STD3", DefaultValue = 3)]
    public double STD3 { get; set; }
    // outputs
    [Output("MA", PlotType = PlotType.Line, LineColor = "Yellow", Thickness = 2)]
    public IndicatorDataSeries MA { get; set; }
    
    [Output("BAND1TOP", PlotType = PlotType.Line, LineColor = "Blue")]
    public IndicatorDataSeries BAND1TOP { get; set; }
    
    [Output("BAND1BOTTOM", PlotType = PlotType.Line, LineColor = "Blue")]
    public IndicatorDataSeries BAND1BOTTOM{ get; set; }
    
    [Output("BAND2TOP", PlotType = PlotType.Line, LineColor = "Yellow")]
    public IndicatorDataSeries BAND2TOP { get; set; }
    
    [Output("BAND2BOTTOM", PlotType = PlotType.Line, LineColor = "Yellow")]
    public IndicatorDataSeries BAND2BOTTOM{ get; set; }
    
    [Output("BAND3TOP", PlotType = PlotType.Line, LineColor = "Red")]
    public IndicatorDataSeries BAND3TOP { get; set; }
    
    [Output("BAND3BOTTOM", PlotType = PlotType.Line, LineColor = "Red")]
    public IndicatorDataSeries BAND3BOTTOM{ get; set; }
    
    // variables
    private DataSeries ma;
    private DataSeries std;

    protected override void Initialize() {
        ma = Indicators.MovingAverage(Bars.ClosePrices, MALength, MAType).Result;
        std = Indicators.StandardDeviation(Bars.ClosePrices, MALength, MAType).Result;
    }

    public override void Calculate(int i) {
        MA[i] = ma[i];
        BAND1TOP[i] = ma[i] + STD1 * std[i];
        BAND1BOTTOM[i] = ma[i] - STD1 * std[i];
        BAND2TOP[i] = ma[i] + STD2 * std[i];
        BAND2BOTTOM[i] = ma[i] - STD2 * std[i];
        BAND3TOP[i] = ma[i] + STD3 * std[i];
        BAND3BOTTOM[i] = ma[i] - STD3 * std[i];
    }
}

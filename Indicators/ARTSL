using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo;

[Indicator(AccessRights = AccessRights.None, IsOverlay = true)]
public class ATRSL : Indicator {

    // parameters
    [Parameter("Length", DefaultValue = 14, MinValue = 1)]
    public int Length { get; set; }

    [Parameter("Multiplier", DefaultValue = 1.5)]
    public double Mult { get; set; }

    // outputs
    [Output("Upper", PlotType = PlotType.Line, LineColor = "Red", LineStyle = LineStyle.LinesDots, Thickness = 2)]
    public IndicatorDataSeries Upper { get; set; }

    [Output("Lower", PlotType = PlotType.Line, LineColor = "Green", LineStyle = LineStyle.LinesDots, Thickness = 2)]
    public IndicatorDataSeries Lower { get; set; }
    
    // variables
    private DataSeries atr;

    protected override void Initialize() {
        atr = Indicators.AverageTrueRange(Length, MovingAverageType.Simple).Result;
    }

    public override void Calculate(int i) {
        Upper[i] = Bars.HighPrices[i] + atr[i] * Mult;
        Lower[i] = Bars.LowPrices[i] - atr[i] * Mult;
    }
}

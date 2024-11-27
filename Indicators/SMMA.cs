using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo;

// Smoothed Moving Average
[Indicator(AccessRights = AccessRights.None, IsOverlay = true)]
public class SMMA : Indicator {
    
    // parameters
    [Parameter("Source", DefaultValue = DataSource.Close)]
    public DataSource Src { get; set; }
    
    [Parameter("Length", DefaultValue = 7, MinValue = 1)] 
    public int Length { get; set; }
    
    // outputs
    [Output("SMMA", PlotType = PlotType.Line, LineColor = "Purple", Thickness = 2)]
    public IndicatorDataSeries Result { get; set; }
    
    // indicators
    private DataSeries sma;
    private DataSeries src;
    
    protected override void Initialize() {
        src = Src switch {
            DataSource.Close => Bars.ClosePrices,
            DataSource.High => Bars.HighPrices,
            DataSource.Low => Bars.LowPrices,
            DataSource.Open => Bars.OpenPrices,
            DataSource.Median => Bars.MedianPrices,
            DataSource.Weighted => Bars.WeightedPrices,
            DataSource.Typical => Bars.TypicalPrices,
            _ => Bars.ClosePrices
        };
        sma = Indicators.SimpleMovingAverage(src, Length).Result;
    }

    public override void Calculate(int i) {
        Result[i] = double.IsNaN(Result[i-1]) ? sma[i] : (Result[i-1] * (Length - 1) + src[i]) / Length;
    }
    
    public enum DataSource {
        Open,
        Close,
        High,
        Low,
        Median,
        Typical,
        Weighted,
    }
    
}

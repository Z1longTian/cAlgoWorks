using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo;

[Indicator(AccessRights = AccessRights.None, IsOverlay = true)]
public class PDHL : Indicator {

    // parameters
    [Parameter("Include Sunday", DefaultValue = false)]
    public bool SundayIncluded { get; set; }
    
    // outputs
    [Output("High", PlotType = PlotType.Line, LineColor = "Purple", LineStyle = LineStyle.Lines, Thickness = 2)]
    public IndicatorDataSeries High { get; set; }
    [Output("Low", PlotType = PlotType.Line, LineColor = "Purple", LineStyle = LineStyle.Lines, Thickness = 2)]
    public IndicatorDataSeries Low { get; set; }
    
    private double prev_high;
    private double prev_low;
    private double high;
    private double low;

    protected override void Initialize() {}
 
    public override void Calculate(int i) {
        if(TimeFrame > TimeFrame.Daily) return; // only works for timeframe under d1
        if(i < 1) return; // at least 2 candles needed
        
        Bar current = Bars.LastBar; // current bar
        bool new_day = Bars.Last(1).OpenTime.Day != current.OpenTime.Day; // new day condition
        bool include_sunday = SundayIncluded || current.OpenTime.DayOfWeek != DayOfWeek.Sunday; // include sunday condition

        if(new_day && include_sunday) {
            prev_high = double.IsNaN(high) ? double.NaN : high; // if high is NaN -> prev_high = Nan else prev_high = high
            prev_low = double.IsNaN(low) ? double.NaN : low;
            high = double.NaN;
            low = double.NaN; 
        } else {
            high = double.IsNaN(high) ? current.High : Math.Max(current.High, high);
            low = double.IsNaN(low) ? current.Low: Math.Min(current.Low, low);
        }
        
        High[i] = double.IsNaN(prev_high) ? double.NaN : prev_high;
        Low[i] = double.IsNaN(prev_low) ? double.NaN : prev_low;
    }
}

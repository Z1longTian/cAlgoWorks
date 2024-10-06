using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Plugins;

[Plugin(AccessRights = AccessRights.None)]
public class DataWindow : Plugin {

    // variables
    private AspTab _datawindow; // ActiveSymbolPanel tab 
    private StackPanel _container; // container for datawindow
    private Grid _data; // data value grid
    
    // data labels
    private readonly List<string> _datalabels = new List<string> {
        "Symbol", "Date", "Time", "Open", "High", "Low", "Close", "Size", "Tick Volume", "Spread", "Trading Session"
    };
    private readonly List<TextBlock> _datatextblocks = new List<TextBlock>(); // data values
    
    // indicator labels
    private readonly List<string> _indicatorlabels = new List<string>();
    private readonly List<TextBlock> _indicatortextblocks = new List<TextBlock>();
    
    // ToDos
    // 1. styles
    // 2. when index is not valid, show default values => active frame chart's values
    // 3. adding 0s according to the pip size
    // 4. Date needs to remove time
    // 5. Both date and time needs to be same as UTC time ( change from server to utc time)
    // 6. Trading session using a different handler
    // 7. adding a new textblock pipsize
    // 8. maybe adding current bid & ask of the symbol where mouse is moving at
    // 9. reorder textblocks
    
    protected override void OnStart() {
        // asp setup
        _datawindow = Asp.AddTab("Data Window"); // add a new tab to asp
        _datawindow.Index = 1; // set the tab index, next to SymbolTab
        // event handlers setup
        ChartManager.FramesAdded += FramesAdded; // handle new added frames
        ChartManager.FramesRemoved += FramesRemoved; // handle removed frames
        // adding MouseMove event handler to all chart frames
        foreach (ChartContainer cc in ChartManager.ChartContainers) {
            for (int i = 0; i < cc.Count; i++) {
                if (cc[i] is ChartFrame chartframe) {
                    chartframe.Chart.MouseMove += MouseMove;
                }
            }
        }

        // container for data window
        _container = new StackPanel() {
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        // creating new data grid
        _data = new Grid(0, 2) {
            MinWidth = 200,
            MaxWidth = 500,
        };

        // adding label and data to grid
        AddRow(_data, Title("Data"));
        foreach (string label in _datalabels) {
            TextBlock value = Value();
            _datatextblocks.Add(value);
            AddRow(_data, Border(Label(label)), Border(value));
        };

        _container.AddChild(_data);
        _datawindow.Child = _container;
        // update textblocks for first time
        SyncToActiveFrame();
    }

    // event handlers
    // MouseMove event - getting new index
    private void MouseMove(ChartMouseEventArgs obj) {
        int i = (int)obj.BarIndex;
        Bars bars = MarketData.GetBars(obj.Chart.TimeFrame, obj.Chart.SymbolName);
        if(i > 0 && i < bars.Count - 1) {
            Bar target = bars[i];
            UpdateData(obj.Chart,target);
            // indicators
            foreach(ChartIndicator indicator in obj.Chart.Indicators) {
                Print(indicator.Type.Outputs);
                Print($"{indicator.Name}");
                foreach(var param in indicator.Parameters) {
                    Print($"{param.Name}: {indicator.Parameters[param.Name].Value}");
                }
                foreach(var output in indicator.Type.Outputs) {
                    Print($"{output.Name}");
                }

            }
        } else {
            SyncToActiveFrame();
        }
    }
    // add MouseMove event to new frames
    private void FramesAdded(FramesAddedEventArgs obj) {
        foreach(Frame frame in obj.AddedFrames) {
            if(frame is ChartFrame chartframe) {
                chartframe.Chart.MouseMove += MouseMove;
            }
        }
    }
    // remove MouseMove event from frames
    private void FramesRemoved(FramesRemovedEventArgs obj) {
        foreach(Frame frame in obj.RemovedFrames) {
            if(frame is ChartFrame chartframe) {
                chartframe.Chart.MouseMove -= MouseMove;
            }
        }
    }

    // ui control methods
    private ControlBase Border(ControlBase control) => new Border {
        Child = control,
        BorderThickness = new Thickness(1),
        Padding = new Thickness(5),
        BorderColor = Color.White,
    };
    private TextBlock Title(string text = "-") => new TextBlock {
        Text = text,
        Margin = new Thickness(0, 0, 0, 10),
    };
     private TextBlock Label(string text = "-") => new TextBlock {
        Text = text,
        HorizontalContentAlignment = HorizontalAlignment.Left,
        MinWidth = 150,
    };
    private TextBlock Value(string text = "-") => new TextBlock {
        Text = text,
        HorizontalAlignment = HorizontalAlignment.Right,
        MinWidth = 150
    };

   
    
    private void AddRow(Grid grid, ControlBase child) {
        int i = grid.AddRow().Index;
        grid.AddChild(child, i, 0);
    }
    
    private void AddRow(Grid grid, ControlBase row_item, ControlBase col_item) {
        int i = grid.AddRow().Index;
        grid.AddChild(row_item, i, 0);
        grid.AddChild(col_item, i, 1);
    }
    
    //"Symbol", "Date", "Time", "Open", "High", "Low", "Close", "Size", "Tick Volume", "Spread", "Pip Size"
    // utility methods
    private void UpdateData(Chart chart, Bar bar) {
        _datatextblocks[0].Text = $"{chart.Symbol.Name}, {chart.TimeFrame.ShortName}"; // symbol name
        _datatextblocks[1].Text = $"{bar.OpenTime.Date.Date}";                      // open date
        _datatextblocks[2].Text = $"{bar.OpenTime.TimeOfDay}";                      // open time
        _datatextblocks[3].Text = bar.Open.ToString();                                    // open
        _datatextblocks[4].Text = $"{bar.High}";                                    // high
        _datatextblocks[5].Text = $"{bar.Low}";                                     // low
        _datatextblocks[6].Text = $"{bar.Close}";                                   // close
        _datatextblocks[7].Text = $"{ToPips(bar.Close - bar.Open, chart.Symbol.PipSize)}";                // size
        _datatextblocks[8].Text = $"{bar.TickVolume}";                              // tickvolume
        _datatextblocks[9].Text = $"{ToPips(chart.Symbol.Spread, chart.Symbol.PipSize)}";               // spread
        _datatextblocks[10].Text = $"{MarketSessions}";                             // market sessions
    }
    
    // convert value to pips
    private double ToPips(double val, double pipsize) => Math.Round(val/pipsize, 1);
    
    // update data from active frame
    private void SyncToActiveFrame() {
        if (ChartManager.ActiveFrame is ChartFrame active) {
            UpdateData(active.Chart, MarketData.GetBars(active.Chart.TimeFrame, active.Chart.SymbolName).LastBar);
        }
    }
}        

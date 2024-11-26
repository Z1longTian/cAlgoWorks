using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.FullAccess)]
public class zBot : Robot {

    // enums
    // stoploss methods
    public enum StopLossMethod {
        Custom,
        Fixed5, // pips
        Fixed10, 
        Minimum,
        Maximum,
        ATRStoploss
    }
    
    // takeprofit methods
    public enum TakeProfitMethod {
        Custom,
        Fixed100, // 1 : 1
        Fixed150, // 1 : 1.5
        Fixed200, // 1 : 2
        TrailingStopLoss,
        ATRTrailingStopLoss
    }
    
    // parameters
    [Parameter("Session", DefaultValue = MarketSession.None, Group = "Trading")]
    public MarketSession Session { get; set; }
   
    [Parameter("Risk %", DefaultValue = 2.0, MinValue = 1.0, MaxValue = 5.0, Step = 0.5, Group = "Trading")]
    public double Risk { get; set; }
    
    [Parameter("Enable BreakEven", DefaultValue = false, Group = "Trading")]
    public bool BEEnabled { get; set; }
    
    [Parameter("BreakEven Trigger", DefaultValue = 5, MinValue = 1, Group = "Trading")]
    public double BETrigger { get; set; }
    
    [Parameter("ATR Length", DefaultValue = 14, MinValue = 1, Group = "Trading")]
    public int ATRLength { get; set; }
    
    [Parameter("ATR Multiplier", DefaultValue = 1.5, MinValue = 1, Group = "Trading")]
    public double ATRMult { get; set; }
    
    [Parameter("Enable Trade Protection", DefaultValue = true, Group = "Trading")]
    public bool ProtectionEnabled { get; set; }
    
    [Parameter("Stoploss", DefaultValue = StopLossMethod.Fixed5, Group = "StopLoss")]
    public StopLossMethod SLMethod { get; set; }
    
    [Parameter("Minimuim", DefaultValue = 5, MinValue = 5, Group = "StopLoss")]
    public double MinSL { get; set; }
    
    [Parameter("Maximum", DefaultValue = 20, MinValue = 20, Group = "StopLoss")]
    public double MaxSL { get; set; }
    
    [Parameter("TakeProfit", DefaultValue = TakeProfitMethod.Fixed100, Group = "TakeProfit")]
    public TakeProfitMethod TPMethod { get; set; }
    
    [Parameter("TSL Trigger", DefaultValue = 5, MinValue = 1, Group = "TakeProfit")]
    public double TSLTrigger { get; set; }
    
    // indicators
    private DataSeries atr;


    // variables
    
    protected override void OnStart() {
        // atr for atr trailing stoploss
        atr = Indicators.AverageTrueRange(ATRLength, MovingAverageType.Simple).Result;

    }

    protected override void OnTick() {
        //Strategy();
        foreach(Position p in Positions.FindAll(InstanceId)) {
            // trade protection will set to minimum stoploss
            if(!p.StopLoss.HasValue && ProtectionEnabled) p.ModifyStopLossPips(MinSL + GetSpread());
            // breakeven
            if(p.Pips > BETrigger && BEEnabled && !HasStoplossModified(p)) 
                p.ModifyStopLossPips(-1); // breakeven set to 1 pip  
            AdvancedTakeprofit(p);
        }
    }
    
    protected override void OnBarClosed() {
        Strategy();
    }
    
    // trading logic here, place this inside OnTick, OnBarClosed or OnBar
    private void Strategy() {
        if(!InSession()) return;
        if(MaxPositionsReached()) return;
        
        // buy conditions 
        bool buy = false;
        
        // sell conditions
        bool sell = false;
        
        if(buy || sell) {
            TradeType direction = buy ? TradeType.Buy : TradeType.Sell;
            ExecuteTrade(direction, GetStopLoss(direction));
        }
    }
    
    // custom stoploss method, impplement this method for your own need
    private double CustomStopLoss(TradeType direction) {
        return 0; // has a problem here 
    }
    
    // custom takeprofit method, impplement this method for your own need
    // this custom method is only running in OnTick
    // by seleting this option, takeprofit will not be set in the beginning
    // so if you want custom fixed ratio or value, you can do it here
    // but to increase performane if you want fixed ratio, you may want to add a check whehter tp is set or not
    private void CustomTakeProfit(Position p) {
    
    }
    
    //
    // utilities
    //
    // go into a trade, stoploss in pips
    // becareful here as if your stoploss falls into spread, sl won't be set properly
    private void ExecuteTrade(TradeType direction, double stoploss) =>
        ExecuteMarketOrder(direction, SymbolName, GetPositionSize(stoploss), InstanceId, stoploss, GetTakeProfit(stoploss), $"{stoploss}");
    
    // get the position size in units, 
    private double GetPositionSize(double stoploss) { 
        stoploss += GetSpread();
        double limit = Account.Balance * Risk / 100; // risk limit for a trade
        double size = (Account.Balance - limit) * Account.PreciseLeverage; // maximum trading volume
        double risk = Symbol.AmountRisked(size, stoploss); // risk amount
        double volume = risk > limit ? Symbol.VolumeForProportionalRisk(ProportionalAmountType.Balance, Risk, stoploss) : size;
        return Symbol.NormalizeVolumeInUnits(Math.Max(Symbol.VolumeInUnitsMin, volume), RoundingMode.Down);
    }
    
    // stoploss in pips
    private double GetStopLoss(TradeType direction) {
         // set stoploss except custom
        double sl = SLMethod switch {
            StopLossMethod.Custom => CustomStopLoss(direction),
            StopLossMethod.Fixed5 => 5,
            StopLossMethod.Fixed10 => 10,
            StopLossMethod.Minimum => MinSL,
            StopLossMethod.Maximum => MaxSL,
            StopLossMethod.ATRStoploss => ATRStopLoss(direction),
            _ => MinSL
        };
        return ProtectionEnabled ?  Math.Min(MaxSL, Math.Max(MinSL + GetSpread(), sl)) : sl;
    }
    
    private double ATRStopLoss(TradeType direction) {
        (double upper, double lower) = ATRSL();
        return ToPips(direction == TradeType.Buy ? Bid - lower : upper - Ask);
    }
    
    // get takeprofit in pips
    private double GetTakeProfit(double stoploss) =>
         TPMethod switch {
            TakeProfitMethod.Fixed100 => 1,
            TakeProfitMethod.Fixed150 => 1.5,
            TakeProfitMethod.Fixed200 => 2,
            _ => Double.NaN
        } * stoploss;        
    
    // advanced takeprofit
    private void AdvancedTakeprofit(Position p) {
        // custom method here
        if(TPMethod == TakeProfitMethod.Custom) CustomTakeProfit(p);
        
        // trailing stoploss
        if(TPMethod == TakeProfitMethod.TrailingStopLoss && !p.TakeProfit.HasValue) {
            if(p.Pips >= TSLTrigger && !p.HasTrailingStop) p.ModifyTrailingStop(true);
        }
        
        // atr trailing stoploss
        if(TPMethod == TakeProfitMethod.ATRTrailingStopLoss && !p.TakeProfit.HasValue) { // tsl method here
            if(HasStoplossModified(p)) { // stoploss has already modified
                TradeType direction = p.TradeType;
                (double upper, double lower) = ATRSL();
                double distance = direction == TradeType.Buy ? Bid - lower : upper - Ask; // atr tsl
                double newsl = direction == TradeType.Buy ? Bid - distance : Ask + distance;
                if(newsl > p.StopLoss && direction == TradeType.Buy || newsl < p.StopLoss && direction == TradeType.Sell) 
                   p.ModifyStopLossPrice(newsl);   
            } else { // stoploss hasn't modified yet
                if(p.Pips >= TSLTrigger) { // met trigger
                    p.ModifyStopLossPips(-1);
                }
            }
        }
    }
    
    // market session check
    private bool InSession() => MarketSessions.HasFlag(Session);

    // return true if max open positions are reached
    private bool MaxPositionsReached() => Positions.Count >= 1;

    // get spread in pips
    private double GetSpread() => ToPips(Symbol.Spread);
 
    // convert value to pips
    private double ToPips(double value) => Math.Round(value / Symbol.PipSize, 1);
    
    // true if the bar is a bull candle
    private bool IsBull(Bar bar) => bar.Close > bar.Open;
    
    private bool IsBear(Bar bar) => !IsBull(bar);
    
    private bool HasSignal(IndicatorDataSeries group) => !double.IsNaN(group.LastValue);
    
    // true if the bar has crossed above a level
    private bool HasCrossedOver(Bar bar, double level) => bar.Open < level && bar.Close > level;
    
    // true if the bas has crossed below a level
    private bool HasCrossedDown(Bar bar, double level) => bar.Open > level && bar.Close < level;
    
    // return the atr stoploss bound
    private (double upper, double lower) ATRSL() =>
        (Bars.HighPrices.Last(0) + ATRMult * atr.Last(0), Bars.LowPrices.Last(0) - ATRMult * atr.Last(0));
    
    // return true if stoploss set above / below entry price
    private bool HasStoplossModified(Position p) =>
        p.TradeType == TradeType.Buy && p.StopLoss > p.EntryPrice || p.TradeType == TradeType.Sell && p.StopLoss < p.EntryPrice;
    
}

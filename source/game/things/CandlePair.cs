using Godot;
using static Game;

public partial class CandlePair : Thing, MaybeBurningThing {
    static PackedScene candle =
        GD.Load<PackedScene>("res://scenes/things/candle.tscn");

    [Export] Texture2D tex_normal;
    [Export] Texture2D tex_lit;
    
    
    bool is_lit;
    public bool is_burning() => is_lit;
    
    
    public override void _Ready() {
        FlipH = GD.RandRange(1, 2) == 1;
    }

    public override void _push(PushEvent e, Thing by, Thing into) {
        if (!is_burning() && FireHelper.is_lit(by, into)) {
            start_burning();
            shake();
            game.tell("You light the candles.");
        }
        
        if (are_surrounding_tiles_free(2))
            e.add_yielder(this, CANDLE_PAIR_YIELD, yield);
    }
    

    void yield(PushEvent e) {
        var candle1 = candle.Instantiate<Candle>();
        var candle2 = candle.Instantiate<Candle>();
        toss_nearby(candle1);
        toss_nearby(candle2);
        if (is_burning()) {
            candle1.start_burning();
            candle2.start_burning();
        }
        immediately_disappear();
        
        game.tell("You kick the candles out of your way."
            + " They neatly land on their feet.");
    }

    public void start_burning() {
        is_lit = true;
        Texture = tex_lit;
    }
}
using Godot;
using static Game;

public partial class Candle : Thing, MaybeBurningThing {
    static PackedScene candle_pair =
        GD.Load<PackedScene>("res://scenes/things/candle_pair.tscn");

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
            game.tell("You light the candle.");
        }
        if (into is Candle) {
            bool is_new_burning = (is_burning() || FireHelper.is_lit(into));
            var result = combine_into(into, candle_pair) as CandlePair;
            if (is_new_burning)
                result.start_burning();
            game.tell("You shove the candles next to each other.");
        }
    }

    public void start_burning() {
        is_lit = true;
        Texture = tex_lit;
    }
}
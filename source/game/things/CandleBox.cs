using Godot;
using static Game;

public partial class CandleBox : Thing {
    static PackedScene candle =
        GD.Load<PackedScene>("res://scenes/things/candle.tscn");

    StareHelper stare = new();

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        stare.add("It's a box of candles.");
        int i = 0;
        while (are_surrounding_tiles_free(i + 1)) {
            var another = (i == 0 ? "a" : "another");
            stare.add($"You start taking {another} candle out...",
                ponder: true, time: 0);
            stare.add(action: unpack, rummage: true);
            i += 1;
        }
    });

    void unpack(StareEvent e) {
        toss_nearby(candle);
        game.tell("You take a candle out of the box.");
    }
    
    public override void _push(PushEvent e, Thing by, Thing into) {
        if (are_surrounding_tiles_free(1))
            e.add_yielder(this, CUP_STACK_YIELD, yield);
    }

    void yield(PushEvent e) {
        var old_pos = pos;
        if (try_get_nearby_empty_tile(out var p)) {
            move(p, MoveAnimation.HOP);
            if (try_get_nearby_empty_tile(out var q) && q != old_pos) {
                toss(q, candle);
                game.tell("You kick the box of candles away."
                    + " A candle falls out.");
            } else game.tell("You kick the box of candles away.");
        }
    }
}
using Godot;
using static Game;

public partial class CupStack : Thing {
    static PackedScene cup =
        GD.Load<PackedScene>("res://scenes/things/cup.tscn");

    bool has_taken_cup;
    StareHelper stare = new();

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        if (!has_taken_cup) {
            stare.add("It's a stack of disposable cups.");
            stare.add("The stack is too big for you"
                + " to grab a cup from the top. You can't reach.", time: 0);
        }
    });
    
    public override void _push(PushEvent e, Thing by, Thing into) {
        if (are_surrounding_tiles_free(1))
            e.add_yielder(this, CUP_STACK_YIELD, yield);
    }

    void yield(PushEvent e) {
        var old_pos = pos;
        if (try_get_nearby_empty_tile(out var p)) {
            move(p, MoveAnimation.HOP);
            
            var max_cups_lost = GD.RandRange(1, 2);
            int cups_lost = 0;
            
            for (int i = 0; i < max_cups_lost; i++) {
                if (try_get_nearby_empty_tile(out var q) && q != old_pos) {
                    toss(q, cup);
                    cups_lost += 1;
                }
            }

            var cups_message =
                cups_lost == 0 ? "" :
                cups_lost == 1 ? " A cup comes off the stack." :
                " Several cups fall off the stack.";
            if (cups_lost != 0)
                has_taken_cup = true;
            
            game.tell($"You kick the cup stack.{cups_message}");
        }
    }
}
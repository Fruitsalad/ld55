using Godot;
using static Game;

public partial class Knife : Thing {
    static PackedScene deer_corpse =
        GD.Load<PackedScene>("res://scenes/things/deer_corpse.tscn");

    StareHelper stare = new();
    int pity = 0;

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        stare.add("It's a small kitchen knife, good for peeling potatoes.");
        stare.add("This is a dangerous weapon in sufficiently incompetent"
            + " hands.",
            time: 0);
    });

    public override void _push(PushEvent e, Thing by, Thing into) {
        if (by is Player && e.dir == Dir2D.SOUTH)
            e.add_yielder(this, KNIFE_YIELD, yield_stab_player);
        if (into is Deer) {
            if (e.dir != Dir2D.SOUTH && e.dir != Dir2D.EAST) {
                combine_into(into, deer_corpse);
                game.tell("You stab the deer with the knife.");
                game.tell("It immediately falls to the ground dead.");
            } else if (e.dir == Dir2D.EAST && (++pity % 4) == 3)
                game.tell("That's not the sharp side of the knife.");
        }
    }

    void yield_stab_player(PushEvent e) {
        game._unset(pos);
        game.tell("You impale yourself on the knife.");
        game.after(0.2f, () => {
            explode();
            game.player.impale();
            QueueFree();
        });
    }

}
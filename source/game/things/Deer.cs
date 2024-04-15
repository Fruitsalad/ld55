using Godot;
using static Game;

public partial class Deer : Thing {
    static PackedScene deer_corpse =
        GD.Load<PackedScene>("res://scenes/things/deer_corpse.tscn");
    
    StareHelper stare = new();

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        stare.add("It's a deer.");
        stare.add(
            "You should have been able to tell from the excellent"
            + " drawing that it is a deer.", time: 0);
    });

    public override void _push(PushEvent e, Thing by, Thing into) {
        if (by is Player)
            game.tell("The deer firmly stands its ground and won't budge.");
        e.block();
    }

}
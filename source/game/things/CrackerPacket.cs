using Godot;
using static Game;
using static Util;

public partial class CrackerPacket : Thing {
    PackedScene cracker =
        GD.Load<PackedScene>("res://scenes/things/cracker.tscn");
    [Export] Texture2D tex_torn;
    
    StareHelper stare = new();
    int remaining = 4;


    public override void _Ready() {
        assuming (tex_torn != null);
        FlipH = GD.RandRange(1, 2) == 1;
    }

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        if (remaining == 0) {
            stare.add("The torn wrapper of a packet of sea salt crackers.");
        } else {
            stare.add("A packet of four sea salt crackers.");
            stare.add("You start tearing it open...", ponder: true, time: 0);
            stare.add(action: unpack, rummage: true);
        }
    });

    void unpack(StareEvent e) {
        while (remaining > 0 && are_surrounding_tiles_free(1)) {
            toss_nearby(cracker);
            remaining -= 1;
        }
        if (remaining != 0)
            game.tell(
                "You take some of the crackers out of the packet, "
                + "but you don't have enough space to unpack all of them."
            );
        else game.tell(
            "You tear open the cracker packet and toss the crackers "
            + "on the ground."
        );

        if (remaining == 0) {
            Texture = tex_torn;
            shake();
        }
    }
}
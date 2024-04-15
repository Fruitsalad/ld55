using Godot;
using static Game;
using static Util;

public partial class Crackers : Thing {
    PackedScene cracker_packet =
        GD.Load<PackedScene>("res://scenes/things/cracker_packet.tscn");
    [Export] Texture2D tex_torn;
    
    StareHelper stare = new();
    int remaining = 4;
    
    
    public override void _Ready() {
        assuming (tex_torn != null);
    }
    
    public override void _stare(StareEvent e) => stare.handle(e, () => {
        if (remaining == 0) {
            stare.add("The torn wrapper of a package of sea salt crackers.");
        } else {
            stare.add("Sea salt crackers.");
            stare.add(
                "You start opening the packaging...", ponder: true, time: 0
            );
            stare.add(action: unpack, rummage: true);
        }
    });

    void unpack(StareEvent e) {
        while (remaining > 0 && are_surrounding_tiles_free(1)) {
            toss_nearby(cracker_packet);
            remaining -= 1;
        }
        if (remaining != 0)
            game.tell(
                "You take some of the cracker packets out of the packaging, "
                + "but you don't have enough space to unpack all of them."
            );
        else game.tell(
            "You tear open the packaging and toss the cracker "
            + "packets on the ground."
        );

        if (remaining == 0) {
            Texture = tex_torn;
            shake();
        }
    }
}
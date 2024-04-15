using Godot;
using static Game;
using static Util;

public partial class Tree : Thing, MaybeBurningThing {
    static PackedScene log =
        GD.Load<PackedScene>("res://scenes/things/log.tscn");
    static PackedScene ash =
        GD.Load<PackedScene>("res://scenes/things/ash.tscn");
    
    const int FULL_HP = 6;
    int hp = FULL_HP;


    FireHelper fire = new();
    public bool is_burning() => fire.is_burning;
    

    public override void _push(PushEvent e, Thing by, Thing into) {
        if (!is_burning() && FireHelper.is_lit(by)) {
            fire.start_burning(this);
            game.tell("You light the tree on fire.");
        }
        e.block();
        if (by is Player) {
            hp -= 1;
            if (hp <= 0) {
                if (is_burning()) {
                    game.tell("The tree turns to ash.");
                    turn_into(ash);
                } else {
                    var smack = pick("smack", "headbutt", "slap");
                    game.tell($"You {smack} the tree into a log.");
                    turn_into(log);
                }
            } else if (hp == FULL_HP - 1)
                game.tell($"You {pick("smack", "headbutt", "slap")} the tree.");
            else game.tell(pick("Smack", "Bonk", "Slap"));
        }
    }
}
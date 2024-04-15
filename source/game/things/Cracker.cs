using Godot;
using static Game;

public partial class Cracker : Thing {
    StareHelper stare = new();
    
    
    public override void _Ready() {
        FlipH = GD.RandRange(1, 2) == 1;
    }
    
    public override void _push(PushEvent e, Thing by, Thing into) {
        e.add_yielder(this, CRACKER_YIELD, yield);
    }

    void yield(PushEvent e) {
        explode();
        disappear();
        game.tell("You crush the cracker.");
    }

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        stare.add("A sea salt cracker.");
        stare.add("You start eating it...", ponder: true, time: 0);
        stare.add(rummage: true, action: _ => {
            animate_move(e.source.pos, MoveAnimation.HOP);
            game.after(0.15f, () => {
                explode();
                Visible = false;
                disappear();
            });

            if (!game.player.try_heal(5)) {
                var salty = Util.pick("salty", "tasty");
                game.tell($"You ate the cracker. It was {salty}.");
            } else if (game.player.hp == game.player.max_hp)
                game.tell("You ate the cracker. That hit the spot!");
            else if (game.player.hp < 10)
                game.tell("You ate the cracker. You really needed that.");
            else game.tell("You ate the cracker. You feel a much better.");
        });
    });
}
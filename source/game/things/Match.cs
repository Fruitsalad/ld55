using Godot;
using static Game;

public partial class Match : Thing, MaybeBurningThing {
    [Export] Texture2D unlit;
    [Export] Texture2D lit;
    
    bool is_burning = false;
    int pity = 0;


    public override void _push(PushEvent e, Thing by, Thing into) {
        if (is_burning) {
            
        } else {
            if (into is MatchBox) {
                if (e.dir == Dir2D.NORTH)
                    light();
                else if ((++pity % 11) == 10)
                    game.tell("You're pushing the match into a cardboard"
                        + " side of the match box.", true);
            } else if (FireHelper.is_lit(by, into))
                light();
        }
    }

    void light() {
        if (is_burning)
            return;
        is_burning = true;
        game.after(0.2f, () => {
            update_texture();
            explode();
        });
        game.tell("You light the match.");
    }

    void update_texture() =>
        Texture = is_burning ? lit : unlit;

    bool MaybeBurningThing.is_burning() => is_burning;
}
using Godot;
using static Game;

public partial class Cup : Thing {
    public enum State { EMPTY, SODA, BLOOD }
    public State state { get; private set; }
    bool is_laying;

    [Export] Texture2D tex_empty;
    [Export] Texture2D tex_soda;
    [Export] Texture2D tex_blood;
    [Export] Texture2D tex_laying;
    
    
    public override void _Ready() {
        FlipH = GD.RandRange(1, 2) == 1;
    }
    
    public override void _push(PushEvent e, Thing by, Thing into) {
        if (state == State.EMPTY && (by is Soda || into is Soda)) {
            game.tell("You fill the cup with fizzy pop.");
            game.after(0.15f, () => swap_state(State.SODA));
        }

        if (state == State.EMPTY || are_surrounding_tiles_free(1))
            e.add_yielder(this, CUP_YIELD, yield);
    }

    void yield(PushEvent e) {
        if (try_get_nearby_empty_tile(out var p)) {
            move(p, MoveAnimation.HOP);
            game.after(0.25f, toss_over);
            game.tell("You kick the cup.");
        } else {
            explode();
            disappear();
            game.tell("You crush the cup into nothingness.");
        }
    }

    void toss_over() {
        is_laying = true;
        
        swap_state(State.EMPTY);
        swap_texture();
    }

    void swap_state(State new_state) {
        if (state == new_state)
            return;
        state = new_state;
        explode();
        swap_texture();
    }

    void swap_texture() {
        shake();
        Texture = state switch {
            State.SODA => tex_soda,
            State.BLOOD => tex_blood,
            State.EMPTY => (is_laying ? tex_laying : tex_empty),
            _ => tex_laying
        };
    }
}
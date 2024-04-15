using System;
using Godot;
using static Util;
using static Game;
using static Vector2IntUtil;

public partial class Player : Thing {
    [Export] Texture2D normal;
    [Export] Texture2D tex_dead;
    [Export] Texture2D tex_dead_stab;
    
    public bool is_wearing_hat = true;

    public int hp { get; private set; } = 15;
    public int max_hp { get; private set; } = 20;

    bool is_impaled;

    Sprite2D stab_wound;
    Sprite2D hat;

    int prev_dist;


    public override void _Ready() {
        stab_wound = this.get_node<Sprite2D>("StabWound");
        hat = this.get_node<Sprite2D>("Hat");
        stab_wound.Visible = false;
    }

    public override void _push(PushEvent e, Thing by, Thing into) {
        if (e.dir == Dir2D.EAST)
            set_mirrored(true);
        if (e.dir == Dir2D.WEST)
            set_mirrored(false);
        if (is_impaled) {
            shake(strength: 2, duration: .4f);
            if (try_kill(1))
                game.knife_ending();
            else if (hp == 5)
                game.tell("You are not long for this world.");
        }
    }

    public override void _after_successful_push(PushEvent e) {
        if (is_impaled) {
            game.after(0.15f, () => {
                game.add_trail(pos, Dir2D.get_opposite(e.dir));
            });
        } else {
            var dist1 = elem_max(Vector2I.Zero, visible_min - pos);
            var dist2 = elem_max(Vector2I.Zero, pos - visible_max);
            var dist = elem_max(dist1, dist2);
            var distance = dist.X + dist.Y;
            
            if (distance > 14) {
                game.tell("You walk right into the wolf.");
                game.wolf_ending();
            } else if (distance > 10 && prev_dist <= 10) {
                game.tell("Oh crap!!! It's a wolf!!!"
                    + " You can't see it but it's menacing!!!");
                game.tell("Better get out of here!!!");
            } else if (distance > 2 && prev_dist <= 2) {
                game.tell("You walk to a different part of the forest.");
                game.tell("It's very difficult to see in this part of the"
                    + " forest, because it is off-screen.");
            }

            prev_dist = distance;
        }
    }

    public bool try_kill(int damage) {
        assuming (damage >= 0);
        if (damage >= hp) {
            hp = 0;
            return true;
        }
        hp -= damage;
        return false;
    }

    public void hurt_but_dont_kill(int damage) {
        var new_health = hp - damage;
        if (new_health < 1)
            hp = 1;
        else hp = new_health;
    }

    public bool try_heal(int gain) {
        if (hp == max_hp)
            return false;
        hp = Math.Min(max_hp, hp + gain);
        return true;
    }

    public void impale() {
        is_impaled = true;
        stab_wound.Visible = true;
        shake(duration: 1.5f, strength: 2f);
        hurt_but_dont_kill(5);
        game.tell("You are bleeding out.");
    }

    public void set_hatted(bool is_hatted) {
        hat.Visible = is_hatted;
    }

    public void set_dead() {
        Texture = (is_impaled ? tex_dead_stab : tex_dead);
        hat.Visible = false;
        stab_wound.Visible = false;
    }

    void set_mirrored(bool is_mirrored) {
        FlipH = is_mirrored;
        stab_wound.FlipH = is_mirrored;
    }
}
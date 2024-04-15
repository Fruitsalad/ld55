using System;
using Godot;
using static Game;
using static Util;

public partial class Car : Thing {
    static PackedScene crackers =
        GD.Load<PackedScene>("res://scenes/things/crackers.tscn");
    static PackedScene soda = 
        GD.Load<PackedScene>("res://scenes/things/soda.tscn");
    static PackedScene cup_stack =
        GD.Load<PackedScene>("res://scenes/things/cup_stack.tscn");
    static PackedScene matches =
        GD.Load<PackedScene>("res://scenes/things/match_box.tscn");
    static PackedScene knife =
        GD.Load<PackedScene>("res://scenes/things/knife.tscn");
    static PackedScene candles =
        GD.Load<PackedScene>("res://scenes/things/candle_box.tscn");
    static PackedScene tome =
        GD.Load<PackedScene>("res://scenes/things/tome.tscn");
    
    static (string, PackedScene)[] supplies = {
        ("You grab a box of candles from the trunk.", candles),
        ("You grab a knife from the trunk.", knife),
        ("You grab a box of matches from the trunk.", matches),
        ("You grab a package of sea salt crackers from the trunk.", crackers),
        ("You grab a bottle of fizzy pop from the trunk.", soda),
        ("You grab a stack of disposable cups from the trunk.", cup_stack),
        ("You grab your tome from the trunk.", tome),
        ("You grab more sea salt crackers from the trunk.", crackers)
    };
    
    [Export] Texture2D tex_normal;
    [Export] Texture2D tex_trunk_open;
    [Export] Texture2D tex_bottom;
    [Export] Texture2D tex_top;
    [Export] Texture2D tex_upside_down;
    [Export] Texture2D tex_upside_down_trunk_open;

    int rotation;
    int trunk_status;
    bool is_trunk_unlocked;
    int supplies_remaining;
    StareHelper stare = new();

    public override void _Ready() {
        supplies_remaining = supplies.Length;
    }

    public void unlock_trunk() {
        game.after(0.3f, () => {
            is_trunk_unlocked = true;
            update_sprite();
            explode();
            game.tell("You unlock the trunk of the car.");
        });
    }

    public override void _after_successful_push(PushEvent e) {
        if (rotation != 0) {
            shake(duration: 0.5f);
            if (Math.Abs(rotation) > 1)
                return;
        }
        if (e.dir == Dir2D.NORTH || e.dir == Dir2D.SOUTH) {
            rotation += (e.dir == Dir2D.NORTH ? 1 : -1);
            game.after(0.25f, () => {
                shake(strength: 10, duration: 0.5f);
                explode();
                update_sprite();
            });
            if (Math.Abs(rotation) == 1)
                game.tell("You push your car on its side.");
            else if (rotation == 0)
                game.tell("You push your car upright again.");
            else game.tell("You push your car upside-down.");
        }
    }

    void update_sprite() {
        if (rotation == 0) {
            Texture = (is_trunk_unlocked ? tex_trunk_open : tex_normal);
        } else if (rotation == 1)
            Texture = tex_bottom;
        else if (rotation == -1)
            Texture = tex_top;
        else if (is_trunk_unlocked)
            Texture = tex_upside_down_trunk_open;
        else Texture = tex_upside_down;
    }

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        if (is_trunk_unlocked && e.dir == Dir2D.WEST) {
            if (supplies_remaining != 0)
                stare.add(
                    "You start rummaging through the trunk...", ponder: true);
            bool has_more_left = supplies_remaining > 7;
            for (int i = Math.Min(7, supplies_remaining); i > 0; i -= 1)
                stare.add(action: handle_grabbing_supplies, rummage: true,
                    time: 4);
            if (has_more_left)
                stare.add("There's more left in the trunk, but you have nowhere"
                    + " to put it.", time: 5f);
            else stare.add("The trunk is empty.", time: 5f);
            return;
        }
        
        stare.add("It's your car. Your trusty Toyota Aygo.");
        if (Math.Abs(rotation) == 1)
            stare.add("You've pushed it on its side.");
        if (Math.Abs(rotation) > 1) {
            stare.add("It's upside-down.");
            stare.add("It's more of a Toyota Aydontgo at this point.", time: 0);
        }
        if (!is_trunk_unlocked) {
            stare.add(
                "You have a bunch of supplies in the trunk, "
                + "but the trunk is locked."
            );
        } else if (supplies_remaining != 0)
            stare.add("There are still supplies in the trunk.");
    });

    void handle_grabbing_supplies(StareEvent e) {
        assuming (supplies_remaining > 0);
        if (!are_surrounding_tiles_free(1)) {
            game.tell("You tried to grab something"
                + " but there's nowhere to toss it.");
            e.stop_staring();
            return;
        }
        var (msg, thing) = supplies[^supplies_remaining];
        game.tell(msg, allow_pondering: true);
        e.source.toss_nearby(thing);
        
        supplies_remaining -= 1;
        if (supplies_remaining != 0)
            e.keep_staring(new_state: StareState.RUMMAGE);
    }
}
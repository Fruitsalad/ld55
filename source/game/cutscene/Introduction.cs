using Godot;
using static GodotUtil;

public partial class Introduction : Cutscene {
    static PackedScene packed_game =
        GD.Load<PackedScene>("res://scenes/game.tscn");
    Tween tween;
    
    public override void _Ready() {
        init_cutscene();
        
        create_tween(ref tween, this);
        tween.TweenCallback(Callable.From(() => {
            tell("You've loaded your car full of equipment and are driving deep"
                + " into a secluded part of the forest.");
        }));
        tween.TweenInterval(6);
        tween.TweenCallback(Callable.From(() => {
            tell("Your mission is simple:"
                + " Summon a really hot succubus.");
        }));
        tween.TweenInterval(4);
        tween.TweenCallback(Callable.From(() => {
            tell("You're well prepared. This can barely go wrong.");
        }));
        tween.TweenInterval(4);
        tween.TweenCallback(Callable.From(() => {
            text_box_panel.Visible = false;
        }));
        tween.TweenInterval(1);
        tween.TweenCallback(Callable.From(() => {
            GetTree().ChangeSceneToPacked(packed_game);
        }));
    }
}
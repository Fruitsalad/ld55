using Godot;
using static GodotUtil;

public partial class ImpEnding : Cutscene {
    Tween tween;
    
    public override void _Ready() {
        init_cutscene();
        
        create_tween(ref tween, this);
        tween.TweenInterval(7);
        tween.TweenCallback(Callable.From(() => {
            tell("An imp appears in your view...");
        }));
        tween.TweenInterval(6);
        tween.TweenCallback(Callable.From(() => {
            tell("He immediately starts eating all your soda and crackers.");
        }));
        tween.TweenInterval(10);
        tween.TweenCallback(Callable.From(() => {
            tell("After he's done, you manage to take him home with you.");
        }));
        tween.TweenInterval(12);
        tween.TweenCallback(Callable.From(() => {
            tell("He's not sexy and he doesn't turn out to be very smart,"
                + " but he occasionally does the dishes and sometimes"
                + " helps clean.");
        }));
        tween.TweenInterval(12);
        tween.TweenCallback(Callable.From(() => {
            tell("Looks like you got yourself an occult servant! Neat!");
        }));
        tween.TweenInterval(12);
        tween.TweenCallback(Callable.From(() => {
            tell("GAME OVER: You summoned an imp! (Not a succubus)");
        }));
        tween.TweenInterval(8);
        tween.TweenCallback(Callable.From(() => {
            GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
        }));
    }
}
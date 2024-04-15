using Godot;
using static GodotUtil;

public partial class SuccubusEnding : Cutscene {
    Tween tween;
    
    public override void _Ready() {
        init_cutscene();
        
        create_tween(ref tween, this);
        tween.TweenInterval(5);
        tween.TweenCallback(Callable.From(() => {
            tell("\"I am here.\"");
        }));
        tween.TweenInterval(7);
        tween.TweenCallback(Callable.From(() => {
            tell("\"It is a great delight to meet a human as fine as you.\"");
        }));
        tween.TweenInterval(12);
        tween.TweenCallback(Callable.From(() => {
            tell("\"I imagine you must have your reasons for summoning "
                + "a succubus...\"");
        }));
        tween.TweenInterval(10);
        tween.TweenCallback(Callable.From(() => {
            tell("\"Come... Let me show you those things which only"
                + " a succubus can show you.\"");
        }));
        tween.TweenInterval(12);
        tween.TweenCallback(Callable.From(() => {
            tell("GAME OVER: You summoned a succubus!");
        }));
        tween.TweenInterval(15);
        tween.TweenCallback(Callable.From(() => {
            tell("Made by Zowie van Dillen for Ludum Dare 55."
                + " Thanks for playing!");
        }));
        tween.TweenInterval(8);
        tween.TweenCallback(Callable.From(() => {
            GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
        }));
    }
}
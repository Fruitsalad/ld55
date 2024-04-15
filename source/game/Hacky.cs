using Godot;

public partial class Hacky : Control {
    public override void _Ready() {
        CallDeferred(MethodName.swap_scene);
    }
    
    void swap_scene() =>
        GetTree().ChangeSceneToFile("res://scenes/game.tscn");
}
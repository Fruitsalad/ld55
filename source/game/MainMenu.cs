using Godot;

public partial class MainMenu : Control {
    static PackedScene packed_introduction =
        GD.Load<PackedScene>("res://scenes/cutscenes/introduction.tscn");
    
    public override void _Ready() {
        var start_button = this.get_node<Button>("%StartButton");
        start_button.Pressed += () => {
            GetTree().ChangeSceneToPacked(packed_introduction);
        };
    }
}
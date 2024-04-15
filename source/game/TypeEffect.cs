using Godot;

public partial class TypeEffect : RichTextEffect {
    public string bbcode = "type";
    
    public override bool _ProcessCustomFX(CharFXTransform fx) {
        float speed = 40;
        float delay = 0;
        if (fx.Env.TryGetValue("speed", out var speed_variant))
            speed = (float)speed_variant.AsDouble();
        if (fx.Env.TryGetValue("delay", out var delay_variant))
            delay = (float)delay_variant.AsDouble();

        bool is_visible = (fx.ElapsedTime * speed - delay >= fx.RelativeIndex);
        fx.Visible = is_visible;
        return true;
    }
}
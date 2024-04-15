using Godot;
using static Game;

public class FireHelper {
    static PackedScene fire = GD.Load<PackedScene>("res://scenes/fx/fire.tscn");

    public bool is_burning { get; private set; }
    Node2D fire_effect;
    GpuParticles2D smoke_particles;
    GpuParticles2D fire_particles;

    public static bool is_lit(Thing thing) =>
        thing is MaybeBurningThing other && other.is_burning();
    public static bool is_lit(Thing thing, Thing thing2) =>
        is_lit(thing) || is_lit(thing2);
    
    public void start_burning(Node parent) {
        is_burning = true;
        fire_effect = fire.Instantiate<Node2D>();
        parent.AddChild(fire_effect);
        fire_particles = fire_effect.get_node<GpuParticles2D>("Fire");
        smoke_particles = fire_effect.get_node<GpuParticles2D>("Smoke");
        fire_particles.Emitting = true;
        smoke_particles.Emitting = true;
    }

    public void stop_burning() {
        fire_particles.Emitting = false;
        smoke_particles.Emitting = false;
        fire_particles.Finished += fire_effect.QueueFree;
        is_burning = false;
    }
}

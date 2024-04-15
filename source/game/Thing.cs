using Godot;
using static Game;
using static Dir2D;
using static Util;

public partial class Thing : Sprite2D {
	public enum MoveAnimation { HOP, SLIDE, INSTANT }
	const int hop_height = 10;

	public static PackedScene packed_explosion =
		GD.Load<PackedScene>("res://scenes/fx/explosion.tscn");
	
	public Vector2I pos;
	Tween pos_tween;
	Tween scale_tween;
	Tween offset_tween;
	bool is_rummaging;
	
    public virtual void _push(PushEvent e, Thing by, Thing into) {}
    public virtual void _stare(StareEvent e) => e.stop_staring();
    public virtual void _after_successful_push(PushEvent e) {}

    public void move(
	    Vector2I new_pos, MoveAnimation anim = MoveAnimation.SLIDE,
	    float duration = 0.25f
	) {
	    game._unset(pos);
	    game._set(new_pos, this);
	    animate_move(new_pos, anim, duration);
	    pos = new_pos;
    }

    public void animate_move(
	    Vector2I new_pos, MoveAnimation anim = MoveAnimation.SLIDE,
	    float duration = 0.25f
    ) {
	    var start_pos = Position;
	    var end_pos = grid_to_world(new_pos);
	    
	    if (anim == MoveAnimation.SLIDE) {
		    GodotUtil.create_tween(
			    ref pos_tween, this, "position", end_pos, duration: duration
		    );
	    } else if (anim == MoveAnimation.HOP) {
		    GodotUtil.create_tween(
			    ref pos_tween, this, trans: Tween.TransitionType.Linear
		    ).TweenMethod(
			    Callable.From<float>(t => {
				    var base_pos = start_pos + t * (end_pos - start_pos);
				    var y_offset = hop_height * Mathf.Sin(t * Mathf.Pi);
				    Position = base_pos + Vector2.Up * y_offset;
			    }), 0f, 1f, duration
		    );
	    } else if (anim == MoveAnimation.INSTANT) {
		    Position = end_pos;
	    }
	    
	    game.lock_input_for(pos_tween);
    }

    public Thing combine_into(Thing other, PackedScene packed) {
	    var thing = packed.Instantiate<Thing>();
	    combine_into(other, thing);
	    return thing;
    }

    public void combine_into(Thing other, Thing replacement) {
	    assuming (!replacement.IsInsideTree());
	    
	    GodotUtil.create_tween(
		    ref pos_tween, this, "position", other.Position, duration: 0.25f
	    );
	    game.lock_input_for(pos_tween);
	    game._unset(pos);
	    game.after(smack_delay, () => {
		    _disappear();
		    other.immediately_turn_into(replacement);
	    });
    }

    public Thing turn_into(PackedScene packed) {
	    var thing = packed.Instantiate<Thing>();
	    turn_into(thing);
	    return thing;
    }

    public void turn_into(Thing replacement) {
	    assuming (!replacement.IsInsideTree());
	    game.after(smack_delay, () => immediately_turn_into(replacement));
    }

    public void immediately_turn_into(Thing replacement) {
	    explode();
	    disappear();
	    game.add_thing(replacement, pos);
	    replacement.shake(strength: 3, duration: 0.5f);
    }

    public void disappear() {
	    game._unset(pos);
	    _disappear();
    }

    public void immediately_disappear() {
	    game._unset(pos);
	    QueueFree();
    }

    public void animate_disappear() {
	    GodotUtil.create_tween(
		    ref scale_tween, this, "scale", Vector2.Zero, Tween.EaseType.In,
		    duration: 0.3f
	    );
	    game.lock_input_for(scale_tween);
    }

    void _disappear() {
	    animate_disappear();
	    scale_tween.Finished += QueueFree;
    }

    public void dunk(int dir) {
	    float duration = smack_delay;
	    var new_pos = pos + ONE_STEP[dir];
	    var start_pos = grid_to_world(pos);
	    var end_pos = start_pos + 2/5f * (grid_to_world(new_pos) - start_pos);
	    
	    // Move 2/5 of the way there...
	    GodotUtil.create_tween(
		    ref pos_tween, this, "position", end_pos,
		    trans: Tween.TransitionType.Linear, duration: duration
	    );
	    // Move back...
	    pos_tween.TweenProperty(this, "position", start_pos, duration);
	    // Wait a bit...
	    pos_tween.TweenInterval(smack_delay);
	    
	    game.lock_input_for(pos_tween);
    }

    public void shake(float strength = 5, float duration = 0.25f) {
	    GodotUtil.create_tween(ref offset_tween, this, Tween.EaseType.In)
		    .TweenMethod(
		    Callable.From<float>(t => {
			    var range = (1 - t) * strength;
			    var x = (float)GD.RandRange(-range, range);
			    var y = (float)GD.RandRange(-range, range);
			    Offset = new(x, y);
		    }), 0f, 1f, duration
	    );
    }

    public void explode() {
	    var explosion = packed_explosion.Instantiate<GpuParticles2D>();
	    explosion.Position = Position;
	    game.effects_parent.AddChild(explosion);
	    explosion.OneShot = true;
	    explosion.Emitting = true;
	    explosion.Finished += () => explosion.QueueFree();
    }

    public bool are_surrounding_tiles_free(int amount) {
	    var count = 0;
	    for (int y = 0; y < 3; y++)
		    for (int x = 0; x < 3; x++)
			    if (!game.has_thing(pos + new Vector2I(x-1, y-1)))
				    count += 1;
	    return count >= amount;
    }

    public bool try_get_nearby_empty_tile(out Vector2I tile) {
	    var indices = new[] { 0, 1, 2, 3, 4, 5, 6, 7 };
	    for (int i = 2; i < 8; i++) {
		    var i1 = GD.RandRange(i, 7);
		    var i2 = GD.RandRange(0, i - 1);
		    (indices[i1], indices[i2]) = (indices[i2], indices[i1]);
	    }

	    foreach (var _i in indices) {
		    var i = _i;
		    if (i > 3)
			    i += 1;
		    var x = i % 3 - 1;
		    var y = i / 3 - 1;
		    var p = pos + new Vector2I(x, y);
		    if (!game.has_thing(p)) {
			    tile = p;
			    return true;
		    }
	    }

	    tile = default;
	    return false;
    }

    public void toss_nearby(PackedScene packed) =>
	    toss_nearby(packed.Instantiate<Thing>());

    public void toss_nearby(Thing thing) {
	    if (try_get_nearby_empty_tile(out var p))
		    toss(p, thing);
	    else game.tell("An item was lost because no tile was empty :(");
    }

    public void toss(Vector2I pos, PackedScene packed) =>
	    toss(pos, packed.Instantiate<Thing>());

    public void toss(Vector2I pos, Thing thing) {
	    assuming (!thing.IsInsideTree());
	    game.add_thing(thing, pos);
	    thing.Position = Position;
	    game.lock_input();
	    thing.animate_move(pos, MoveAnimation.HOP);
	    thing.pos_tween.Finished += () => {
		    thing.shake();
		    thing.explode();
		    game.unlock_input();
	    };
    }

    public void start_rummaging() {
	    is_rummaging = true;
	    GodotUtil.create_tween(
		    ref offset_tween, this, ease: Tween.EaseType.InOut
		).SetLoops().TweenMethod(
		    Callable.From<float>(t => {
			    Offset = new(0, Mathf.Sin(t));
		    }), 0f, 1f, 0.4f
	    );
    }

    public void stop_rummaging() {
	    if (is_rummaging)
			offset_tween.Kill();
	    is_rummaging = false;
    }
}
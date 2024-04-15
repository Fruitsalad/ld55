using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using static Util;
using static Dir2D;


public partial class Game : Control {
	static PackedScene packed_ominous_effect =
		GD.Load<PackedScene>("res://scenes/fx/ominous.tscn");
    
	public const int tile_width = 64;
	public const float smack_delay = 0.1f;
	public const float default_ponder_time = 2f;
	public static Vector2I tile_size = new(tile_width, tile_width);
	public static Vector2I half_tile = tile_size / 2;
	
	public static Vector2I visible_min = new(-1, -1);
	public static Vector2I visible_max = new(17, 10);
	public static Rect2I visible_rect =
		new(visible_min, visible_max - visible_min);

	public const int KNIFE_YIELD = 1;
	public const int CANDLE_BOX_YIELD = 5;
	public const int DOUBLE_LOG_YIELD = 10;
	public const int CANDLE_PAIR_YIELD = 20;
	public const int CUP_STACK_YIELD = 30;
	public const int BURNING_LOG_YIELD = 35;
	public const int CUP_YIELD = 40;
	public const int CRACKER_YIELD = 50;
	
	public static Game game;
	public int turn;
	
	// Messages
	public class Message {
		public string text;
		public bool allow_pondering;
		public bool does_not_time_out;
	}

	AnimationPlayer animation_player;
	Control text_box_panel;
	RichTextLabel text_box;
	Control more_arrow;
	Node2D world;
	Control fade_away;
	Node2D animation_car;
	Control pause_menu;
	
	List<Message> messages_this_turn = new();
	int last_message_turn;
	int next_message;
	bool is_showing_message;
	SceneTreeTimer message_timeout;
	
	// Things
	public Node2D effects_parent;
	Node2D thing_parent;
	Dictionary<Vector2I, Thing> things = new();
	public Player player { get; private set; }
	Thing car;
	Thing car_key;
	
	// Inputs
	int input_locks;
	bool is_movement_locked => input_locks != 0;
	int last_dir = SOUTH;
	bool is_left_down;
	bool is_right_down;
	bool is_up_down;
	bool is_down_down;
	bool is_paused;
	static StringName LEFT = "left";
	static StringName RIGHT = "right";
	static StringName UP = "up";
	static StringName DOWN = "down";
	static StringName PAUSE = "pause";
	
	// Staring
	Sprite2D ellipsis;
	Tween ponder_timer;  // Tweens also have timer-like functionality
	bool is_staring;
	bool has_finished_staring = true;
	int ponder_locks;
	Action ponder_callback;
	
	// Trails and circles
	public enum Summon {
		NO_SUMMON, SUCCUBUS, IMP
	}
	
	public struct Circle {
		public bool has_candles;
		public bool are_candles_lit;
		public bool has_book;
		public bool is_center_empty;
		public Summon summon;
	}
	
	public Trails trails;
	Dictionary<Vector2I, Circle> circles = new();
	HashSet<Vector2I> pending_reevaluations = new();
	GpuParticles2D ominous_effect;
	bool has_had_circle;
	bool has_had_candles;
	
	// Game Over
	bool is_game_over;
	
	
	public override void _Ready() {
		game = this;
		world = this.get_node<Node2D>("%World");
		thing_parent = this.get_node<Node2D>("%Things");
		effects_parent = this.get_node<Node2D>("%Effects");
		text_box = this.get_node<RichTextLabel>("%TextBox");
		text_box_panel = this.get_node<Control>("%TextBoxPanel");
		ellipsis = this.get_node<Sprite2D>("%Ellipsis");
		more_arrow = this.get_node<Control>("%MoreArrow");
		trails = this.get_node<Trails>("%Trails");
		fade_away = this.get_node<Control>("%FadeAway");
		animation_player = this.get_node<AnimationPlayer>("%AnimationPlayer");
		car = this.get_node<Thing>("%Car");
		car_key = this.get_node<Thing>("%CarKey");
		animation_car = this.get_node<Node2D>("%AnimationCar");
		pause_menu = this.get_node<Control>("%PauseMenu");
		
		lock_input();
		init_pause_menu();
		load_level();
		run_start_animation();
	}

	void run_start_animation() {
		animation_player.Play("Start");
		animation_player.AnimationFinished += _ => {
			animation_car.Visible = false;
			car.Visible = true;
			player.Position = car.Position;
			player.Visible = true;
			player.animate_move(
				player.pos, Thing.MoveAnimation.HOP, duration: 0.5f
			);
			after(0.5f, () => {
				player.explode();
				player.shake();
				game.tell("You're here.");
				game.tell("Let's start.");
				after(2, () => {
					car_key.Visible = true;
					car_key.Position = player.Position;
					car_key.animate_move(
						car_key.pos, Thing.MoveAnimation.HOP, duration: 0.5f
					);
					after(0.5f, () => {
						car_key.explode();
						car_key.shake();
					});
				});
			});
			unlock_input();
		};
	}
	
	
	// Map...

	void load_level() {
		assuming (things.Count == 0);
		var count = thing_parent.GetChildCount();

		for (int i = 0; i < count; i++) {
			var node = thing_parent.GetChild(i);
			var thing = node as Thing;
			assuming (thing != null);
			var pos = world_to_grid(thing.Position);
			assuming (!things.ContainsKey(pos));
			thing.pos = pos;
			things.Add(pos, thing);
			
			if (thing is Player _player)
				player = _player;
		}
	}

	public static Vector2I world_to_grid(Vector2 world_pos) =>
		(Vector2I)world_pos / tile_width;
	public static Vector2 grid_to_world(Vector2I grid_pos) =>
		grid_pos * tile_width;

	public void _try_unset(Vector2I pos) {
		things.Remove(pos);
	}
	
	public void _unset(Vector2I pos) {
		assuming (things.ContainsKey(pos));
		things.Remove(pos);
		after_tile_content_changed(pos);
	}
	
	public void _set(Vector2I pos, Thing new_thing) {
		assuming (!things.ContainsKey(pos));
		things.Add(pos, new_thing);
		after_tile_content_changed(pos);
	}

	public void add_thing(Thing thing, Vector2I pos) {
		thing_parent.AddChild(thing);
		thing.pos = pos;
		thing.Position = grid_to_world(pos);
		_set(pos, thing);
	}

	public bool has_thing(Vector2I pos) => things.ContainsKey(pos);
	
	
	// Input...

	public override void _UnhandledInput(InputEvent e) {
		if (e is InputEventKey action) {
			if (action.IsAction(LEFT))
				is_left_down = action.Pressed;
			else if (action.IsAction(RIGHT))
				is_right_down = action.Pressed;
			else if (action.IsAction(UP))
				is_up_down = action.Pressed;
			else if (action.IsAction(DOWN))
				is_down_down = action.Pressed;
			else if (action.IsAction(PAUSE) && action.Pressed == false)
				toggle_paused();
			maybe_do_turn();
		}
	}

	void end_turn() {
		reevaluate_circles();
		GD.Print($"End of turn {turn}");
	}

	void maybe_do_turn() {
		if (input_locks != 0)
			return;
			
		var dir =
			is_left_down ? WEST :
			is_right_down ? EAST :
			is_up_down ? NORTH :
			is_down_down ? SOUTH : -1;
		if (dir != -1)
			do_turn(dir);
		else if (!has_finished_staring)
			maybe_stare(last_dir);
	}

	void do_turn(int dir) {
		stop_staring();
		has_finished_staring = false;
		turn += 1;
		push(player, dir);
		last_dir = dir;
	}

	public void lock_input() {
		input_locks += 1;
		update_more_arrow();
	}
	public void unlock_input() {
		assuming (input_locks > 0);
		input_locks -= 1;
		if (input_locks == 0) {
			end_turn();
			maybe_do_turn();
		}
		update_more_arrow();
	}
	public void lock_input_for(Tween tween) {
		if (tween == null || !tween.IsRunning())
			return;
		lock_input();
		tween.Finished += unlock_input;
	}
	
	
	// Pushing...

	void push(Thing source, int dir) {
		var e = new PushEvent {
			current = source,
			source = source,
			dir = dir,
			yield_points = int.MinValue
		};
		
		var step = ONE_STEP[dir];
		var start = source.pos;
		var pos = start;
		var current_thing = source;
		Thing pushed_by = null;
		
		for (;;) {
			var neigh_pos = pos + step;
			bool has_neigh = things.TryGetValue(neigh_pos, out var pushed_into);
			current_thing._push(e, pushed_by, has_neigh ? pushed_into : null);
			
			if (e.is_blocked) {
				if (e.yield == null) {
					// Push failed because it's blocked and nothing would yield
					source.dunk(dir);
					after(smack_delay, () => {
						if (IsInstanceValid(current_thing))
							current_thing.shake();
					});
					
					return;
				}
				var yielder_pos = e.yielder.pos;
				e.yield(e);
				assuming (!game.has_thing(yielder_pos));
				break;
			}

			bool has_self = things.ContainsKey(pos);
			if (!has_self || !has_neigh)
				break;

			pos = neigh_pos;
			pushed_by = current_thing;
			current_thing = pushed_into;
			e.current = current_thing;
		}
		
		_actually_push(start, e);
	}

	void _actually_push(Vector2I start, PushEvent e) {
		var step = ONE_STEP[e.dir];

		// Find the last item's position...
		var last_pos = start;
		while (things.ContainsKey(last_pos + step))
			last_pos += step;
		
		// Go backwards from the last position, moving each item...
		var pushed_things = new List<Thing>();
		var pos = last_pos;
		while (pos != start) {
			var thing = things[pos];
			thing.move(pos + step);
			pushed_things.Add(thing);
			pos -= step;
		}
		if (things.TryGetValue(start, out var source) && source == e.source) {
			source.move(start + step, Thing.MoveAnimation.HOP);
		}
		
		// Inform all items about the successful push...
		foreach (var thing in pushed_things)
			thing._after_successful_push(e);
		e.source._after_successful_push(e);
	}
	
	
	// Staring...

	void maybe_stare(int dir) {
		if (is_staring)
			return;
		is_staring = true;
		
		var e = new StareEvent {
			dir = dir,
			turns = 0,
			source = player
		};

		var neigh_pos = player.pos + ONE_STEP[dir];
		if (things.TryGetValue(neigh_pos, out var neigh))
			do_stare_turn();
		else finish_staring();

		
		void do_stare_turn(bool do_turn = false) {
			ellipsis.Visible = false;
			player.stop_rummaging();
			e.next_ponder_time = default_ponder_time;
			e.will_continue = true;
			if (do_turn) {
				turn += 1;
				e.turns += 1;
			}
			neigh._stare(e);
			if (e.will_continue) {
				wait_for_next_stare_turn();
				update_more_arrow();
			} else finish_staring();
		}


		void finish_staring() {
			is_staring = false;
			has_finished_staring = true;
			update_more_arrow();
		}
		
		void wait_for_next_stare_turn() {
			try_ponder(() => {
				var tiny_delay = Math.Min(e.next_ponder_time, 0.05f);
				var third = (e.next_ponder_time - tiny_delay) / 3;
				if (e.state == StareState.RUMMAGE)
					player.start_rummaging();
				
				GodotUtil.create_tween(ref ponder_timer, player);
				ponder_timer.TweenInterval(tiny_delay);
				ponder_timer.TweenCallback(Callable.From(() => {
					ellipsis.Visible = true;
					ellipsis.Frame = 0;
				}));
				ponder_timer.TweenInterval(third);
				ponder_timer.TweenProperty(ellipsis, "frame", 1, 0);
				ponder_timer.TweenInterval(third);
				ponder_timer.TweenProperty(ellipsis, "frame", 2, 0);
				ponder_timer.TweenInterval(third);
				ponder_timer.TweenCallback(Callable.From(
					() => do_stare_turn(true)
				));
			});
		}
	}

	void stop_staring() {
		ponder_timer?.Kill();
		ellipsis.Visible = false;
		player.stop_rummaging();
		ponder_callback = null;
		is_staring = false;
		update_more_arrow();
	}

	void try_ponder(Action callback) {
		ponder_callback = callback;
		maybe_ponder();
	}

	public void lock_ponder() {
		ponder_locks += 1;
		update_more_arrow();
	}
	public void unlock_ponder() {
		assuming (ponder_locks > 0);
		ponder_locks -= 1;
		maybe_ponder();
		update_more_arrow();
	}

	void maybe_ponder() {
		if (ponder_locks != 0 || ponder_callback == null)
			return;
		lock_ponder();
		var current_ponder = ponder_callback;
		ponder_callback = null;
		current_ponder();
		unlock_ponder();
	}
	
	
	// Interface...

	public void tell(string message, bool allow_pondering = false) {
		add_message(new() {
			text = message,
			allow_pondering = allow_pondering
		});
	}

	public void gameover_message(string message) {
		add_message(new() {
			text = message,
			does_not_time_out = true
		});
	}
	
	public void add_message(Message message) {
		if (turn > last_message_turn) {
			for (int i = next_message; i < messages_this_turn.Count; i++)
				if (!messages_this_turn[i].allow_pondering)
					unlock_ponder();
			messages_this_turn.Clear();
			next_message = 0;
			last_message_turn = turn;
			if (message_timeout != null)
				message_timeout.TimeLeft = 0;
		}
		if (!message.allow_pondering)
			lock_ponder();
		messages_this_turn.Add(message);
		try_update_message_box();
	}

	void try_update_message_box() {
		if (is_showing_message)
			return;
		var is_visible = (next_message < messages_this_turn.Count);
		text_box_panel.Visible = is_visible;
		if (!is_visible)
			return;

		var message = messages_this_turn[next_message];
		_show_message(message);
		
		next_message += 1;
		update_more_arrow();
	}

	void _show_message(Message message) {
		is_showing_message = true;
		var letters = message.text.Length;
		var duration = 1 + 0.05f * letters;
		// If the old message is the same as the new message, it breaks the
		// typing effect, so we clear the text first.
		text_box.Text = "";
		text_box.Text = $"[type]{message.text}[/type]";

		if (!message.does_not_time_out) {
			message_timeout = GetTree().CreateTimer(duration);
			message_timeout.Timeout += () =>  {
				if (!IsInstanceValid(this))
					return;
				if (!message.allow_pondering)
					unlock_ponder();
				is_showing_message = false;
				try_update_message_box();
			};
		}
	}

	void update_more_arrow() {
		more_arrow.Visible = (is_staring && !has_finished_staring
				|| next_message != messages_this_turn.Count);
	}
	
	
	// Animation...

	public void after(float delay, Action action) {
		game.lock_input();
		GetTree().CreateTimer(delay).Timeout += () =>  {
			if (!IsInstanceValid(this))
				return;
			action();
			unlock_input();
		};
	}
	
	
	// Trails and circles...

	public void add_trail(Vector2I pos, int dir) {
		trails.add_trail(pos, dir);
		queue_reevaluate_circles_around(pos);
	}

	public void after_tile_content_changed(Vector2I pos) =>
		queue_reevaluate_circles_around(pos);

	void queue_reevaluate_circles_around(Vector2I pos) {
		// If it's not on a trail, it's not part of a circle.
		// Don't bother reevaluating.
		if (!trails.get(pos, EAST) && !trails.get(pos, SOUTH)
		    && !trails.get(pos, WEST))
			return;
		
		for (int y = -1; y < 2; y++)
			for (int x = -1; x < 2; x++)
				pending_reevaluations.Add(pos + new Vector2I(x, y));
	}

	void reevaluate_circles() {
		// Reevaluate...
		bool had_circle =
			try_get_best_circle(out var old_pos, out var old_circle);
		foreach (var pos in pending_reevaluations)
			reevaluate_circle(pos);
		bool has_circle =
			try_get_best_circle(out var new_pos, out var new_circle);
		pending_reevaluations.Clear();
		
		// Remove embers...
		if (old_pos != new_pos)
			unlight_circle();
		
		// Notify the player when he creates and destroys a circle...
		assuming (has_had_circle || !had_circle);
		if (has_circle && !has_had_circle) {
			game.tell("That's a pretty decent looking circle you've made!");
			game.tell("Maybe a bit of a square circle... Good enough!");
			has_had_circle = true;
		} else if (has_circle && !had_circle) {
			game.tell("You draw another circle.");
		} else if (had_circle && !has_circle) {
			game.tell("You ruined your perfectly good circle.");
		}

		if (!has_circle)
			return;
		
		// Notify the player about the candles...
		bool had_candles = had_circle && old_circle.has_candles;
		bool were_candles_lit = had_circle && old_circle.are_candles_lit;
		bool now_has_candles = new_circle.has_candles && !had_candles;
		bool now_has_lit_candles =
			new_circle.are_candles_lit && !were_candles_lit;
		
		var lit = (now_has_lit_candles ? " lit" : "");
		if (now_has_candles && !has_had_candles) {
			game.tell($"You have a{lit} candle in every corner of your"
				+ $" square-ish circle. Looking good!");
			has_had_candles = true;
		} else if (now_has_candles) {
			game.tell($"You put the{lit} candles back in the corners.");
		} else if (now_has_lit_candles) {
			game.tell("You've lit the candles.");
		}
		
		// Embers...
		bool deserves_embers =
			new_circle.are_candles_lit && new_circle.summon != Summon.NO_SUMMON;
		if (deserves_embers)
			light_up_circle(new_pos);
		else unlight_circle();
	}

	void light_up_circle(Vector2I pos) {
		bool was_lit = (ominous_effect != null);
		unlight_circle();
		if (!was_lit)
			game.tell("Embers are wavering over the summoning circle.");
		ominous_effect = packed_ominous_effect.Instantiate<GpuParticles2D>();
		ominous_effect.Position = grid_to_world(pos);
		effects_parent.AddChild(ominous_effect);
	}

	void unlight_circle() {
		if (ominous_effect == null)
			return;
		ominous_effect.Emitting = false;
		ominous_effect.Finished += ominous_effect.QueueFree;
		ominous_effect = null;
	}

	void reevaluate_circle(Vector2I pos) {
		if (try_get_circle(pos, out var new_circle))
			circles[pos] = new_circle;
		else circles.Remove(pos);
	}

	bool try_get_circle(Vector2I pos, out Circle new_circle) {
		new_circle = default;
		
		// Check the trails...
		var p = pos - Vector2I.One;
		for (var dir = EAST; dir < 4; dir++) {
			var inwards = (dir + 1) % 4;
			var q = p + ONE_STEP[dir];
			if (!trails.get(p, dir) || !trails.get(q, dir)
			    || trails.get(q, inwards))
				return false;
			p = q + ONE_STEP[dir];
		}

		// Check for candles...
		bool has_candles = true;
		bool are_candles_lit = true;
		
		for (var dir = EAST; dir < 4; dir++) {
			var next_dir = (dir + 1) % 4;
			var candle_pos = pos + ONE_STEP[dir] + ONE_STEP[next_dir];
			if (!things.TryGetValue(candle_pos, out var hopefully_candle)
			    || (hopefully_candle is not Candle
				    && hopefully_candle is not CandlePair))
				has_candles = false;
			else if (!FireHelper.is_lit(hopefully_candle))
				are_candles_lit = false;
		}
		
		// Check offerings...
		bool has_book = false;
		var offerings = new List<Thing>();
		
		for (int dir = 0; dir < 4; dir++) {
			if (!things.TryGetValue(pos + ONE_STEP[dir], out var thing))
				continue;
			if (thing is Tome)
				has_book = true;
			else if (thing is not Player)
				offerings.Add(thing);
		}
		
		var summon = Summon.NO_SUMMON;
		if (offerings.Count == 3) {
			if (has_succubus_offerings(offerings))
				summon = Summon.SUCCUBUS;
			else if (has_imp_offerings(offerings))
				summon = Summon.IMP;
		}
		
		// Center...
		bool is_center_empty = !has_thing(pos);
		
		// Success!
		new_circle = new() {
			has_candles = has_candles,
			are_candles_lit = has_candles && are_candles_lit,
			has_book = has_book,
			summon = summon,
			is_center_empty = is_center_empty
		};
		return true;
	}

	static bool has_succubus_offerings(List<Thing> offerings) =>
		has<WoodCarving>(offerings) && has<Car>(offerings)
		&& has<DeerCorpse>(offerings);

	static bool has_imp_offerings(List<Thing> offerings) {
		int count = 0;
		if (has<DeerCorpse>(offerings))
			count += 1;
		if (try_get<Cup>(offerings, out var cup) && cup.state == Cup.State.SODA
		    || has<Soda>(offerings))
			count += 1;
		if (has<Cracker>(offerings) || has<CrackerPacket>(offerings)
		    || has<Crackers>(offerings))
			count += 1;
		if (has<WoodCarving>(offerings))
			count += 1;
		return count == 3;
	}

	static bool has<T>(List<Thing> things) {
		foreach (var thing in things)
			if (thing is T)
				return true;
		return false;
	}

	static bool try_get<T>(List<Thing> things, out T result) {
		foreach (var thing in things) {
			if (thing is T _result) {
				result = _result;
				return true;
			}
		}
		result = default;
		return false;
	}

	public bool try_get_best_circle(out Vector2I pos, out Circle best_circle) {
		if (circles.Count == 0) {
			best_circle = new();
			pos = Vector2I.Zero;
			return false;
		}

		(pos, best_circle) = circles.First();
		
		foreach (var (p, circle) in circles) {
			bool c1 = circle.has_candles && !best_circle.has_candles;
			bool c2 = circle.are_candles_lit && !best_circle.are_candles_lit;
			if (c1 || circle.has_candles && c2)
				(pos, best_circle) = (p, circle);
		}

		return true;
	}
	

	// Game over...

	public bool try_game_over() {
		if (is_game_over)
			return false;
		is_game_over = true;
		lock_input();
		return true;
	}

	public void knife_ending() {
		if (!try_game_over())
			return;
		player.shake(duration: 2);
		after(2, () => {
			player.explode();
			player.shake();
			player.set_dead();
			after(2, () => tell("You bled out..."));
			after(10, () => {
				gameover_message(
					"GAME OVER. You died after impaling yourself on a knife."
				);
				after(10, () => {
					GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
				});
			});
		});
	}

	public void wolf_ending() {
		if (!try_game_over())
			return;
		player.shake(duration: 2);
		after(2, () => {
			player.explode();
			player.shake();
			player.set_dead();
			after(2, () =>
				tell("The wolf bites you. You die in great pain..."));
			after(10, () => {
				gameover_message(
					"GAME OVER. You were killed by an off-screen wolf."
				);
				after(10, () => {
					GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
				});
			});
		});
	}

	public void succubus_ending() {
		if (!try_game_over())
			return;
		var tween = start_summoning_fade_away();
		tween.Finished += () => GetTree().ChangeSceneToFile(
			"res://scenes/cutscenes/succubus_ending.tscn"
		);
	}
	
	public void imp_ending() {
		if (!try_game_over())
			return;
		var tween = start_summoning_fade_away();
		tween.Finished += () => GetTree().ChangeSceneToFile(
			"res://scenes/cutscenes/imp_ending.tscn"
		);
	}

	public Tween start_summoning_fade_away(
		float duration = 8, float strength = 5
	) {
		var tween = CreateTween();
        fade_away.Visible = true;
        fade_away.SelfModulate = Colors.Transparent;
        
		tween.TweenMethod(Callable.From<float>(t => {
			var range = t * strength;
			var x = (float)GD.RandRange(-range, range);
			var y = (float)GD.RandRange(-range, range);
			world.Position = new(x, y);

			var fade = Math.Clamp((t - 0.6f) / 0.4f, 0, 1);
			var fade_color = Colors.White;
			fade_color.A = fade;
			fade_away.SelfModulate = fade_color;
		}), 0f, 1f, duration);
		
		return tween;
	}
	
	
	// Pause menu...

	void toggle_paused() {
		set_paused(!is_paused);
	}

	void set_paused(bool new_is_paused) {
		if (new_is_paused && !is_paused)
			lock_input();
		else if (!new_is_paused && is_paused)
			unlock_input();
		is_paused = new_is_paused;
		pause_menu.Visible = new_is_paused;
	}

	void init_pause_menu() {
		ProcessMode = ProcessModeEnum.Always;
		var continue_button = this.get_node<Button>("%ContinueButton");
		var restart_button = this.get_node<Button>("%RestartButton");
		continue_button.Pressed += () => set_paused(false);
		restart_button.Pressed +=
			() => GetTree().ChangeSceneToFile("res://scenes/hacky.tscn");
		set_paused(is_paused);
	}
}

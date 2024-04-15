using System;

public class PushEvent {
    public int dir;
    public Thing source;
    public Thing current;
    public Thing yielder;
    public Action<PushEvent> yield;
    public int yield_points = 0;
    public bool is_blocked { get; private set; }

    public void block() => is_blocked = true;
    public void add_yielder(
        Thing thing, int new_yield_points, Action<PushEvent> new_yield
    ) {
        if (new_yield_points > yield_points) {
            yield_points = new_yield_points;
            yielder = thing;
            yield = new_yield;
        }
    }
}
	
public enum StareState { PONDER, RUMMAGE }

public class StareEvent {
    public int dir;
    public int turns;
    public StareState state;
    public Thing source;
    public bool will_continue;
    public float next_ponder_time = Game.default_ponder_time;

    public void keep_staring(
        StareState new_state = StareState.PONDER,
        float ponder_time = Game.default_ponder_time
    ) {
        will_continue = true;
        state = new_state;
        next_ponder_time = ponder_time;
    }

    public void stop_staring() =>
        will_continue = false;
}

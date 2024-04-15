
using System;
using System.Collections.Generic;

public class StareHelper {
    class Turn {
        public Action<StareEvent> action;
        public string message;
        public bool should_rummage;
        public bool allow_pondering;
        public float time;
    }

    List<Turn> turns = new();


    public void add(
        string message = null, Action<StareEvent> action = null,
        bool rummage = false, bool ponder = false,
        float time = Game.default_ponder_time
    ) {
        turns.Add(new() {
            message = message, action = action,
            should_rummage = rummage, allow_pondering = ponder,
            time = time
        });
    }
    
    public void handle(StareEvent e, Action on_new_stare) {
        if (e.turns == 0) {
            turns.Clear();
            on_new_stare();
            if (e.turns >= turns.Count)
                e.stop_staring();
            return;
        }

        var turn = turns[e.turns - 1];
        if (turn.message != null)
            Game.game.tell(turn.message, turn.allow_pondering);
        if (turn.action != null)
            turn.action(e);
        
        if (e.turns >= turns.Count)
            e.stop_staring();
        else {
            var next_turn = turns[e.turns];
            var state = (next_turn.should_rummage ?
                StareState.RUMMAGE : StareState.PONDER);
            e.keep_staring(state, next_turn.time);
        }
    }
}
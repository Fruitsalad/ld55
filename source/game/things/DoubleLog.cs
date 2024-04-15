using Godot;
using static Game;
using static Util;

public partial class DoubleLog : Thing, MaybeBurningThing {
    static PackedScene log =
        GD.Load<PackedScene>("res://scenes/things/log.tscn");
    static PackedScene log_wall =
        GD.Load<PackedScene>("res://scenes/things/log_wall.tscn");
    
    
    public FireHelper fire = new();
    public bool is_burning() => fire.is_burning;
    

    public override void _push(PushEvent e, Thing by, Thing into) {
        if (into is Log) {
            bool is_new_burning = (is_burning() || FireHelper.is_lit(into));
            var log2 = combine_into(into, log_wall) as LogWall;
            if (is_new_burning) {
                log2.fire.start_burning(log2);
                game.tell("You toss the log onto the pile of logs and get "
                    + "a burning log wall for your hard work.");
            } else game.tell("You toss the log onto the pile of logs. "
                + "The logs crystallize together and turn into a sturdy wall.");
        }
        if (FireHelper.is_lit(by, into))
            fire.start_burning(this);
        if (are_surrounding_tiles_free(2))
            e.add_yielder(this, DOUBLE_LOG_YIELD, yield);
    }

    void yield(PushEvent e) {
        var log1 = log.Instantiate<Log>();
        var log2 = log.Instantiate<Log>();
        toss_nearby(log1);
        toss_nearby(log2);
        if (is_burning()) {
            log1.fire.start_burning(log1);
            log2.fire.start_burning(log2);
        }
        immediately_disappear();
        
        game.tell("You slam into the pile of logs, breaking the pile apart.");
    }
}
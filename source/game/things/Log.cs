using Godot;
using static Game;

public partial class Log : Thing, MaybeBurningThing {
    static PackedScene double_log =
        GD.Load<PackedScene>("res://scenes/things/double_log.tscn");
    static PackedScene log_wall =
        GD.Load<PackedScene>("res://scenes/things/log_wall.tscn");
    static PackedScene ash =
        GD.Load<PackedScene>("res://scenes/things/ash.tscn");
    static PackedScene wood_carving =
        GD.Load<PackedScene>("res://scenes/things/wood_carving.tscn");
    
    
    public FireHelper fire = new();
    public bool is_burning() => fire.is_burning;
    

    public override void _push(PushEvent e, Thing by, Thing into) {
        if (!is_burning() && FireHelper.is_lit(by, into)) {
            fire.start_burning(this);
            shake();
        }
        if (into is Log) {
            bool is_new_burning = (is_burning() || FireHelper.is_lit(into));
            var result = combine_into(into, double_log) as DoubleLog;
            if (is_new_burning)
                result.fire.start_burning(result);
            game.tell("You toss the log onto the other log.");
        } else if (into is DoubleLog) {
            bool is_new_burning = (is_burning() || FireHelper.is_lit(into));
            var log2 = combine_into(into, log_wall) as LogWall;
            if (is_new_burning) {
                log2.fire.start_burning(log2);
                game.tell("You toss the log onto the pile of logs and get "
                    + "a burning log wall for your hard work.");
            } else game.tell("You toss the log onto the pile of logs. "
                + "The logs crystallize together and turn into a sturdy wall.");
        } else if (by is Knife
                   && (e.dir == Dir2D.NORTH || e.dir == Dir2D.WEST)) {
            var was_burning = is_burning();
            var result = turn_into(wood_carving) as WoodCarving;
            game.tell("You carve the log into a sculpture of yourself.");
            if (was_burning)
                result.fire.start_burning(result);
            e.block();
        } else if (is_burning() && are_surrounding_tiles_free(1))
            e.add_yielder(this, BURNING_LOG_YIELD, yield);
    }

    void yield(PushEvent e) {
        explode();
        toss_nearby(ash);
        immediately_disappear();
        game.tell("The log turns to ash.");
    }
}
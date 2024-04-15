using Godot;
using static Game;

public partial class MatchBox : Thing {
    static PackedScene match =
        GD.Load<PackedScene>("res://scenes/things/match.tscn");
    
    StareHelper stare = new();
    
    
    public override void _stare(StareEvent e) => stare.handle(e, () => {
        stare.add("A match box.");
        if (!e.source.are_surrounding_tiles_free(1))
            stare.add("There isn't enough space to grab a match.");
        stare.add("You reach into the match box...", ponder: true, time: 0);
        stare.add(rummage: true, action: _ => {
            game.tell("You grab a match.");
            e.source.toss_nearby(match);
        });
    });
}
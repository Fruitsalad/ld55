using Godot;
using static Game;
using static Util;

public partial class Tome : Thing {
    [Export] Texture2D tex_normal;
    [Export] Texture2D tex_open;
    
    StareHelper stare = new();
    bool is_open = false;

    bool has_seen_circle_msg;
    bool has_seen_candles_msg;
    bool has_seen_lit_msg;
    
    
    public override void _stare(StareEvent e) => stare.handle(e, () => {
        stare.add("It's your tome.");
        if (!is_open) {
            stare.add("You open up your tome...", ponder: true, time: 0);
            stare.add(action: _ => open(), rummage: true);
            stare.add("You start reading...", ponder: true, time: 0);
        } else {
            var uhhh = pick("Uhmmm", "Uhhh", "Uhh");
            stare.add($"{uhhh}...", ponder: true, time: 0);
        }

        if (!game.try_get_best_circle(out _, out var circle)) {
            if (!has_seen_circle_msg) {
                stare.add("It says \"He who wisheth to summon creatures of"
                    + " the unearthly must heed my words:\"");
                stare.add("\"Firstly, thy must draw a summoning circle"
                    + " of fresh blood...\"", time: 0);
                stare.add(action: _ => has_seen_circle_msg = true, time: 0);
                stare.add("...", time: 0);
                stare.add("He goes into boring specifics, but drawing a circle"
                    + " is probably a good start.", time: 0);
                stare.add("He doesn't say how big it needs to be...", time: 0);
            } else {
                stare.add("You need to draw a summoning circle with fresh"
                    + " blood.", time: 1f);
                stare.add("It isn't specified how big it needs to be, but you"
                    + " can figure it out yourself.", time: 0);
            }
        } else if (!circle.has_candles) {
            if (!has_seen_candles_msg) {
                stare.add("It says you need \"a summoning circle of fresh"
                    + " blood, adorned with candles\".");
                stare.add(action: _ => has_seen_candles_msg = true, time: 0);
                stare.add("You should probably get some candles...", time: 0);
                stare.add("Just try placing them somewhere on the circle until"
                    + " it feels right?", time: 0);
            } else {
                stare.add("Just need to place candles somewhere on the circle"
                    + " until it feels right.", time: 1f);
            }
        } else if (!circle.are_candles_lit) {
            if (!has_seen_lit_msg) {
                stare.add("It says that the candles need to be burning.");
                stare.add(action: _ => has_seen_lit_msg = true, time: 0);
                stare.add("You should probably light the candles...", time: 0);
            } else {
                stare.add("You need to light the candles.", time: 1f);
            }
        } else if (circle.summon == Summon.NO_SUMMON) {
            stare.add(
                "It says you need to put three offerings around the circle."
                + " Your offerings have to be different items."
            );
            stare.add("The book mentions the succubus Naamah, who demands these"
                + " three offerings: One symbol of wealth, one symbol of"
                + " strength and one symbol of beauty.", time: 0);
        } else if (!circle.has_book) {
            stare.add("At this point you can start reading the words and"
                + " request the presence of the creature.");
            stare.add("Just put the book in the right spot and"
                + " start reading.", time: 0);
        } else if (!circle.is_center_empty) {
            stare.add("You should probably clear the center of the circle.");
            stare.add("That's where the summoned creature"
                + " is supposed to appear.", time: 0);
        } else if (circle.summon == Summon.SUCCUBUS) {
            stare.add("You start saying stuff...");
            stare.add("O beautiful Naamah! Hear me!", time: 0);
            stare.add("I have prepared all that you ask for!", time: 0);
            stare.add("A symbol of beauty: A wood carving of the most handsome"
                + " of the land!", time: 0);
            stare.add("A symbol of wealth: A Toyota Aygo in all of"
                + " its splendor and opulence!", time: 0);
            stare.add("A symbol of strength: A deer defeated in honorable"
                + " battle!", time: 0);
            stare.add(action: _ => game.lock_input(), time: 0);
            stare.add("Appear, o Naamah! Appear before me!", time: 0);
            stare.add(action: _ => game.succubus_ending(), time: 0);
        } else if (circle.summon == Summon.IMP) {
            stare.add("You're pretty sure whatever these offerings are"
                + " attracting is not a succubus.");
            stare.add("Unless it's a very not-classy succubus.", time: 0);
            stare.add("Nevertheless, you start saying stuff...", time: 0);
            stare.add("O creature of the unknown! Hear me!", time: 0);
            stare.add("I have prepared all that you see!", time: 0);
            stare.add("Crackers! Soda!", time: 0);
            stare.add("I have all this and much more to offer!", time: 0);
            stare.add(action: _ => game.lock_input(), time: 0);
            stare.add("Appear, creature! Show yourself to me!", time: 0);
            stare.add(action: _ => game.imp_ending(), time: 0);
        } else {
            stare.add($"TODO: You summon {circle.summon}");
            stare.add("You shouldn't be able to read this :(", time: 0);
        }
    });

    void open() {
        shake(strength: 0.2f);
        Texture = tex_open;
        is_open = true;
    }
}
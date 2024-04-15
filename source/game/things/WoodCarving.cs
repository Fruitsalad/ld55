using static Game;

public partial class WoodCarving : Thing {
    public FireHelper fire = new();
    public bool is_burning() => fire.is_burning;


    StareHelper stare = new();

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        if (is_burning()) {
            stare.add("It's a wood carving of what you would look like if"
                + " you were on fire.");
            stare.add("Looking hot!", time: 0);
        } else {
            stare.add("It's a wood carving of yourself.");
            stare.add("Looking handsome!", time: 0);
        }
    });

    public override void _push(PushEvent e, Thing by, Thing into) {
        if (!is_burning() && FireHelper.is_lit(by, into)) {
            fire.start_burning(this);
            game.tell("You light your sculpture on fire.");
        }
    }
}
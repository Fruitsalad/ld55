using static Game;

public partial class LogWall : Thing {
    public FireHelper fire = new();
    public bool is_burning() => fire.is_burning;


    StareHelper stare = new();

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        if (is_burning()) {
            stare.add("It's a burning wall.");
            stare.add("It looks like it's just going to keep on burning.",
                time: 0);
        } else {
            stare.add("It's a wall.");
            stare.add("I'm not sure what the point of it is, but it's there.",
                time: 0);
        }
    });

    public override void _push(PushEvent e, Thing by, Thing into) {
        if (!is_burning() && FireHelper.is_lit(by)) {
            fire.start_burning(this);
            game.tell("You light your log wall on fire.");
        }
        e.block();
    }
}
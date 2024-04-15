using static Game;

public partial class DeerCorpse : Thing {
    StareHelper stare = new();
    int push_count = 0;

    public override void _stare(StareEvent e) => stare.handle(e, () => {
        stare.add("It's the corpse of a deer.");
        if (push_count < 10)
            stare.add("It's still bleeding.", time: 0);
        else stare.add(
            "It's somehow still bleeding, despite how much blood it has"
            + " already lost.", time: 0
        );
    });
    
    public override void _after_successful_push(PushEvent e) {
        push_count += 1;
        game.after(0.15f, () => game.add_trail(pos, Dir2D.get_opposite(e.dir)));
    }
}
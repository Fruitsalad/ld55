public partial class CarKey : Thing {
    public override void _push(PushEvent e, Thing by, Thing into) {
        if (into is Car car && e.dir == Dir2D.WEST) {
            animate_move(into.pos, MoveAnimation.HOP);
            disappear();
            car.unlock_trunk();
        }
    }
}
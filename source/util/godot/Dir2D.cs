using Godot;
using static Util;


public class Dir2D {
    public const int EAST = 0;
    public const int SOUTH = 1;
    public const int WEST = 2;
    public const int NORTH = 3;
    public const int COUNT = 4;
    
    public static Vector2I[] ONE_STEP = {
        Vector2I.Right, Vector2I.Down, Vector2I.Left, Vector2I.Up
    };
    public static bool is_valid(int dir) => dir >= 0 && dir < 4;
    
    public static int get_opposite(int dir) {
        return (dir + 2) % 4;
    }

    public static Vector2I get_corner_offset(int i) => (i % 4) switch {
        EAST => new(1, 0),
        SOUTH => new(1, 1),
        WEST => new(0, 1),
        _ => new(0, 0)
    };
    
    public static Vector2I get_corner_offset__y_up(int i) => (i % 4) switch {
        EAST => new(1, 1),
        SOUTH => new(1, 0),
        WEST => new(0, 0),
        _ => new(0, 1)
    };

    public static int round_to_dir2d(float radian_angle) {
        var full = 2 * Mathf.Pi;
        var eighth = Mathf.Pi / 4f;
        var quarter = Mathf.Pi / 2f;
        radian_angle = (full - radian_angle) % full;
        
        if (radian_angle < eighth)
            return EAST;
        if (radian_angle < quarter + eighth)
            return SOUTH;
        if (radian_angle < 2*quarter + eighth)
            return WEST;
        if (radian_angle < 3*quarter + eighth)
            return NORTH;
        return EAST;
    }
}
using System.Collections.Generic;
using Godot;
using static Dir2D;


public partial class Trails : Node2D {
    [Export] Texture2D tex_end;
    [Export] Texture2D tex_straight;
    [Export] Texture2D tex_corner;
    [Export] Texture2D tex_split;
    [Export] Texture2D tex_crossing;
    
    HashSet<Vector2I> eastern_trails = new();
    HashSet<Vector2I> southern_trails = new();
    Dictionary<Vector2I, Sprite2D> sprites = new();
    

    public void add_trail(Vector2I pos, int dir) {
        if (try_get_east_pos(pos, dir, out var p))
            add_trail_east(p);
        else add_trail_south(p);
    }

    public bool get(Vector2I pos, int dir) {
        if (try_get_east_pos(pos, dir, out var p))
            return eastern_trails.Contains(p);
        return southern_trails.Contains(p);
    }

    bool try_get_east_pos(Vector2I pos, int dir, out Vector2I p) {
        if (dir == EAST || dir == WEST) {
            p = (dir == EAST ? pos : pos + ONE_STEP[WEST]);
            return true;
        }
        p = (dir == SOUTH ? pos : pos + ONE_STEP[NORTH]);
        return false;
    }

    public void add_trail_east(Vector2I pos) {
        eastern_trails.Add(pos);
        refresh_trails(pos);
        refresh_trails(pos + ONE_STEP[EAST]);
    }
	
    public void add_trail_south(Vector2I pos) {
        southern_trails.Add(pos);
        refresh_trails(pos);
        refresh_trails(pos + ONE_STEP[SOUTH]);
    }

    void refresh_trails(Vector2I pos) {
        const float quarter_turn = Mathf.Pi/2f;
        var sprite = get_or_create_sprite(pos);
        var dirs = new bool[4];
        int count = 0;
        int first_dir = -1;

        for (int i = 0; i < 4; i++) {
            dirs[i] = get(pos, i);
            if (dirs[i]) {
                if (first_dir == -1)
                    first_dir = i;
                count += 1;
            }
        }
        
        sprite.Visible = true;
        if (count == 1) {
            sprite.Texture = tex_end;
            sprite.Rotation = first_dir * quarter_turn;
        } else if (count == 2) {
            bool is_corner = dirs[first_dir + 1];
            if (first_dir == 0 && dirs[3]) {
                is_corner = true;
                first_dir = 3;
            }
            sprite.Texture = (is_corner ? tex_corner : tex_straight);
            sprite.Rotation = first_dir * quarter_turn;
        } else if (count == 3) {
            var empty_dir = 0;
            for (int i = 0; i < 4; i++)
                if (!dirs[i])
                    empty_dir = i;
            sprite.Texture = tex_split;
            sprite.Rotation = empty_dir * quarter_turn;
        } else if (count == 4) {
            sprite.Texture = tex_crossing;
        } else sprite.Visible = false;
    }

    Sprite2D get_or_create_sprite(Vector2I pos) {
        if (sprites.TryGetValue(pos, out var sprite))
            return sprite;
        var new_sprite = new Sprite2D();
        AddChild(new_sprite);
        new_sprite.Position = Game.grid_to_world(pos);
        sprites.Add(pos, new_sprite);
        return new_sprite;
    }
}
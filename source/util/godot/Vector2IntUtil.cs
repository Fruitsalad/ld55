using System;
using System.Collections.Generic;
using Godot;
using static Util;
using static System.Math;

public static class Vector2IntUtil {
    public static Vector2I MINIMUM = new(int.MinValue, int.MinValue);
    
    public static int manhattan(Vector2I a, Vector2I b) =>
        Abs(a.X - b.X) + Abs(a.Y - b.Y);


    // Basic math stuff...
    
    // Gives a / b with integer division, except the result is rounded down
    // instead of being rounded towards zero.
    public static Vector2I div_down(Vector2I a, Vector2I b) =>
        new(Util.div_down(a.X, b.X), Util.div_down(a.Y, b.Y));
    public static Vector2I div_up(Vector2I a, Vector2I b) =>
        new(Util.div_up(a.X, b.X), Util.div_up(a.Y, b.Y));
    
    public static Vector2I elem_max(Vector2I a, Vector2I b) =>
        new(Max(a.X, b.X), Max(a.Y, b.Y));
    
    public static Vector2I elem_min(Vector2I a, Vector2I b) =>
        new(Min(a.X, b.X), Min(a.Y, b.Y));

    public static Vector2I elem_clamp(Vector2I a, Vector2I min, Vector2I max) =>
        elem_max(elem_min(a, max), min);
    
    public static (Vector2I min, Vector2I max) elem_minmax(
        Vector2I a, Vector2I b
    ) {
        var x1 = (a.X < b.X ? a.X : b.X);
        var x2 = (a.X < b.X ? b.X : a.X);
        var y1 = (a.Y < b.Y ? a.Y : b.Y);
        var y2 = (a.Y < b.Y ? b.Y : a.Y);
        return (new(x1, y1), new(x2, y2));
    }
    
    // You can find round_to_int, floor_to_int and ceil_to_int in Vector2Util!
    
    // A proper modulo that does what you want it to do with negative numbers.
    public static Vector2I mod(Vector2I a, Vector2I b) => ((a % b) + b) % b;
    public static Vector2I mod(Vector2I a, int b) => mod(a, new Vector2I(b, b));
    
    
    // Rotating...
    
    // Note that clockwise & counter-clockwise is dependent on coordinate system
    // conventions. This is written for Godot's conventions and may not work
    // correctly with different conventions.
    public static Vector2I rotate_90deg_cw(Vector2I a, int rotations) {
        return (modulo(rotations, 4)) switch {
            0 => a,
            1 => new(-a.Y, a.X),
            2 => new(-a.X, -a.Y),
            _ => new(a.Y, -a.X)
        };
    }
    
    public static Vector2I rotate_90deg_ccw(Vector2I a, int rotations) {
        return (modulo(rotations, 4)) switch {
            0 => a,
            1 => new(a.Y, -a.X),
            2 => new(-a.X, -a.Y),
            _ => new(-a.Y, a.X)
        };
    }
    
    
    // Rectangle stuff...
    
    public static Rect2I get_bounding_rect(Vector2I a, Vector2I b) {
        var (min, max) = elem_minmax(a, b);
        return new Rect2I(min, max - min);
    }

    public static IEnumerable<Vector2I> each_pos_in_rect(int w, int h) {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                yield return new(x, y);
    }
    
    public static IEnumerable<Vector2I> each_pos_in_rect(Vector2I size) {
        return each_pos_in_rect(size.X, size.Y);
    }
    
    public static IEnumerable<Vector2I> each_pos_in_rect(Rect2I rect) {
        for (int y = rect.Position.Y; y < rect.End.Y; y++)
            for (int x = rect.Position.X; x < rect.End.X; x++)
                yield return new(x, y);
    }
    
    public static IEnumerable<Vector2I> each_pos_in_rect(
        Vector2I min, Vector2I exmax
    ) {
        for (int y = min.Y; y < exmax.Y; y++)
            for (int x = min.X; x < exmax.X; x++)
                yield return new(x, y);
    }
    
    public static (Rect2I bottom_rect, Rect2I right_rect) get_remainder_rects(
        Rect2I original, Vector2I removed_area_size
    ) {
        var size = original.Size;
        var pos = original.Position;
        assuming (removed_area_size.X <= size.X);
        assuming (removed_area_size.Y <= size.Y);
        
        var a = size - removed_area_size;
        var right_size = new Vector2I(a.X, removed_area_size.Y);
        var bottom_size = new Vector2I(size.X, a.Y);
        var right_pos = pos + new Vector2I(removed_area_size.X, 0);
        var bottom_pos = pos + new Vector2I(0, removed_area_size.Y);
        
        return (new(bottom_pos, bottom_size), new(right_pos, right_size));
    }
    
    public struct RectDiff {
        public Rect2I top;
        public Rect2I right;
        public Rect2I bottom;
        public Rect2I left;
    }
    
    public static RectDiff subtract_rects(Rect2I a, Rect2I b) {
        var a1 = a.Position;
        var a2 = a.End;  // Note that `a2` is not actually inside `a`
        var b1 = b.Position;
        var b2 = b.End;  // Same thing as for `a2`
        var w = a.Size.X;
        var h = a.Size.Y;
        
        var left_width = Clamp(b1.X - a1.X, 0, w);
        var right_width = Clamp(a2.X - b2.X, 0, w);
        var right_pos = a2 - new Vector2I(right_width, h);
        
        var top_width = w - left_width - right_width;
        var top_height = Clamp(b1.Y - a1.Y, 0, h);
        var top_pos = a1 + new Vector2I(left_width, 0);
        
        var bottom_width = top_width;
        var bottom_height = Clamp(a2.Y - b2.Y, 0, h);
        var bottom_pos =
            a2 - new Vector2I(bottom_width + right_width, bottom_height);
        
        return new() {
            left = new Rect2I(a1, left_width, h),
            right = new Rect2I(right_pos, right_width, h),
            top = new Rect2I(top_pos, top_width, top_height),
            bottom = new Rect2I(bottom_pos, bottom_width, bottom_height)
        };
    }

    public static IEnumerable<Vector2I> each_pos_only_in_A(Rect2I a, Rect2I b) {
        var diff = subtract_rects(a, b);
        foreach (var pos in each_pos_in_rect(diff.top))
            yield return pos;
        foreach (var pos in each_pos_in_rect(diff.right))
            yield return pos;
        foreach (var pos in each_pos_in_rect(diff.bottom))
            yield return pos;
        foreach (var pos in each_pos_in_rect(diff.left))
            yield return pos;
    }


    // Grid stuff...
    
    public static IEnumerable<(Vector2I pos, int dir)> each_edge_in_outline(
        HashSet<Vector2I> positions
    ) {
        foreach (var pos in positions) {
            for (int i = 0; i < Dir2D.COUNT; i++) {
                var neighbor = pos + Dir2D.ONE_STEP[i];
                if (positions.Contains(neighbor))
                    continue;
                yield return (pos, i);
            }
        }
    }

}
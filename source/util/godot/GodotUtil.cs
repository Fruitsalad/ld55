using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using static Util;

public static class GodotUtil {
    static public Vector2 viewport_to_global(CanvasItem node, Vector2 view) =>
        node.GetCanvasTransform().AffineInverse() * view;
    
    static public Vector2[] get_viewports_global_corner_points(CanvasItem node) {
        var view_rect = node.GetViewportRect();
        var points = new[] {
            viewport_to_global(node, new(0,0)),
            viewport_to_global(node, new(view_rect.Size.X, 0)),
            viewport_to_global(node, view_rect.Size),
            viewport_to_global(node, new(0,view_rect.Size.Y))
        };
        return points;
    }
    
    static public Rect2 get_viewports_global_bounding_rect(CanvasItem node) {
        var points = get_viewports_global_corner_points(node);
        
        var min = points[0];
        var max = points[0];
        
        for (int i = 1; i < points.Length; i++) {
            var p = points[i];
            if (p.X < min.X)
                min.X = p.X;
            else if (p.X > max.X)
                max.X = p.X;
            if (p.Y < min.Y)
                min.Y = p.Y;
            else if (p.Y > max.Y)
                max.Y = p.Y;
        }
        
        return new(min, max - min);
    }
    

    // Nodes...
    
    public static T get_node<T>(this Node node, NodePath path) where T: class {
        var result = node.GetNode<T>(path);
        assuming (result != null);
        return result;
    }

    // public static T get_module<T>(this Node node, NodePath path)
    //     where T: class => Modules.get<T>(node, path);

    public static void delete_children(Node node) {
        var count = node.GetChildCount();
        for (int i = 0; i < count; i++) {
            var child = node.GetChild(0);
            node.RemoveChild(child);
            child.QueueFree();
        }
    }
    
    // Returns each descendant with type T
    public static IEnumerable<T> each_descendant<T>(
        this Node node
    ) where T: Node {
        var count = node.GetChildCount();
        
        for (int i = 0; i < count; i++) {
            var child = node.GetChild(i);
            if (child is T desired_child)
                yield return desired_child;
            foreach (var descendant in child.each_descendant<T>())
                yield return descendant;
        }
    }
    
    public static List<Node> get_ancestors(Node node) {
        var ancestors = new List<Node>();
        while (node != null) {
            ancestors.Insert(0, node);
            node = node.GetParent();
        }
        return ancestors;
    }
    
    
    // Tween...

    public static Tween create_tween(
        ref Tween tween, Node node,
        Tween.EaseType ease = Tween.EaseType.Out,
        Tween.TransitionType trans = Tween.TransitionType.Quad
    ) {
        tween?.Kill();
        tween = node.CreateTween();
        tween.SetEase(ease).SetTrans(trans);
        return tween;
    }

    public static Tween create_tween(
        ref Tween tween, Node node,
        NodePath property, Variant final_value,
        Tween.EaseType ease = Tween.EaseType.Out,
        Tween.TransitionType trans = Tween.TransitionType.Quad,
        float duration = 0.2f
    ) {
        create_tween(ref tween, node, ease, trans);
        tween.TweenProperty(node, property, final_value, duration);
        return tween;
    }


    // UI...

    public static Button create_button(string text, Action on_press) {
        var button = new Button();
        button.Text = text;
        button.Pressed += on_press;
        return button;
    }
    
    
    // TileMap...
    
    public static int find_layer(TileMap tiles, string layer_name) {
        var layer_count = tiles.GetLayersCount();
        for (int i = 0; i < layer_count; i++)
            if (tiles.GetLayerName(i) == layer_name)
                return i;
        throw new ZException($"Layer not found: {layer_name}");
    }
    
    
    // Matrices...
    
    public static Transform3D Translation(Vector3 translation) {
        var tf = Transform3D.Identity;
        tf.Origin = translation;
        return tf;
    }
    public static Transform3D Translation(float x, float y, float z) {
        return Translation(new(x,y,z));
    }
    
    public static Transform3D Rotation(Quaternion rotation) {
        return new Transform3D(new(rotation), Vector3.Zero);
    }
    
    public static Transform3D TRS(
        Vector3 translation, Vector3 rotation, Vector3 scale
    ) {
        var tf = Transform3D.Identity;
        var s = Basis.FromScale(scale);
        var r = Basis.FromEuler(rotation);
        tf.Basis = r * s;
        tf.Origin = translation;
        return tf;
    }
    
    public static Transform3D TRS(
        Vector3 translation, Quaternion rotation, Vector3 scale
    ) {
        var tf = Transform3D.Identity;
        var s = Basis.FromScale(scale);
        var r = new Basis(rotation);
        tf.Basis = r * s;
        tf.Origin = translation;
        return tf;
    }
    
    
    // Camera...
    
    public static Projection get_clip_to_view(Camera3D camera) =>
        camera.GetCameraProjection().Inverse();

    public static float derive_camera_distance(float radius, Camera3D camera) =>
        derive_camera_distance(radius, get_clip_to_view(camera));

    public static float derive_camera_distance(
        float radius, Projection clip_to_view
    ) {
        var x_dist = derive_camera_distance(radius, new(1, 0), clip_to_view);
        var y_dist = derive_camera_distance(radius, new(0, 1), clip_to_view);
        return Math.Max(x_dist, y_dist);
    }

    public static float derive_camera_distance(
        float radius, Vector2 clip_space_edge_pos, Projection clip_to_view
    ) {
        var dirvec = xy0(clip_space_edge_pos.Normalized());
        var (ray_start, ray_dir) = 
            get_local_space_ray(clip_space_edge_pos, clip_to_view);
        
        // These are not necessarily x, can also be y or something else entirely
        // But it's a nice simplification to just consider the problem for x.
        var ray_start_x = ray_start.Dot(dirvec);
        var ray_dir_x = ray_dir.Dot(dirvec);
        if (Mathf.IsEqualApprox(ray_dir_x, 0))
            return radius + 1;  // Otherwise it breaks for orthographic cameras
        
        var circle_normal = ray_dir.Cross(Vector3.Forward).Normalized();
        var intersection_tangent_normal =
            ray_dir.Cross(circle_normal).Normalized();
        var center_to_intersection = intersection_tangent_normal * radius;
        var intersection_x = center_to_intersection.Dot(dirvec);
        
        var t = (intersection_x - ray_start_x) / ray_dir_x;
        var intersection_z = ray_start.Z + ray_dir.Z * t;
        var distance = -intersection_z + center_to_intersection.Z;
        return distance;
    }
	
    public static (Vector3, Vector3) get_local_space_ray(
        Vector2 clip_space_pos, Projection clip_to_view
    ) {
        var q = clip_space_pos;
        var near_pos = clip_to_view * new Vector4(q.X, q.Y, -1, 1);
        var deeper_pos = clip_to_view * new Vector4(q.X, q.Y, 1, 1);
        near_pos /= near_pos.W;
        deeper_pos /= deeper_pos.W;
        var ray_start = xyz(near_pos);
        var point_on_ray = xyz(deeper_pos);
        var ray_dir = (point_on_ray - ray_start).Normalized();
        return (ray_start, ray_dir);
    }
    
    
    // Vector swizzling...
    
    public static Vector2 xy(Vector3 xyz) {
        return new(xyz.X, xyz.Y);
    }
    
    public static Vector2I xy(Vector3I xyz) {
        return new(xyz.X, xyz.Y);
    }
    
    public static Vector2 xz(Vector3 xyz) {
        return new(xyz.X, xyz.Z);
    }
    
    public static Vector3 xy0(Vector2 xy) {
        return new Vector3(xy.X, xy.Y, 0);
    }
    
    public static Vector3 x0y(Vector2 xy) {
        return new Vector3(xy.X, 0, xy.Y);
    }
    
    public static Vector3 xy_z(Vector2 xy, float z) {
        return new Vector3(xy.X, xy.Y, z);
    }
    
    public static Vector3 xyz(Vector4 xyzw) {
        return new Vector3(xyzw.X, xyzw.Y, xyzw.Z);
    }
    
    
    // Compute shaders...
    public static RDUniform image_uniform(int binding, Rid texture) {
        var uniform = new RDUniform {
            Binding = binding, UniformType = RenderingDevice.UniformType.Image
        };
        uniform.AddId(texture);
        return uniform;
    }
}
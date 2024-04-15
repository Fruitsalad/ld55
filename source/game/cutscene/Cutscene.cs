using System;
using System.Collections.Generic;
using Godot;

public partial class Cutscene : Control {
    public Control text_box_panel;
    RichTextLabel text_box;
    
    public void init_cutscene() {
        text_box_panel = this.get_node<Control>("%TextBoxPanel");
        text_box = this.get_node<RichTextLabel>("%TextBox");
    }

    public void tell(string text) {
        text_box_panel.Visible = true;
        text_box.Text = $"[type]{text}[/type]";
    }
    
    // List<(float, Action)> callbacks = new();
    //
    // float now;
    
    // public void after(float time, Action new_callback) {
    //     callbacks.Add((now + time, new_callback));
    // }
    //
    // public override void _Process(double delta) {
    //     now += (float)delta;
    //
    //     for (int i = 0; i < callbacks.Count;) {
    //         var (time, callback) = callbacks[i];
    //         if (time <= now) {
    //             callbacks.RemoveAt(i);
    //             callback();
    //         } else i++;
    //     } 
    // }
}
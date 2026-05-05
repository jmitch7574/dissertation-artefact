using System;
using System.Runtime.Versioning;
using Godot;

[GlobalClass]
[Tool]
public partial class WindowData : Resource
{
    [Export]
    public float Width;

    [Export]
    public float Height;

    [Export]
    public PackedScene WindowScene;

    public WindowData(float width = 5f, float height = 3.5f, PackedScene windowScene = null)
    {
        Width = width;
        Height = height;
        WindowScene = windowScene;
    }

    public WindowData()
    {
        Width = 5;
        Height = 3.5f;
        WindowScene = null;
    }
}

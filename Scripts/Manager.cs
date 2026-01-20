using System;
using System.Text.Json;
using GeoJSON.Text.Feature;
using Godot;

public partial class Manager : Node3D
{
    public static readonly Vector2 offset = new(497276, 370866);

    PackedScene building = GD.Load<PackedScene>("res://Scenes/building.tscn");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        using (
            var file = FileAccess.Open(
                @"res://OSM Files/lincoln_bng.geojson",
                FileAccess.ModeFlags.Read
            )
        )
        {
            string json = file.GetAsText();
            FeatureCollection fc = JsonSerializer.Deserialize<FeatureCollection>(json);

            foreach (Feature f in fc.Features)
            {
                Building build = building.Instantiate<Building>();
                AddChild(build);
                build.feature = f;
                build.Generate();
            }
        }
    }
}

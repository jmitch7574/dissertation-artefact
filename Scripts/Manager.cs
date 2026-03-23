using System;
using System.Text.Json;
using GeoJSON.Text.Feature;
using Godot;

[Tool]
public partial class Manager : Node3D
{
    public static readonly Vector2 offset = new(497276, 370866);

    [ExportToolButton("Generate City!")]
    public Callable GenerateButton => Callable.From(Generate);

    [ExportToolButton("Kill Children")]
    public Callable KillChildrenButton => Callable.From(KillChildren);

    public override void _Ready()
    {
        //Generate();
    }

    public void Generate()
    {
        GD.Print("Generating");
        KillChildren();

        PackedScene building = (PackedScene)GD.Load("res://Scenes/building.tscn");

        using (
            var file = FileAccess.Open(
                @"res://OSM Files/lincoln_bng_height.geojson",
                FileAccess.ModeFlags.Read
            )
        )
        {
            string json = file.GetAsText();
            FeatureCollection fc = JsonSerializer.Deserialize<FeatureCollection>(json);

            foreach (Feature f in fc.Features)
            {
                var instance = building.Instantiate();
                if (instance is not Building build)
                {
                    GD.PushError(
                        $"Expected Building but got {instance.GetType().Name}. Check building.tscn root node script."
                    );
                    instance.QueueFree();
                    continue;
                }
                AddChild(build);
                build.feature = f;
                build.Generate();
            }
        }
    }

    public void KillChildren()
    {
        while (GetChildCount() > 0)
        {
            GetChild(0).Free();
        }
    }
}

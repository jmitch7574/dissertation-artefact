using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using Godot;

[Tool]
public partial class Manager : Node3D
{
    public static readonly Vector2 offset = new(497276, 370866);

    [ExportToolButton("Generate City!")]
    public Callable GenerateButton => Callable.From(Generate);

    [ExportToolButton("Kill Children")]
    public Callable KillChildrenButton => Callable.From(KillChildren);

    public override void _EnterTree()
    {
        var ctx = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        if (ctx != null)
            ctx.Unloading += OnUnloading;
    }

    private static void OnUnloading(AssemblyLoadContext ctx)
    {
        GD.Print("Unloading");

        var jsonAssembly = typeof(JsonSerializerOptions).Assembly;
        var updateHandlerType = jsonAssembly.GetType(
            "System.Text.Json.JsonSerializerOptionsUpdateHandler"
        );
        var clearCacheMethod = updateHandlerType?.GetMethod(
            "ClearCache",
            BindingFlags.Static | BindingFlags.Public
        );
        clearCacheMethod?.Invoke(null, new object?[] { null });

        Assembly[] assembliesToWipe =
        [
            typeof(OsmSharp.Node).Assembly,
            typeof(GeoJSON.Text.GeoJSONObject).Assembly,
        ];

        foreach (var type in assembliesToWipe.SelectMany(a => a.GetTypes()))
            ClassStateWiper.Unload(type, null, false);
    }

    public override void _Ready()
    {
        Generate();
    }

    public void Generate()
    {
        GD.Print("Generating!");
        KillChildren();

        // Generate Buildings ---------------------------------------------------------------------------------------------------------------------------
        PackedScene building = (PackedScene)GD.Load("res://Scenes/building.tscn");
        using (
            var file = FileAccess.Open(
                @"res://OSM Files/lincoln/buildings.geojson",
                FileAccess.ModeFlags.Read
            )
        )
        {
            string json = file.GetAsText();
            FeatureCollection featureCollection = JsonSerializer.Deserialize<FeatureCollection>(
                json
            );

            foreach (Feature f in featureCollection.Features)
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
                build.Owner = GetTree().EditedSceneRoot;
                build.feature = f;
                build.Generate();
            }
        }

        // Generate Terrain ---------------------------------------------------------------------------------------------------------------------------
        int terrains = 0;
        using (
            var file = FileAccess.Open(
                @"res://OSM Files/lincoln/terrain.geojson",
                FileAccess.ModeFlags.Read
            )
        )
        {
            string json = file.GetAsText();
            FeatureCollection featureCollection = JsonSerializer.Deserialize<FeatureCollection>(
                json
            );

            var waterPolygons = new List<Vector2[]>();

            foreach (Feature feature in featureCollection.Features)
            {
                var props = feature.Properties;
                var geom = feature.Geometry;

                bool isWater =
                    (props.TryGetValue("natural", out var n) && n.ToString() == "water")
                    || props.ContainsKey("waterway");

                if (geom is LineString line)
                {
                    Vector2[] thisPoly =
                    [
                        .. line.Coordinates.Select(pos => new Vector2(
                            (float)(pos.Longitude - Manager.offset.X),
                            (float)-(pos.Latitude - Manager.offset.Y)
                        )),
                    ];

                    if (isWater)
                    {
                        //waterPolygons.Add(thisPoly);
                        continue;
                    }

                    if (!props.TryGetValue("highway", out object _highway))
                        continue;

                    string highway = _highway.ToString();

                    var node = RoadBuilder.Build(thisPoly, highway);
                    if (node != null)
                    {
                        node.Name = $"Road {terrains++}";
                        AddChild(node);
                        node.Owner = GetTree().EditedSceneRoot;
                    }
                }

                if (!props.TryGetValue("landuse", out object _landuse) && !isWater)
                    continue; // ← skip only if NEITHER landuse NOR water

                string landuse = isWater ? "water" : _landuse.ToString();

                if (geom is Polygon poly)
                {
                    foreach (LineString ls in poly.Coordinates)
                    {
                        Vector2[] thisPoly =
                        [
                            .. ls.Coordinates.Select(pos => new Vector2(
                                (float)(pos.Longitude - Manager.offset.X),
                                (float)-(pos.Latitude - Manager.offset.Y)
                            )),
                        ];

                        if (isWater)
                        {
                            waterPolygons.Add(thisPoly);
                            continue; // ← don't build a terrain mesh for water, CSG handles it
                        }

                        var node = TerrainBuilder.Build(thisPoly, landuse);
                        if (node != null)
                        {
                            node.Name = $"Terrain {terrains++}";
                            AddChild(node);
                            node.Owner = GetTree().EditedSceneRoot;
                        }
                    }
                }
                if (geom is MultiPolygon multipoly)
                {
                    foreach (Polygon onepoly in multipoly.Coordinates)
                    {
                        foreach (LineString ls in onepoly.Coordinates)
                        {
                            Vector2[] thisPoly =
                            [
                                .. ls.Coordinates.Select(pos => new Vector2(
                                    (float)(pos.Longitude - Manager.offset.X),
                                    (float)-(pos.Latitude - Manager.offset.Y)
                                )),
                            ];

                            if (isWater)
                            {
                                waterPolygons.Add(thisPoly);
                                continue; // ← don't build a terrain mesh for water, CSG handles it
                            }

                            var node = TerrainBuilder.Build(thisPoly, landuse);
                            if (node != null)
                            {
                                node.Name = $"Terrain {terrains++}";
                                AddChild(node);
                                node.Owner = GetTree().EditedSceneRoot;
                            }
                        }
                    }
                }
            }

            GenerateBasePlane(waterPolygons);
        }
    }

    void GenerateBasePlane(List<Vector2[]> waterPolygons)
    {
        var baseCombiner = new CsgCombiner3D();
        baseCombiner.Name = "BaseTerrain";
        baseCombiner.RotationDegrees = new Vector3(90, 0, 0);
        baseCombiner.Position = new Vector3(0, -5, 0);
        AddChild(baseCombiner);
        baseCombiner.Owner = GetTree().EditedSceneRoot;
        baseCombiner.MaterialOverlay = MaterialLibrary.GetForLanduse("gravel");

        CsgPolygon3D ground = new CsgPolygon3D();
        ground.Name = "grass";
        ground.Polygon = new Vector2[]
        {
            new(-2500, -2500),
            new(-2500, 2500),
            new(2500, 2500),
            new(2500, -2500),
        };
        ground.Depth = 5f;
        baseCombiner.AddChild(ground);
        ground.Owner = GetTree().EditedSceneRoot;

        // Union all water into a single combiner, then subtract that from base
        var waterUnion = new CsgCombiner3D();
        waterUnion.Name = "Water";
        waterUnion.Operation = CsgShape3D.OperationEnum.Subtraction;
        baseCombiner.AddChild(waterUnion);
        waterUnion.Owner = GetTree().EditedSceneRoot;

        GD.Print($"Water polygons count: {waterPolygons.Count}");

        foreach (var waterRing in waterPolygons)
        {
            var water = new CsgPolygon3D();
            water.Polygon = waterRing.Select(p => new Vector2(p.X, p.Y)).ToArray();
            water.Depth = 5.5f;
            water.Operation = CsgShape3D.OperationEnum.Union;
            waterUnion.AddChild(water);
            water.Owner = GetTree().EditedSceneRoot;
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

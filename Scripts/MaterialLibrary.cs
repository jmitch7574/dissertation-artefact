using System.Collections.Generic;
using Godot;

public static class MaterialLibrary
{
    private static readonly Dictionary<string, string> LanduseMaterials = new()
    {
        { "grass", "res://materials/grass.tres" },
        { "meadow", "res://materials/grass.tres" },
        { "park", "res://materials/grass.tres" },
        { "industrial", "res://materials/concrete.tres" },
        { "retail", "res://materials/paving_stones.tres" },
        { "pedestrian", "res://materials/paving_stones.tres" },
        { "residential", "res://materials/paving_stones.tres" },
        { "university", "res://materials/paving_stones.tres" },
        { "gravel", "res://materials/gravel.tres" },
    };

    public static Material GetForLanduse(string landuseTag)
    {
        var key = LanduseMaterials.ContainsKey(landuseTag) ? landuseTag : "grass"; // fallback
        return GD.Load<Material>(LanduseMaterials[key]);
    }

    public static Material GetForRoad(string highwayTag) =>
        highwayTag switch
        {
            "footway" or "path" or "pavement" => GD.Load<Material>("res://materials/pavement.tres"),
            _ => GD.Load<Material>("res://materials/road.tres"),
        };

    public static Material GetForBuilding(string buildingType) =>
        buildingType switch
        {
            "residential" or "apartments" => GD.Load<Material>("res://Materials/red_brick.tres"),
            "university" => GD.Load<Material>("res://Materials/dark-concrete.tres"),
            _ => null,
        };
}

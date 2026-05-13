using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MaterialLibrary
{
    private static readonly Dictionary<string, string> LanduseMaterials = new()
    {
        { "base", "res://materials/concrete.tres" },
        { "industrial", "res://materials/concrete.tres" },
        { "retail", "res://materials/paving_stones.tres" },
        { "pedestrian", "res://materials/paving_stones.tres" },
        { "residential", "res://materials/paving_stones.tres" },
        { "university", "res://materials/paving_stones.tres" },
        { "parking", "res://materials/road.tres" },
        { "gravel", "res://materials/gravel.tres" },
        { "grass", "res://materials/grass.tres" },
        { "meadow", "res://materials/grass.tres" },
        { "park", "res://materials/grass.tres" },
    };

    private static readonly Dictionary<string, string> BuildingMaterials = new()
    {
        { "residential", "res://Materials/red_brick.tres" },
        { "apartments", "res://Materials/red_brick.tres" },
        { "university", "res://Materials/dark-concrete.tres" },
        { "retail", "res://Materials/retail_brick.tres" },
    };

    private static readonly Dictionary<string, string> RoadMaterials = new()
    {
        { "footway", "res://Materials/pavement.tres" },
        { "path", "res://Materials/pavement.tres" },
        { "pavement", "res://Materials/pavement.tres" },
        { "road", "res://Materials/road.tres" },
    };

    public static Material GetForLanduse(string landuseTag)
    {
        var key = LanduseMaterials.ContainsKey(landuseTag) ? landuseTag : "grass"; // fallback
        return key != null ? GD.Load<Material>(LanduseMaterials[key]) : null;
    }

    public static int GetLanduseOffset(string landuseTag)
    {
        var key = LanduseMaterials.ContainsKey(landuseTag) ? landuseTag : "grass"; // fallback
        return LanduseMaterials.Keys.ToList().IndexOf(key);
    }

    public static Material GetForRoad(string highwayTag)
    {
        var key = RoadMaterials.ContainsKey(highwayTag) ? highwayTag : "road"; // fallback
        return key != null ? GD.Load<Material>(RoadMaterials[key]) : null;
    }

    public static int GetRoadOffset(string landuseTag)
    {
        var key = RoadMaterials.ContainsKey(landuseTag) ? landuseTag : "road"; // fallback
        return RoadMaterials.Keys.ToList().IndexOf(key) + RoadMaterials.Keys.Count; // Offset above all landuse materials
    }

    public static Material GetForBuilding(string buildingType)
    {
        var key = BuildingMaterials.ContainsKey(buildingType) ? buildingType : null; // fallback
        return key != null ? GD.Load<Material>(BuildingMaterials[key]) : null;
    }

    public static Material GetRoofMaterial()
    {
        return GD.Load<Material>("res://Materials/roof-material.tres");
    }

    public static Material GetRoofLipMaterial()
    {
        return GD.Load<Material>("res://Materials/concrete.tres");
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using Godot;

[Tool]
[GlobalClass]
public partial class Building : Node3D
{
    public Feature feature;

    [Export]
    public float Height = -1;

    [Export]
    public float Floors;

    [Export]
    public float HeightPerFloor = 3.5f;

    [Export]
    public Vector2 Offset;

    [Export]
    public BuildingPerimeter bp;

    // Called when the node enters the scene tree for the first time.
    public void Generate()
    {
        if (feature == null)
        {
            return;
        }

        bp.footprint = PerimeterFromFeature();
        bp.building = this;

        if (feature.Properties.ContainsKey("lidar:m_height"))
        {
            Height = float.Parse(feature.Properties["lidar:m_height"].ToString());
        }
        if (Height < 1)
        {
            if (feature.Properties.TryGetValue("building", out object buildingType))
            {
                if (buildingType.ToString() == "apartments")
                {
                    Height = 15;
                }
                if (buildingType.ToString() == "university")
                {
                    Height = 14;
                }
            }
        }

        bp.Generate();

        if (feature.Properties.TryGetValue("name", out object name))
        {
            Name = name.ToString();
        }
        else
        {
            Name = $"Building {feature.Id}";
        }
    }

    public BuildingFootprint PerimeterFromFeature()
    {
        BuildingFootprint footprint = new();

        if (feature.Geometry is Polygon poly)
        {
            footprint.Polygons.AddRange(ParsePolygon(poly));
        }
        else if (feature.Geometry is MultiPolygon mp)
        {
            foreach (var polygon in mp.Coordinates)
            {
                footprint.Polygons.AddRange(ParsePolygon(polygon));
            }
        }

        return footprint;
    }

    public static List<FootprintPolygon> ParsePolygon(Polygon polygon)
    {
        var result = new List<FootprintPolygon>();

        for (int ringIndex = 0; ringIndex < polygon.Coordinates.Count; ringIndex++)
        {
            LineString ring = polygon.Coordinates[ringIndex];
            FootprintPolygon fp = new FootprintPolygon { IsInner = ringIndex > 0 };

            foreach (IPosition pos in ring.Coordinates)
            {
                fp.Add(
                    new Vector3(
                        (float)(pos.Longitude - Manager.offset.X),
                        0,
                        (float)-(pos.Latitude - Manager.offset.Y)
                    )
                );
            }

            fp.EnsureWinding();

            result.Add(fp);
        }

        return result;
    }

    public string GetBuildingType()
    {
        if (feature.Properties.TryGetValue("building", out object type))
        {
            if (type.ToString() == "yes")
            {
                if (feature.Properties.TryGetValue("amenity", out object amenity))
                {
                    return amenity.ToString();
                }
            }
            return type.ToString();
        }
        return "default";
    }
}

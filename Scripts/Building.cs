using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using Godot;

public partial class Building : Node3D
{
    public Feature feature;

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
        bp.buildingParent = this;

        if (feature.Properties.ContainsKey("lidar:m_height"))
        {
            bp.Height = float.Parse(feature.Properties["lidar:m_height"].ToString());
        }
        if (bp.Height < 1)
        {
            if (feature.Properties.TryGetValue("building", out object buildingType))
            {
                if (buildingType.ToString() == "apartments")
                {
                    bp.Height = 15;
                }
                if (buildingType.ToString() == "university")
                {
                    bp.Height = 14;
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
}

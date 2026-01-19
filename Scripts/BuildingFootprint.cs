using System.Collections.Generic;
using Godot;

public class BuildingFootprint
{
    public List<FootprintPolygon> Polygons;

    public BuildingFootprint(List<FootprintPolygon> footprintPolygons)
    {
        Polygons = footprintPolygons;
    }
}

public class FootprintPolygon : List<Vector3>
{
    public bool IsInner;

    public FootprintPolygon(bool isInner = false)
        : base()
    {
        IsInner = isInner;
    }
}

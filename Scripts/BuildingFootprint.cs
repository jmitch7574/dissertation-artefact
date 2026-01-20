using System.Collections.Generic;
using Godot;

public class BuildingFootprint
{
    public List<FootprintPolygon> Polygons;

    public BuildingFootprint(List<FootprintPolygon> footprintPolygons)
    {
        Polygons = footprintPolygons;
    }

    public BuildingFootprint()
    {
        Polygons = new();
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

    public void EnsureWinding()
    {
        bool wantClockwise = IsInner;
        bool isClockwise = IsClockwise(this);

        if (isClockwise != wantClockwise)
            Reverse();
    }

    public static bool IsClockwise(IReadOnlyList<Vector3> poly)
    {
        float area = 0f;

        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            area += (poly[i].X - poly[j].X) * (poly[i].Z + poly[j].Z);
        }

        return area > 0f;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Poly2Tri;
using Poly2Tri.Triangulation.Polygon;

public partial class BuildingPerimeter : MeshInstance3D
{
    [Export]
    float Height = 12.0f;

    readonly BuildingFootprint TestFootprint = new(
        new List<FootprintPolygon>()
        {
            new FootprintPolygon()
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 0, 6),
                new Vector3(4, 0, 6),
                new Vector3(4, 0, 10),
                new Vector3(0, 0, 10),
            },
            new FootprintPolygon(true)
            {
                new Vector3(2, 0, 2),
                new Vector3(2, 0, 4),
                new Vector3(4, 0, 4),
                new Vector3(4, 0, 2),
            },
        }
    );

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Generate(TestFootprint);
    }

    void Generate(BuildingFootprint footprint)
    {
        List<Vector3> verts = new();
        List<Vector3> normals = new();
        List<int> indices = new();

        int offset = 0;

        for (int i = 0; i < footprint.Polygons.Count; i++)
        {
            FootprintPolygon polygonVertices = footprint.Polygons[i];
            for (int j = 0; j < polygonVertices.Count; j++)
            {
                Vector3 thisPoint = polygonVertices[j];
                Vector3 nextPoint = polygonVertices[(j + 1) % polygonVertices.Count];

                Vector3 thisTop = thisPoint + (Vector3.Up * Height);
                Vector3 nextTop = nextPoint + (Vector3.Up * Height);

                Vector3 edge = nextPoint - thisPoint;
                Vector3 normal;

                if (polygonVertices.IsInner)
                {
                    normal = Vector3.Down.Cross(edge).Normalized();
                }
                else
                {
                    normal = edge.Cross(Vector3.Up).Normalized();
                }

                verts.Add(thisPoint);
                verts.Add(nextPoint);
                verts.Add(nextTop);
                verts.Add(thisTop);
                for (int k = 0; k < 4; k++)
                {
                    normals.Add(normal);
                }
                indices.Add(offset + 0);
                indices.Add(offset + 1);
                indices.Add(offset + 2);
                indices.Add(offset + 2);
                indices.Add(offset + 3);
                indices.Add(offset + 0);
                offset += 4;
            }
        }

        int roofStart = verts.Count;
        Vector3 roofNormal = Vector3.Up;

        foreach (FootprintPolygon OuterPolygon in footprint.Polygons.Where(e => !e.IsInner))
        {
            PolygonPoint[] points = new PolygonPoint[OuterPolygon.Count];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new PolygonPoint(OuterPolygon[i].X, OuterPolygon[i].Z);
            }

            Polygon polygon = new Polygon(points);

            foreach (FootprintPolygon InnerPolygon in footprint.Polygons.Where(e => e.IsInner))
            {
                PolygonPoint[] holePoints = new PolygonPoint[InnerPolygon.Count];

                for (int i = 0; i < holePoints.Length; i++)
                {
                    holePoints[i] = new PolygonPoint(InnerPolygon[i].X, InnerPolygon[i].Z);
                }

                Polygon holePolygon = new Polygon(holePoints);
                polygon.AddHole(holePolygon);
            }

            P2T.Triangulate(polygon);

            foreach (var tri in polygon.Triangles)
            {
                foreach (var p in tri.Points)
                {
                    verts.Add(new Vector3((float)p.X, Height, (float)p.Y));
                    normals.Add(Vector3.Up);
                    indices.Add(offset++);
                }
            }
        }

        var arrays = new Godot.Collections.Array();

        arrays.Resize((int)Mesh.ArrayType.Max);

        arrays[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();

        var mesh = new ArrayMesh();

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        Mesh = mesh;
    }
}

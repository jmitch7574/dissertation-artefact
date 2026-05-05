using System.Collections.Generic;
using System.Linq;
using Godot;
using Poly2Tri;
using Poly2Tri.Triangulation.Polygon;

public static class TerrainBuilder
{
    public static MeshInstance3D Build(Vector2[] ring, string landuseTag)
    {
        if (ring.Length < 3)
            return null;

        var verts = new List<Vector3>();
        var normals = new List<Vector3>();
        var indices = new List<int>();
        int idx = 0;

        // Triangulate using poly2tri (same as your buildings)
        var points = ring.Select(selector: p => new PolygonPoint(p.X, p.Y)).ToArray();
        var polygon = new Polygon(points);
        P2T.Triangulate(polygon);

        foreach (var tri in polygon.Triangles)
        {
            foreach (var p in tri.Points)
            {
                verts.Add(new Vector3((float)p.X, 0f, (float)p.Y));
                normals.Add(Vector3.Up);
                indices.Add(idx++);
            }
        }

        if (verts.Count == 0)
            return null;

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();

        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        var instance = new MeshInstance3D { Mesh = mesh };

        instance.MaterialOverlay = MaterialLibrary.GetForLanduse(landuseTag);
        instance.Position = new(
            instance.Position.X,
            instance.Position.Y + 0.01f,
            instance.Position.Z
        );
        return instance;
    }
}

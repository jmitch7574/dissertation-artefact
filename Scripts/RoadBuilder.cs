using System.Collections.Generic;
using System.Linq;
using Godot;

public static class RoadBuilder
{
    private static readonly Dictionary<string, float> HighwayWidths = new()
    {
        { "motorway", 16f },
        { "primary", 12f },
        { "secondary", 8f },
        { "tertiary", 7f },
        { "residential", 6f },
        { "service", 4f },
        { "footway", 2f },
        { "path", 1.5f },
        { "cycleway", 2f },
    };

    public static MeshInstance3D Build(Vector2[] points, string highwayTag)
    {
        if (points.Length < 2)
            return null;

        float halfWidth = HighwayWidths.GetValueOrDefault(highwayTag, 6f) / 2f;

        var verts = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        float uvV = 0f; // tracks V coordinate along the road length

        for (int i = 0; i < points.Length; i++)
        {
            // Get direction along the line at this point
            Vector2 dir;
            if (i == 0)
                dir = (points[1] - points[0]).Normalized();
            else if (i == points.Length - 1)
                dir = (points[i] - points[i - 1]).Normalized();
            else
                dir = (points[i + 1] - points[i - 1]).Normalized(); // average direction

            // Perpendicular to direction
            var perp = new Vector2(-dir.Y, dir.X) * halfWidth;

            var left = points[i] - perp;
            var right = points[i] + perp;

            verts.Add(new Vector3(left.X, 0.06f, left.Y));
            verts.Add(new Vector3(right.X, 0.06f, right.Y));

            uvs.Add(new Vector2(0f, uvV));
            uvs.Add(new Vector2(1f, uvV));

            normals.Add(Vector3.Up);
            normals.Add(Vector3.Up);

            // Build Quad
            if (i > 0)
            {
                int b = (i - 1) * 2;
                indices.Add(b + 0);
                indices.Add(b + 2);
                indices.Add(b + 1);
                indices.Add(b + 1);
                indices.Add(b + 2);
                indices.Add(b + 3);
            }

            // Advance V by distance to next point
            if (i < points.Length - 1)
                uvV += (points[i + 1] - points[i]).Length() / (halfWidth * 2f);
        }

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();

        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        var instance = new MeshInstance3D { Mesh = mesh };
        instance.MaterialOverlay = MaterialLibrary.GetForRoad(highwayTag);
        instance.Position = new(
            instance.Position.X,
            instance.Position.Y + MaterialLibrary.GetRoadOffset(highwayTag) / 100.0f,
            instance.Position.Z
        );
        return instance;
    }
}

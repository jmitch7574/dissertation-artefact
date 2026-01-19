using Godot;
using System;
using System.Collections.Generic;

public partial class BuildingPerimeter : MeshInstance3D
{
	[Export] float Height = 12.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Generate();
	}

	void Generate()
	{
		// Testing footprint
		Vector3[] footprint =
		{
			new Vector3(0, 0, 0),
			new Vector3(10, 0, 0),
			new Vector3(10, 0, 6),
			new Vector3(4, 0, 6),
			new Vector3(4, 0, 10),
			new Vector3(0, 0, 10)
		};

		List<Vector3> verts = new();
		List<Vector3> normals = new();
		List<int> indices = new();

		int offset = 0;

		for (int i = 0; i < footprint.Length; i++)
		{
			Vector3 thisPoint = footprint[i];
			Vector3 nextPoint = footprint[(i + 1) % footprint.Length];

			Vector3 thisTop = thisPoint + (Vector3.Up * Height);
			Vector3 nextTop = nextPoint + (Vector3.Up * Height);

			Vector3 edge = nextPoint - thisPoint;
			Vector3 normal = edge.Cross(Vector3.Up).Normalized();

			verts.Add(thisPoint);
			verts.Add(nextPoint);
			verts.Add(nextTop);
			verts.Add(thisTop);

			for (int j = 0; j < 4; j++)
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

		int roofStart = verts.Count;
		Vector3 roofNormal = Vector3.Up;

		for (int i = 0; i < footprint.Length; i++)
		{
			verts.Add(footprint[i] + Vector3.Up * Height);
			normals.Add(roofNormal);
		}

		for (int i = 1; i < footprint.Length - 1; i++)
		{
			indices.Add(roofStart);
			indices.Add(roofStart + i);
			indices.Add(roofStart + i + 1);
		}

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);

		arrays[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		arrays[(int)Mesh.ArrayType.Index]  = indices.ToArray();

		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

		Mesh = mesh;
	}
	
}

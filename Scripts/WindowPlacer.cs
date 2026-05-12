using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Godot;

[Tool]
[GlobalClass]
public partial class WindowPlacer : Node3D
{
    [Export]
    WindowData SquareWindow;

    [Export]
    WindowData UniversityWindow;

    public void Generate(Building b)
    {
        float height = b.Height;
        string type = b.GetBuildingType();
        float minSpacing = 3f;

        WindowData chosenWindow = type switch
        {
            "residential" => SquareWindow,
            "apartments" => SquareWindow,
            "retail" => SquareWindow,
            "university" => UniversityWindow,
            _ => null,
        };

        if (chosenWindow == null)
            return;

        foreach (FootprintPolygon fp in b.bp.footprint.Polygons)
        {
            for (int i = 0; i < fp.Count; i++)
            {
                Vector3 pointOne = fp[i];
                Vector3 pointTwo = fp[(i + 1) % fp.Count];

                float dist = pointOne.DistanceTo(pointTwo);

                int count = (int)
                    Math.Floor((dist - minSpacing) / (chosenWindow.Width + minSpacing));

                float totalWindowSpace = count * chosenWindow.Width;
                float remainingSpace = dist - totalWindowSpace;
                float realGap = remainingSpace / (count + 1);

                Vector3 wallDir = (pointTwo - pointOne).Normalized();

                for (int j = 0; j < count; j++)
                {
                    // Distance along wall to the center of this window
                    float distAlongWall =
                        realGap + j * (chosenWindow.Width + realGap) + chosenWindow.Width * 0.5f;

                    foreach (float windowHeight in CalculateHeightPoints(b))
                    {
                        // Vector2 paraAngle = new Vector2(1, 0);
                        Vector2 perpAngle = new(wallDir.Z, -wallDir.X);
                        // float angle = paraAngle.AngleTo(perpAngle);
                        // angle = Mathf.RadToDeg(angle);
                        // angle += 90;
                        // angle = -angle;

                        float angle = Mathf.RadToDeg(Mathf.Atan2(perpAngle.X, perpAngle.Y));

                        Node3D newWindow = (Node3D)chosenWindow.WindowScene.Instantiate();

                        AddChild(newWindow);

                        newWindow.GlobalPosition =
                            GlobalPosition + (pointOne + (wallDir * distAlongWall));
                        newWindow.GlobalPosition = new Vector3(
                            newWindow.GlobalPosition.X,
                            windowHeight,
                            newWindow.GlobalPosition.Z
                        );
                        newWindow.GlobalRotationDegrees = new(0, angle, 0);
                        newWindow.GlobalPosition -=
                            new Vector3(-wallDir.Z, 0, wallDir.X).Normalized() / 10;
                        newWindow.Owner = GetTree().EditedSceneRoot;
                    }
                }
            }
        }
    }

    List<float> CalculateHeightPoints(Building b)
    {
        int floors = (int)Math.Floor(b.Height / 3.5f);
        float realGap = b.Height / floors;

        List<float> multiples = new();

        for (float i = realGap; i < b.Height; i += realGap)
        {
            multiples.Add(i);
        }

        return multiples;
    }
}

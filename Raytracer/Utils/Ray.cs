using OpenTK.Mathematics;
using System.Security.Cryptography;

namespace Raytracer.Utils;

public class Ray
{
    public Vector3d Origin;
    public Vector3d Direction;

    public Ray(Vector3d origin, Vector3d direction) {
        Origin = origin; 
        Direction = direction;
    }

    public Ray Transformed(Matrix4d transform) {
        Vector3d originTransformed = Vector3d.TransformPosition(Origin, transform);
        Vector3d directionTransformed = Vector3d.TransformVector(Direction, transform);
        return new Ray(originTransformed, directionTransformed);
    }
}

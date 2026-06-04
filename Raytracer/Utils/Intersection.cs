using OpenTK.Mathematics;
using Raytracer.Environment.Scene;

namespace Raytracer.Utils;

public class Intersection
{
    public Vector3d Position = new(0.0);
    public Vector3d Normal = new(0.0);
    public SolidSceneNode? ClosestSolid;
    public double T = 0.0;

    public Intersection() { }
    public Intersection(double t) {
        T = t;
    }

    public void Transform(Matrix4d posTransform, Matrix4d normalTransform, Ray ray) {
        var newPos = Vector3d.TransformPosition(Position, posTransform);
        var newNorm = Vector3d.TransformVector(Normal, normalTransform);

        newNorm.Normalize();

        double newT = Vector3d.Dot(newPos - ray.Origin, ray.Direction)
                    / Vector3d.Dot(ray.Direction, ray.Direction);

        Position = newPos;
        Normal = newNorm;
        T = newT;
    }

    public Intersection Transformed(Matrix4d posTransform, Matrix4d normalTransform, Ray ray) { 
        var newIntersection = new Intersection();

        var newPos = Vector3d.TransformPosition(Position, posTransform);
        var newNorm = Vector3d.TransformVector(Normal, normalTransform);

        newNorm.Normalize();

        double newT = Vector3d.Dot(newPos - ray.Origin, ray.Direction)
                    / Vector3d.Dot(ray.Direction, ray.Direction);

        newIntersection.Position = newPos;
        newIntersection.Normal = newNorm;
        newIntersection.T = newT;
        newIntersection.ClosestSolid = this.ClosestSolid;

        return newIntersection;
    }

    public bool HasValidColorSource()
    {
        return ClosestSolid?.Material != null;
    }
}

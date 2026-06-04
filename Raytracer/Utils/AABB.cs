using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Raytracer.Utils;
public class AABB
{
    public Vector3d Min;
    public Vector3d Max;

    public AABB()
    {
        Min = new(double.PositiveInfinity);
        Max = new(double.NegativeInfinity);
    }

    public AABB(Vector3d min, Vector3d max)
    {
        Min = min;
        Max = max;
    }

    public void Merge(AABB other)
    {
        Min.X = Math.Min(Min.X, other.Min.X);
        Min.Y = Math.Min(Min.Y, other.Min.Y);
        Min.Z = Math.Min(Min.Z, other.Min.Z);

        Max.X = Math.Max(Max.X, other.Max.X);
        Max.Y = Math.Max(Max.Y, other.Max.Y);
        Max.Z = Math.Max(Max.Z, other.Max.Z);
    }

    public AABB Transformed(Matrix4d transform)
    {
        Vector3d[] corners =
        {
            new(Min.X, Min.Y, Min.Z),
            new(Min.X, Min.Y, Max.Z),
            new(Min.X, Max.Y, Min.Z),
            new(Min.X, Max.Y, Max.Z),
            new(Max.X, Min.Y, Min.Z),
            new(Max.X, Min.Y, Max.Z),
            new(Max.X, Max.Y, Min.Z),
            new(Max.X, Max.Y, Max.Z),
        };

        AABB result = new AABB();

        foreach (Vector3d corner in corners)
        {
            Vector3d transformed = Vector3d.TransformPosition(corner, transform);
            result.Expand(transformed);
        }

        return result;
    }

    public void Expand(Vector3d point)
    {
        Min = Vector3d.ComponentMin(Min, point);
        Max = Vector3d.ComponentMax(Max, point);
    }

    public bool RayIntersection(in Ray ray, out Intersection intersection, 
        double tMin = 0.0,
        double tMax = double.PositiveInfinity) {

        intersection = new Intersection(double.PositiveInfinity);

        for (int axis = 0; axis < 3; axis++)
        {
            double origin = ray.Origin[axis];
            double direction = ray.Direction[axis];

            double min = Min[axis];
            double max = Max[axis];

            if (Math.Abs(direction) < 1e-12)
            {
                if (origin < min || origin > max)
                    return false;

                continue;
            }

            double invD = 1.0 / direction;

            double t0 = (min - origin) * invD;
            double t1 = (max - origin) * invD;

            if (t0 > t1)
                (t0, t1) = (t1, t0);

            tMin = Math.Max(tMin, t0);
            tMax = Math.Min(tMax, t1);

            if (tMax < tMin)
                return false;
        }

        intersection.T = tMin;
        return true;
    }

    public override string ToString()
    {
        return $"Min=({Min.X:F2}, {Min.Y:F2}, {Min.Z:F2}), " +
               $"Max=({Max.X:F2}, {Max.Y:F2}, {Max.Z:F2})";
    }
}
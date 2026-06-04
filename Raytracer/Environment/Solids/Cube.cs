using System.Text;
using Microsoft.VisualBasic;
using OpenTK.Mathematics;
using Raytracer.Environment.Materials;
using Raytracer.Environment.Scene;
using Raytracer.Systems;
using Raytracer.Utils;
using Utils;

namespace Raytracer.Environment.Solids;

public record CubeConfig(
    string Id,
    string? MaterialId,
    Vector3d? LocalPos
    ) : SceneNodeConfigBase(Id, MaterialId);

public class Cube : SolidSceneNode
{
    public static readonly double Size = 1.0;

    public Cube(Vector3d? localPos)
    {
        LocalPos = localPos ?? Vector3d.Zero;
    }

    public Cube(CubeConfig config)
        : this(config.LocalPos) { }


    private bool IntersectSlab(
        double origin,
        double direction,
        double min,
        double max,
        Vector3d axisNormal,
        ref double tMin,
        ref double tMax,
        ref Vector3d hitNormal)
    {
        if (Math.Abs(direction) < MathUtil.Epsilon)
        {
            return origin >= min && origin <= max;
        }

        double t1 = (min - origin) / direction;
        double t2 = (max - origin) / direction;

        Vector3d normal1 = -axisNormal;
        Vector3d normal2 = axisNormal;

        if (t1 > t2)
        {
            (t1, t2) = (t2, t1);
            (normal1, normal2) = (normal2, normal1);
        }

        if (t1 > tMin)
        {
            tMin = t1;
            hitNormal = normal1;
        }

        if (t2 < tMax)
        {
            tMax = t2;
        }

        return tMin <= tMax;
    }

    public override bool RayIntersection(in Ray ray, out Intersection intersection)
    {
        intersection = new Intersection(double.PositiveInfinity);

        double half = Size * 0.5;

        Vector3d min = LocalPos - new Vector3d(half, half, half);
        Vector3d max = LocalPos + new Vector3d(half, half, half);

        double tMin = double.NegativeInfinity;
        double tMax = double.PositiveInfinity;

        Vector3d hitNormal = Vector3d.Zero;

        // X slab
        if (!IntersectSlab(
            ray.Origin.X, ray.Direction.X,
            min.X, max.X,
            Vector3d.UnitX,
            ref tMin, ref tMax, ref hitNormal))
        {
            return false;
        }

        // Y slab
        if (!IntersectSlab(
            ray.Origin.Y, ray.Direction.Y,
            min.Y, max.Y,
            Vector3d.UnitY,
            ref tMin, ref tMax, ref hitNormal))
        {
            return false;
        }

        // Z slab
        if (!IntersectSlab(
            ray.Origin.Z, ray.Direction.Z,
            min.Z, max.Z,
            Vector3d.UnitZ,
            ref tMin, ref tMax, ref hitNormal))
        {
            return false;
        }

        double t;

        if (tMin > MathUtil.Epsilon)
        {
            t = tMin;
        }
        else if (tMax > MathUtil.Epsilon)
        {
            // Ray starts inside the cube
            t = tMax;
            hitNormal = -hitNormal;
        }
        else
        {
            return false;
        }

        intersection.T = t;
        intersection.Position = ray.Origin + t * ray.Direction;
        intersection.Normal = hitNormal;
        intersection.ClosestSolid = this;

        return true;
    }

    public override Ray? GetRefractedRay(Ray incomingRay, Intersection intersection)
    {
        var material = Material as DielectricMaterial;
        if (material == null) return null;

        // Object space
        var localIncomingRay = incomingRay.Transformed(WorldToObjectTransform);
        var localIntersection = intersection.Transformed(WorldToObjectTransform, WorldToObjectNormalsTransform, localIncomingRay);

        double n1 = 1.0; // Assume air outside
        double n2 = material.RefractiveIndex;

        if (!DielectricMaterial.TryRefract(
            localIncomingRay.Direction,
            localIntersection.Normal,
            n1,
            n2,
            out Vector3d refractedInsideDir))
            return null;

        Vector3d refractedInsideOrigin = localIntersection.Position + refractedInsideDir * MathUtil.Epsilon;
        Ray localInsideRay = new Ray(refractedInsideOrigin, refractedInsideDir);

        if (!RayIntersection(
            localInsideRay,
            out Intersection insideIntersection))
            return null;

        if (!DielectricMaterial.TryRefract(
            localInsideRay.Direction,
            -insideIntersection.Normal,
            n2,
            n1,
            out Vector3d refractedOutsideDir))
            return null;

        Vector3d refractedOutsideOrigin = insideIntersection.Position + refractedOutsideDir * MathUtil.Epsilon;

        Ray localOusideRay = new Ray(refractedOutsideOrigin, refractedOutsideDir);

        // World space
        return localOusideRay.Transformed(ObjectToWorldTransform);
    }

    public override AABB GetBoundingBox()
    {
        double half = Size * 0.5;
        Vector3d r = new Vector3d(half, half, half);
        return new AABB(LocalPos - r, LocalPos + r);
    }

    public override SceneNode Clone()
    {
        var copy = new Cube(LocalPos);
        if (Material != null) copy.SetMaterial(Material);
        return copy;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Solid: Cube");
        sb.AppendLine($"Material: {Material?.Id}");
        sb.AppendLine($"LocalPos: {LocalPos.ToString()}");

        return sb.ToString();
    }
}

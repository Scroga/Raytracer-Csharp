using System.Text;
using OpenTK.Mathematics;
using Raytracer.Environment.Materials;
using Raytracer.Environment.Scene;
using Raytracer.Systems;
using Raytracer.Utils;
using Utils;

namespace Raytracer.Environment.Solids;

public record SphereConfig(
    string Id,
    string? MaterialId,
    Vector3d? LocalPos,
    double Radius
    ) : SceneNodeConfigBase(Id, MaterialId);

public class Sphere : SolidSceneNode
{
    public double Radius { get; private set; }

    public Sphere(Vector3d? localPos, double radius)
    {
        LocalPos = localPos ?? Vector3d.Zero;
        Radius = radius;
    }

    public Sphere(SphereConfig config)
        : this(config.LocalPos, config.Radius) { }

    public override bool RayIntersection(in Ray ray, out Intersection intersection)
    {
        intersection = new Intersection(double.PositiveInfinity);

        Vector3d oc = ray.Origin - LocalPos;

        double a = Vector3d.Dot(ray.Direction, ray.Direction);
        double b = 2.0 * Vector3d.Dot(oc, ray.Direction);
        double c = Vector3d.Dot(oc, oc) - Radius * Radius;

        double discriminant = b * b - 4.0 * a * c;

        if (discriminant < 0)
        {
            return false;
        }

        double sqrtD = Math.Sqrt(discriminant);

        double t1 = (-b - sqrtD) / (2.0 * a);
        double t2 = (-b + sqrtD) / (2.0 * a);

        if (t1 > MathUtil.Epsilon) intersection.T = t1;
        else if (t2 > MathUtil.Epsilon) intersection.T = t2;
        else return false;

        intersection.Position = ray.Origin + intersection.T * ray.Direction;
        intersection.Normal = (intersection.Position - LocalPos).Normalized();
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

        Vector3d r = new Vector3d(Radius, Radius, Radius);
        return new AABB(LocalPos - r, LocalPos + r);
    }

    public override SceneNode Clone()
    {
        var copy = new Sphere(LocalPos, Radius);
        if (Material != null) copy.SetMaterial(Material);
        return copy;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Solid: Sphere");
        sb.AppendLine($"Material: {Material?.Id}");
        sb.AppendLine($"LocalPos: {LocalPos.ToString()}");
        sb.AppendLine($"Radius: {Radius}");

        return sb.ToString();
    }
}

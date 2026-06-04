using System.Text;
using OpenTK.Mathematics;
using Raytracer.Environment.Materials;
using Raytracer.Environment.Scene;
using Raytracer.Systems;
using Raytracer.Utils;
using Utils;

namespace Raytracer.Environment.Solids;

public record TorusConfig(
    string Id,
    string? MaterialId,
    Vector3d? LocalPos,
    double Radius,
    double RingRadius
    ) : SceneNodeConfigBase(Id, MaterialId);

public class Torus : SolidSceneNode
{
    public double Radius { get; private set; }
    public double RingRadius { get; private set; }

    public Torus(Vector3d? localPos, double radius, double ringRadius)
    {
        LocalPos = localPos ?? Vector3d.Zero;
        Radius = radius;
        RingRadius = ringRadius;
    }

    public Torus(TorusConfig config)
        : this(config.LocalPos, config.Radius, config.RingRadius) { }

    private Vector3d GetNormal(Vector3d worldPoint)
    {
        Vector3d p = worldPoint - LocalPos;

        double x = p.X;
        double y = p.Y;
        double z = p.Z;

        double R = Radius;
        double r = RingRadius;

        double sum = x * x + y * y + z * z + R * R - r * r;

        Vector3d normal = new Vector3d(
            4.0 * x * (sum - 2.0 * R * R),
            4.0 * y * sum,
            4.0 * z * (sum - 2.0 * R * R) 
        );

        return normal.Normalized();
    }

    public override bool RayIntersection(in Ray ray, out Intersection intersection)
    {
        intersection = new Intersection(double.PositiveInfinity);

        Vector3d o = ray.Origin - LocalPos;
        Vector3d d = ray.Direction;

        double R = Radius;
        double r = RingRadius;

        double ox = o.X;
        double oy = o.Y;
        double oz = o.Z;

        double dx = d.X;
        double dy = d.Y;
        double dz = d.Z;

        double dd = Vector3d.Dot(d, d);
        double od = Vector3d.Dot(o, d);
        double oo = Vector3d.Dot(o, o);

        double k = oo + R * R - r * r;

        double a4 = dd * dd;

        double a3 = 4.0 * dd * od;

        double a2 =
            2.0 * dd * k +
            4.0 * od * od -
            4.0 * R * R * (dx * dx + dz * dz);

        double a1 =
            4.0 * od * k -
            8.0 * R * R * (ox * dx + oz * dz);

        double a0 =
            k * k -
            4.0 * R * R * (ox * ox + oz * oz);

        List<double> roots = MathUtil.SolveQuartic(a4, a3, a2, a1, a0);

        double closestT = double.PositiveInfinity;

        foreach (double t in roots)
        {
            if (t > MathUtil.Epsilon && t < closestT)
            {
                closestT = t;
            }
        }

        if (double.IsPositiveInfinity(closestT))
            return false;

        intersection.T = closestT;
        intersection.Position = ray.Origin + closestT * ray.Direction;
        intersection.Normal = GetNormal(intersection.Position);
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
        double outer = Radius + RingRadius;

        Vector3d min = LocalPos + new Vector3d(-outer, -RingRadius, -outer);
        Vector3d max = LocalPos + new Vector3d(outer, RingRadius, outer);

        return new AABB(min, max);
    }

    public override SceneNode Clone()
    {
        var copy = new Torus(LocalPos, Radius, RingRadius);
        if (Material != null) copy.SetMaterial(Material);
        return copy;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Solid: Torus");
        sb.AppendLine($"Material: {Material?.Id}");
        sb.AppendLine($"LocalPos: {LocalPos.ToString()}");
        sb.AppendLine($"Radius: {Radius}");
        sb.AppendLine($"RingRadius: {RingRadius}");

        return sb.ToString();
    }
}

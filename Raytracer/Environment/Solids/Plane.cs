using System.Text;
using OpenTK.Mathematics;
using Raytracer.Environment.Materials;
using Raytracer.Environment.Scene;
using Raytracer.Systems;
using Raytracer.Utils;
using Utils;

namespace Raytracer.Environment.Solids;

public record PlaneConfig(
    string Id,
    string? MaterialId,
    Vector3d? LocalPos
    ) : SceneNodeConfigBase(Id, MaterialId);

public class Plane : SolidSceneNode
{
    public static readonly Vector3d Normal = Vector3d.UnitY;
    public static readonly double Size = 1.0;

    public Plane(Vector3d? localPos)
    {
        LocalPos = localPos ?? Vector3d.Zero;
    }

    public Plane(PlaneConfig config)
        : this(config.LocalPos) { }

    public override bool RayIntersection(in Ray ray, out Intersection intersection)
    {
        intersection = new Intersection(double.PositiveInfinity);

        double denom = Vector3d.Dot(Normal, ray.Direction);

        if (IsZero(denom)) return false;

        double t = Vector3d.Dot(Normal, LocalPos - ray.Origin) / denom;
        if (t < 0.0) return false;

        intersection.T = t;
        intersection.Position = ray.Origin + ray.Direction * t;
        intersection.Normal = Normal;
        intersection.ClosestSolid = this;

        return true;
    }

    public override Ray? GetRefractedRay(Ray incomingRay, Intersection intersection)
    {
        var material = Material as DielectricMaterial;
        if (material == null) return null;

        double n1 = 1.0; // Assume air outside
        double n2 = material.RefractiveIndex;

        bool success = DielectricMaterial.TryRefract(
            incomingRay.Direction,
            intersection.Normal,
            n1,
            n2,
            out Vector3d refractedDir);

        if (!success) return null;

        Vector3d refractedOrigin = intersection.Position + refractedDir * MathUtil.Epsilon;

        return new Ray(refractedOrigin, refractedDir);
    }

    public override AABB GetBoundingBox()
    {
        var corner = new Vector3d(Size, MathUtil.Epsilon, Size);
        return new AABB(LocalPos - corner, LocalPos + corner);
    }

    public override SceneNode Clone()
    {
        var copy = new Plane(LocalPos);
        if (Material != null) copy.SetMaterial(Material);
        return copy;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Solid: Plane");
        sb.AppendLine($"Material: {Material?.Id}");
        sb.AppendLine($"LocalPos: {LocalPos.ToString()}");
        sb.AppendLine($"Normal: {Normal}");

        return sb.ToString();   
    }
}

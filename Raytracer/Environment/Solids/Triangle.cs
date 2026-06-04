using System.Text;
using OpenTK.Mathematics;
using Raytracer.Environment.Materials;
using Raytracer.Environment.Scene;
using Raytracer.Systems;
using Raytracer.Utils;
using Utils;

namespace Raytracer.Environment.Solids;

public record TriangleConfig(
    string Id,
    string? MaterialId,
    Vector3d? LocalPos,
    Vector3d V1,
    Vector3d V2,
    Vector3d V3
    ) : SceneNodeConfigBase(Id, MaterialId);

public class Triangle : SolidSceneNode
{
    public Vector3d V1 { get; set; }
    public Vector3d V2 { get; set; }
    public Vector3d V3 { get; set; }

    public Triangle(Vector3d? localPos, Vector3d v1, Vector3d v2, Vector3d v3)
    {
        LocalPos = localPos ?? Vector3d.Zero;
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }

    public Triangle(TriangleConfig config)
        : this(config.LocalPos, config.V1, config.V2, config.V3) { }

    public override bool RayIntersection(in Ray ray, out Intersection intersection)
    {
        intersection = new Intersection(double.PositiveInfinity);

        Vector3d a = LocalPos + V1;
        Vector3d b = LocalPos + V2;
        Vector3d c = LocalPos + V3;

        Vector3d e1 = b - a;
        Vector3d e2 = c - a;

        Vector3d.Cross(in ray.Direction, in e2, out Vector3d pvec);
        Vector3d.Dot(in e1, in pvec, out double det);
        if (IsZero(det)) return false;

        double detInv = 1.0 / det;
        Vector3d tvec = ray.Origin - a;
        double u = Vector3d.Dot(tvec, pvec) * detInv;
        if (u < 0.0 || u > 1.0) return false;

        Vector3d qvec = Vector3d.Cross(tvec, e1);
        double v = Vector3d.Dot(ray.Direction, qvec) * detInv;
        if (v < 0.0 || u + v > 1.0) return false;

        double t = Vector3d.Dot(e2, qvec) * detInv;
        if (t < 0.0) return false;

        intersection.T = t;
        intersection.Position = ray.Origin + ray.Direction * t;

        Vector3d normal = Vector3d.Cross(e1, e2);
        normal.Normalize();
        intersection.Normal = normal;
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
        Vector3d a = LocalPos + V1;
        Vector3d b = LocalPos + V2;
        Vector3d c = LocalPos + V3;

        var min = new Vector3d(
            Math.Min(a.X, Math.Min(b.X, c.X)),
            Math.Min(a.Y, Math.Min(b.Y, c.Y)),
            Math.Min(a.Z, Math.Min(b.Z, c.Z))
        );

        var max = new Vector3d(
            Math.Max(a.X, Math.Max(b.X, c.X)),
            Math.Max(a.Y, Math.Max(b.Y, c.Y)),
            Math.Max(a.Z, Math.Max(b.Z, c.Z))
        );

        min -= new Vector3d(MathUtil.Epsilon);
        max += new Vector3d(MathUtil.Epsilon);

        return new AABB(min, max);
    }

    public override SceneNode Clone()
    {
        var copy = new Triangle(LocalPos, V1, V2, V3);
        if (Material != null) copy.SetMaterial(Material);
        return copy;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Solid: Triangle");
        sb.AppendLine($"Material: {Material?.Id}");
        sb.AppendLine($"LocalPos: {LocalPos.ToString()}");
        sb.AppendLine($"V1: {V1.ToString()}");
        sb.AppendLine($"V2: {V2.ToString()}");
        sb.AppendLine($"V3: {V3.ToString()}");

        return sb.ToString();
    }
}

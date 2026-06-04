using OpenTK.Mathematics;
using Raytracer.Systems;
using Raytracer.Utils;
using Utils;

namespace Raytracer.Environment.Materials;

public record DielectricMaterialConfig(
    string Id,
    Vector3d Color,
    double Ambient,
    double Diffuse,
    double Specular,
    double Shininess,
    double Reflection,
    double Transparency,
    double RefractiveIndex
    ) : MaterialConfigBase(
        Id,
        Color,
        Ambient,
        Diffuse,
        Specular,
        Shininess,
        Reflection);

public class DielectricMaterial : MaterialBase
{
    public double Transparency { get; init; } // how much refracted color contributes
    public double RefractiveIndex { get; init; } // how strongly the ray bends

    public DielectricMaterial(
    string id,
    Vector3d color,
    double ambient,
    double diffuse,
    double specular,
    double shininess,
    double reflection,
    double transparency,
    double refractiveIndex) : base(
        id,
        color,
        ambient,
        diffuse, 
        specular, 
        shininess,
        reflection
        )
    {
        Transparency = transparency;
        RefractiveIndex = refractiveIndex;
    }
    public DielectricMaterial(DielectricMaterialConfig config)
    : this(
          config.Id,
          config.Color,
          config.Ambient,
          config.Diffuse,
          config.Specular,
          config.Shininess,
          config.Reflection,
          config.Transparency,
          config.RefractiveIndex)
    { }

    public static bool TryRefract(
        Vector3d incident,
        Vector3d normal,
        double n1,
        double n2,
        out Vector3d refracted)
    {
        incident = incident.Normalized();
        normal = normal.Normalized();

        // value controls how much the ray bends
        double eta = n1 / n2;

        double cosI = -Vector3d.Dot(normal, incident);
        if (cosI < 0.0)
        {
            normal = -normal;
            cosI = -Vector3d.Dot(normal, incident);
        }

        double sinT2 = eta * eta * (1.0 - cosI * cosI);

        if (sinT2 > 1.0)
        {
            refracted = Vector3d.Zero;
            return false;
        }

        double cosT = Math.Sqrt(1.0 - sinT2);

        refracted = eta * incident + (eta * cosI - cosT) * normal;
        refracted.Normalize();

        return true;
    }
}

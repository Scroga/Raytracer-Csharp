using OpenTK.Mathematics;
using Raytracer.Utils;
using Raytracer.Systems;
using Utils;

namespace Raytracer.Environment.Materials;
public record MetallicMaterialConfig(
    string Id,
    Vector3d Color,
    double Ambient,
    double Diffuse,
    double Specular,
    double Shininess,
    double Reflection
    ) : MaterialConfigBase(
        Id,
        Color,
        Ambient,
        Diffuse,
        Specular,
        Shininess,
        Reflection);

public class MetallicMaterial : MaterialBase
{ 
    public MetallicMaterial(
    string id,
    Vector3d color,
    double ambient,
    double diffuse,
    double specular,
    double shininess,
    double reflection) : base(
        id,
        color,
        ambient,
        diffuse,
        specular,
        shininess,
        reflection)
    {
    }
    public MetallicMaterial(MetallicMaterialConfig config)
    : this(
          config.Id,
          config.Color,
          config.Ambient,
          config.Diffuse,
          config.Specular,
          config.Shininess,
          config.Reflection)
    { }
}

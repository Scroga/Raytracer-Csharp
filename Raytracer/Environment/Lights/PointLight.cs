using OpenTK.Mathematics;
using Raytracer.Systems;

namespace Raytracer.Environment.Lights;

public record PointLightConfig(
    Vector3d Position,
    Vector3d Color
   ) : LightConfigBase(Color);

public class PointLight : ILight
{
    public Vector3d Position { get; set; }
    public Vector3d Color { get; set; }

    // TODO: attenuation
    //public double Constant;
    //public double Linear;
    //public double Quadratic;

    public PointLight(Vector3d position, Vector3d color )
    {
        Position = position;
        Color = color;
    }

    public PointLight(PointLightConfig config)
        : this(config.Position, config.Color) { }

    public Vector3d GetDirection(Vector3d viewPoint)
    {
        return Position - viewPoint;
    }
}

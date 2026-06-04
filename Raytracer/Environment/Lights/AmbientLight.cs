using OpenTK.Mathematics;
using Raytracer.Systems;

namespace Raytracer.Environment.Lights;

public record AmbientLightConfig(
    Vector3d Color
   ) : LightConfigBase(Color);

public class AmbientLight : ILight
{
    public Vector3d Color { get; set; }

    public AmbientLight(Vector3d color)
    {
        Color = color;
    }

    public AmbientLight(AmbientLightConfig config)
        : this(config.Color) { }

    public Vector3d GetDirection(Vector3d viewPoint) {
        return new Vector3d(0.0);
    }
}

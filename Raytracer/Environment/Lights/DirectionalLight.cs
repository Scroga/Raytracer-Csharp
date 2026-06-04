using OpenTK.Mathematics;
using Raytracer.Systems;

namespace Raytracer.Environment.Lights;

public record DirectionalLightConfig(
    Vector3d Direction,
    Vector3d Color
   ) : LightConfigBase(Color);

public class DirectionalLight : ILight
{
    public Vector3d Direction { get; set; }
    public Vector3d Color { get; set; }

    public DirectionalLight(Vector3d direction, Vector3d color)
    {
        if (direction.LengthSquared < 1e-12)
            throw new ArgumentException("Direction cannot be zero.", nameof(direction));

        Direction = direction;
        Color = color;
    }

    public DirectionalLight(DirectionalLightConfig config)
        : this(config.Direction, config.Color) { }

    public Vector3d GetDirection(Vector3d viewPoint)
    {
        return Direction;
    }
}

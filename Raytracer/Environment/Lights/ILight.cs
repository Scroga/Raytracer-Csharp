using OpenTK.Mathematics;

namespace Raytracer.Environment.Lights;

public interface ILight
{
    public Vector3d Color { get; set; }
    public Vector3d GetDirection(Vector3d viewPoint); 
}

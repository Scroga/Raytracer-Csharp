using OpenTK.Mathematics;

namespace Raytracer.Environment.Camera;
using Raytracer.Utils;

public interface ICamera
{
    public int Width { get; init; }
    public int Height { get; init; }
    public void GenerateRay(double x, double y, out Ray ray);
    public void GenerateRaysWithJitter(double x, double y, int gridSize, out List<Ray> rays);
}

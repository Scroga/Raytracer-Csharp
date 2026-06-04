using OpenTK.Mathematics;
using Raytracer.Systems;
using Raytracer.Utils;

namespace Raytracer.Environment.Camera;

public record OrthographicCameraConfig(
    int Width,
    int Height,
    double Pitch,
    double Yaw
) : CameraConfigBase(Width, Height);

public class OrthographicCamera : ICamera
{
    public int Width { get; init; }
    public int Height { get; init; }

    public double Pitch { get; private set; }
    public double Yaw { get; private set; }

    public Vector3d P1 { get; private set; }
    public Vector3d P00 { get; private set; }
    public Vector3d Dx { get; private set; }
    public Vector3d Dy { get; private set; }

    public Random _random = new Random();

    public OrthographicCamera(int width, int height, double pitchDeg, double yawDeg)
    {
        Width = width;
        Height = height;
        Pitch = pitchDeg;
        Yaw = yawDeg;

        Setup();
    }

    public OrthographicCamera(OrthographicCameraConfig config)
    : this(config.Width, config.Height, config.Pitch, config.Yaw) { }


    private void Setup()
    {
        double xMin = -1.0;
        double xMax = 1.0;
        double yMin = -(double)Height / Width;
        double yMax = -yMin;

        Vector3d p1 = new(0.0, 0.0, 1.0);                    // orthographic ray direction (basic orientation = z)
        Vector3d p00 = new(xMin, yMax, -5.0);                // upper left corner of the screen

        Vector3d dx = new((xMax - xMin) / Width, 0.0, 0.0);  // horizontal pixel step
        Vector3d dy = new(0.0, (yMin - yMax) / Height, 0.0); // vertical pixel step

        Matrix4d rotationMat =
            Matrix4d.CreateRotationX(MathHelper.DegreesToRadians(Pitch)) *
            Matrix4d.CreateRotationY(MathHelper.DegreesToRadians(-Yaw));

        P1 = Vector3d.TransformVector(p1, rotationMat);
        P00 = Vector3d.TransformPosition(p00, rotationMat);
        Dx = Vector3d.TransformVector(dx, rotationMat);
        Dy = Vector3d.TransformVector(dy, rotationMat);
    }

    public void GenerateRay(double x, double y, out Ray ray)
    {
        Vector3d origin = P00 + x * Dx + y * Dy;
        Vector3d direcion = P1;

        ray = new Ray(origin, direcion);
    }

    public void GenerateRaysWithJitter(double x, double y, int gridSize, out List<Ray> rays)
    {
        rays = new List<Ray>(gridSize * gridSize);

        for (int sy = 0; sy < gridSize; sy++)
        {
            for (int sx = 0; sx < gridSize; sx++)
            {
                double offsetX = (sx + _random.NextDouble()) / gridSize;
                double offsetY = (sy + _random.NextDouble()) / gridSize;

                int rayIndex = sy * gridSize + sx;

                GenerateRay(x + offsetX, y + offsetY, out Ray ray);
                rays.Add(ray);
            }
        }
    }

}
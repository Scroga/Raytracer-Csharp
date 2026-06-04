using System;
using OpenTK.Mathematics;
using Raytracer.Systems;
using Raytracer.Utils;

namespace Raytracer.Environment.Camera;

public record PerspectiveCameraConfig(
    int Width,
    int Height,
    double FovDeg,
    Vector3d Center,
    Vector3d Dir,
    Vector3d Up
) : CameraConfigBase(Width, Height);

public class PerspectiveCamera : ICamera
{
    private const double Epsilon = 1e-12;

    public int Width { get; init; }
    public int Height { get; init; }
    public double FovDeg { get; private set; }

    public Vector3d Center { get; private set; }
    public Vector3d Dir { get; private set; }
    public Vector3d Up { get; private set; }
    public Vector3d P00 { get; private set; }
    public Vector3d Dx { get; private set; }
    public Vector3d Dy { get; private set; }

    private Vector3d _right;

    private Random _random = new Random();

    public PerspectiveCamera(
        int width, int height, double fovDeg,
        Vector3d center, Vector3d dir, Vector3d up)
    {
        Width = width;
        Height = height;
        FovDeg = fovDeg;
        Center = center;
        Dir = dir.Normalized();
        Up = up.Normalized();
        _right = Vector3d.Cross(Up, Dir).Normalized();
        Setup();
    }

    public PerspectiveCamera(PerspectiveCameraConfig config)
    : this(
          config.Width, config.Height, config.FovDeg,
          config.Center, config.Dir, config.Up)
    { }

    private void Setup()
    {
        double aspect = (double)Width / Height;
        double fovRad = MathHelper.DegreesToRadians(FovDeg);

        double halfHeight = Math.Tan(fovRad * 0.5);
        double halfWidth = aspect * halfHeight;

        Vector3d imageCenter = Center + Dir;

        P00 = imageCenter - halfWidth * _right + halfHeight * Up;
        Dx = (2.0 * halfWidth / Width) * _right;
        Dy = (-2.0 * halfHeight / Height) * Up;
    }

    private void GenerateRayAtPixelPos(double x, double y, out Ray ray) {
        Vector3d origin = Center;
        Vector3d pixelCenter = P00 + x * Dx + y * Dy;
        Vector3d direction = (pixelCenter - origin).Normalized();

        ray = new Ray(origin, direction);
    }

    public void GenerateRay(double x, double y, out Ray ray)
    {
        GenerateRayAtPixelPos(x + 0.5, y + 0.5, out ray);
    }

    public void GenerateRaysWithJitter(double x, double y, int gridSize, out List<Ray> rays) {
        rays = new List<Ray>(gridSize * gridSize);

        for (int sy = 0; sy < gridSize; sy++) {
            for (int sx = 0; sx < gridSize; sx++) {
                double offsetX = (sx + _random.NextDouble()) / gridSize;
                double offsetY = (sy + _random.NextDouble()) / gridSize;

                int rayIndex = sy * gridSize + sx;

                GenerateRayAtPixelPos(x + offsetX, y + offsetY, out Ray ray);
                rays.Add(ray);
            }
        }
    }
}
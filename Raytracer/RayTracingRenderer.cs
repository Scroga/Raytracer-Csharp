using System.Text;
using OpenTK.Mathematics;
using Raytracer.Environment.Lights;
using Raytracer.Environment.Materials;
using Raytracer.Environment.Scene;
using Raytracer.Systems;
using Raytracer.Utils;
using Utils;

namespace Raytracer;

public record RaytracerAlgorithmConfig(
    bool Threaded,
    bool Shadows,
    bool Reflections,
    int SamplesPerPixel,
    int MaxDepth,
    int TileSize,
    float MinPerformance
    );

public class RayTracingRenderer
{
    private bool _threaded;
    private bool _shadowsEnabled;
    private bool _reflectionsEnabled;
    private int _samplesPerPixel;
    private int _maxDepth;
    private float _minPerformance;
    private int _samplesPerPixelSide;
    private int _tileSize;

    private bool _verbose;

    public RayTracingRenderer(
        bool threaded,
        bool shadowsEnabled,
        bool reflectionsEnabled,
        int samplesPerPixel,
        int maxDepth,
        float minPerformance,
        int tileSize,
        bool verbose)
    {
        _verbose = verbose;

        int gridSize = (int)Math.Sqrt(samplesPerPixel);
        if (gridSize * gridSize != samplesPerPixel)
        {
            if (_verbose)
            {
                Console.Error.WriteLine("Warning: SamplesPerPixel must be a perfect square: 1, 4, 9, 16, 25, ...");
            }
            samplesPerPixel = gridSize * gridSize;
        }

        _threaded = threaded;
        _shadowsEnabled = shadowsEnabled;
        _reflectionsEnabled = reflectionsEnabled;
        _samplesPerPixel = samplesPerPixel;
        _samplesPerPixelSide = gridSize;
        _maxDepth = maxDepth;
        _minPerformance = minPerformance;
        _tileSize = tileSize;
    }
    public RayTracingRenderer(RaytracerAlgorithmConfig config, bool verbose)
        : this(
              config.Threaded,
              config.Shadows,
              config.Reflections,
              config.SamplesPerPixel,
              config.MaxDepth,
              config.MinPerformance,
              config.TileSize,
              verbose)
    { }

    private bool IsInShadow(ILight lightSource, SceneNode rootNode, Intersection intersection)
    {
        Vector3d toLight = lightSource.GetDirection(intersection.Position);
        double lightDistance = toLight.Length;
        Vector3d lightDir = toLight.Normalized();

        Vector3d shadowOrigin = intersection.Position + intersection.Normal * MathUtil.Epsilon;
        var shadowRay = new Ray(shadowOrigin, lightDir);

        if (FindClosestIntersection(rootNode, shadowRay, out Intersection shadowHit))
        {
            return shadowHit.T > MathUtil.Epsilon && shadowHit.T < lightDistance;
        }

        return false;
    }

    private Vector3d TraceRay(Scene scene, Ray ray, int depth, double rayIntensity = 1.0)
    {
        if (depth <= 0) return scene.BackgroundColor;

        Vector3d color = Vector3d.Zero;

        Intersection intersection = new();
        FindClosestIntersection(scene.RootNode, ray, out intersection);

        if (!intersection.HasValidColorSource())
            return scene.BackgroundColor;

        // Compute local lighting
        foreach (var lightSource in scene.Lights)
        {
            if (lightSource is AmbientLight || !_shadowsEnabled)
            {
                color += intersection.ClosestSolid!.Material!.Evaluate(lightSource, intersection, ray);
            }
            else if (!IsInShadow(lightSource, scene.RootNode, intersection))
            {
                color += intersection.ClosestSolid!.Material!.Evaluate(lightSource, intersection, ray);
            }
        }

        // Trace reflected ray recursively
        if (depth > 1 && rayIntensity > _minPerformance)
        {
            var material = intersection.ClosestSolid!.Material;
            Vector3d secondaryColor = Vector3d.Zero;

            double secondaryRayContribution = material!.Reflection * rayIntensity;
   
            if (material is DielectricMaterial dielectricMaterial)
            {
                var reflectedRay = dielectricMaterial.GetReflectedRay(ray, intersection);
                var refractedRay = intersection.ClosestSolid!.GetRefractedRay(ray, intersection);

                Vector3d reflectedColor = TraceRay(scene, reflectedRay, depth - 1, secondaryRayContribution);
                Vector3d refractedColor = refractedRay != null
                          ? TraceRay(scene, refractedRay, depth - 1, secondaryRayContribution)
                          : reflectedColor;

                secondaryColor = dielectricMaterial.Transparency * refractedColor
                    + (1.0 - dielectricMaterial.Transparency) * reflectedColor;
            }
            else if (material is MetallicMaterial metallicMaterial)
            {
                var reflectedRay = metallicMaterial.GetReflectedRay(ray, intersection);

                secondaryColor = TraceRay(scene, reflectedRay, depth - 1, secondaryRayContribution);
            }

            color = (1.0 - secondaryRayContribution) * color + secondaryRayContribution * secondaryColor;

        }

        return color;
    }

    private bool FindClosestIntersection(SceneNode node, Ray ray, out Intersection closestIntersection)
    {
        closestIntersection = new Intersection(double.PositiveInfinity);

        if (node is GroupSceneNode group)
        {
            if (!group.GetBoundingBox().RayIntersection(ray, out Intersection aabbInter))
            {
                return false;
            }

            var localRay = ray.Transformed(group.InverseTransform);

            bool hitAnything = false;

            foreach (SceneNode child in group.Children)
            {
                if (FindClosestIntersection(child, localRay, out Intersection childInter))
                {
                    childInter.Transform(group.DirectTransform, group.InverseTransposedTransform, ray);

                    if (childInter.T > MathUtil.Epsilon && childInter.T < closestIntersection.T)
                    {
                        closestIntersection = childInter;
                        hitAnything = true;
                    }
                }
            }

            return hitAnything;
        }
        else if (node is SolidSceneNode solid)
        {
            return solid.RayIntersection(ray, out closestIntersection);
        }

        return false;
    }

    private Vector3d ComputeColor(Scene scene, int x, int y)
    {
        int recursionDepth = _reflectionsEnabled ? _maxDepth : 1;

        if (_samplesPerPixel > 1)
        {
            scene.Camera.GenerateRaysWithJitter(x, y, _samplesPerPixelSide, out var rays);

            Vector3d color = Vector3d.Zero;
            for (int i = 0; i < _samplesPerPixel; i++)
                color += TraceRay(scene, rays[i], recursionDepth);

            return color / _samplesPerPixel;
        }
        else
        {
            scene.Camera.GenerateRay(x, y, out var ray);
            return TraceRay(scene, ray, recursionDepth);
        }
    }

    public void Render(Scene scene, FloatImage image)
    {
        if (_threaded)
        {
            int tilesCountX = (scene.Camera.Width + _tileSize - 1) / _tileSize;
            int tilesCountY = (scene.Camera.Height + _tileSize - 1) / _tileSize;

            int totalTiles = tilesCountX * tilesCountY;

            Parallel.For(0, totalTiles, tileIndex =>
            {
                int tileX = tileIndex % tilesCountX;
                int tileY = tileIndex / tilesCountX;

                int startX = tileX * _tileSize;
                int startY = tileY * _tileSize;

                int endX = Math.Min(startX + _tileSize, scene.Camera.Width);
                int endY = Math.Min(startY + _tileSize, scene.Camera.Height);

                for (int x = startX; x < endX; x++)
                {
                    for (int y = startY; y < endY; y++)
                    {
                        var color = ComputeColor(scene, x, y);
                        image.PutPixel(x, y, color);
                    }
                }
            });
        }
        else
        {
            for (int x = 0; x < scene.Camera.Width; x++)
            {
                for (int y = 0; y < scene.Camera.Height; y++)
                {
                    var color = ComputeColor(scene, x, y);
                    image.PutPixel(x, y, color);
                }
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Threaded:            {_threaded.ToString()}");
        sb.AppendLine($"TileSize:            {_tileSize.ToString()}");
        sb.AppendLine($"Shadows enabled:     {_shadowsEnabled.ToString()}");
        sb.AppendLine($"Reflections enabled: {_reflectionsEnabled.ToString()}");
        sb.AppendLine($"Samples per pixel:   {_samplesPerPixel}");
        sb.AppendLine($"Max recursion depth: {_maxDepth}");
        sb.AppendLine($"Min performance:     {_minPerformance}");

        return sb.ToString();
    }
}
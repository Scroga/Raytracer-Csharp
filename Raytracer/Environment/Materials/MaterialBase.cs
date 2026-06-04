using OpenTK.Mathematics;
using Raytracer.Environment.Lights;
using Raytracer.Utils;
using Utils;

namespace Raytracer.Environment.Materials;

// Opaque
// Transparent
// Translucent
// Emissive

public abstract class MaterialBase
{
    public string Id { get; init; }
    public Vector3d Color { get; init; }
    public double Ambient { get; init; }
    public double Diffuse { get; init; }
    public double Specular { get; init; }
    public double Shininess { get; init; }

    public double Reflection { get; init; }

    protected MaterialBase(
        string id,
        Vector3d color,
        double ambient,
        double diffuse,
        double specular,
        double shininess,
        double reflection)
    {
        Id = id;
        Color = color;
        Ambient = ambient;
        Diffuse = diffuse;
        Specular = specular;
        Shininess = shininess;
        Reflection = reflection;
    }

    protected virtual Vector3d EvaluateAmbientLight(
        AmbientLight lightSource,
        Intersection intersection,
        Ray ray)
    {
        return Color * lightSource.Color * Ambient;
    }

    protected virtual Vector3d EvaluatePointLight(
        PointLight lightSource,
        Intersection intersection,
        Ray ray)
    {
        // TODO: attenuation

        // vector from intersectionPos to lightPos
        Vector3d lightDir = (lightSource.Position - intersection.Position).Normalized();
        Vector3d viewDir = (ray.Origin - intersection.Position).Normalized();

        double diff = Math.Max(Vector3d.Dot(intersection.Normal, lightDir), 0.0);
        Vector3d diffuse = Color * lightSource.Color * Diffuse * diff;

        Vector3d specular = Vector3d.Zero;
        if (diff > 0.0)
        {
            Vector3d halfwayDir = (lightDir + viewDir).Normalized();
            double spec = Math.Pow(
                Math.Max(Vector3d.Dot(intersection.Normal, halfwayDir), 0.0),
                Shininess);

            specular = lightSource.Color * Specular * spec;
        }

        return diffuse + specular;
    }

    protected virtual Vector3d EvaluateDirectionalLight(
        DirectionalLight lightSource,
        Intersection intersection,
        Ray ray)
    {
        Vector3d lightDir = (-lightSource.Direction).Normalized();
        Vector3d viewDir = (ray.Origin - intersection.Position).Normalized();

        double diff = Math.Max(Vector3d.Dot(intersection.Normal, lightDir), 0.0);
        Vector3d diffuse = Color * lightSource.Color * Diffuse * diff;

        Vector3d specular = Vector3d.Zero;
        if (diff > 0.0)
        {
            Vector3d reflectDir = Reflect(-lightDir, intersection.Normal).Normalized();

            double spec = Math.Pow(
                Math.Max(Vector3d.Dot(viewDir, reflectDir), 0.0),
                Shininess);

            specular = lightSource.Color * Specular * spec;
        }

        return diffuse + specular;
    }

    protected static Vector3d Reflect(Vector3d incident, Vector3d normal)
    {
        return incident - 2.0 * Vector3d.Dot(incident, normal) * normal;
    }

    public Vector3d Evaluate(
        ILight lightSource,
        Intersection intersection,
        Ray ray)
    {
        return lightSource switch
        {
            AmbientLight ambientLight => EvaluateAmbientLight(ambientLight, intersection, ray),
            PointLight pointLight => EvaluatePointLight(pointLight, intersection, ray),
            DirectionalLight dirLight => EvaluateDirectionalLight(dirLight, intersection, ray),
            _ => Vector3d.Zero
        };
    }

    public virtual Ray GetReflectedRay(Ray incomingRay, Intersection intersection)
    {
        Vector3d normal = intersection.Normal.Normalized();
        Vector3d incoming = incomingRay.Direction.Normalized();
        Vector3d reflectedDir = incoming - 2.0 * Vector3d.Dot(incoming, normal) * normal;
        reflectedDir.Normalize();

        Vector3d reflectedOrigin = intersection.Position + reflectedDir * MathUtil.Epsilon;
        var reflectedRay = new Ray(reflectedOrigin, reflectedDir);

        return reflectedRay;
    }
}

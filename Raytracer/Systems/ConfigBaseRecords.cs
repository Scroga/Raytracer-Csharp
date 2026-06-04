using System.Text.Json.Serialization;
using OpenTK.Mathematics;
using Raytracer.Environment.Camera;
using Raytracer.Environment.Lights;
using Raytracer.Environment.Materials;
using Raytracer.Environment.Scene;
using Raytracer.Environment.Solids;

namespace Raytracer.Systems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(OrthographicCameraConfig), "orthographic")]
[JsonDerivedType(typeof(PerspectiveCameraConfig), "perspective")]
public abstract record CameraConfigBase(
    int Width,
    int Height
);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(NodeReferenceConfig), "nodeId")]
[JsonDerivedType(typeof(GroupSceneNodeConfig), "group")]
[JsonDerivedType(typeof(SphereConfig), "sphere")]
[JsonDerivedType(typeof(TriangleConfig), "triangle")]
[JsonDerivedType(typeof(TorusConfig), "torus")]
[JsonDerivedType(typeof(PlaneConfig), "plane")]
[JsonDerivedType(typeof(CubeConfig), "cube")]
public abstract record SceneNodeConfigBase(
    string? Id,
    string? MaterialId
);

public record NodeReferenceConfig(
    string RefId,
    string? MaterialId
) : SceneNodeConfigBase(null, MaterialId);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(DielectricMaterialConfig), "dielectric")]
[JsonDerivedType(typeof(MetallicMaterialConfig), "metallic")]
public abstract record MaterialConfigBase(
    string Id,
    Vector3d Color,
    double Ambient,
    double Diffuse,
    double Specular,
    double Shininess,
    double Reflection
);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(AmbientLightConfig), "ambient")]
[JsonDerivedType(typeof(PointLightConfig), "point")]
[JsonDerivedType(typeof(DirectionalLightConfig), "directional")]
public abstract record LightConfigBase(
    Vector3d Color
);
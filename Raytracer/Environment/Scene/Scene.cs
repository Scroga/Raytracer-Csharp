using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using Raytracer.Environment.Camera;
using Raytracer.Environment.Lights;
using Raytracer.Environment.Materials;
using Raytracer.Environment.Solids;
using Raytracer.Systems;

namespace Raytracer.Environment.Scene;

public record SceneConfig(
    Vector3d BackgroundColor,
    CameraConfigBase CameraConfig,
    List<LightConfigBase> LightsConfig,
    List<MaterialConfigBase> MaterialsConfig,
    List<SceneNodeConfigBase> SceneNodes,
    SceneNodeConfigBase SceneHierarchy
    );

public class Scene
{
    public ICamera Camera { get; private set; }
    public List<ILight> Lights { get; private set; }
    public Vector3d BackgroundColor { get; set; }

    public SceneNode RootNode { get; private set; }

    private Dictionary<string, MaterialBase> _materialsDict;
    private Dictionary<string, SceneNode> _nodesDict;

    public Scene(SceneConfig config)
    {
        SetupCamera(config.CameraConfig);
        Debug.Assert(Camera != null);

        SetupLights(config.LightsConfig);
        Debug.Assert(Lights != null);

        SetupMaterials(config.MaterialsConfig);
        Debug.Assert(_materialsDict != null);

        SetupNodes(config.SceneNodes);
        Debug.Assert(_nodesDict != null);

        RootNode = BuildNode(config.SceneHierarchy);
        Debug.Assert(RootNode != null);

        BackgroundColor = config.BackgroundColor;

        EvaluateSolidMatrices();
    }

    private void SetupCamera(CameraConfigBase cameraConfig)
    {
        Camera = cameraConfig switch
        {
            OrthographicCameraConfig ortho => new OrthographicCamera(ortho),
            PerspectiveCameraConfig persp => new PerspectiveCamera(persp),

            _ => throw new InvalidOperationException("Unknown camera config type.")
        };
    }

    private void SetupMaterials(List<MaterialConfigBase> materialsConfig)
    {
        _materialsDict = new Dictionary<string, MaterialBase>();
        foreach (var materialConfig in materialsConfig)
        {
            MaterialBase material = materialConfig switch
            {
                DielectricMaterialConfig dielectricConfig => new DielectricMaterial(dielectricConfig),
                MetallicMaterialConfig metalicConfig => new MetallicMaterial(metalicConfig),

                _ => throw new InvalidOperationException("Unknown material config type.")
            };
            _materialsDict[material.Id] = material;
        }
    }

    private void SetupLights(List<LightConfigBase> lightsConfig)
    {
        Lights = new List<ILight>();
        foreach (var lightConfig in lightsConfig)
        {
            ILight? light = lightConfig switch
            {
                AmbientLightConfig ambientConfig => new AmbientLight(ambientConfig),
                PointLightConfig pointLightConfig => new PointLight(pointLightConfig),
                DirectionalLightConfig directionalLightConfig => new DirectionalLight(directionalLightConfig),

                _ => throw new InvalidOperationException("Unknown light config type.")
            };
            Lights.Add(light);
        }
    }

    private void SetupNodes(List<SceneNodeConfigBase> nodesConfig)
    {
        _nodesDict = new Dictionary<string, SceneNode>();
        foreach (var nodeConfig in nodesConfig)
        {
            if (nodeConfig.Id == null)
            {
                Console.Error.WriteLine($"Warning: invalid node id.");
            }
            else if (!_nodesDict.TryAdd(nodeConfig.Id, BuildNode(nodeConfig)))
            {
                Console.Error.WriteLine($"Warning: node with id {nodeConfig.Id} already exists.");
            }
        }
    }

    private SceneNode BuildNode(SceneNodeConfigBase nodeConfig, MaterialBase? parentMaterial = null)
    {
        SceneNode sceneNode;
        MaterialBase? material = parentMaterial;

        if (nodeConfig.MaterialId != null)
        {
            if (!_materialsDict.TryGetValue(nodeConfig.MaterialId, out material))
                throw new InvalidOperationException($"Unknown material: {nodeConfig.MaterialId}");
        }

        switch (nodeConfig)
        {
            case NodeReferenceConfig nodeRefConfig:
                if (!_nodesDict.TryGetValue(nodeRefConfig.RefId!, out SceneNode? refNode))
                    throw new InvalidOperationException($"Unknown node id: {nodeRefConfig.RefId}");

                if (nodeRefConfig.MaterialId != null && !_materialsDict.TryGetValue(nodeRefConfig.MaterialId, out material))
                    throw new InvalidOperationException($"Unknown material: {nodeConfig.MaterialId}");

                sceneNode = refNode;
                break;

            case GroupSceneNodeConfig groupConfig:
                var children = new List<SceneNode>();
                foreach (SceneNodeConfigBase config in groupConfig.Children)
                {
                    var child = BuildNode(config, material);
                    children.Add(child);
                }

                sceneNode = new GroupSceneNode(
                    children,
                    groupConfig.Translation ?? Vector3d.Zero,
                    groupConfig.Rotation ?? Vector3d.Zero,
                    groupConfig.Scale ?? Vector3d.One);
                break;

            case SphereConfig sphereConfig:
                sceneNode = new Sphere(sphereConfig);
                break;
            case TriangleConfig triangleConfig:
                sceneNode = new Triangle(triangleConfig);
                break;
            case PlaneConfig planeConfig:
                sceneNode = new Plane(planeConfig);
                break;
            case CubeConfig cubeConfig:
                sceneNode = new Cube(cubeConfig);
                break;
            case TorusConfig torusConfig:
                sceneNode = new Torus(torusConfig);
                break;

            default:
                throw new InvalidOperationException("Unknown scene node config type.");
        }

        if (material != null)
            sceneNode?.TrySetMaterial(material);

        return sceneNode!;
    }

    private void EvaluateSolidMatrices()
    {
        EvaluateSolidMatricesRecursive(RootNode, Matrix4d.Identity);
    }

    private void EvaluateSolidMatricesRecursive(SceneNode node, Matrix4d parentObjectToWorld)
    {
        if (node is GroupSceneNode group)
        {
            Matrix4d currentObjectToWorld = group.DirectTransform * parentObjectToWorld;

            foreach (SceneNode child in group.Children)
            {
                EvaluateSolidMatricesRecursive(child, currentObjectToWorld);
            }
        }
        else if (node is SolidSceneNode solid)
        {
            solid.SetTransformMatrix(parentObjectToWorld);
        }
    }
}

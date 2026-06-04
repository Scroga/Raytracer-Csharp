# Checkpoint 2 

## Author: Ilia Riabko

At this checkpoint, the program receives the path to the `.json` file as a command-line argument. The `.json` file contains the scene definition; see the scene definition syntax [here](sceneDefinitionSyntax.md).
At the moment, the program includes a perspective camera, a material class, light sources, and several simple solids such as a sphere, triangle, and plane.

### Command line arguments
| short       | long                 | description                                                                | mandatory | 
| ----------- | -------------------- | ---------------------------------------------------------------------------| --------- |
| -c <string> | --config <string>    | Path to `.json` file with program configs (default `Configs/defaultScene.json`) | No        |

#### Example
The preview scene is defined in `previewScene.json` and can be rendered using the following command-line argument:
```
-c Configs/previewScene.json
```

#### Camera

The `PerspectiveCamera` class was implemented at this checkpoint. Camera parameters are set via the `.json` file. The class implements the `ICamera` interface, which contains `GenerateRay` method. This method takes pixel coordinate and outputs the ray origin and direction.
Additionally, the `OrthographicCamera` class is implemented.

### Solids

Classes for specific solids were implemented. Specifically, the program contains `Sphere`, `Triangle` and `Plane` classes, which implement the abstract method `RayIntersection` of the `SolidBase` class. This method takes the origin and direction of the ray and outputs an instance of the `Intersection` class. It returns `true` if the intersection occurs and `false` otherwise.
The `Intersection` class contains `Position`, `Normal` and `T`, where `T` represents the distance from the ray origin to the intersection point. This class may be changed later.

### Materials

The program contains the `OpaqueMaterial` class, which is implemented as a Blinn–Phong model. This class is derived from the `MaterialBase` class. The derived class implements abstract methods containing the logic for computing the effect of specific light sources.

### Lights

The program currently contains light source classes such as `AmbientLight`, `DirectionalLight` and `PointLight`, which implement the `ILight` interface. EEach class contains values used by the material class to evaluate color under different light source types.
The `PointLight` class will be later extended to support light attenuation.

### Use of AI
I used AI mainly as a quick check for some C# features. It also helped me to generate [sceneDefinitionSyntax.md](sceneDefinitionSyntax.md) file.
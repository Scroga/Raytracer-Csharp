# Checkpoint 3

## Author: Ilia Riabko

At this checkpoint, the program supports rendering simple shadows. Recursive ray tracing, including reflections, has also been implemented. The program is structured into logical components following object-oriented programming (OOP) principles.

### Command line arguments
| short       | long                 | description                                                                | mandatory | 
| ----------- | -------------------- | ---------------------------------------------------------------------------| --------- |
| -c <string> | --config <string>    | Path to `.json` file with program configs (default `Configs/defaultScene.json`) | No        |

#### Example
The preview scene is defined in `previewScene.json` and can be rendered using the following command-line argument:
```
-c Configs/previewScene.json
```

#### OOP
The program was divided into well-defined components from the beginning of development, so only minimal refactoring was required at this checkpoint.

### Shadows
Simple shadow computation is implemented by casting a secondary ray from each intersection point toward each light source. The light contribution is considered only if no obstacle is found between the light source and the point on the object’s surface.

### Recursive Ray-tracing
The program now supports reflection on shiny materials. When a ray intersects an object, the recursion depth determines whether a reflected ray is generated and traced further.


### Use of AI
I used AI mainly as a quick check for some C# features. It also helped me to generate [sceneDefinitionSyntax.md](sceneDefinitionSyntax.md) file.
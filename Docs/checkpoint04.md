# Checkpoint 4

## Author: Ilia Riabko

At this checkpoint, the program supports loading the complete scene hierarchy from a `JSON` file, including transformed group nodes, object references, and material assignment. Transparent dielectric materials and refraction computation have also been implemented, including handling rays passing through objects and total internal reflection. The renderer now supports anti-aliasing using multiple jittered samples per pixel.

### Command line arguments
| short         | long                | description                                                                           | mandatory |
| ------------- | ------------------- | ------------------------------------------------------------------------------------- | --------- |
| `-o <string>` | `--output <string>` | Output image file name. Default: `Output/output.pfm`                                  | No        |
| `-c <string>` | `--config <string>` | Path to `.json` file with renderer algorithm configs. Default: `Configs/default.json` | No        |
| `-s <string>` | `--scene <string>`  | Path to `.json` file with scene configs. Default: `Configs/Scenes/Spheres.json`       | No        |
| `-v`          | `--verbose`         | Enables detailed information output during program execution. Default: `false`        | No        |


#### Example
The preview scene is defined in `previewScene.json` and can be rendered using the following command-line argument:
```
--verbose -s "Configs/Scenes/Spheres.json" -c "Configs/default.json"
```

### Scene hierarchy read from file
I extended the ray tracer so that the whole scene can be loaded from a JSON configuration file instead of being hardcoded in the program. The scene is represented as a hierarchy of nodes: group nodes contain transformations and child nodes, while leaf nodes represent actual solids such as spheres, triangles, and planes.

Each group node stores translation, rotation, and scale parameters. During ray tracing, rays are transformed into the local coordinate space of child objects using inverse transformation matrices. I also added material inheritance, so a material assigned to a group can be automatically applied to its children unless they define their own material.

### Refractions
I added support for transparent dielectric materials. These materials contain a transparency value and a refractive index. When a ray hits a dielectric object, the renderer computes both reflected and refracted rays and combines their colors according to the material parameters.

Refraction is computed using Snell's law. The implementation handles the transition from air into the object and, for spheres, also traces the ray through the inside of the object and computes the outgoing refracted ray.

### Anti-aliasing
I added anti-aliasing by supporting multiple samples per pixel. The number of samples is read from the renderer configuration file as `SamplesPerPixel`.

For each pixel, the renderer can generate several rays with small jittered offsets inside the pixel area. The final pixel color is computed as the average of all sampled rays. The implementation expects the number of samples per pixel to be a perfect square, such as `1`, `4`, `9`, `16`, because samples are distributed over a square grid.

### Use of AI
I used AI mainly to check and improve some parts of the C# implementation, especially the scene hierarchy.
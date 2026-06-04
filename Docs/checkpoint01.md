# Checkpoint 1

## Author: Ilia Riabko

At this checkpoint, the program obtains path to .json file via command line argument using `CommandLine`. The provided `.json` file is parsed using `System.Text.Json`, and its contents are stored in configuration records, which are then used to initialize the program.

### Command line arguments
| short       | long                 | description                                                                | mandatory | 
| ----------- | -------------------- | ---------------------------------------------------------------------------| --------- |
| -c <string> | --config <string>    | Path to `.json` file with program configs (default `Configs/default.json`) | No        |

#### Example
```
-c Configs/multipleSpheres.json
```

### Config file

There are two available `.json` files: `default.json`, which sets up a simple scene with one sphere placed at the center, and `multipleSpheres.json` which sets up a scene with four spheres of different radii and positions.

The program reads all scene data from a `.json` file. The window size and the camera rotation are set via `.json` file as well as spheres with their center positions and radii.

A simple `OrthographicCamera` class was implemented. It sets up camera object from config. Instead of the single triangle implemented in the template program, the current version renders spheres defined in the input `.json` file. The `RaySphereIntersection` method was implemented in the `MathUtil` class.

### Use of AI
[Link to the chat about .json file reading in c#](https://chatgpt.com/share/69a18e00-7010-8007-9150-7bdf1939eb53).
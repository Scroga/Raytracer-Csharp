# Raytracer JSON Configuration and Scene Definition
 
This document describes the JSON files used by the raytracer project. There are two main kinds of configuration files:

1. **Renderer configuration** file which controls the raytracing algorithm.
2. **Scene configuration** file which defines the camera, lights, materials, reusable objects, and final scene hierarchy.

## 1. Renderer configuration: `default.json`
Example:

```json
{
  "Threaded": true,
  "TileSize": 16,
  "Shadows": true,
  "Reflections": true,
  "SamplesPerPixel": 9,
  "MaxDepth": 8,
  "MinPerformance": 0.01
}
```
| Field | Type | Meaning |
|---|---:|---|
| `Threaded` | `bool` | Enables parallel rendering. When enabled, the image is split into tiles and rendered using `Parallel.For`. |
| `TileSize` | `int` | Size of one render tile in pixels. Used only when threaded rendering is enabled. |
| `Shadows` | `bool` | Enables shadow rays. If disabled, all non-ambient lights contribute even if another object blocks the light. |
| `Reflections` | `bool` | Enables recursive secondary rays for reflective/refractive materials. If disabled, recursion depth is effectively `1`. |
| `SamplesPerPixel` | `int` | Number of rays per pixel. Used for anti-aliasing. The value should be a perfect square: `1`, `4`, `9`, `16`, ... |
| `MaxDepth` | `int` | Maximum recursion depth for reflected and refracted rays. |
| `MinPerformance` | `float` | Minimum ray contribution threshold. Very weak secondary rays stop being traced. |

### Notes about `SamplesPerPixel`

The renderer expects `SamplesPerPixel` to be a perfect square because it creates a square jittered sampling grid per pixel. For example:

- `1` means `1 x 1` sample.
- `4` means `2 x 2` samples.
- `9` means `3 x 3` samples.
- `16` means `4 x 4` samples.

If the value is not a perfect square, the renderer prints a warning in verbose mode and reduces it to the nearest lower square value.

---

## 2. Scene configuration overview

Scene files such as `Spheres.json` and `Solids.json` have this high-level structure:

```json
{
  "BackgroundColor": { "X": 0.1, "Y": 0.2, "Z": 0.3 },
  "CameraConfig": { ... },
  "LightsConfig": [ ... ],
  "MaterialsConfig": [ ... ],
  "SceneNodes": [ ... ],
  "SceneHierarchy": { ... }
}
```

The scene is structured as follows:

1. The camera is created from `CameraConfig`.
2. Lights are created from `LightsConfig`.
3. Materials are created from `MaterialsConfig` and stored by `Id`.
4. Reusable nodes are created from `SceneNodes` and stored by `Id`.
5. The final render tree is built from `SceneHierarchy`.
6. Object-to-world matrices are evaluated for all solid objects.

This means that `SceneNodes` is a library of reusable node definitions, while `SceneHierarchy` describes what is actually placed in the rendered scene.

---

## 3. Vector format
Many fields use `Vector3d`. In JSON, a vector is written as an object with `X`, `Y`, and `Z` fields:

```json
{
  "X": 1.0,
  "Y": 2.0,
  "Z": 3.0
}
```

---

## 4. Background color
The color returned when a ray does not hit any object, or when recursion ends without a valid intersection.

---

## 5. Camera definition
Example from the scene files:

```json
"CameraConfig": {
  "Type": "perspective",
  "Width": 1080,
  "Height": 720,
  "FovDeg": 40.0,
  "Center": {
    "X": 0.6,
    "Y": 0.0,
    "Z": -5.6
  },
  "Dir": {
    "X": 0.0,
    "Y": -0.03,
    "Z": 1.0
  },
  "Up": {
    "X": 0.0,
    "Y": 1.0,
    "Z": 0.0
  }
}
```

The camera describes how primary rays are generated.

| Field | Type | Meaning |
|---|---:|---|
| `Type` | `string` | Camera type. In the examples, this is `"perspective"`. The code also supports an orthographic camera config type. |
| `Width` | `int` | Output image width in pixels. |
| `Height` | `int` | Output image height in pixels. |
| `FovDeg` | `double` | Field of view in degrees. Used by the perspective camera. |
| `Center` | `Vector3d` | Camera position in world space. |
| `Dir` | `Vector3d` | Direction in which the camera looks. |
| `Up` | `Vector3d` | Camera up vector. |

---

## 6. Light definitions
Lights are defined in the `LightsConfig` array.

Example:

```json
"LightsConfig": [
  {
    "Type": "ambient",
    "Color": {
      "X": 1.0,
      "Y": 1.0,
      "Z": 1.0
    }
  },
  {
    "Type": "point",
    "Position": {
      "X": -10.0,
      "Y": 8.0,
      "Z": -6.0
    },
    "Color": {
      "X": 1.0,
      "Y": 1.0,
      "Z": 1.0
    }
  }
]
```

The scene builder supports three light config types:

- `ambient`
- `point`
- `directional`

### 6.1 Ambient light
Ambient light has no position or direction. It adds a constant base illumination to visible surfaces. In material evaluation, the ambient contribution is:

Example:
```json
{
  "Type": "ambient",
  "Color": { "X": 1.0, "Y": 1.0, "Z": 1.0 }
}
```

### 6.2 Point light
A point light emits light from a specific position. For every hit point, the renderer computes a direction from the intersection point to the light position. 

Example:
```json
{
  "Type": "point",
  "Position": { "X": -10.0, "Y": 8.0, "Z": -6.0 },
  "Color": { "X": 1.0, "Y": 1.0, "Z": 1.0 }
}
```

### 6.3 Directional light
A directional light represents light coming from a constant direction, as if from a very distant source.

Example:
```json
{
  "Type": "directional",
  "Direction": { "X": 0.0, "Y": -1.0, "Z": 0.0 },
  "Color": { "X": 1.0, "Y": 1.0, "Z": 1.0 }
}
```

---

## 7. Material definitions
Materials are defined in `MaterialsConfig`.

Example:

```json
{
  "Type": "metallic",
  "Id": "blue",
  "Ambient": 0.1,
  "Diffuse": 0.5,
  "Specular": 0.5,
  "Shininess": 150,
  "Reflection": 0.4,
  "Color": {
    "X": 0.2,
    "Y": 0.3,
    "Z": 1.0
  }
}
```

Each material must have an `Id`. Objects and groups use `MaterialId` to refer to a material by this id.

Common material fields:

| Field | Type | Meaning |
|---|---:|---|
| `Type` | `string` | Material type. Supported values include `"metallic"` and `"dielectric"`. |
| `Id` | `string` | Unique material name used by `MaterialId`. |
| `Color` | `Vector3d` | Base RGB color. |
| `Ambient` | `double` | Strength of ambient lighting. |
| `Diffuse` | `double` | Strength of diffuse lighting. |
| `Specular` | `double` | Strength of specular highlights. |
| `Shininess` | `double` | Controls specular highlight size. Larger values produce smaller, sharper highlights. |
| `Reflection` | `double` | Reflection coefficient used for recursive secondary rays. |

### 7.1 Metallic material
A metallic material uses the common material fields. It can produce diffuse lighting, specular lighting, and recursive reflections. Higher `Reflection` makes the material depend more on the reflected ray color.

Example:
```json
{
  "Type": "metallic",
  "Id": "gold",
  "Ambient": 0.2,
  "Diffuse": 0.2,
  "Specular": 0.8,
  "Shininess": 400,
  "Reflection": 0.6,
  "Color": {
    "X": 0.3,
    "Y": 0.2,
    "Z": 0.0
  }
}
```

### 7.2 Dielectric material
A dielectric material represents transparent or glass-like material.

Example:
```json
{
  "Type": "dielectric",
  "Id": "transparent",
  "Ambient": 0.1,
  "Diffuse": 0.6,
  "Specular": 0.9,
  "Shininess": 80,
  "Reflection": 0.9,
  "Transparency": 0.9,
  "RefractiveIndex": 1.5,
  "Color": {
    "X": 0.9,
    "Y": 0.9,
    "Z": 0.9
  }
}
```
It has all common material fields and two additional fields:

| Field | Type | Meaning |
|---|---:|---|
| `Transparency` | `double` | Controls how much refracted color contributes compared to reflected color. |
| `RefractiveIndex` | `double` | Index of refraction. Higher values bend rays more strongly. |

When the renderer hits a dielectric object and recursive reflections are enabled, it computes both reflected and refracted rays. The final secondary color is mixed using `Transparency`.

---

## 8. Scene nodes and scene hierarchy
The most important part of a scene file is the object hierarchy. It is split into two sections:

```json
"SceneNodes": [ ... ],
"SceneHierarchy": { ... }
```

### 8.1 `SceneNodes`: reusable object definitions
`SceneNodes` is a list of named reusable node definitions. These nodes are stored in a dictionary using their `Id`.

Example:
```json
"SceneNodes": [
  {
    "Type": "sphere",
    "Id": "yellowSphere",
    "MaterialId": "yellow",
    "Radius": 1.0
  },
  {
    "Type": "plane",
    "Id": "horizontalPlane"
  }
]
```

A node from `SceneNodes` is not necessarily rendered directly. It becomes part of the final scene only when it is referenced from `SceneHierarchy`, or from another group node that is itself referenced.

### 8.2 `SceneHierarchy`: final render tree
`SceneHierarchy` is the root node of the rendered scene. It can contain groups, direct solid definitions, and references to nodes from `SceneNodes`.

Example:
```json
"SceneHierarchy": {
  "Type": "group",
  "MaterialId": "white",
  "Translation": { "X": 0.0, "Y": 0.0, "Z": 0.0 },
  "Children": [
    {
      "Type": "group",
      "Translation": { "X": 0.0, "Y": -1.3, "Z": 0.0 },
      "Scale": { "X": 200.0, "Y": 1.0, "Z": 200.0 },
      "Children": [
        {
          "Type": "nodeId",
          "RefId": "horizontalPlane"
        }
      ]
    }
  ]
}
```

The root is usually a `group` node, because a group can contain several child objects and apply a transform or material to all of them.

---

## 9. Node types
Supported scene node types include:
- `group`
- `nodeId`
- `sphere`
- `plane`
- `cube`
- `torus`
- `triangle`

---

## 10. `nodeId`: node reference

A node reference is written as:

```json
{
  "Type": "nodeId",
  "RefId": "yellowSphere"
}
```

`nodeId` does not define new geometry. It refers to an existing node from `SceneNodes` using `RefId`.

| Field | Type | Meaning |
|---|---:|---|
| `Type` | `string` | Must be `"nodeId"`. |
| `RefId` | `string` | Id of a node defined in `SceneNodes`. |
| `MaterialId` | `string?` | Optional material override. |

Example with material override:

```json
{
  "Type": "nodeId",
  "RefId": "yellowSphere",
  "MaterialId": "red"
}
```

---

## 11. `group`: transform and hierarchy node
A group node contains child nodes and applies a transformation to them.

Example:
```json
{
  "Type": "group",
  "Translation": {
    "X": 1.4,
    "Y": -0.7,
    "Z": -0.5
  },
  "Scale": {
    "X": 1.0,
    "Y": 1.0,
    "Z": 1.0
  },
  "Rotation": {
    "X": 0.0,
    "Y": 0.0,
    "Z": 0.0
  },
  "Children": [
    {
      "Type": "nodeId",
      "RefId": "blueTorus"
    }
  ]
}
```

| Field | Type | Meaning |
|---|---:|---|
| `Type` | `string` | Must be `"group"`. |
| `Id` | `string?` | Optional when the group appears inside `SceneHierarchy`; required when the group is stored in `SceneNodes`. |
| `MaterialId` | `string?` | Optional material applied to children that do not already have a material. |
| `Translation` | `Vector3d?` | Optional translation. Defaults to `(0, 0, 0)`. |
| `Rotation` | `Vector3d?` | Optional rotation in degrees around X, Y, and Z axes. Defaults to `(0, 0, 0)`. |
| `Scale` | `Vector3d?` | Optional scale. Defaults to `(1, 1, 1)`. |
| `Children` | `array` | Child node definitions. |

### 11.1 Transform order

The group transform is built from components in this order:

```text
Scale * RotationX * RotationY * RotationZ * Translation
```

The rotation values are in degrees.

### 11.2 Bounding boxes

A group computes its bounding box by transforming each child bounding box by the group transform and merging the results. During ray traversal, the renderer first tests the ray against the group bounding box. If the ray misses the group bounding box, all children are skipped.

### 11.3 Material inheritance

Groups can define `MaterialId`. This material is passed to children as a parent material. A child receives the parent material only if it does not already have its own material.

For example:

```json
{
  "Type": "group",
  "MaterialId": "white",
  "Children": [
    {
      "Type": "nodeId",
      "RefId": "horizontalPlane"
    }
  ]
}
```

If `horizontalPlane` has no material, it receives `white`. If it already has a material, its own material is kept.

This behavior is implemented through `TrySetMaterial`: it sets the material only when the current material is `null`.

---

## 12. Solid node types
Solid nodes represent actual renderable geometry. Each solid node can have:

| Field | Type | Meaning |
|---|---:|---|
| `Type` | `string` | Geometry type. |
| `Id` | `string?` | Required when stored in `SceneNodes`. |
| `MaterialId` | `string?` | Optional material reference. |
| `LocalPos` | `Vector3d?` | Optional local position. Defaults to `(0, 0, 0)`. |

---
### 12.1 `sphere`
Example:

```json
{
  "Type": "sphere",
  "Id": "yellowSphere",
  "MaterialId": "yellow",
  "Radius": 1.0
}
```

| Field | Type | Meaning |
|---|---:|---|
| `Radius` | `double` | Sphere radius. |
| `LocalPos` | `Vector3d?` | Optional sphere center in local/object space. |

---
### 12.2 `plane`
Example:

```json
{
  "Type": "plane",
  "Id": "horizontalPlane",
  "Normal": {
    "X": 0.0,
    "Y": 1.0,
    "Z": 0.0
  }
}
```

---
### 12.3 `torus`
Example:

```json
{
  "Type": "torus",
  "Id": "blueTorus",
  "MaterialId": "blue",
  "Radius": 0.6,
  "RingRadius": 0.2
}
```
| Field | Type | Meaning |
|---|---:|---|
| `Radius` | `double` | Major radius: distance from the torus center to the center of the tube. |
| `RingRadius` | `double` | Minor radius: radius of the tube itself. |
| `LocalPos` | `Vector3d?` | Optional local torus position. |

---
### 12.4 `triangle`
Example:

```json
{
  "Type": "triangle",
  "Id": "triangle1",
  "MaterialId": "red",
  "LocalPos": { "X": 0.0, "Y": 0.0, "Z": 0.0 },
  "V1": { "X": 0.0, "Y": 0.0, "Z": 0.0 },
  "V2": { "X": 1.0, "Y": 0.0, "Z": 0.0 },
  "V3": { "X": 0.0, "Y": 1.0, "Z": 0.0 }
}
```

| Field | Type | Meaning |
|---|---:|---|
| `V1` | `Vector3d` | First triangle vertex. |
| `V2` | `Vector3d` | Second triangle vertex. |
| `V3` | `Vector3d` | Third triangle vertex. |
| `LocalPos` | `Vector3d?` | Optional offset added to all three vertices. |

---
### 12.5 'cube'
Example:

```json
{
  "Type": "cube",
  "Id": "redCube",
  "MaterialId": "red"
}
```

---

## 13. Transform propagation

Groups are responsible for transformations. Solids store object-to-world matrices after the whole hierarchy is built.

The scene builder recursively walks from the root node:

1. Start with identity matrix.
2. For each group, multiply the group transform by the parent transform.
3. For each solid, store the resulting matrix as its object-to-world transform.

During ray traversal, when the renderer enters a group, it transforms the ray into the group's local coordinate system. After a child intersection is found, the intersection position and normal are transformed back to the parent/world space.

This allows the same primitive intersection code to work in local object space while the hierarchy controls placement, scaling, and rotation.

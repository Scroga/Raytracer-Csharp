using OpenTK.Mathematics;
using Raytracer.Environment.Materials;
using Raytracer.Utils;

namespace Raytracer.Environment.Scene;

public abstract class SceneNode
{
    public MaterialBase? Material { get; private set; }
    public virtual void SetMaterial(MaterialBase material) {
        Material = material;
    }

    public virtual bool TrySetMaterial(MaterialBase material) {
        if (Material == null) {
            Material = material;
            return true;
        }
        return false;
    }

    public abstract SceneNode Clone();
    public abstract AABB GetBoundingBox();
}


using System.Data;
using OpenTK.Mathematics;
using Raytracer.Environment.Materials;
using Raytracer.Utils;

namespace Raytracer.Environment.Scene;

public abstract class SolidSceneNode : SceneNode
{
    public Matrix4d ObjectToWorldTransform { get; private set; } = Matrix4d.Identity;
    public Matrix4d WorldToObjectTransform { get; private set; } = Matrix4d.Identity;
    public Matrix4d ObjectToWorldNormalsTransform { get; private set; } = Matrix4d.Identity;
    public Matrix4d WorldToObjectNormalsTransform { get; private set; } = Matrix4d.Identity;

    public Vector3d LocalPos { get; set; }
   
    public static bool IsZero(double a) {
        return a <= double.Epsilon && a >= -double.Epsilon;
    }

    public void SetTransformMatrix(Matrix4d objectToWorld)
    {
        ObjectToWorldTransform = objectToWorld;
        WorldToObjectTransform = objectToWorld.Inverted();
        ObjectToWorldNormalsTransform = WorldToObjectTransform.Transposed();
        WorldToObjectNormalsTransform = ObjectToWorldTransform.Transposed();
    }

    public abstract bool RayIntersection(in Ray ray, out Intersection intersection);

    public abstract Ray? GetRefractedRay(Ray incomingRay, Intersection intersection);
}

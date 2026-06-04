using System.Text;
using OpenTK.Mathematics;
using Raytracer.Environment.Materials;
using Raytracer.Systems;
using Raytracer.Utils;

namespace Raytracer.Environment.Scene;

public record NodeId(
    string Id);

public record GroupSceneNodeConfig(
    string Id,
    string? MaterialId,
    Vector3d? Translation,
    Vector3d? Rotation,
    Vector3d? Scale,
    List<SceneNodeConfigBase> Children
    ) : SceneNodeConfigBase(Id, MaterialId);

public class GroupSceneNode : SceneNode
{
    private AABB _boundingBox = new();
    public List<SceneNode> Children { get; private set; }
    public Matrix4d DirectTransform { get; private set; } = Matrix4d.Identity;
    public Matrix4d InverseTransform { get; private set; } = Matrix4d.Identity;
    public Matrix4d InverseTransposedTransform { get; private set; } = Matrix4d.Identity;

    private GroupSceneNode(AABB bb, List<SceneNode> children)
    {
        _boundingBox = bb;
        Children = children;
    }

    public GroupSceneNode(
        List<SceneNode> children,
        Vector3d translation,
        Vector3d rotationDegrees,
        Vector3d scale)
    {
        SetTransformFromComponents(translation, rotationDegrees, scale);

        Children = children;

        foreach (var child in Children)
        {
            _boundingBox.Merge(child.GetBoundingBox().Transformed(DirectTransform));
        }
    }

    public void SetTransformFromComponents(
        Vector3d translation,
        Vector3d rotationDegrees,
        Vector3d scale)
    {
        Matrix4d scaleMatrix = Matrix4d.CreateScale(scale);

        Matrix4d rotationX = Matrix4d.CreateRotationX(MathHelper.DegreesToRadians(rotationDegrees.X));
        Matrix4d rotationY = Matrix4d.CreateRotationY(MathHelper.DegreesToRadians(rotationDegrees.Y));
        Matrix4d rotationZ = Matrix4d.CreateRotationZ(MathHelper.DegreesToRadians(rotationDegrees.Z));

        Matrix4d translationMatrix = Matrix4d.CreateTranslation(translation);

        SetTransformMatrix(scaleMatrix * rotationX * rotationY * rotationZ * translationMatrix);
    }

    public void SetTransformMatrix(Matrix4d directTransform)
    {
        DirectTransform = directTransform;
        InverseTransform = DirectTransform.Inverted();
        InverseTransposedTransform = InverseTransform.Transposed();
    }

    public override AABB GetBoundingBox()
    {
        return _boundingBox;
    }

    public override void SetMaterial(MaterialBase material)
    {
        foreach (var child in Children)
            child.SetMaterial(material);
    }

    public override bool TrySetMaterial(MaterialBase material)
    {
        bool allSet = true;
        foreach (var child in Children)
        {
            allSet &= child.TrySetMaterial(material);
        }
        return allSet;
    }

    public override SceneNode Clone()
    {
        var copiedAABB = new AABB(_boundingBox.Min, _boundingBox.Max);
        var copiedChildren = Children.Select(child => child.Clone()).ToList();

        var copy = new GroupSceneNode(copiedAABB, copiedChildren);
        copy.SetTransformMatrix(DirectTransform);
        if (Material != null) copy.SetMaterial(Material);
        return copy;
    }
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("_____________________________");
        sb.AppendLine("GroupNode");
        sb.AppendLine($"Bounding box: {_boundingBox}");
        sb.AppendLine($"Material: {Material?.Id ?? "null"}");

        sb.AppendLine("Children: ");
        foreach (var child in Children)
        {
            sb.AppendLine($"\n{child.ToString()}");
        }

        return sb.ToString();
    }
}

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Manual mesh-collider fix only. Automatic scene scanning was removed because it
/// freezes the Editor on SampleScene.
/// Use Tools/Fix High-Poly Mesh Colliders instead.
/// </summary>
public static class MeshColliderHighPolyEditorFix
{
}
#endif

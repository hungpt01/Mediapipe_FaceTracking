using UnityEngine;

[CreateAssetMenu(menuName = "FaceMask/Face Mesh 468 Mapping", fileName = "FaceMesh468Mapping")]
public class FaceMesh468MappingAsset : ScriptableObject
{
    [Tooltip("map[landmarkIndex] = meshVertexIndex")]
    public int[] map = new int[468];
}
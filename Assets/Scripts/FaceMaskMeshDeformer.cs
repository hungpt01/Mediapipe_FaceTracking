using UnityEngine;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using mptcc = Mediapipe.Tasks.Components.Containers;

[RequireComponent(typeof(MeshFilter))]
public class FaceMaskMeshDeformer : MonoBehaviour
{
  [Header("Mapping asset")]
  [SerializeField] private FaceMesh468MappingAsset mapping;

  [Header("Tuning")]
  public float xyScale = 1.0f;
  public float zScale = 1.0f;
  public bool flipY = false;

  [Header("Match Screen")]
  public Rect screenUvRect = new Rect(0, 0, 1, 1);

  private Mesh _mesh;
  private Vector3[] _verts;
  private bool _has;
  private mptcc.NormalizedLandmarks _face;

  void Awake()
  {
    _mesh = GetComponent<MeshFilter>().mesh;
    _mesh.MarkDynamic();

    if (_mesh.vertexCount != 468)
      Debug.LogError($"[DeformerMapped] Mesh vertexCount={_mesh.vertexCount}, expected 468. (Import may have split vertices)");

    _verts = _mesh.vertices;

    if (mapping == null || mapping.map == null || mapping.map.Length != 468)
      Debug.LogError("[DeformerMapped] Missing/invalid mapping asset. Bake it first.");
  }

  public void SetResult(FaceLandmarkerResult result)
  {
    _has = false;
    if (result.faceLandmarks == null || result.faceLandmarks.Count == 0) return;

    _face = result.faceLandmarks[0];
    if (_face.landmarks == null || _face.landmarks.Count < 468) return;

    _has = true;
  }

  void LateUpdate()
  {
    if (!_has || mapping == null || mapping.map == null || mapping.map.Length != 468) return;
    if (_mesh.vertexCount < 468) return;

    for (int i = 0; i < 468; i++)
    {
      int v = mapping.map[i]; // mesh vertex index for landmark i
      if ((uint)v >= (uint)_verts.Length) continue;

      var lm = _face.landmarks[i];

      // apply uvRect (mirror/flip from Screen)
      float px = screenUvRect.x + lm.x * screenUvRect.width;
      float py = screenUvRect.y + lm.y * screenUvRect.height;

      float x = (px - 0.5f) * xyScale;
      float y = (flipY ? (py - 0.5f) : (0.5f - py)) * xyScale;
      float z = lm.z * zScale;

      _verts[v] = new Vector3(x, y, z);
    }

    _mesh.vertices = _verts;
    _mesh.RecalculateBounds();
  }
}

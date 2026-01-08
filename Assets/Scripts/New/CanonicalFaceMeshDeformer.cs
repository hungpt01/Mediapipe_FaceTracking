using UnityEngine;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using mptcc = Mediapipe.Tasks.Components.Containers;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
public class CanonicalFaceMeshDeformer : MonoBehaviour
{
  [Header("Tuning")]
  public float xyScale = 1.0f;
  public float zScale = 1.0f;
  public bool flipY = false;

  [Header("Match Screen (copy from _screen.uvRect each frame)")]
  public Rect screenUvRect = new Rect(0, 0, 1, 1);

  [Header("Aspect")]
  [Tooltip("Nếu true, sẽ nhân X với aspect để tránh méo khi RT không vuông.")]
  public bool applyAspectToX = true;

  [Tooltip("Nếu bạn render ra RenderTexture, set aspect = (rtWidth/rtHeight). Nếu 0 sẽ dùng camera.aspect nếu có.")]
  public float overrideAspect = 0f;

  private Mesh _mesh;
  private Vector3[] _verts;

  private bool _has;
  private mptcc.NormalizedLandmarks _face;

  private void Awake()
  {
    _mesh = GetComponent<MeshFilter>().mesh;
    if (_mesh == null)
    {
      Debug.LogError("[CanonicalFaceMeshDeformer] Mesh is null. Did you build it first?");
      enabled = false;
      return;
    }

    if (_mesh.vertexCount != 468)
    {
      Debug.LogError($"[CanonicalFaceMeshDeformer] Mesh.vertexCount={_mesh.vertexCount}, expected 468. (Use CanonicalFaceMeshBuilder)");
      enabled = false;
      return;
    }

    _mesh.MarkDynamic();
    _verts = _mesh.vertices;
    if (_verts == null || _verts.Length != 468) _verts = new Vector3[468];
  }

  public void SetResult(FaceLandmarkerResult result)
  {
    _has = false;
    if (result.faceLandmarks == null || result.faceLandmarks.Count == 0) return;

    _face = result.faceLandmarks[0];
    if (_face.landmarks == null || _face.landmarks.Count < 468) return;

    _has = true;
  }

  private void LateUpdate()
  {
    if (!_has) return;

    float aspect = 1f;
    if (overrideAspect > 0f) aspect = overrideAspect;
    else
    {
      var cam = Camera.main;
      if (cam != null) aspect = cam.aspect;
    }

    for (int i = 0; i < 468; i++)
    {
      var lm = _face.landmarks[i];

      // Apply uvRect giống Screen (uvRect có thể flip bằng width/height âm)
      float px = screenUvRect.x + lm.x * screenUvRect.width;
      float py = screenUvRect.y + lm.y * screenUvRect.height;

      float x = (px - 0.5f) * xyScale;
      float y = (flipY ? (py - 0.5f) : (0.5f - py)) * xyScale;
      float z = lm.z * zScale;

      if (applyAspectToX) x *= aspect;

      _verts[i] = new Vector3(x, y, z);
    }

    _mesh.vertices = _verts;
    _mesh.RecalculateBounds();
  }
}

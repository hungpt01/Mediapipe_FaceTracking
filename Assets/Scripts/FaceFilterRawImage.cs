using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using mptcc = Mediapipe.Tasks.Components.Containers;

public class FaceFilterRawImage : RawImage
{
  [Header("Filter Texture")]
  [SerializeField] private Texture templateTexture;

  [Header("Match Screen")]
  public Rect screenUvRect = new Rect(0, 0, 1, 1);
  
  public Vector2[] uv468; // length 468
  public int[] triangles; // length multiple of 3

  private mptcc.NormalizedLandmarks _face;
  private bool _has;

  public void SetFaceAssets(TextAsset faceUv468, TextAsset faceTriangles468)
  {
    uv468 = Face468AssetsLoader.LoadUV(faceUv468);
    triangles = Face468AssetsLoader.LoadTriangles(faceTriangles468);
  }

  public void SetResult(FaceLandmarkerResult result)
  {
    _has = false;
    if (result.faceLandmarks == null || result.faceLandmarks.Count == 0) { SetVerticesDirty(); return; }
    _face = result.faceLandmarks[0];
    _has = _face.landmarks != null && _face.landmarks.Count >= 468;
    SetVerticesDirty();
  }

  protected override void OnEnable()
  {
    base.OnEnable();
    // if (templateTexture != null) texture = templateTexture;
  }

  private bool _warned;
  
  protected override void OnPopulateMesh(VertexHelper vh)
  {
    vh.Clear();

    if (!_has)
    {
      if (!_warned) { Debug.LogWarning("[FaceFilterRawImage] No face result yet"); _warned = true; }
      return;
    }

    if (uv468 == null || uv468.Length != 468 || triangles == null || triangles.Length < 3)
    {
      if (!_warned)
      {
        Debug.LogWarning($"[FaceFilterRawImage] Invalid assets. uv={uv468?.Length ?? 0}, tri={triangles?.Length ?? 0}");
        _warned = true;
      }
      return;
    }
    var rect = rectTransform.rect;

    for (int i = 0; i < 468; i++)
    {
      var lm = _face.landmarks[i]; // normalized (0..1) in input image space
      var p = ApplyUvRect(lm.x, lm.y, screenUvRect);

      float x = (p.x - 0.5f) * rect.width;
      float y = (p.y - 0.5f) * rect.height;

      var v = UIVertex.simpleVert;
      v.color = color;
      v.position = new Vector3(x, y, 0);
      v.uv0 = uv468[i];
      vh.AddVert(v);
    }

    for (int t = 0; t < triangles.Length; t += 3)
    {
      vh.AddTriangle(triangles[t], triangles[t + 1], triangles[t + 2]);
    }
  }

  // Map landmark normalized coords into the displayed texture region/flip defined by RawImage.uvRect
  private static Vector2 ApplyUvRect(float x, float y, Rect uv)
  {
    // IMPORTANT: uv.width/height can be negative (flip) as in Screen.ResetUvRect()【:contentReference[oaicite:5]{index=5}】
    // So we must use lerp with signed size.
    return new Vector2(uv.x + x * uv.width, uv.y + y * uv.height);
  }
}

using System;
using System.Globalization;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
public class CanonicalFaceMeshBuilder : MonoBehaviour
{
  [Header("Canonical Assets (TextAsset)")]
  [SerializeField] private TextAsset faceUv468;            // face_uv_468.txt
  [SerializeField] private TextAsset faceTriangulation468; // face_triangulation_468.txt

  [Header("Build Options")]
  [Tooltip("Nếu bật, sẽ build mesh ở Awake().")]
  [SerializeField] private bool buildOnAwake = true;

  [Tooltip("Dùng nếu bạn muốn thấy mesh khi chưa có landmark (debug).")]
  [SerializeField] private bool debugFillAsFlatGrid = false;

  public Mesh Mesh { get; private set; }

  private void Awake()
  {
    if (buildOnAwake) Build();
  }

  [ContextMenu("Build")]
  public void Build()
  {
    if (faceUv468 == null || faceTriangulation468 == null)
    {
      Debug.LogError("[CanonicalFaceMeshBuilder] Missing faceUv468 or faceTriangulation468 TextAsset.");
      return;
    }

    var uv = ParseUv(faceUv468);
    var tri = ParseTriangles(faceTriangulation468);

    if (uv == null || uv.Length != 468)
    {
      Debug.LogError($"[CanonicalFaceMeshBuilder] UV count invalid: {(uv == null ? 0 : uv.Length)} (expected 468)");
      return;
    }
    if (tri == null || tri.Length < 3 || tri.Length % 3 != 0)
    {
      Debug.LogError($"[CanonicalFaceMeshBuilder] Triangles count invalid: {(tri == null ? 0 : tri.Length)} (must be multiple of 3)");
      return;
    }
    for (int i = 0; i < tri.Length; i++)
    {
      int v = tri[i];
      if (v < 0 || v >= 468)
      {
        Debug.LogError($"[CanonicalFaceMeshBuilder] Triangle index out of range at {i}: {v} (expected 0..467)");
        return;
      }
    }

    var mf = GetComponent<MeshFilter>();

    Mesh = new Mesh { name = "CanonicalFaceMesh468" };
    Mesh.MarkDynamic();

    // vertices placeholder (sẽ update bằng landmark)
    var verts = new Vector3[468];
    if (debugFillAsFlatGrid)
    {
      // Debug: đặt vertex theo UV lên mặt phẳng để thấy mesh ngay cả khi chưa có landmark
      for (int i = 0; i < 468; i++)
      {
        verts[i] = new Vector3(uv[i].x - 0.5f, 0.5f - uv[i].y, 0f);
      }
    }

    Mesh.vertices = verts;
    Mesh.uv = uv;
    Mesh.triangles = tri;
    Mesh.RecalculateBounds();

    mf.sharedMesh = Mesh;

    Debug.Log($"[CanonicalFaceMeshBuilder] Built mesh: v=468, uv=468, tri={tri.Length / 3} triangles");
  }

  private static Vector2[] ParseUv(TextAsset txt)
  {
    var lines = txt.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    var uv = new Vector2[lines.Length];

    for (int i = 0; i < lines.Length; i++)
    {
      var parts = lines[i].Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length < 2) return null;

      float u = float.Parse(parts[0], CultureInfo.InvariantCulture);
      float v = float.Parse(parts[1], CultureInfo.InvariantCulture);
      uv[i] = new Vector2(u, v);
    }

    return uv;
  }

  private static int[] ParseTriangles(TextAsset txt)
  {
    // Triangulation txt thường là list int, có thể cách nhau bởi comma / whitespace / newline
    var tokens = txt.text.Split(new[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    var tri = new int[tokens.Length];
    for (int i = 0; i < tokens.Length; i++)
    {
      tri[i] = int.Parse(tokens[i], CultureInfo.InvariantCulture);
    }
    return tri;
  }
}

using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public class FaceMesh468MappingBaker
{
  [MenuItem("Tools/FaceMask/Bake 468 Mapping (UV -> Mesh Vertex)")]
  public static void Bake()
  {
    // 1) Bạn chọn 1 GameObject có MeshFilter trong Scene/PrefabStage
    var go = Selection.activeGameObject;
    if (!go)
    {
      Debug.LogError("Select a GameObject with MeshFilter (your canonical face mesh).");
      return;
    }

    var mf = go.GetComponent<MeshFilter>();
    if (!mf || !mf.sharedMesh)
    {
      Debug.LogError("Selected object has no MeshFilter / Mesh.");
      return;
    }

    var mesh = mf.sharedMesh;

    // 2) Validate mesh
    if (mesh.vertexCount != 468)
    {
      Debug.LogError($"Mesh vertexCount = {mesh.vertexCount}, expected 468. " +
                     "If it's not 468, Unity likely split vertices on import.");
      return;
    }

    var meshUv = mesh.uv;
    if (meshUv == null || meshUv.Length != 468)
    {
      Debug.LogError($"Mesh uv count = {(meshUv == null ? 0 : meshUv.Length)}, expected 468.");
      return;
    }

    // 3) Pick canonical UV text asset
    // Bạn sửa path này theo đúng project của bạn
    var canonicalUvTxt = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/FaceMask/face_uv_468.txt");
    if (!canonicalUvTxt)
    {
      Debug.LogError("Cannot find canonical UV TextAsset at: Assets/FaceMask/face_uv_468.txt. Update path in baker.");
      return;
    }

    var canonicalUv = ParseUv468(canonicalUvTxt);
    if (canonicalUv.Length != 468)
    {
      Debug.LogError($"Canonical UV count = {canonicalUv.Length}, expected 468.");
      return;
    }

    // 4) Create mapping using nearest-UV with one-to-one constraint
    int[] map = new int[468];
    bool[] used = new bool[468];

    float maxDist = 0f;
    int worstI = -1;

    for (int i = 0; i < 468; i++)
    {
      float best = float.MaxValue;
      int bestJ = -1;

      for (int j = 0; j < 468; j++)
      {
        if (used[j]) continue;
        float d = (meshUv[j] - canonicalUv[i]).sqrMagnitude;
        if (d < best)
        {
          best = d;
          bestJ = j;
        }
      }

      if (bestJ < 0)
      {
        Debug.LogError($"Failed to map landmark {i}");
        return;
      }

      used[bestJ] = true;
      map[i] = bestJ;

      float dist = Mathf.Sqrt(best);
      if (dist > maxDist)
      {
        maxDist = dist;
        worstI = i;
      }
    }

    // 5) Save as ScriptableObject asset
    var asset = ScriptableObject.CreateInstance<FaceMesh468MappingAsset>();
    asset.map = map;

    string outPath = "Assets/FaceMask/FaceMesh468Mapping.asset";
    AssetDatabase.CreateAsset(asset, outPath);
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();

    Debug.Log($"✅ Saved mapping: {outPath}\nWorst UV distance: {maxDist} at landmark {worstI}\n" +
              "If maxDist is huge, your mesh UVs probably don't match canonical UV space.");
  }

  private static Vector2[] ParseUv468(TextAsset txt)
  {
    var lines = txt.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    var uv = new Vector2[lines.Length];

    for (int i = 0; i < lines.Length; i++)
    {
      var parts = lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
      float u = float.Parse(parts[0], CultureInfo.InvariantCulture);
      float v = float.Parse(parts[1], CultureInfo.InvariantCulture);
      uv[i] = new Vector2(u, v);
    }

    return uv;
  }
}

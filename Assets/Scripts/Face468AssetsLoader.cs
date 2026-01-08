using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class Face468AssetsLoader
{
    public static int[] LoadTriangles(TextAsset txt)
    {
        return txt.text
            .Split(new[] {',', '\n', '\r', ' ', '\t'}, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.Parse(s))
            .ToArray();
    }

    public static Vector2[] LoadUV(TextAsset txt)
    {
        var lines = txt.text.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
        var uv = new Vector2[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            float u = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float v = float.Parse(parts[1], CultureInfo.InvariantCulture);
            uv[i] = new Vector2(u, v);
        }
        return uv;
    }
}
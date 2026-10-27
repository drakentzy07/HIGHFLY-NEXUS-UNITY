#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Highfly.Editor
{
    /// <summary>
    /// Minimal ORIGINAL Highfly item silhouettes saved as Unity sprites.
    /// No UI sprites or proprietary icons copied from ClaudeCraft / MLBB.
    /// </summary>
    public static class HighflyW2CIconArt
    {
        private const string Dir = "Assets/Highfly/Generated/UI/W2C";
        public static Sprite Get(string id)
        {
            if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
            string file = Dir + "/" + id + ".png";
            Sprite found = AssetDatabase.LoadAssetAtPath<Sprite>(file);
            if (found != null) return found;

            const int n = 96;
            Texture2D bitmap = new Texture2D(n, n, TextureFormat.RGBA32, false);
            bitmap.name = "HIGHFLY_W2C_" + id;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + .5f) / n;
                float v = (y + .5f) / n;
                float cx = u - .5f, cy = v - .5f;
                Color pixel = Color.clear;
                if (id == "sword")
                {
                    // Diagonal hilt, guard and tapering silver-blue blade.
                    float bladeAxis = u - (.28f + .45f * v);
                    bool blade = v > .29f && v < .89f &&
                        Mathf.Abs(bladeAxis) < .035f * (1.08f - .5f * v);
                    bool hilt = v > .12f && v < .34f &&
                        Mathf.Abs(bladeAxis) < .030f;
                    bool guard = v > .26f && v < .34f &&
                        Mathf.Abs(bladeAxis) < .145f - .52f * Mathf.Abs(v-.30f);
                    if (blade) pixel = Color.Lerp(
                        new Color(.24f,.59f,.76f,1f),
                        new Color(.88f,.95f,1f,1f), u);
                    if (hilt) pixel = new Color(.43f,.31f,.18f,1f);
                    if (guard) pixel = new Color(.94f,.69f,.34f,1f);
                }
                else if (id == "armor")
                {
                    float width = .30f * (1f-Mathf.Clamp01((.80f-v)*.60f));
                    bool body = v >= .23f && v < .77f &&
                        Mathf.Abs(cx) <= width &&
                        v <= .80f - .9f * Mathf.Abs(cx);
                    bool crest = body && Mathf.Abs(cx) < .045f && v > .36f;
                    if (body) pixel = new Color(.45f,.37f,.78f,1f);
                    if (crest) pixel = new Color(.85f,.76f,1f,1f);
                }
                else if (id == "bone")
                {
                    float spine = Mathf.Abs(v - (.5f + .53f * (u-.5f)));
                    float tip = (new Vector2(u-.27f,v-.37f)).sqrMagnitude;
                    float end = (new Vector2(u-.74f,v-.63f)).sqrMagnitude;
                    if ((spine < .063f && u > .23f && u < .77f) ||
                        tip < .012f || end < .012f)
                        pixel = new Color(.95f,.87f,.66f,1f);
                }
                else
                {
                    float d = Mathf.Abs(cx)*1.05f + Mathf.Abs(cy)*.75f;
                    float wave = .06f * Mathf.Sin(u * 19f) *
                        Mathf.Cos(v * 23f);
                    if (d + wave < .39f)
                        pixel = new Color(.64f,.40f,.25f,1f);
                }
                bitmap.SetPixel(x, y, pixel);
            }
            bitmap.Apply(false, false);
            File.WriteAllBytes(file, bitmap.EncodeToPNG());
            Object.DestroyImmediate(bitmap);
            AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(file) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.spritePixelsPerUnit = 96f;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
            Sprite result = AssetDatabase.LoadAssetAtPath<Sprite>(file);
            if (result == null) throw new System.InvalidOperationException(
                "W2-C icon sprite missing after import: " + file);
            return result;
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Highfly.Editor
{
    /// <summary>
    /// REUSE FIRST audit for the imported RG Poly city.
    /// Produces deterministic diagnostics only; it never edits gameplay,
    /// player, camera, combat, colliders or scene topology.
    /// </summary>
    public static class HighflyRgPolyCityAudit
    {
        private const string DiagnosticsDirectory = "build/diagnostics";

        public static void Write(Scene scene)
        {
            Directory.CreateDirectory(DiagnosticsDirectory);

            List<Row> rows = new List<Row>();
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
                Visit(roots[i].transform, rows, 0);

            WriteAll(rows);
            WriteCandidates(rows);
            WriteWaterReport(scene);

            Debug.Log(
                "HIGHFLY RG POLY CITY AUDIT | objects=" + rows.Count +
                " | candidates=" + rows.Count(r => r.IsCandidate));
        }

        private static void Visit(Transform transform, List<Row> rows, int depth)
        {
            if (transform == null)
                return;

            Renderer[] renderers =
                transform.GetComponentsInChildren<Renderer>(true);

            Collider[] colliders =
                transform.GetComponentsInChildren<Collider>(true);

            Bounds bounds = default;
            bool hasBounds = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                    continue;

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            string path = GetPath(transform);
            string name = transform.name ?? string.Empty;
            string lower = name.ToLowerInvariant();

            bool keyword =
                lower.Contains("house") ||
                lower.Contains("building") ||
                lower.Contains("blacksmith") ||
                lower.Contains("smith") ||
                lower.Contains("forge") ||
                lower.Contains("market") ||
                lower.Contains("shop") ||
                lower.Contains("inn") ||
                lower.Contains("tavern") ||
                lower.Contains("guild") ||
                lower.Contains("church") ||
                lower.Contains("castle") ||
                lower.Contains("tower") ||
                lower.Contains("gate") ||
                lower.Contains("stable") ||
                lower.Contains("warehouse") ||
                lower.Contains("mill");

            bool largeAssembly =
                hasBounds &&
                bounds.size.x >= 4.5f &&
                bounds.size.y >= 3.0f &&
                bounds.size.z >= 4.5f &&
                depth <= 5;

            bool candidate = keyword || largeAssembly;

            rows.Add(new Row
            {
                Path = path,
                Name = name,
                Depth = depth,
                Active = transform.gameObject.activeInHierarchy,
                Position = transform.position,
                Euler = transform.eulerAngles,
                RendererCount = renderers.Length,
                ColliderCount = colliders.Length,
                HasBounds = hasBounds,
                Bounds = bounds,
                IsKeyword = keyword,
                IsCandidate = candidate
            });

            for (int i = 0; i < transform.childCount; i++)
                Visit(transform.GetChild(i), rows, depth + 1);
        }

        private static void WriteWaterReport(Scene scene)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(
                "name\tpath\tpos_x\tpos_y\tpos_z\tbounds_x\tbounds_y\tbounds_z" +
                "\tmin_y\tmax_y\tmaterial\tcolliders\ttriggers");

            Renderer[] renderers =
                UnityEngine.Object.FindObjectsOfType<Renderer>(true);

            CultureInfo ci = CultureInfo.InvariantCulture;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.scene != scene)
                    continue;

                string rendererName = renderer.name ?? string.Empty;
                string lowerName = rendererName.ToLowerInvariant();
                Material[] materials = renderer.sharedMaterials;

                bool looksLikeWater =
                    lowerName.Contains("water") ||
                    lowerName.Contains("river") ||
                    lowerName.Contains("lake");

                string materialNames = string.Empty;

                if (materials != null)
                {
                    List<string> names = new List<string>();
                    for (int m = 0; m < materials.Length; m++)
                    {
                        Material mat = materials[m];
                        if (mat == null)
                            continue;

                        string matName = mat.name ?? string.Empty;
                        names.Add(matName);

                        string lowerMat = matName.ToLowerInvariant();
                        if (lowerMat.Contains("water") ||
                            lowerMat.Contains("river") ||
                            lowerMat.Contains("lake"))
                            looksLikeWater = true;
                    }

                    materialNames = string.Join("|", names);
                }

                if (!looksLikeWater)
                    continue;

                Collider[] colliders =
                    renderer.GetComponents<Collider>();

                int triggers = 0;
                for (int k = 0; k < colliders.Length; k++)
                {
                    Collider collider = colliders[k];
                    if (collider != null && collider.isTrigger)
                        triggers++;
                }

                Bounds b = renderer.bounds;
                Vector3 p = renderer.transform.position;

                sb.Append(Sanitize(rendererName)).Append('\t')
                  .Append(Sanitize(GetPath(renderer.transform))).Append('\t')
                  .Append(p.x.ToString("0.###", ci)).Append('\t')
                  .Append(p.y.ToString("0.###", ci)).Append('\t')
                  .Append(p.z.ToString("0.###", ci)).Append('\t')
                  .Append(b.size.x.ToString("0.###", ci)).Append('\t')
                  .Append(b.size.y.ToString("0.###", ci)).Append('\t')
                  .Append(b.size.z.ToString("0.###", ci)).Append('\t')
                  .Append(b.min.y.ToString("0.###", ci)).Append('\t')
                  .Append(b.max.y.ToString("0.###", ci)).Append('\t')
                  .Append(Sanitize(materialNames)).Append('\t')
                  .Append(colliders.Length).Append('\t')
                  .Append(triggers).AppendLine();
            }

            File.WriteAllText(
                Path.Combine(
                    DiagnosticsDirectory,
                    "rgpoly-water-report.tsv"),
                sb.ToString());
        }

        private static void WriteAll(List<Row> rows)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(
                "path\tname\tdepth\tactive\tpos_x\tpos_y\tpos_z\tyaw" +
                "\trenderers\tcolliders\tbounds_x\tbounds_y\tbounds_z\tkeyword\tcandidate");

            foreach (Row row in rows.OrderBy(r => r.Path, StringComparer.Ordinal))
                AppendRow(sb, row);

            File.WriteAllText(
                Path.Combine(DiagnosticsDirectory, "rgpoly-city-hierarchy.tsv"),
                sb.ToString());
        }

        private static void WriteCandidates(List<Row> rows)
        {
            List<Row> candidates = rows
                .Where(r => r.IsCandidate)
                .OrderByDescending(r =>
                    r.HasBounds
                        ? r.Bounds.size.x * r.Bounds.size.y * r.Bounds.size.z
                        : 0f)
                .ThenBy(r => r.Path, StringComparer.Ordinal)
                .Take(500)
                .ToList();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(
                "rank\tpath\tname\tdepth\tactive\tpos_x\tpos_y\tpos_z\tyaw" +
                "\trenderers\tcolliders\tbounds_x\tbounds_y\tbounds_z\tkeyword");

            for (int i = 0; i < candidates.Count; i++)
            {
                Row row = candidates[i];
                sb.Append(i + 1).Append('\t');
                AppendRow(sb, row, includeCandidate: false);
            }

            File.WriteAllText(
                Path.Combine(DiagnosticsDirectory, "rgpoly-building-candidates.tsv"),
                sb.ToString());
        }

        private static void AppendRow(
            StringBuilder sb,
            Row row,
            bool includeCandidate = true)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            sb.Append(Sanitize(row.Path)).Append('\t')
              .Append(Sanitize(row.Name)).Append('\t')
              .Append(row.Depth).Append('\t')
              .Append(row.Active ? "1" : "0").Append('\t')
              .Append(row.Position.x.ToString("0.###", ci)).Append('\t')
              .Append(row.Position.y.ToString("0.###", ci)).Append('\t')
              .Append(row.Position.z.ToString("0.###", ci)).Append('\t')
              .Append(row.Euler.y.ToString("0.###", ci)).Append('\t')
              .Append(row.RendererCount).Append('\t')
              .Append(row.ColliderCount).Append('\t')
              .Append((row.HasBounds ? row.Bounds.size.x : 0f).ToString("0.###", ci)).Append('\t')
              .Append((row.HasBounds ? row.Bounds.size.y : 0f).ToString("0.###", ci)).Append('\t')
              .Append((row.HasBounds ? row.Bounds.size.z : 0f).ToString("0.###", ci)).Append('\t')
              .Append(row.IsKeyword ? "1" : "0");

            if (includeCandidate)
                sb.Append('\t').Append(row.IsCandidate ? "1" : "0");

            sb.AppendLine();
        }

        private static string GetPath(Transform transform)
        {
            List<string> names = new List<string>();
            Transform cursor = transform;

            while (cursor != null)
            {
                names.Add(cursor.name);
                cursor = cursor.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }

        private static string Sanitize(string value)
        {
            return (value ?? string.Empty)
                .Replace("\t", " ")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private sealed class Row
        {
            public string Path;
            public string Name;
            public int Depth;
            public bool Active;
            public Vector3 Position;
            public Vector3 Euler;
            public int RendererCount;
            public int ColliderCount;
            public bool HasBounds;
            public Bounds Bounds;
            public bool IsKeyword;
            public bool IsCandidate;
        }
    }
}
#endif

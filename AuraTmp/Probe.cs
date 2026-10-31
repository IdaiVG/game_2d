using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Text;
using System.Collections.Generic;

public class Probe
{
    public static object Execute()
    {
        var sb = new StringBuilder();

        // ---- PlayerController ground layer ----
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            sb.AppendLine("groundLayer value=" + player.groundLayer.value
                + " (" + LayerMask.LayerToName(3) + ")");
            sb.AppendLine("player pos=" + player.transform.position + " scale=" + player.transform.localScale);
        }

        // ---- Tilemap cells ----
        var grid = GameObject.Find("Grid");
        if (grid != null)
        {
            var tm = grid.GetComponentInChildren<Tilemap>(true);
            if (tm != null)
            {
                sb.AppendLine("Tilemap=" + tm.name + " cellSize=" + tm.cellSize + " bounds=" + tm.cellBounds);
                // surface height per integer column
                var cols = new Dictionary<int, float>();
                foreach (var pos in tm.cellBounds.allPositionsWithin)
                {
                    if (!tm.HasTile(pos)) continue;
                    Vector3 w = tm.GetCellCenterWorld(pos);
                    float top = w.y + tm.cellSize.y * 0.5f;
                    if (!cols.ContainsKey(pos.x) || cols[pos.x] < top) cols[pos.x] = top;
                }
                var keys = new List<int>(cols.Keys);
                keys.Sort();
                sb.AppendLine("COLUMN_SURFACES:");
                string line = "";
                int prevX = int.MinValue; float prevY = float.MinValue;
                foreach (var k in keys)
                {
                    if (prevX != int.MinValue && (k != prevX + 1 || Mathf.Abs(cols[k] - prevY) > 0.01f))
                    {
                        sb.AppendLine("  x " + runStart + ".." + prevX + " top=" + prevY);
                    }
                    if (prevX == int.MinValue || k != prevX + 1 || Mathf.Abs(cols[k] - prevY) > 0.01f)
                        runStart = k;
                    prevX = k; prevY = cols[k];
                }
                if (prevX != int.MinValue) sb.AppendLine("  x " + runStart + ".." + prevX + " top=" + prevY);
            }
        }

        // ---- Sprite import info ----
        string[] paths = {
            "Assets/Art/Enviroment/Cementery/Bone (1).png",
            "Assets/Art/Enviroment/Cementery/Bone (2).png",
            "Assets/Art/Enviroment/Cementery/Bone (3).png",
            "Assets/Art/Enviroment/Cementery/Bone (4).png",
            "Assets/Art/Obstacles/Crate.png",
            "Assets/Art/Obstacles/Skeleton.png"
        };
        foreach (var p in paths)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(p);
            foreach (var a in all)
            {
                var sp = a as Sprite;
                if (sp == null) continue;
                sb.AppendLine("SPRITE " + p + " name='" + sp.name + "' rect=" + sp.rect
                    + " pivot=" + sp.pivot + " ppu=" + sp.pixelsPerUnit
                    + " worldSize=" + (sp.rect.width / sp.pixelsPerUnit) + "x" + (sp.rect.height / sp.pixelsPerUnit));
            }
        }

        // ---- Existing crates/decor in scene ----
        sb.AppendLine("--- scene objects ---");
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name.Contains("Crate") || t.name.Contains("Bone") || t.name.Contains("Decor"))
            {
                var sr = t.GetComponent<SpriteRenderer>();
                sb.AppendLine("  " + t.name + " pos=" + t.position + " scale=" + t.localScale
                    + " order=" + (sr != null ? sr.sortingOrder : -999)
                    + " sprite=" + (sr != null && sr.sprite != null ? sr.sprite.name : "NULL")
                    + " layer=" + t.gameObject.layer);
            }
        }

        // ---- Canvas / text style ----
        var t2 = GameObject.Find("VictoryCanvas/VictoryOverlay/Panel/Title");
        if (t2 != null)
        {
            var txt = t2.GetComponent<UnityEngine.UI.Text>();
            if (txt != null)
                sb.AppendLine("Title font=" + (txt.font != null ? txt.font.name : "NULL")
                    + " fontSize=" + txt.fontSize + " align=" + txt.alignment
                    + " resizeTextForBestFit=" + txt.resizeTextForBestFit);
        }

        return sb.ToString();
    }

    static int runStart;
}

// Aufrufen über: Unity Menüleiste → Tools → Scene Diagnostics → ...

using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class SceneDiagnostics : EditorWindow
{
    [MenuItem("Tools/Scene Diagnostics/1. Finde NavMeshObstacles mit Carving")]
    static void FindCarvingObstacles()
    {
        var obstacles = GameObject.FindObjectsOfType<NavMeshObstacle>();
        var carving = obstacles.Where(o => o.carving).ToList();

        if (carving.Count == 0)
        {
            Debug.Log("✅ Keine NavMeshObstacles mit Carving gefunden.");
            return;
        }

        Debug.LogWarning($"⚠️ {carving.Count} NavMeshObstacle(s) mit Carving aktiv:");
        foreach (var o in carving)
        {
            Debug.LogWarning(
                $"  → [{o.gameObject.name}] " +
                $"| carveOnlyStationary: {o.carveOnlyStationary} " +
                $"| Path: {GetPath(o.gameObject)}",
                o.gameObject
            );
        }

        // Alle anwählen im Editor
        Selection.objects = carving.Select(o => o.gameObject).Cast<Object>().ToArray();
        Debug.LogWarning("👆 Objekte wurden in der Hierarchie ausgewählt.");
    }

    [MenuItem("Tools/Scene Diagnostics/2. Finde AudioSources (potenzielle FMOD-Trigger)")]
    static void FindAudioSources()
    {
        var sources = GameObject.FindObjectsOfType<AudioSource>();
        Debug.Log($"🔊 {sources.Length} AudioSource(s) in der Scene:");

        var playOnAwake = sources.Where(s => s.playOnAwake).ToList();
        if (playOnAwake.Count > 0)
        {
            Debug.LogWarning($"  ⚠️ {playOnAwake.Count} mit 'Play On Awake' aktiv (lädt beim Start):");
            foreach (var s in playOnAwake)
                Debug.LogWarning($"    → [{s.gameObject.name}] Clip: {(s.clip != null ? s.clip.name : "NULL")} | Path: {GetPath(s.gameObject)}", s.gameObject);
        }

        var noClip = sources.Where(s => s.clip == null && !s.playOnAwake).ToList();
        if (noClip.Count > 0)
        {
            Debug.LogWarning($"  ⚠️ {noClip.Count} ohne Clip (wahrscheinlich FMOD/dynamisch geladen):");
            foreach (var s in noClip)
                Debug.LogWarning($"    → [{s.gameObject.name}] | Path: {GetPath(s.gameObject)}", s.gameObject);
        }

        Selection.objects = sources.Select(s => s.gameObject).Cast<Object>().ToArray();
    }

    [MenuItem("Tools/Scene Diagnostics/3. Finde High-Poly Meshes (über Schwellwert)")]
    static void FindHighPolyMeshes()
    {
        int threshold = 40000;
        var filters = GameObject.FindObjectsOfType<MeshFilter>();
        var smrs = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();

        // Alle Instanzen sammeln (MeshFilter + SkinnedMeshRenderer)
        var instances = new List<(Mesh mesh, string name, string path, int tris, int verts, string type, GameObject go)>();

        foreach (var f in filters)
        {
            if (f.sharedMesh == null) continue;
            int tris = f.sharedMesh.triangles.Length / 3;
            if (tris >= threshold)
                instances.Add((f.sharedMesh, f.gameObject.name, GetPath(f.gameObject), tris, f.sharedMesh.vertexCount, "Static", f.gameObject));
        }

        foreach (var s in smrs)
        {
            if (s.sharedMesh == null) continue;
            int tris = s.sharedMesh.triangles.Length / 3;
            if (tris >= threshold)
                instances.Add((s.sharedMesh, s.gameObject.name, GetPath(s.gameObject), tris, s.sharedMesh.vertexCount, "Skinned", s.gameObject));
        }

        if (instances.Count == 0)
        {
            Debug.Log($"✅ Keine Meshes über {threshold} Dreiecke gefunden.");
            return;
        }

        // Nach Mesh-Asset gruppieren: Ein Stein mit 50.000 Tris, der 10x in der Szene
        // steht, frisst 500.000 Tris Gesamtlast — mehr als eine einzelne 300.000-Tris-Statue.
        // Rangliste daher nach GESAMT-Tris-Last sortieren, nicht nach Tris pro Instanz.
        var groups = instances
            .GroupBy(r => r.mesh)
            .Select(g => new
            {
                mesh = g.Key,
                trisPerInstance = g.First().tris,
                vertsPerInstance = g.First().verts,
                type = g.First().type,
                instanceCount = g.Count(),
                totalTris = g.First().tris * g.Count(),
                instances = g.ToList()
            })
            .OrderByDescending(g => g.totalTris)
            .ToList();

        int grandTotalTris = groups.Sum(g => g.totalTris);

        Debug.LogWarning($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.LogWarning($"⚠️ HIGH-POLY MESH RANGLISTE (>{threshold} Tris pro Instanz)");
        Debug.LogWarning($"   {groups.Count} Mesh-Assets | {instances.Count} Instanzen | Gesamt-Tris (Szenenlast): {grandTotalTris:N0}");
        Debug.LogWarning($"   Sortiert nach Gesamt-Tris-Last (Tris/Instanz × Vorkommen) — das kostet performancetechnisch am meisten");
        Debug.LogWarning($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        // Rangliste ausgeben
        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            float percentage = (float)g.totalTris / grandTotalTris * 100f;

            // Emoji je nach Gesamtlast, nicht nach Einzel-Tris
            string severity = g.totalTris > 200000 ? "🔴" : g.totalTris > 100000 ? "🟠" : g.totalTris > 50000 ? "🟡" : "🟢";
            string multiplier = g.instanceCount > 1 ? $" (×{g.instanceCount} Instanzen!)" : "";

            Debug.LogWarning(
                $"{severity} #{i + 1:D2} | {g.totalTris:N0} Gesamt-Tris ({percentage:F1}%) | " +
                $"{g.trisPerInstance:N0} Tris/Instanz × {g.instanceCount}{multiplier} | " +
                $"{g.vertsPerInstance:N0} Verts | [{g.type}] | " +
                $"{g.mesh.name}",
                g.instances[0].go
            );

            // Bei mehreren Instanzen alle Fundorte mit auflisten
            if (g.instanceCount > 1)
            {
                foreach (var inst in g.instances)
                    Debug.LogWarning($"      → {inst.name} | {inst.path}", inst.go);
            }
        }

        Debug.LogWarning($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        // Top 3 nochmal zusammenfassen
        Debug.LogWarning("🏆 TOP 3 SCHLIMMSTE MESHES (nach Gesamt-Szenenlast):");
        foreach (var g in groups.Take(3))
        {
            string hint = g.instanceCount > 1
                ? $"{g.mesh.name}: {g.trisPerInstance:N0} Tris × {g.instanceCount} Instanzen = {g.totalTris:N0} Gesamt-Tris — EIN MAL in Blender reduzieren wirkt sich auf alle Instanzen aus!"
                : $"{g.mesh.name}: {g.totalTris:N0} Tris — in Blender reduzieren!";
            Debug.LogWarning($"   → {hint}", g.instances[0].go);
        }

        Selection.objects = instances.Select(r => r.go).Cast<Object>().ToArray();
    }

    [MenuItem("Tools/Scene Diagnostics/4. Finde Realtime Lights")]
    static void FindRealtimeLights()
    {
        var lights = GameObject.FindObjectsOfType<Light>();
        var realtime = lights.Where(l => l.lightmapBakeType == LightmapBakeType.Realtime).ToList();
        var mixed   = lights.Where(l => l.lightmapBakeType == LightmapBakeType.Mixed).ToList();

        Debug.Log($"💡 Lights gesamt: {lights.Length} | Realtime: {realtime.Count} | Mixed: {mixed.Count}");

        if (realtime.Count > 0)
        {
            Debug.LogWarning($"⚠️ {realtime.Count} Realtime Light(s) — teuer in VR:");
            foreach (var l in realtime)
                Debug.LogWarning($"  → [{l.gameObject.name}] Type: {l.type} | Shadows: {l.shadows} | Path: {GetPath(l.gameObject)}", l.gameObject);
        }

        if (mixed.Count > 0)
        {
            Debug.Log($"ℹ️ {mixed.Count} Mixed Light(s):");
            foreach (var l in mixed)
                Debug.Log($"  → [{l.gameObject.name}] Type: {l.type} | Path: {GetPath(l.gameObject)}", l.gameObject);
        }

        Selection.objects = realtime.Select(l => l.gameObject).Cast<Object>().ToArray();
    }

    [MenuItem("Tools/Scene Diagnostics/5. Finde nicht-statische Objekte (Batching-Blocker)")]
    static void FindNonStaticObjects()
    {
        var all = GameObject.FindObjectsOfType<MeshRenderer>();
        var nonStatic = all.Where(r => !r.gameObject.isStatic).ToList();

        Debug.Log($"📦 MeshRenderer gesamt: {all.Length} | Nicht-statisch: {nonStatic.Count}");

        if (nonStatic.Count > 50)
            Debug.LogWarning($"⚠️ {nonStatic.Count} nicht-statische Objekte — viele davon könnten Static gesetzt werden!");

        // Gruppiert nach Parent ausgeben
        var grouped = nonStatic
            .GroupBy(r => r.transform.parent?.gameObject.name ?? "Root")
            .OrderByDescending(g => g.Count())
            .Take(10);

        Debug.LogWarning("Top 10 Parent-Gruppen mit nicht-statischen Objekten:");
        foreach (var g in grouped)
            Debug.LogWarning($"  → Parent [{g.Key}]: {g.Count()} Objekte");
    }

    [MenuItem("Tools/Scene Diagnostics/6. Finde Scripts mit Update() Methoden")]
    static void FindUpdateScripts()
    {
        var allMono = GameObject.FindObjectsOfType<MonoBehaviour>();
        var withUpdate = new List<(MonoBehaviour mb, string[] methods)>();

        foreach (var mb in allMono)
        {
            if (mb == null) continue;
            var type = mb.GetType();
            var found = new List<string>();
            foreach (var method in new[] { "Update", "FixedUpdate", "LateUpdate" })
            {
                var mi = type.GetMethod(method,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.DeclaredOnly);
                if (mi != null) found.Add(method);
            }
            if (found.Count > 0)
                withUpdate.Add((mb, found.ToArray()));
        }

        Debug.Log($"🔄 {withUpdate.Count} MonoBehaviours mit Update-Methoden:");
        foreach (var (mb, methods) in withUpdate.OrderBy(x => x.mb.gameObject.name))
            Debug.Log($"  → [{mb.gameObject.name}] {mb.GetType().Name}: {string.Join(", ", methods)} | Path: {GetPath(mb.gameObject)}", mb.gameObject);
    }

    // ── Hilfsfunktion ──────────────────────────────────────────────────────────
    static string GetPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        int depth = 0;
        while (t != null && depth < 4)
        {
            path = t.name + "/" + path;
            t = t.parent;
            depth++;
        }
        return path;
    }
}

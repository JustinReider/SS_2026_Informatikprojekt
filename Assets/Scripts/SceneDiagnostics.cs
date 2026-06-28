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
		
		    // Alle Meshes sammeln (MeshFilter + SkinnedMeshRenderer)
		    var results = new List<(string name, string path, int tris, int verts, string type, GameObject go)>();
		
		    foreach (var f in filters)
		    {
		        if (f.sharedMesh == null) continue;
		        int tris = f.sharedMesh.triangles.Length / 3;
		        if (tris >= threshold)
		            results.Add((f.gameObject.name, GetPath(f.gameObject), tris, f.sharedMesh.vertexCount, "Static", f.gameObject));
		    }
		
		    foreach (var s in smrs)
		    {
		        if (s.sharedMesh == null) continue;
		        int tris = s.sharedMesh.triangles.Length / 3;
		        if (tris >= threshold)
		            results.Add((s.gameObject.name, GetPath(s.gameObject), tris, s.sharedMesh.vertexCount, "Skinned", s.gameObject));
		    }
		
		    if (results.Count == 0)
		    {
		        Debug.Log($"✅ Keine Meshes über {threshold} Dreiecke gefunden.");
		        return;
		    }
		
		    // Sortieren: schlimmste zuerst
		    results = results.OrderByDescending(r => r.tris).ToList();
		
		    // Gesamtstatistik
		    int totalTris = results.Sum(r => r.tris);
		    Debug.LogWarning($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		    Debug.LogWarning($"⚠️ HIGH-POLY MESH RANGLISTE (>{threshold} Tris)");
		    Debug.LogWarning($"   Gefunden: {results.Count} Meshes | Gesamt-Tris: {totalTris:N0}");
		    Debug.LogWarning($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		    // Rangliste ausgeben
		    for (int i = 0; i < results.Count; i++)
		    {
		        var r = results[i];
		        float percentage = (float)r.tris / totalTris * 100f;
		
		        // Emoji je nach Schwere
		        string severity = r.tris > 50000 ? "🔴" : r.tris > 20000 ? "🟠" : r.tris > 10000 ? "🟡" : "🟢";
		
		        Debug.LogWarning(
		            $"{severity} #{i + 1:D2} | {r.tris:N0} Tris ({percentage:F1}%) | " +
		            $"{r.verts:N0} Verts | [{r.type}] | " +
		            $"{r.name} | {r.path}",
		            r.go
		        );
		    }
		
		    Debug.LogWarning($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		    // Top 3 nochmal zusammenfassen
		    Debug.LogWarning("🏆 TOP 3 SCHLIMMSTE MESHES:");
		    foreach (var r in results.Take(3))
		        Debug.LogWarning($"   → {r.name}: {r.tris:N0} Tris — in Blender reduzieren!", r.go);
		
		    Selection.objects = results.Select(r => r.go).Cast<Object>().ToArray();
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

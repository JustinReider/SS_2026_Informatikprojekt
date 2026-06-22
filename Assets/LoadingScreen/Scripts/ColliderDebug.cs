using UnityEngine;
using System.Linq;

public class ColliderDebug : MonoBehaviour
{
    void Start()
    {
        var colliders = FindObjectsByType<MeshCollider>(FindObjectsSortMode.None);

        Debug.Log($"MeshCollider Count: {colliders.Length}");

        foreach (var mc in colliders.OrderByDescending(
                     c => c.sharedMesh != null ? c.sharedMesh.triangles.Length : 0))
        {
            if (mc.sharedMesh == null)
                continue;

            Debug.Log(
                $"{mc.name} -> {mc.sharedMesh.triangles.Length / 3} tris");
        }
    }
}

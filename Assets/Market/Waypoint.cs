using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Tooltip("Wartezeit an diesem Waypoint in Sekunden. Negativer Wert = Standardwartezeit aus GuardPatrol verwenden.")]
    public float customWaitTime = -1f;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
}

using UnityEngine;
public class Waypoint : MonoBehaviour
{
    [Header("Wartezeit")]
    [Tooltip("Wartezeit in Sekunden. Negativer Wert = Standardwartezeit aus NPCNavigator verwenden.")]
    public float customWaitTime = -1f;

    [Header("Audio")]
    [Tooltip("Diesem Waypoint zugewiesene Audio-Datei. Kann vom aufrufenden Script beliebig verwendet werden.")]
    public AudioClip waypointSound;

    [Header("Gizmo")]
    [Tooltip("Farbe des Waypoint-Gizmos im Scene-View.")]
    public Color gizmoColor = Color.cyan;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
}

using UnityEngine;
using UnityEngine.AI;

public class BauerBewegung : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] waypoints; // Marktstände, Personen, etc.
    private int aktuellerWaypoint = 0;
    public float stopAbstand = 0.5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Ist Ziel erreicht?
        if (!agent.pathPending && agent.remainingDistance < stopAbstand)
        {
            // Zum nächsten Waypoint
            aktuellerWaypoint = (aktuellerWaypoint + 1) % waypoints.Length;
            agent.SetDestination(waypoints[aktuellerWaypoint].position);
        }
    }
}

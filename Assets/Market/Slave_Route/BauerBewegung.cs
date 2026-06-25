using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BauerBewegung : MonoBehaviour
{
    private NavMeshAgent agent;

    public Transform[] waypoints;
    public float[] waitTimes; // wie lange er bleibt

    private int index = 0;
    private bool waiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNext();
    }

    void Update()
    {
        if (waiting) return;

        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            StartCoroutine(WaitAndGo());
        }
    }

    IEnumerator WaitAndGo()
    {
        waiting = true;

        float wait = 2f; // fallback
        if (waitTimes != null && index < waitTimes.Length)
            wait = waitTimes[index];

        yield return new WaitForSeconds(wait);

        index = (index + 1) % waypoints.Length;
        GoToNext();

        waiting = false;
    }

    void GoToNext()
    {
        agent.SetDestination(waypoints[index].position);
    }
}

using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GuardPatrol : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;
    public float waitTimeAtWaypoint = 2f;

    [Header("Movement")]
    public float walkSpeed = 1.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypoint = 0;
    private bool waiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = walkSpeed;
        GoToNextWaypoint();
    }

    void Update()
    {
        bool isMoving = agent.remainingDistance > agent.stoppingDistance && !agent.pathPending;
        animator.SetBool("isWalking", isMoving);

        if (!waiting && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        waiting = true;
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(waitTimeAtWaypoint);

        waiting = false;
        GoToNextWaypoint();
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        agent.SetDestination(waypoints[currentWaypoint].position);
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length; // Loop
    }
}
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
    public float rotationSpeed = 300f;

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypoint = 0;
    private bool isPatrolling = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = walkSpeed;
        agent.updateRotation = false;
        GoToNextWaypoint();
    }

    void Update()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0 || isPatrolling) return;
        isPatrolling = true;

        Vector3 nextPos = waypoints[currentWaypoint].position;
        int targetIndex = currentWaypoint;
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;

        StartCoroutine(PatrolRoutine(nextPos, targetIndex));
    }

    IEnumerator PatrolRoutine(Vector3 target, int waypointIndex)
    {
        // --- Phase 1: Drehen ---
        animator.SetBool("isWalking", false);
        agent.ResetPath();

        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 5f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        // --- Phase 2: Laufen ---
        animator.SetBool("isWalking", true);
        agent.SetDestination(target);

        yield return new WaitUntil(() =>
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance
        );

        // --- Phase 3: Warten ---
        animator.SetBool("isWalking", false);
        agent.ResetPath();

        Quaternion waypointRotation = waypoints[waypointIndex].rotation;
        if (Quaternion.Angle(transform.rotation, waypointRotation) > 5f)
        {
            while (Quaternion.Angle(transform.rotation, waypointRotation) > 2f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    waypointRotation,
                    rotationSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        // Custom- oder Standardwartezeit ermitteln
        float wait = waitTimeAtWaypoint;
        Waypoint wp = waypoints[waypointIndex].GetComponent<Waypoint>();
				if (wp != null && wp.customWaitTime >= 0f)
				    wait = wp.customWaitTime;

        yield return new WaitForSeconds(wait);

        // --- Weiter ---
        isPatrolling = false;
        GoToNextWaypoint();
    }
}

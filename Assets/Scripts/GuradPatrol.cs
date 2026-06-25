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

    [Header("Follow Mode")]
    [Tooltip("Ab diesem Waypoint-Index wird in den Follow-Modus gewechselt. -1 = kein Follow-Modus.")]
    public int followFromWaypointIndex = -1;
    [Tooltip("Das Transform das verfolgt werden soll (anderer NPC oder Spieler).")]
    public Transform followTarget;
    [Tooltip("Abstand den der NPC zum Ziel hält.")]
    public float followDistance = 2f;
    [Tooltip("Wie oft pro Sekunde das Ziel neu angesteuert wird.")]
    public float followUpdateRate = 0.1f;
    [Tooltip("Wie lange der Follow-Modus aktiv bleibt in Sekunden. 0 = unendlich.")]
    public float followDuration = 0f;

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypoint = 0;
    private bool isPatrolling = false;
    private bool isFollowing = false;

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
        if (isPatrolling) return;

        if (followFromWaypointIndex >= 0 && currentWaypoint >= followFromWaypointIndex)
        {
            if (followTarget != null && !isFollowing)
            {
                isFollowing = true;
                StartCoroutine(FollowRoutine());
            }
            return;
        }

        if (waypoints.Length == 0) return;

        isPatrolling = true;
        int targetIndex = currentWaypoint;
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        StartCoroutine(PatrolRoutine(waypoints[targetIndex].position, targetIndex));
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

        float wait = waitTimeAtWaypoint;
        Waypoint wp = waypoints[waypointIndex].GetComponent<Waypoint>();
        if (wp != null && wp.customWaitTime >= 0f)
            wait = wp.customWaitTime;

        yield return new WaitForSeconds(wait);

        // --- Weiter ---
        isPatrolling = false;
        GoToNextWaypoint();
    }

    IEnumerator FollowRoutine()
    {
        float elapsed = 0f;

        while (isFollowing)
        {
            if (followDuration > 0f)
            {
                elapsed += followUpdateRate;
                if (elapsed >= followDuration)
                {
                    StopFollowing();
                    yield break;
                }
            }

            if (followTarget == null)
            {
                animator.SetBool("isWalking", false);
                agent.ResetPath();
                yield break;
            }

            float distanceToTarget = Vector3.Distance(transform.position, followTarget.position);

            if (distanceToTarget > followDistance + 0.2f)
            {
                Vector3 dirToSelf = (transform.position - followTarget.position).normalized;
                Vector3 destination = followTarget.position + dirToSelf * followDistance;
                agent.SetDestination(destination);
                animator.SetBool("isWalking", true);
            }
            else
            {
                agent.ResetPath();
                animator.SetBool("isWalking", false);

                Vector3 lookDir = (followTarget.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRot,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }

            yield return new WaitForSeconds(followUpdateRate);
        }
    }

    public void StopFollowing()
    {
        isFollowing = false;
        agent.ResetPath();
        animator.SetBool("isWalking", false);
    }
}

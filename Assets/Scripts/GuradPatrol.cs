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
    public float rotationSpeed = 300f;  // Grad pro Sekunde

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypoint = 0;
    private bool isPatrolling = false;  // Verhindert doppelte Coroutine-Starts

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = walkSpeed;
        agent.updateRotation = false;  // Wir übernehmen die Rotation selbst
        GoToNextWaypoint();
    }

    void Update()
    {
        // Rotation immer zur Bewegungsrichtung hin (wenn der Agent sich bewegt)
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
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        StartCoroutine(PatrolRoutine(nextPos));
    }

    IEnumerator PatrolRoutine(Vector3 target)
    {
        // --- Phase 1: Drehen ---
        animator.SetBool("isWalking", false);
        agent.ResetPath();  // Sicherstellen dass Agent still steht

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

        // Warten bis angekommen
        yield return new WaitUntil(() =>
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance
        );

        // --- Phase 3: Warten ---
		animator.SetBool("isWalking", false);
		agent.ResetPath();

		// Zur Waypoint-Rotation drehen (der aktuelle Waypoint ist currentWaypoint - 1)
		int arrivedAt = (currentWaypoint - 1 + waypoints.Length) % waypoints.Length;
		Quaternion waypointRotation = waypoints[arrivedAt].rotation;

		// Nur drehen wenn der Waypoint eine nennenswerte Rotation hat
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

		yield return new WaitForSeconds(waitTimeAtWaypoint);

        // --- Weiter ---
        isPatrolling = false;
        GoToNextWaypoint();
    }
}
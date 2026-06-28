using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCNavigator : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;
    public float waitTimeAtWaypoint = 2f;

    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float rotationSpeed = 300f;

    [Header("Rotation")]
    [Tooltip("Wenn false, wird jegliche Rotation komplett deaktiviert. Der NPC behält dann seine Start-Rotation.")]
    public bool enableRotation = true;

    [Header("Arrival Detection")]
    [Tooltip("Unter dieser Geschwindigkeit gilt der Agent als 'gestoppt'.")]
    public float arrivalVelocityThreshold = 0.05f;
    [Tooltip("Wie nah muss der Agent am Waypoint sein um anzuhalten (overridet NavMesh stoppingDistance).")]
    public float arrivalDistance = 0.25f;

    [Header("Audio")]
    [Tooltip("AudioSource auf der Waypoint-Sounds abgespielt werden. Leer lassen = kein Sound.")]
    public AudioSource waypointAudioSource;

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
    private Coroutine activeCoroutine;

    private static readonly int AnimWalking = Animator.StringToHash("isWalking");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = walkSpeed;
        agent.stoppingDistance = arrivalDistance;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;

        GoToNextWaypoint();
    }

    void Update()
    {
        if (!enableRotation) return;

        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 flatVelocity = new Vector3(agent.velocity.x, 0f, agent.velocity.z);
            if (flatVelocity.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatVelocity.normalized);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    // -----------------------------------------------------------------------
    // Zentrale Steuerung
    // -----------------------------------------------------------------------
    void GoToNextWaypoint()
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        if (followFromWaypointIndex >= 0 && currentWaypoint >= followFromWaypointIndex)
        {
            if (followTarget != null)
                activeCoroutine = StartCoroutine(FollowRoutine());
            return;
        }

        if (waypoints.Length == 0) return;

        int targetIndex = currentWaypoint;
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        activeCoroutine = StartCoroutine(NavigateRoutine(waypoints[targetIndex], targetIndex));
    }

    // -----------------------------------------------------------------------
    // Navigation
    // -----------------------------------------------------------------------
    IEnumerator NavigateRoutine(Transform waypointTransform, int waypointIndex)
    {
        Vector3 target = waypointTransform.position;

        // --- Phase 1: Laufen ---
        SetWalking(true);
        agent.SetDestination(target);

        yield return new WaitUntil(() => !agent.pathPending);
        yield return new WaitUntil(() =>
            agent.remainingDistance <= arrivalDistance &&
            agent.velocity.sqrMagnitude < arrivalVelocityThreshold * arrivalVelocityThreshold
        );

        agent.ResetPath();
        SetWalking(false);

        // --- Phase 2: Rotation zum Waypoint (optional) ---
        if (enableRotation)
            yield return StartCoroutine(RotateTo(waypointTransform.rotation));

        // --- Phase 3: Waypoint-Komponente auslesen ---
        Waypoint wp = waypointTransform.GetComponent<Waypoint>();

        if (wp != null && wp.waypointSound != null && waypointAudioSource != null)
            waypointAudioSource.PlayOneShot(wp.waypointSound);

        // --- Phase 4: Warten ---
        float wait = waitTimeAtWaypoint;
        if (wp != null && wp.customWaitTime >= 0f)
            wait = wp.customWaitTime;

        yield return new WaitForSeconds(wait);

        GoToNextWaypoint();
    }

    // -----------------------------------------------------------------------
    // Follow
    // -----------------------------------------------------------------------
    IEnumerator FollowRoutine()
    {
        float elapsed = 0f;

        while (true)
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
                StopMoving();
                yield break;
            }

            float dist = Vector3.Distance(transform.position, followTarget.position);

            if (dist > followDistance + 0.25f)
            {
                Vector3 dirToSelf = (transform.position - followTarget.position).normalized;
                agent.SetDestination(followTarget.position + dirToSelf * followDistance);
                SetWalking(true);
            }
            else
            {
                agent.ResetPath();
                SetWalking(false);

                if (enableRotation)
                {
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
            }

            yield return new WaitForSeconds(followUpdateRate);
        }
    }

    // -----------------------------------------------------------------------
    // Hilfsmethoden
    // -----------------------------------------------------------------------
    IEnumerator RotateTo(Quaternion target, float threshold = 2f)
    {
        while (Quaternion.Angle(transform.rotation, target) > threshold)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }
        transform.rotation = target;
    }

    void SetWalking(bool walking)
    {
        animator.SetBool(AnimWalking, walking);
    }

    void StopMoving()
    {
        agent.ResetPath();
        SetWalking(false);
    }

    public void StopFollowing()
    {
        StopMoving();
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }
}

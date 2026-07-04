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
    [Tooltip("Wie schnell der NPC auf Gehgeschwindigkeit beschleunigt bzw. beim Ankommen abbremst (NavMeshAgent.acceleration). Niedriger = spürbar weicheres Anlaufen und Ausrollen statt sofort voller Geschwindigkeit.")]
    public float walkAcceleration = 2f;

    [Header("Rotation")]
		[Tooltip("Wenn false, wird jegliche Rotation komplett deaktiviert. Der NPC behält dann seine Start-Rotation.")]
		public bool enableRotation = true;
		[Tooltip("Maximale Drehgeschwindigkeit in Grad/Sekunde. Begrenzt nur die Spitzengeschwindigkeit, die eigentliche Weichheit kommt von rotationAcceleration.")]
		public float rotationSpeed = 300f;
		[Tooltip("Wie zügig der NPC in die Zielrichtung eindreht (wird intern als Glättungszeit verwendet). Höherer Wert = direkter, niedrigerer Wert = weicher/träger.")]
		public float rotationAcceleration = 30f;
		[Tooltip("Wie stark kleine Drehungen zusätzlich (über das kinematische Minimum hinaus) gedämpft werden. Höher = kleine Korrekturdrehungen wirken deutlich gemächlicher als große Drehungen, statt nur proportional langsamer.")]
		[Range(1f, 4f)]
		public float kleinwinkelDaempfung = 2.5f;
		[Tooltip("Ab dieser Drehgröße (in Grad) gilt eine Drehung als 'groß' und wird NICHT zusätzlich gedrosselt - nur kleinere Korrekturen darunter werden von kleinwinkelDaempfung erfasst.")]
		public float kleinwinkelReferenzWinkel = 20f;

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
		private float currentYawSpeed = 0f; // aktuelle Drehgeschwindigkeit in Grad/Sekunde (immer >= 0)
		private float letzteDrehRichtung = 0f; // -1, 0 oder 1; für Erkennung von Richtungswechseln
		private float rotationsGroesse = 0f; // größter bisher in der laufenden Drehung gesehener Restwinkel (für kleinwinkelDaempfung)
		private Transform blickZiel; // im Stand zu fixierendes Ziel (z.B. followTarget), wird in Update() jeden Frame sanft angesehen
		private bool coroutineDrehtAktiv = false; // solange true, übernimmt eine Coroutine (z.B. Wegpunkt-Ausrichtung) die Drehung; Update() hält sich raus

    private static readonly int AnimWalking = Animator.StringToHash("isWalking");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = walkSpeed;
        agent.acceleration = walkAcceleration;
        agent.stoppingDistance = arrivalDistance;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;

        GoToNextWaypoint();
    }

    
		void Update()
		{
		    if (!enableRotation) return;
		    if (coroutineDrehtAktiv) return; // eine Coroutine steuert gerade die Drehung, sonst würde Update() ihr die Geschwindigkeit wegnehmen

		    Vector3 flatVelocity = new Vector3(agent.velocity.x, 0f, agent.velocity.z);

		    if (flatVelocity.sqrMagnitude > 0.01f)
		    {
		        DreheSanftZuRichtung(flatVelocity);
		    }
		    else if (blickZiel != null)
		    {
		        // Auch im Stand (z.B. Follow-Modus, wenn der Abstand erreicht ist) jeden Frame
		        // sanft weiterdrehen, statt nur im langsameren Coroutine-Takt.
		        Vector3 lookDir = blickZiel.position - transform.position;
		        lookDir.y = 0f;
		        DreheSanftZuRichtung(lookDir);
		    }
		    else
		    {
		        // Angesammelte Drehgeschwindigkeit verwerfen, damit beim nächsten Loslaufen
		        // nicht plötzlich in eine alte Drehrichtung "nachgezogen" wird.
		        currentYawSpeed = 0f;
		        letzteDrehRichtung = 0f;
		        rotationsGroesse = 0f;
		    }
		}

    // -----------------------------------------------------------------------
    // Zentrale Steuerung
    // -----------------------------------------------------------------------
    void GoToNextWaypoint()
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);
        coroutineDrehtAktiv = false; // falls die alte Coroutine mitten in einer Drehung war

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
        blickZiel = null; // falls zuvor im Follow-Modus gesetzt

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

        // --- Phase 2: Rotation zum Waypoint (optional) ---
        // Die Geh-/Schritt-Animation läuft bewusst WÄHREND der Drehung weiter: die sich bewegenden
        // Beine lassen es wie "Umdrehen mit Schritten" aussehen statt wie ein Pivot im Idle-Stand.
        if (enableRotation)
        {
            coroutineDrehtAktiv = true;
            yield return StartCoroutine(RotateTo(waypointTransform.rotation));
            coroutineDrehtAktiv = false;
        }

        // Erst nach der Drehung in die Idle-Pose wechseln.
        SetWalking(false);

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
                blickZiel = null; // Rotation übernimmt Update() anhand der Bewegungsrichtung
                Vector3 dirToSelf = (transform.position - followTarget.position).normalized;
                agent.SetDestination(followTarget.position + dirToSelf * followDistance);
                SetWalking(true);
            }
            else
            {
                agent.ResetPath();
                SetWalking(false);

                // Update() dreht ab jetzt jeden Frame sanft zu diesem Ziel, statt nur im
                // langsameren Coroutine-Takt von followUpdateRate.
                blickZiel = enableRotation ? followTarget : null;
            }

            yield return new WaitForSeconds(followUpdateRate);
        }
    }

    // -----------------------------------------------------------------------
    // Hilfsmethoden
    // -----------------------------------------------------------------------
    IEnumerator RotateTo(Quaternion target, float threshold = 2f)
    {
        float targetYaw = target.eulerAngles.y;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetYaw)) > threshold)
        {
            DreheSanftZuYaw(targetYaw);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
        currentYawSpeed = 0f;
        letzteDrehRichtung = 0f;
        rotationsGroesse = 0f;
    }

    // Glatte Rotation um die Y-Achse mit einem einzigen, durchgehenden Geschwindigkeitsziel
    // (statt fixer Winkelgeschwindigkeit oder einem harten Umschalten zwischen Beschleunigen/Bremsen):
    // - "idealGeschwindigkeit" ist die Geschwindigkeit, die für den aktuellen Restwinkel gerade richtig
    //   ist: kinematisch nach oben begrenzt (v² = 2·a·s, damit die Drehung exakt im Ziel endet) UND
    //   zusätzlich durch kleinwinkelDaempfung überproportional gedrosselt, wenn der Restwinkel klein ist.
    //   Dadurch bleiben kleine Korrekturen (z.B. durch NavMeshAgent-Ausweichverhalten) spürbar gemächlicher
    //   statt nur proportional langsamer.
    // - currentYawSpeed nähert sich diesem Ziel mit wachsender Beschleunigung an (nie ein Sprung),
    //   dadurch fast exponentieller Anstieg bei großen Drehungen UND sanftes Abbremsen zum Schluss,
    //   weil idealGeschwindigkeit selbst gegen 0 geht, je näher der Zielwinkel rückt.
    private void DreheSanftZuRichtung(Vector3 richtung)
    {
        if (richtung.sqrMagnitude < 0.0001f) return;
        DreheSanftZuYaw(Quaternion.LookRotation(richtung.normalized).eulerAngles.y);
    }

    private void DreheSanftZuYaw(float targetYaw)
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f)
            return;

        float currentYaw = transform.eulerAngles.y;
        float restwinkel = Mathf.DeltaAngle(currentYaw, targetYaw); // signiert, -180..180
        float restwinkelAbs = Mathf.Abs(restwinkel);

        if (restwinkelAbs < 0.05f)
        {
            currentYawSpeed = 0f;
            letzteDrehRichtung = 0f;
            rotationsGroesse = 0f;
            transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
            return;
        }

        float richtungsVorzeichen = Mathf.Sign(restwinkel);
        if (letzteDrehRichtung != 0f && richtungsVorzeichen != letzteDrehRichtung)
        {
            currentYawSpeed = 0f; // Richtungswechsel: nicht mit altem Tempo in die neue Richtung schießen
            rotationsGroesse = 0f;
        }
        letzteDrehRichtung = richtungsVorzeichen;

        // Größter bisher in dieser Drehung gesehener Restwinkel: entscheidet, ob es sich insgesamt
        // um eine kleine Korrektur oder eine "richtige" Drehung handelt. Absichtlich NICHT der aktuelle
        // (schrumpfende) Restwinkel, sonst würde fälschlich auch das Ende jeder großen Drehung gedrosselt.
        rotationsGroesse = Mathf.Max(rotationsGroesse, restwinkelAbs);

        float rotationSpeedSicher = Mathf.Max(rotationSpeed, 0.01f);
        float bremsrate = Mathf.Max(rotationAcceleration, 0.01f) * rotationSpeedSicher; // Grad/Sekunde²

        // Kinematisches Limit: die höchste Geschwindigkeit, mit der wir noch exakt im Ziel stehen bleiben.
        float kinematischesLimit = Mathf.Sqrt(2f * bremsrate * restwinkelAbs);

        // Zusätzliche, überproportionale Drosselung nur für insgesamt kleine Drehungen.
        float winkelVerhaeltnis = Mathf.Clamp01(rotationsGroesse / Mathf.Max(kleinwinkelReferenzWinkel, 0.01f));
        float daempfung = Mathf.Pow(winkelVerhaeltnis, kleinwinkelDaempfung);

        float idealGeschwindigkeit = Mathf.Min(rotationSpeedSicher * daempfung, kinematischesLimit);

        // currentYawSpeed nähert sich idealGeschwindigkeit an - egal ob das ein Beschleunigen oder
        // ein Abbremsen bedeutet, mit derselben, mit dem Tempo wachsenden Beschleunigung.
        float wachsendeBeschleunigung = bremsrate * (1f + currentYawSpeed / rotationSpeedSicher);
        float maxAenderung = wachsendeBeschleunigung * deltaTime;
        currentYawSpeed = Mathf.MoveTowards(currentYawSpeed, idealGeschwindigkeit, maxAenderung);

        float schritt = Mathf.Min(currentYawSpeed * deltaTime, restwinkelAbs);
        float neueYaw = currentYaw + richtungsVorzeichen * schritt;
        transform.rotation = Quaternion.Euler(0f, neueYaw, 0f);
    }

    void SetWalking(bool walking)
    {
        animator.SetBool(AnimWalking, walking);
    }

    void StopMoving()
    {
        agent.ResetPath();
        SetWalking(false);
        blickZiel = null;
        coroutineDrehtAktiv = false;
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

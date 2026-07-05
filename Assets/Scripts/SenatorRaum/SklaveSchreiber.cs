using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Eigenständiges Skript statt Wiederverwendung von NPCNavigator: NPCNavigator ist auf
// Wegpunkt-Patrouillen/Follow-Modus ausgelegt (Market-Sklave), hier braucht es aber nur
// einen einmaligen "rein -> hinsetzen -> raus"-Ablauf. Nutzt bewusst denselben
// "isWalking"-Animator-Parameter wie NPCNavigator/sklave_player.controller, damit die
// Lauf-Animation identisch bleibt.
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class SklaveSchreiber : MonoBehaviour
{
    [Header("Bewegung")]
    public Transform tischPunkt;
    public Transform ausgangPunkt;
    public float gehGeschwindigkeit = 1.5f;
    public float ankunftsAbstand = 0.3f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip begruessungClip;
    public AudioClip fertigClip;

    [Header("Animator-Parameter")]
    [Tooltip("Bool-Parameter fürs Gehen (wie bei NPCNavigator).")]
    public string laufParameter = "isWalking";
    [Tooltip("Bool-Parameter für die Sitz-/Schreib-Pose. Muss im Animator-Controller des Sklaven noch als eigener State ergänzt werden (aktuell existiert nur Idle/Walk).")]
    public string schreibParameter = "isSchreibend";

    private NavMeshAgent agent;
    private Animator animator;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public void GeheZuTischUndSetzeDich(Action beiAnkunft)
    {
        gameObject.SetActive(true);
        agent.enabled = true;
        agent.speed = gehGeschwindigkeit;
        agent.stoppingDistance = ankunftsAbstand;
        agent.SetDestination(tischPunkt.position);
        animator.SetBool(laufParameter, true);
        StartCoroutine(WartenBisAngekommenUndSetzen(beiAnkunft));
    }

    private IEnumerator WartenBisAngekommenUndSetzen(Action beiAnkunft)
    {
        yield return new WaitUntil(() => !agent.pathPending);
        yield return new WaitUntil(() =>
            agent.remainingDistance <= ankunftsAbstand &&
            agent.velocity.sqrMagnitude < 0.01f);

        transform.rotation = tischPunkt.rotation;
        animator.SetBool(laufParameter, false);
        animator.SetBool(schreibParameter, true);

        if (audioSource != null && begruessungClip != null)
            audioSource.PlayOneShot(begruessungClip);

        beiAnkunft?.Invoke();
    }

    public void SteheAufUndGeheRaus()
    {
        StartCoroutine(AufstehenUndGehen());
    }

    private IEnumerator AufstehenUndGehen()
    {
        animator.SetBool(schreibParameter, false);

        if (audioSource != null && fertigClip != null)
        {
            audioSource.PlayOneShot(fertigClip);
            yield return new WaitForSeconds(fertigClip.length);
        }

        animator.SetBool(laufParameter, true);
        agent.SetDestination(ausgangPunkt.position);

        yield return new WaitUntil(() => !agent.pathPending);
        yield return new WaitUntil(() =>
            agent.remainingDistance <= ankunftsAbstand &&
            agent.velocity.sqrMagnitude < 0.01f);

        animator.SetBool(laufParameter, false);
        gameObject.SetActive(false);
    }
}

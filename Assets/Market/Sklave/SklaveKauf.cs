using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class SklaveKauf : MonoBehaviour
{
    public NPCNavigator navigator;
    public NavMeshAgent navMeshAgent;

    public AudioSource verkaeuferAudioSource;
    public AudioClip[] verkaufsVoiceLines;

    public bool IstGekauft = false;

    private XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.activated.AddListener(OnActivated);
    }

    void OnDestroy()
    {
        interactable.activated.RemoveListener(OnActivated);
    }

    public void SetKaufbar(bool aktiv)
    {
        if (interactable != null)
            interactable.enabled = aktiv;
    }

    private void OnActivated(ActivateEventArgs args)
    {
        Kaufen();
    }

    public void Kaufen()
    {
        if (IstGekauft) return;
        IstGekauft = true;
        StartCoroutine(KaufAblauf());
    }

    private IEnumerator KaufAblauf()
    {
        if (verkaeuferAudioSource != null && verkaufsVoiceLines != null)
        {
            foreach (AudioClip clip in verkaufsVoiceLines)
            {
                if (clip == null) continue;
                verkaeuferAudioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }
        }

        GameObject spieler = GameObject.FindWithTag("Player");
        if (navigator != null && spieler != null)
            navigator.followTarget = spieler.transform;

        // Reihenfolge wichtig: NavMeshAgent muss aktiv sein, bevor NPCNavigator.Start() darauf zugreift.
        if (navMeshAgent != null)
            navMeshAgent.enabled = true;
        if (navigator != null)
            navigator.enabled = true;
    }
}

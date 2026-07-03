using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    public Transform senator;
    public Transform senatorStoppPosition;

    [Header("Sklaven")]
    public GameObject sklave1;
    public GameObject sklave2;

    private const float AnkunftToleranz = 0.5f;

    // Audio Clips
    public AudioClip clipSenatorAnkunft; // "Ecce! Servi pulchri hic sunt!"
    public AudioClip clipSenatorDialog1; // "Vendo mihi servum fortem, vir!"
    public AudioClip clipVerkaueferAntwort; // "Certes, domine! Optimi servi mihi sunt!"
    public AudioClip clipSenatorAuswaehlt; // "Ille! Ille servus mihi placet!"
    public AudioClip clipSenatorPreis; // "Quantum pretium pro hoc servo?"
    public AudioClip clipVerkaueferPreis; // "Quinquaginta denarios, domine!"
    public AudioClip clipSenatorAkzeptiert; // "Bene! Accipe denarios! Tu mecum veni, serve!"
    public AudioClip clipVerkaueferVerabschiedet; // "Gratias, domine! Vale!"
    
    public AudioSource senatorAudioSource;
    public AudioSource verkaeuferAudioSource;

    void Start()
    {
        DeaktiviereSklavenBewegung(sklave1);
        DeaktiviereSklavenBewegung(sklave2);

        StartCoroutine(IntroAblauf());
    }

    IEnumerator IntroAblauf()
    {
        // 1. Warten bis der Senator an seiner Startposition (SenatorStoppPosition) angekommen ist
        Debug.Log("Warte auf Senator...");
        if (senator != null && senatorStoppPosition != null)
        {
            yield return new WaitUntil(() =>
                Vector3.Distance(senator.position, senatorStoppPosition.position) <= AnkunftToleranz);
        }

        // 2. Senator kommt an (schaut dich an)
        senatorAudioSource.PlayOneShot(clipSenatorAnkunft); // ~3 Sek
        yield return new WaitForSeconds(3.5f);

        // 4. Senator redet mit Verkäufer
        senatorAudioSource.PlayOneShot(clipSenatorDialog1); // ~3 Sek
        yield return new WaitForSeconds(3.5f);

        // 5. Verkäufer antwortet
        verkaeuferAudioSource.PlayOneShot(clipVerkaueferAntwort); // ~3 Sek
        yield return new WaitForSeconds(3.5f);

        // 6. Senator wählt dich aus
        senatorAudioSource.PlayOneShot(clipSenatorAuswaehlt); // ~2 Sek
        yield return new WaitForSeconds(2.5f);

        // 7. Senator fragt Preis
        senatorAudioSource.PlayOneShot(clipSenatorPreis); // ~2 Sek
        yield return new WaitForSeconds(2.5f);

        // 8. Verkäufer sagt Preis
        verkaeuferAudioSource.PlayOneShot(clipVerkaueferPreis); // ~2 Sek
        yield return new WaitForSeconds(2.5f);

        // 9. Senator akzeptiert
        senatorAudioSource.PlayOneShot(clipSenatorAkzeptiert); // ~3 Sek
        yield return new WaitForSeconds(3.5f);

        // 10. (Optional) Verkäufer verabschiedet
        verkaeuferAudioSource.PlayOneShot(clipVerkaueferVerabschiedet); // ~2 Sek
        yield return new WaitForSeconds(2f);

        // 11. Handel ist abgeschlossen: Sklaven dürfen sich jetzt bewegen
        AktiviereSklavenBewegung(sklave1);
        AktiviereSklavenBewegung(sklave2);
    }

    private void DeaktiviereSklavenBewegung(GameObject sklave)
    {
        if (sklave == null) return;

        NPCNavigator navigator = sklave.GetComponent<NPCNavigator>();
        if (navigator != null)
            navigator.enabled = false;

        NavMeshAgent agent = sklave.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;
    }

    private void AktiviereSklavenBewegung(GameObject sklave)
    {
        if (sklave == null) return;

        // Reihenfolge wichtig: NavMeshAgent muss aktiv sein, bevor NPCNavigator.Start() darauf zugreift.
        NavMeshAgent agent = sklave.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = true;

        NPCNavigator navigator = sklave.GetComponent<NPCNavigator>();
        if (navigator != null)
            navigator.enabled = true;
    }
}

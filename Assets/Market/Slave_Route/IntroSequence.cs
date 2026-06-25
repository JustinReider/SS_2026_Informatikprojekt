using UnityEngine;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    public Transform senator;
    public UnityEngine.AI.NavMeshAgent senatorAgent;
    public Transform tribunePosition;
    public Transform senatorStoppPosition;
    
    // Audio Clips
    public AudioClip clipSenatorAnkunft; // "Ecce! Servi pulchri hic sunt!"
    public AudioClip clipSenatorDialog1; // "Vendo mihi servum fortem, vir!"
    public AudioClip clipVerkaueferAntwort; // "Certes, domine! Optimi servi mihi sunt!"
    public AudioClip clipSenatorAuswaehlt; // "Ille! Ille servus mihi placet!"
    public AudioClip clipSenatorPreis; // "Quantum pretium pro hoc servo?"
    public AudioClip clipVerkaueferPreis; // "Quinquaginta denarios, domine!"
    public AudioClip clipSenatorAkzeptiert; // "Bene! Accipe denarios! Tu mecum veni, serve!"
    public AudioClip clipVerkaueferVerabschiedet; // "Gratias, domine! Vale!"
    
    public AudioSource audioSource;

    void Start()
    {
        StartCoroutine(IntroAblauf());
    }

    IEnumerator IntroAblauf()
    {
        // 1. Warten auf Tribüne
        Debug.Log("Warte auf Senator...");
        yield return new WaitForSeconds(10f);

        // 2. Senator läuft zur Position
        Debug.Log("Senator kommt!");
        senatorAgent.SetDestination(senatorStoppPosition.position);
        
        // Warte bis Senator ankommt
        while (senatorAgent.remainingDistance > 0.5f)
            yield return null;

        yield return new WaitForSeconds(2f);

        // 3. Senator kommt an (schaut dich an)
        audioSource.PlayOneShot(clipSenatorAnkunft); // ~3 Sek
        yield return new WaitForSeconds(3.5f);

        // 4. Senator redet mit Verkäufer
        audioSource.PlayOneShot(clipSenatorDialog1); // ~3 Sek
        yield return new WaitForSeconds(3.5f);

        // 5. Verkäufer antwortet
        audioSource.PlayOneShot(clipVerkaueferAntwort); // ~3 Sek
        yield return new WaitForSeconds(3.5f);

        // 6. Senator wählt dich aus
        audioSource.PlayOneShot(clipSenatorAuswaehlt); // ~2 Sek
        yield return new WaitForSeconds(2.5f);

        // 7. Senator fragt Preis
        audioSource.PlayOneShot(clipSenatorPreis); // ~2 Sek
        yield return new WaitForSeconds(2.5f);

        // 8. Verkäufer sagt Preis
        audioSource.PlayOneShot(clipVerkaueferPreis); // ~2 Sek
        yield return new WaitForSeconds(2.5f);

        // 9. Senator akzeptiert
        audioSource.PlayOneShot(clipSenatorAkzeptiert); // ~3 Sek
        yield return new WaitForSeconds(3.5f);

        // 10. (Optional) Verkäufer verabschiedet
        audioSource.PlayOneShot(clipVerkaueferVerabschiedet); // ~2 Sek
        yield return new WaitForSeconds(2.5f);

        // 11. Cutscene: Du springst hinter Senator
        Debug.Log("Du wirst Sklave des Senators!");
        SprungHinterSenator();

        yield return new WaitForSeconds(1f);
    }

    void SprungHinterSenator()
    {
        // Spieler-Position hinter Senator setzen
        Transform spieler = Camera.main.transform.parent; // Sklave Body
        spieler.position = senator.position - senator.forward * 1f;
        spieler.rotation = senator.rotation;
        
        // SklavenFollowBauer aktivieren
        SklavenFollowBauer follow = spieler.GetComponent<SklavenFollowBauer>();
        if (follow != null)
            follow.enabled = true;
        
        Debug.Log("Du folgst jetzt dem Senator!");
    }
}

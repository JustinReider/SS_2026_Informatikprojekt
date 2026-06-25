using UnityEngine;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    public Transform senator;
    
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
        StartCoroutine(IntroAblauf());
    }

    IEnumerator IntroAblauf()
    {
        // 1. Warten auf Tribüne
        Debug.Log("Warte auf Senator...");
        yield return new WaitForSeconds(3f);

        // 2. Senator läuft zur Position
        Debug.Log("Senator kommt!");

        yield return new WaitForSeconds(2f);

        // 3. Senator kommt an (schaut dich an)
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
    }
}

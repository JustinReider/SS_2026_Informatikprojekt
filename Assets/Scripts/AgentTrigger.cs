using UnityEngine;

public class AgentTrigger : MonoBehaviour
{
    [Header("Einstellungen")]
    [Tooltip("Soll der Agent nur ein einziges Mal angetriggert werden?")]
    public bool triggerOnlyOnce = true;
    private bool hasTriggered = false;

    [Header("Audio Komponenten")]
    public AudioSource agentAudioSource;
    public AudioClip welcomeVoiceLine;

    [Header("UI Komponenten")]
    [Tooltip("Zieh hier das übergeordnete Objekt 'RolesInfos' rein, das deine Schilder enthält")]
    public GameObject infoPanelsParent;

    [Header("Role Manager")]
    [Tooltip("Zieh hier den RoleManger rein")]
    public Role RoleManager;

    // Diese Funktion wird von Unity automatisch aufgerufen, wenn etwas den Trigger betritt
    private void OnTriggerEnter(Collider other)
    {
        // Prüfen, ob das Objekt, das den Trigger betritt, den Tag "Player" hat
        if (other.CompareTag("MainCamera"))
        {
            // Wenn es nur einmal triggern soll und schon getriggert wurde, mach nichts
            if (triggerOnlyOnce && hasTriggered) return;

            // Markieren, dass es ausgelöst wurde
            hasTriggered = true;

            // HIER rufen wir gleich die Logik für das Panel und den Ton auf
            TriggerAgentAction();
        }
        else
        {
            Debug.Log("Nicht spieler");

        }
    }

    private void TriggerAgentAction()
    {
        Debug.Log("Spieler ist nah genug! Agent wird aktiviert.");

        if (agentAudioSource != null && welcomeVoiceLine != null)
        {
            agentAudioSource.clip = welcomeVoiceLine; // Packt die MP3 in den Lautsprecher
            agentAudioSource.Play();                  // Spielt sie ab
            infoPanelsParent.SetActive(true);
            RoleManager.startListen();
        }
    }
}
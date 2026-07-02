using UnityEngine;

// Auf einem GameObject mit einem Trigger-Collider platzieren.
// Schaltet die NPCVoiceLines der Sklaven ein, solange sich der Spieler im Collider befindet.
[RequireComponent(typeof(Collider))]
public class SklaveVoiceTrigger : MonoBehaviour
{
    [System.Serializable]
    public class SklaveEntry
    {
        public NPCVoiceLines voiceLines;

        [Tooltip("Optional: Solange dieses GameObject deaktiviert ist (z.B. weil der Spieler es aufgehoben hat), " +
                 "schweigt dieser Sklave, auch wenn der Spieler im Trigger steht. " +
                 "Auf None lassen, damit er durchgehend redet. Muss per SetActive(false) deaktiviert werden, nicht zerstört.")]
        public GameObject requiredObject;
    }

    [Header("Sklaven")]
    public SklaveEntry[] sklaven;

    [Header("Trigger")]
    [Tooltip("Tag, den der Spieler-Collider haben muss")]
    public string playerTag = "Player";

    private bool playerInside;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
        UpdateVoiceLines();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        UpdateVoiceLines();
    }

    void Update()
    {
        if (playerInside) UpdateVoiceLines();
    }

    void UpdateVoiceLines()
    {
        foreach (SklaveEntry entry in sklaven)
        {
            if (entry.voiceLines == null) continue;
            bool shouldTalk = playerInside && (entry.requiredObject == null || entry.requiredObject.activeInHierarchy);
            entry.voiceLines.enabled = shouldTalk;
        }
    }
}

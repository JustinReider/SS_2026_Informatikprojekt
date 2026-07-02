using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class NPCVoiceLines : MonoBehaviour
{
    [Header("Voice Lines")]
    public AudioClip[] voiceLines;

    [Header("Timing")]
    [Tooltip("Minimale Wartezeit zwischen Voice Lines in Sekunden")]
    public float minInterval = 8f;
    [Tooltip("Maximale Wartezeit zwischen Voice Lines in Sekunden")]
    public float maxInterval = 20f;

    [Header("Playback")]
    [Range(0f, 1f)]
    [Tooltip("0 = nie, 1 = immer wenn Timer abläuft")]
    public float playChance = 0.8f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        StartCoroutine(VoiceLineRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        audioSource.Stop();
    }

    IEnumerator VoiceLineRoutine()
    {
        // Zufälliger Versatz damit nicht alle NPCs gleichzeitig reden
        yield return new WaitForSeconds(Random.Range(0f, maxInterval));

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            if (Random.value > playChance) continue;
            if (audioSource.isPlaying) continue;

            PlayRandomVoiceLine();
        }
    }

    void PlayRandomVoiceLine()
    {
        if (voiceLines == null || voiceLines.Length == 0) return;
        AudioClip clip = voiceLines[Random.Range(0, voiceLines.Length)];
        audioSource.clip = clip;
        audioSource.Play();
    }
}
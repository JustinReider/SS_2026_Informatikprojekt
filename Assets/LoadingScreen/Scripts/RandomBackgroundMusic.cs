using UnityEngine;
using System.Collections;

public class RandomBackgroundMusic : MonoBehaviour, ILoadingScreenScript
{
    [Header("Soundtracks")]
    public AudioClip[] soundtracks;

    [Header("Audio Sources")]
    [Tooltip("Alle AudioSources hier reinziehen. Sie spielen immer das gleiche Lied synchron.")]
    public AudioSource[] audioSources;

    [Header("Playlist Einstellungen")]
    public bool playOnStart = true;
    [Range(0f, 1f)]
    public float volume = 0.65f;

    private AudioClip lastPlayedClip;

    void Awake()
    {
        // Falls keine zugewiesen sind, automatisch alle AudioSources am Objekt suchen
        if (audioSources == null || audioSources.Length == 0)
        {
            audioSources = GetComponents<AudioSource>();
            
            if (audioSources.Length == 0)
            {
                AudioSource newSource = gameObject.AddComponent<AudioSource>();
                audioSources = new AudioSource[] { newSource };
            }
        }

        // Grundeinstellungen auf alle Sources anwenden
        foreach (var source in audioSources)
        {
            if (source != null)
            {
                source.volume = volume;
                source.loop = false;
                source.playOnAwake = false;
                source.spatialBlend = 0f; // 2D Sound (wichtig für Musik)
            }
        }
    }

    void Start()
    {
				StartCoroutine(StartMusicWithDelay(0.5f));
    }

		public void OnLoadingScreenActivated() {
				StartCoroutine(StartMusicWithDelay(4f));
		}

		private IEnumerator StartMusicWithDelay(float delay)
		{
		    yield return new WaitForSeconds(delay);
		
		    if (playOnStart && soundtracks.Length > 0)
		    {
		        PlayRandomTrack();
		    }
		}

    public void PlayRandomTrack()
		{
		    if (soundtracks.Length == 0)
		    {
		        Debug.LogWarning("Keine Soundtracks im Array!");
		        return;
		    }
		
		    if (audioSources.Length == 0)
		    {
		        Debug.LogError("Keine AudioSource gefunden!");
		        return;
		    }
		
		    AudioClip nextClip;
		    do
		    {
		        int randomIndex = Random.Range(0, soundtracks.Length);
		        nextClip = soundtracks[randomIndex];
		    }
		    while (soundtracks.Length > 1 && nextClip == lastPlayedClip);
		
		    lastPlayedClip = nextClip;
		
		    foreach (var source in audioSources)
		    {
		        if (source != null)
		        {
		            source.clip = nextClip;
		
		            source.volume = 0f;   // ⭐ WICHTIG: IMMER 0 starten
		
		            source.Play();
		        }
		    }
		
		    StartCoroutine(WaitForTrackEnd(nextClip.length));
		}

    private IEnumerator WaitForTrackEnd(float clipLength)
    {
        yield return new WaitForSeconds(clipLength + 0.2f);
        PlayRandomTrack();
    }

    public void PlayNext()
    {
        StopAllCoroutines();
        PlayRandomTrack();
    }

    public void StopMusic()
    {
        StopAllCoroutines();
        foreach (var source in audioSources)
        {
            if (source != null) source.Stop();
        }
    }

		public IEnumerator FadeIn(float duration)
		{
		    float t = 0f;
		
		    // Start leise
		    foreach (var source in audioSources)
		    {
		        if (source != null)
		            source.volume = 0f;
		    }
		
		    while (t < duration)
		    {
		        float v = t / duration;
		
		        foreach (var source in audioSources)
		        {
		            if (source != null)
		                source.volume = volume * v;
		        }
		
		        t += Time.deltaTime;
		        yield return null;
		    }
		
		    // Sicherstellen dass Zielwert erreicht wird
		    foreach (var source in audioSources)
		    {
		        if (source != null)
		            source.volume = volume;
		    }
		}
		public IEnumerator FadeOut(float duration)
		{
		    float startVolume = volume;
		    float t = 0f;
		
		    while (t < duration)
		    {
		        float v = 1f - (t / duration);
		
		        foreach (var source in audioSources)
		        {
		            if (source != null)
		                source.volume = startVolume * v;
		        }
		
		        t += Time.deltaTime;
		        yield return null;
		    }
		
		    foreach (var source in audioSources)
		    {
		        if (source != null)
		            source.volume = 0f;
		    }
		}
}

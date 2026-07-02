using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class SoldatAnweiser : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Auf diesem GameObject muss zusätzlich ein zweiter Collider mit aktiviertem 'Is Trigger' liegen.")]
    [SerializeField] private string spielerTag = "Player";

    [Header("Animator")]
    [SerializeField] private string anweisenParameter = "Anweisen";
    [SerializeField] private string anweisenStateName = "Anweisen";

    [Header("Timing")]
    [Tooltip("Wartezeit, bevor die Anweisen-Animation erneut abgespielt wird, solange der Spieler noch im Trigger steht.")]
    [SerializeField] private float wiederholungsVerzoegerung = 5f;

    [Header("Audio")]
    [Tooltip("Die lateinische Sprachaufnahme, die abgespielt wird, sobald Anweisen = true gesetzt wird.")]
    [SerializeField] private AudioClip anweisenAudioClip;

    [Header("Untertitel")]
    [Tooltip("Bestehendes TextMeshPro-Objekt für die Untertitel über dem Kopf. Wenn leer, wird automatisch eines erzeugt.")]
    [SerializeField] private TextMeshPro untertitelText;
    [Tooltip("Position der Untertitel relativ zum Kopf des Soldaten.")]
    [SerializeField] private Vector3 untertitelVersatz = new Vector3(0f, 2.2f, 0f);
    [Tooltip("Die deutsche Übersetzung der lateinischen Audio. Jede Zeile wird nacheinander als eigener Untertitel angezeigt.")]
    [TextArea(2, 6)]
    [SerializeField] private string untertitelDeutsch;
    [Tooltip("Tipp-Geschwindigkeit der Typewriter-Animation, in Zeichen pro Sekunde.")]
    [SerializeField] private float untertitelZeichenProSekunde = 15f;
    [Tooltip("Wie lange eine fertig getippte Zeile stehen bleibt, bevor die nächste beginnt bzw. ausgeblendet wird.")]
    [SerializeField] private float untertitelHaltezeitProZeile = 1f;

    [Header("Untertitel Design")]
    [Tooltip("Schriftgröße des Untertitels (TextMeshPro-Punktgröße in Weltgröße).")]
    [SerializeField] private float untertitelSchriftgroesse = 0.4f;
    [Tooltip("Farbe des Untertitel-Textes.")]
    [SerializeField] private Color untertitelFarbe = Color.white;
    [Tooltip("Optionale, abweichende Schriftart für den Untertitel.")]
    [SerializeField] private TMP_FontAsset untertitelSchriftart;
    [Tooltip("Breite des Textfelds in Weltgröße, ab der die Untertitel umbrechen.")]
    [SerializeField] private float untertitelBreite = 2.5f;

    private Animator animator;
    private AudioSource audioSource;
    private Coroutine anweisenRoutine;
    private Coroutine untertitelRoutine;
    private Camera hauptKamera;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        hauptKamera = Camera.main;

        if (untertitelText == null)
            untertitelText = ErzeugeUntertitelText();

        KonfiguriereUntertitelText();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (untertitelText != null)
            KonfiguriereUntertitelText();
    }
#endif

    private void LateUpdate()
    {
        if (untertitelText == null) return;

        // Bewusst jeden Frame neu geholt statt gecacht: eine einmal gecachte,
        // aber falsche/inaktive Kamera (z.B. beim späteren Aktivieren der
        // echten VR-Kamera) würde sonst nie wieder aktualisiert, da die
        // Referenz dann nicht mehr null ist.
        hauptKamera = Camera.main;
        if (hauptKamera == null) return;

        untertitelText.transform.position = transform.position + untertitelVersatz;
        untertitelText.transform.rotation = Quaternion.LookRotation(
            untertitelText.transform.position - hauptKamera.transform.position);
    }

    private TextMeshPro ErzeugeUntertitelText()
    {
        var go = new GameObject("Untertitel");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = untertitelVersatz;

        return go.AddComponent<TextMeshPro>();
    }

    private void KonfiguriereUntertitelText()
    {
        untertitelText.transform.localPosition = untertitelVersatz;
        untertitelText.alignment = TextAlignmentOptions.Center;
        untertitelText.fontSize = untertitelSchriftgroesse;
        untertitelText.color = untertitelFarbe;
        if (untertitelSchriftart != null)
            untertitelText.font = untertitelSchriftart;
        untertitelText.rectTransform.sizeDelta = new Vector2(untertitelBreite, untertitelText.rectTransform.sizeDelta.y);
        untertitelText.text = string.Empty;
        untertitelText.maxVisibleCharacters = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(spielerTag)) return;

        if (anweisenRoutine == null)
            anweisenRoutine = StartCoroutine(AnweisenSchleife());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(spielerTag)) return;

        if (anweisenRoutine != null)
        {
            StopCoroutine(anweisenRoutine);
            anweisenRoutine = null;
        }

        if (untertitelRoutine != null)
        {
            StopCoroutine(untertitelRoutine);
            untertitelRoutine = null;
        }

        animator.SetBool(anweisenParameter, false);
        audioSource.Stop();
        untertitelText.text = string.Empty;
        untertitelText.maxVisibleCharacters = 0;
    }

    // Setzt Anweisen auf true, spielt die Audio + Untertitel ab, wartet bis die
    // Anweisen-Animation einmal komplett durchgelaufen ist, setzt Anweisen wieder
    // auf false (der Animator wechselt dann von selbst zurück zu Idle) und
    // wiederholt das Ganze nach der eingestellten Verzögerung.
    private IEnumerator AnweisenSchleife()
    {
        while (true)
        {
            animator.SetBool(anweisenParameter, true);

            if (anweisenAudioClip != null)
            {
                audioSource.clip = anweisenAudioClip;
                audioSource.Play();
            }

            if (untertitelRoutine != null)
                StopCoroutine(untertitelRoutine);
            untertitelRoutine = StartCoroutine(ZeigeUntertitel());

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(anweisenStateName));

            float startNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime - startNormalizedTime >= 1f);

            animator.SetBool(anweisenParameter, false);

            yield return new WaitForSeconds(wiederholungsVerzoegerung);
        }
    }

    private IEnumerator ZeigeUntertitel()
    {
        if (string.IsNullOrWhiteSpace(untertitelDeutsch))
            yield break;

        var zeilen = untertitelDeutsch.Split('\n');
        float sekundenProZeichen = untertitelZeichenProSekunde > 0f ? 1f / untertitelZeichenProSekunde : 0f;

        foreach (var rohZeile in zeilen)
        {
            var zeile = rohZeile.Trim();
            if (zeile.Length == 0) continue;

            untertitelText.text = zeile;
            untertitelText.maxVisibleCharacters = 0;

            for (int i = 1; i <= zeile.Length; i++)
            {
                untertitelText.maxVisibleCharacters = i;
                if (sekundenProZeichen > 0f)
                    yield return new WaitForSeconds(sekundenProZeichen);
            }

            yield return new WaitForSeconds(untertitelHaltezeitProZeile);
        }

        untertitelText.text = string.Empty;
        untertitelText.maxVisibleCharacters = 0;
    }
}

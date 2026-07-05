using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using System.Collections;

public class BettInteractable : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    [Header("Ziel & Ablauf")]
    [Tooltip("Szene, in die nach dem Schlafengehen geladen wird.")]
    public string zielSzene = "Lobby";
    [Tooltip("Eingang in der Zielszene.")]
    public string entranceId = "default";
    [Tooltip("Mindestdauer des schwarzen Ladebildschirms – Zeit, um den Text zu lesen.")]
    public float schlafDauer = 8f;

    [Header("Abschlusstext (in VR sichtbar)")]
    [TextArea(2, 5)]
    public string schlafText =
        "Du legst dich zur Ruhe.\n\nDer Ofen ist kalt, das letzte Brot gebacken –\nein erfülltes Leben als Bäcker geht zu Ende.\n\nSchlaf wohl.";

    private bool bettBenutzt = false;

    void Start()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnBettSelected);
    }

    void OnBettSelected(SelectEnterEventArgs args)
    {
        if (!bettBenutzt)
        {
            bettBenutzt = true;
            StartCoroutine(SchlafenAnimation());
        }
    }

    IEnumerator SchlafenAnimation()
    {
        // Kleine "Hinlegen"-Geste: Kamera leicht nach oben fahren
        Transform kamera = Camera.main.transform;
        Vector3 startPos = kamera.position;
        Vector3 endPos = startPos + Vector3.up * 0.5f;

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime / 1f;
            kamera.position = Vector3.Lerp(startPos, endPos, timer);
            yield return null;
        }

        // Übergang über den Simple-Ladebildschirm (durchgehend schwarz) inkl. Abschlusstext
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.SimpleLoadScene(zielSzene, entranceId, schlafText, schlafDauer);
        }
        else
        {
            // Fallback, falls kein LoadingScreenManager vorhanden ist
            SceneManager.LoadScene(zielSzene);
        }
    }
}

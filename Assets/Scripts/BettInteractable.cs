using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using System.Collections;

public class BettInteractable : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    public Canvas fadeCanvas;
    public float fadeDauer = 3f;
    private bool bettBenutzt = false;

    void Start()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnBettSelected);

        // Fade Canvas Setup
        if (fadeCanvas == null)
            fadeCanvas = FindObjectOfType<Canvas>();
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
        Transform kamera = Camera.main.transform;
        Vector3 startPos = kamera.position;
        Vector3 endPos = startPos + Vector3.up * 0.5f;

        float timer = 0f;

        // Kamera nach oben fahren
        while (timer < 1f)
        {
            timer += Time.deltaTime / 1f;
            kamera.position = Vector3.Lerp(startPos, endPos, timer);
            yield return null;
        }

        // Fade to Black
        yield return StartCoroutine(FadeToBlack(fadeDauer));

        // Zur Lobby teleportieren
        SceneManager.LoadScene("Lobby"); // ← Scene Name anpassen!
    }

    IEnumerator FadeToBlack(float dauer)
    {
        CanvasGroup canvasGroup = fadeCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = fadeCanvas.gameObject.AddComponent<CanvasGroup>();

        float timer = 0f;

        while (timer < dauer)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = timer / dauer;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}

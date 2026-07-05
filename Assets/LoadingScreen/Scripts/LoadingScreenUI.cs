using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingScreenUI : MonoBehaviour, ILoadingScreenScript
{
    [Header("Diashow Bilder (RawImage)")]
    public RawImage[] slideshowImages;

    [Header("Spinner")]
    public RectTransform spinner;

    [Header("Zoom Einstellungen")]
    public float imageDisplayTime = 5f;
    public float zoomOutAmount = 0.18f;
    [Range(1f, 4f)]
    public float easeStrength = 2.5f;

    [Header("Fade & Progress")]
    public CanvasGroup canvasGroup;
    public Image progressBar;

    [Header("Nachricht (eigener Canvas, z.B. für Simple-Ladebildschirm)")]
    public CanvasGroup messageCanvasGroup;
    public TMP_Text messageText;

    private int currentImageIndex = 0;
    private Coroutine slideshow;

    void Start()
    {
        if (slideshowImages.Length > 0) {
						ShuffleImages();
            slideshow = StartCoroutine(SlideshowRoutine());
				}

        if (spinner != null)
            StartCoroutine(SpinRoutine());
    }

    public void OnLoadingScreenActivated()
    {
        Start();
    }

    public void SetProgress(float value)
    {
        if (progressBar != null)
            progressBar.fillAmount = value;
    }

    // =========================
    // UI FADE IN (Canvas Alpha)
    // =========================
    public IEnumerator FadeIn(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            float v = t / duration;

            if (canvasGroup != null)
                canvasGroup.alpha = v;

            t += Time.deltaTime;
            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    // =========================
    // UI FADE OUT (Canvas Alpha)
    // =========================
    public IEnumerator FadeOut(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            float v = 1f - (t / duration);

            if (canvasGroup != null)
                canvasGroup.alpha = v;

            t += Time.deltaTime;
            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    // =========================
    // NACHRICHT (Text-Overlay)
    // =========================
    public IEnumerator ShowMessage(string message, float duration)
    {
        if (messageCanvasGroup == null) yield break;

        if (messageText != null) messageText.text = message;

        messageCanvasGroup.gameObject.SetActive(true);
        yield return FadeCanvasGroup(messageCanvasGroup, 0f, 1f, duration);
    }

    public IEnumerator HideMessage(float duration)
    {
        if (messageCanvasGroup == null) yield break;

        yield return FadeCanvasGroup(messageCanvasGroup, messageCanvasGroup.alpha, 0f, duration);
        messageCanvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;

        while (t < duration)
        {
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        cg.alpha = to;
    }

    private IEnumerator SlideshowRoutine()
    {
        while (true)
        {
            for (int i = 0; i < slideshowImages.Length; i++)
                slideshowImages[i].gameObject.SetActive(false);

            RawImage currentImage = slideshowImages[currentImageIndex];
            currentImage.gameObject.SetActive(true);

            Vector3 originalScale = currentImage.rectTransform.localScale;
            Vector3 startScale = originalScale * (1f + zoomOutAmount);
            Vector3 endScale = originalScale;

            float elapsed = 0f;
            while (elapsed < imageDisplayTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / imageDisplayTime;
                float easedT = 1f - Mathf.Pow(1f - t, easeStrength);
                currentImage.rectTransform.localScale =
                    Vector3.Lerp(startScale, endScale, easedT);

                yield return null;
            }

            yield return new WaitForSeconds(0.6f);
            currentImageIndex = (currentImageIndex + 1) % slideshowImages.Length;
        }
    }

    private IEnumerator SpinRoutine()
    {
        while (true)
        {
            spinner.Rotate(0f, 0f, -120f * Time.deltaTime);
            yield return null;
        }
    }

		private void ShuffleImages()
		{
		    for (int i = slideshowImages.Length - 1; i > 0; i--)
		    {
		        int j = Random.Range(0, i + 1);
		
		        RawImage temp = slideshowImages[i];
		        slideshowImages[i] = slideshowImages[j];
		        slideshowImages[j] = temp;
		    }
		}

    void OnDestroy()
    {
        if (slideshow != null)
            StopCoroutine(slideshow);
    }
}

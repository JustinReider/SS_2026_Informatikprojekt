using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;

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

    [Header("POST PROCESS FADE")]
    public Volume postProcessVolume;

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
    // 🔥 POST PROCESS FADE IN
    // =========================
    public IEnumerator FadeIn(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            float v = t / duration;

            // Szene wird dunkler (echter Fade)
            if (postProcessVolume != null)
                postProcessVolume.weight = 1-v;

            // UI optional parallel
            if (canvasGroup != null)
                canvasGroup.alpha = v;

            t += Time.deltaTime;
            yield return null;
        }

        if (postProcessVolume != null)
            postProcessVolume.weight = 0f;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    // =========================
    // 🔥 POST PROCESS FADE OUT
    // =========================
    public IEnumerator FadeOut(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            float v = 1f - (t / duration);

            if (postProcessVolume != null)
                postProcessVolume.weight = 1-v;

            if (canvasGroup != null)
                canvasGroup.alpha = v;

            t += Time.deltaTime;
            yield return null;
        }

        if (postProcessVolume != null)
            postProcessVolume.weight = 1f;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
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

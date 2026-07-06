using System;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Analog zu BettInteractable.cs, aber löst statt eines Szenenwechsels die
// Senator-Diktat-Sequenz aus (SenatorSchreibSequence).
//
// Bewegt bewusst das XR Origin, nicht Camera.main direkt: Der XR Origin lebt
// persistent in LoadingScreen.unity (DontDestroyOnLoad) und die Kamera darunter wird
// jeden Frame vom TrackedPoseDriver anhand der HMD-Pose neu gesetzt - ein Lerp direkt
// auf der Kamera würde also sofort wieder überschrieben. Gleiches Prinzip wie in
// HMDRecenter.cs, das aus demselben Grund ebenfalls das Origin statt der Kamera bewegt.
public class LiegeInteractable : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    [Header("Hinlegen-Geste")]
    [Tooltip("Wie weit sich das XR Origin (und damit der ganze Spieler) beim Hinlegen absenkt.")]
    public float absenkung = 0.6f;
    public float animationsDauer = 1f;

    public event Action OnHingelegt;

    private Transform xrOrigin;
    private bool liegtGerade = false;

    void Start()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnAusgewaehlt);

        var origin = FindFirstObjectByType<XROrigin>();
        if (origin != null) xrOrigin = origin.transform;
        else Debug.LogError("[LiegeInteractable] Kein XROrigin in der Szene gefunden.");
    }

    private void OnAusgewaehlt(SelectEnterEventArgs args)
    {
        if (liegtGerade || xrOrigin == null) return;
        liegtGerade = true;
        StartCoroutine(HinlegenAnimation());
    }

    private IEnumerator HinlegenAnimation()
    {
        Vector3 startPos = xrOrigin.position;
        Vector3 endPos = startPos - Vector3.up * absenkung;

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime / animationsDauer;
            xrOrigin.position = Vector3.Lerp(startPos, endPos, timer);
            yield return null;
        }

        OnHingelegt?.Invoke();
    }

    public void SpielerStehtAuf()
    {
        if (xrOrigin == null) return;
        StartCoroutine(AufstehenAnimation());
    }

    private IEnumerator AufstehenAnimation()
    {
        Vector3 startPos = xrOrigin.position;
        Vector3 endPos = startPos + Vector3.up * absenkung;

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime / animationsDauer;
            xrOrigin.position = Vector3.Lerp(startPos, endPos, timer);
            yield return null;
        }

        liegtGerade = false;
    }
}

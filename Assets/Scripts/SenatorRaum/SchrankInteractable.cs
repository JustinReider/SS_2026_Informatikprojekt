using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Am Schrank: Auswählen entnimmt eine dort abgelegte Schriftrolle (falls vorhanden),
// damit man sie erneut in die Hand nehmen und lesen kann.
public class SchrankInteractable : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    public SchrankAblage ablage;
    public Transform entnahmePunkt;
    public AudioSource audioSource;
    public AudioClip leerClip;

    void Start()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnAusgewaehlt);
    }

    private void OnAusgewaehlt(SelectEnterEventArgs args)
    {
        GameObject rolle = ablage.EntnehmenSchriftrolle(entnahmePunkt);
        if (rolle == null && audioSource != null && leerClip != null)
            audioSource.PlayOneShot(leerClip);
    }
}

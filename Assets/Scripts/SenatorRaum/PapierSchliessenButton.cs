using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Knopf/Interactable zum Schließen der Papier-Textansicht am Ende des Diktats.
public class PapierSchliessenButton : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    public SenatorSchreibSequence sequence;

    void Start()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        interactable.selectEntered.AddListener(_ => sequence.PapierSchliessenUndSchriftrolleErhalten());
    }
}

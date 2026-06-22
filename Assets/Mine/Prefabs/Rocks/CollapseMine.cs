using UnityEngine;

public class TriggerAnimatorBool : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Parameter Name im Animator")]
    public string boolParameter = "Collapse";

    private void Reset()
    {
        // versucht automatisch den Animator vom selben Objekt zu holen
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // optional: nur Player reagieren lassen
        // if (!other.CompareTag("Player")) return;

        if (animator != null)
        {
            animator.SetBool(boolParameter, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // optional
        // if (!other.CompareTag("Player")) return;

        if (animator != null)
        {
            animator.SetBool(boolParameter, false);
        }
    }
}

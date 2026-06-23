using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoubleDoorOpener : MonoBehaviour
{
    [Header("Türen")]
    public Animator door1Animator;
    public Animator door2Animator;

    [Header("Animation")]
    public string openTrigger = "Open";

    [Header("Szene wechseln")]
    public string sceneNameToLoad = "DeineSzeneName";   // <-- Hier den Namen der nächsten Szene eintragen!
    public float delayAfterAnimation = 0.5f;           // Kurze Pause nach Animationsende

    private bool doorsOpened = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    if (!doorsOpened)
                    {
                        OpenDoors();
                    }
                }
            }
        }
    }

    public void OpenDoors()
    {
        if (door1Animator != null)
            door1Animator.SetTrigger(openTrigger);

        if (door2Animator != null)
            door2Animator.SetTrigger(openTrigger);

        doorsOpened = true;

        // Starte die Überprüfung, wann die Animation fertig ist
        StartCoroutine(WaitForAnimationEnd());
    }

		private IEnumerator WaitForAnimationEnd()
{
    if (door1Animator != null)
    {
        AnimatorClipInfo[] clipInfo = door1Animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            float animLength = clipInfo[0].clip.length;
            yield return new WaitForSeconds(animLength + delayAfterAnimation);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }
    }
    else
    {
        yield return new WaitForSeconds(2f);
    }

    // Szene laden
		LoadingScreenManager.Instance.SimpleLoadScene(sceneNameToLoad);
}
}

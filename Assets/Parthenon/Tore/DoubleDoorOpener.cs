using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

[RequireComponent(typeof(XRSimpleInteractable))]
public class DoubleDoorOpener : MonoBehaviour
{
    [Header("Türen")]
    public Animator door1Animator;
    public Animator door2Animator;

    [Header("Animation")]
    public string openTrigger = "Open";

    [Header("Szene wechseln")]
    public string sceneNameToLoad = "DeineSzeneName";
    public float delayAfterAnimation = 0.5f;

    private bool doorsOpened = false;
    private XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.activated.AddListener(OnActivated);
    }

    void OnDestroy()
    {
        interactable.activated.RemoveListener(OnActivated);
    }

    private void OnActivated(ActivateEventArgs args)
    {
        if (!doorsOpened)
            OpenDoors();
    }

    public void OpenDoors()
    {
        if (door1Animator != null)
            door1Animator.SetTrigger(openTrigger);
        if (door2Animator != null)
            door2Animator.SetTrigger(openTrigger);

        doorsOpened = true;
        StartCoroutine(WaitForAnimationEnd());
    }

    private IEnumerator WaitForAnimationEnd()
    {
        if (door1Animator != null)
        {
            yield return null;
            AnimatorClipInfo[] clipInfo = door1Animator.GetCurrentAnimatorClipInfo(0);
            float animLength = clipInfo.Length > 0 ? clipInfo[0].clip.length : 2f;
            yield return new WaitForSeconds(animLength + delayAfterAnimation);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        LoadingScreenManager.Instance.SimpleLoadScene(sceneNameToLoad);
    }
}

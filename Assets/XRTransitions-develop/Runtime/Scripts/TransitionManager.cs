using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class TransitionManager : MonoBehaviour
{
    [SerializeReference] public List<Transition> Transitions;

    public XROrigin XROrigin => _xrOrigin;
    public Camera MainCamera => _mainCamera;
    public Vector3 CenterEyePosition => (_leftEyeTransform.position + _rightEyeTransform.position) / 2f; // Can't use TrackedPoseDriver for CenterEye, as Varjo does not update the MR Offset on the CenterEye, only on the Left and Right Eye.
    public Transform LeftEyeTransform => _leftEyeTransform;
    public Transform RightEyeTransform => _rightEyeTransform;

    [Header("Raum-Kontext Einstellungen")]
    [SerializeField] private Context _currentContext;
    [SerializeField] private Context _targetContext;

    // Die Properties leiten wir einfach an die Inspector-Variablen weiter
    public Context CurrentContext
    {
        get => _currentContext;
        set => _currentContext = value;
    }

    public Context TargetContext
    {
        get => _targetContext;
        set => _targetContext = value;
    }

    public bool IsTransitioning { get; private set; }

    public Transition CurrentTransition { get; private set; }

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _leftEyeTransform;
    [SerializeField] private Transform _rightEyeTransform;

    private XROrigin _xrOrigin;


    private void Awake()
    {
        _xrOrigin = FindObjectOfType<XROrigin>();

        Context.OnExit += context =>
        {
            if (CurrentContext == context)
            {
                CurrentContext = null;
            }
        };

        Context.OnEnter += context => { CurrentContext = context; };

        Transition.OnStartTransition += t =>
        {
            IsTransitioning = true;
            CurrentTransition = t;
        };

        Transition.OnEndTransition += t =>
        {
            IsTransitioning = false;

            if (TargetContext != null)
            {
                CurrentContext = TargetContext;
                TargetContext = null; // Reset für den nächsten Sprachbefehl
                Debug.Log($"[TransitionManager] Portal durchquert! Neuer Standort ist jetzt: {CurrentContext.name}");
            }

            if (t == CurrentTransition)
            {
                CurrentTransition = null;
            }
        };

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += async change =>
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                foreach (var t in Transitions.Where(transition => transition.IsInitialized))
                {
                    await t.Deinitialize();
                }
            }
        };
#endif
    }

    public List<Transition> GetActiveTransitions()
    {
        return Transitions.Where(transition => transition.IsInitialized).ToList();
    }

    public async Task InitializeTransitionType(Type type)
    {
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() != type && transition.IsInitialized)
            .Select(transition => transition.Deinitialize()));
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() == type)
            .Select(transition => transition.Initialize()));
    }

    public async Task DisableTransitions()
    {
        await Task.WhenAll(Transitions.Where(transition => transition.IsInitialized)
            .Select(transition => transition.Deinitialize()));
    }

    /*
    [MenuItem("Transition/Trigger")]
    public static void TriggerAction()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        var tManager = FindObjectOfType<TransitionManager>();
        var transition = tManager.GetActiveTransitions().FirstOrDefault(transition => transition.GetStartContext() == tManager.CurrentContext);
        if (transition != null)
        {
            transition.OnActionDown(true);
        }
    }
    */
}
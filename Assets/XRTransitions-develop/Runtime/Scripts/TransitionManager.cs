using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public PortalTransition transition;

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
    [SerializeField] public GameObject _portalPrefab;

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
    }

    public void RegisterTransition(Portal targetPortal)
    {
        if (targetPortal == null || targetPortal.transform.parent == null)
        {
            Debug.LogError("[TransitionManager] Portal oder Portal-Parent fehlt!");
            return;
        }

        Context zielContext = targetPortal.transform.parent.GetComponent<Context>();
        if (zielContext == null)
        {
            Debug.LogError($"[TransitionManager] Kein Context auf Parent von {targetPortal.name}!");
            return;
        }

        TargetContext = zielContext;
        PortalTransition neueTransition = new PortalTransition();

        // 1. TransitionManager injizieren
        FieldInfo managerField = typeof(Transition).GetField("<TransitionManager>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? typeof(Transition).GetField("TransitionManager", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        managerField?.SetValue(neueTransition, this);

        // 2. Destination setzen
        FieldInfo destinationField = typeof(Transition).GetField("<Destination>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (destinationField != null) destinationField.SetValue(neueTransition, targetPortal.transform);

        // 3. _targetContext injizieren
        FieldInfo targetContextField = typeof(Transition).GetField("_targetContext", BindingFlags.Instance | BindingFlags.NonPublic);
        if (targetContextField != null) targetContextField.SetValue(neueTransition, zielContext);

        // 4. _portalPosition (Start-Portal) setzen
        if (CurrentContext != null)
        {
            Portal lobbyPortal = CurrentContext.GetComponentInChildren<Portal>();
            if (lobbyPortal != null)
            {
                FieldInfo portalPosField = typeof(PortalTransition).GetField("_portalPosition", BindingFlags.Instance | BindingFlags.NonPublic);
                if (portalPosField != null) portalPosField.SetValue(neueTransition, lobbyPortal.transform);
            }
        }

        // 5. _portalPrefab aus Inspector injizieren
        if (_portalPrefab != null)
        {
            FieldInfo prefabField = typeof(PortalTransition).GetField("_portalPrefab", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (prefabField != null) prefabField.SetValue(neueTransition, _portalPrefab);
        }
        else
        {
            Debug.LogError("[TransitionManager] Kein _portalPrefab im Inspector zugewiesen!");
        }

        // 6. Initialisieren und zuweisen
        _ = neueTransition.Initialize();
        transition = neueTransition;

        Debug.Log($"<color=green>[TransitionManager]</color> Neue Transition registriert! Ziel: {targetPortal.name} im Raum: {TargetContext.name}");
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
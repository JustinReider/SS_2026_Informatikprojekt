using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Orb : MonoBehaviour
{
    [SerializeField] private Renderer _orbRenderer;
    public Renderer OrbRenderer => _orbRenderer;
    public Transform LocalDummy => _localDummy;

    private TransitionManager _transitionManager;

    private OrbCamera _leftOrbCamera;
    private OrbCamera _rightOrbCamera;

    private OrbTransition _transition;

    private Transform _localDummy;

    private void Awake()
    {
        _transitionManager = FindObjectOfType<TransitionManager>();

        if (_orbRenderer == null)
        {
            _orbRenderer = GetComponentInChildren<Renderer>();
        }
        
        _leftOrbCamera = new GameObject("LeftCamera").AddComponent<OrbCamera>();
        _rightOrbCamera = new GameObject("RightCamera").AddComponent<OrbCamera>();
    }

    public void Initialize(OrbTransition transition)
    {
        _transition = transition;

        _localDummy = new GameObject("OrbLocalDummy").transform;
        //_localDummy.parent = _transitionManager.XROrigin.transform;
        var camPos = _transitionManager.MainCamera.transform.position;
        camPos.y = _transitionManager.XROrigin.transform.position.y;
        _localDummy.position = camPos;
        _localDummy.rotation = Quaternion.identity;

        _leftOrbCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);
        _rightOrbCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _transitionManager.CenterEyePosition) <= 0.2f)
        {
            _transition.TriggerTransition();
        }
    }

    public void OnDestroy()
    {
        Destroy(_localDummy.gameObject);
        Destroy(_leftOrbCamera.gameObject);
        Destroy(_rightOrbCamera.gameObject);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

public class OrbTransition : Transition
{
    [SerializeField]
    private Context _startContext;
    [SerializeField]
    private GameObject _orbPrefab;

    private Orb _orb;


    internal override Task OnTriggerTransition()
    {
        TransitionManager.XROrigin.transform.position = Destination.position;
        Deinitiate();
        return Task.CompletedTask;
    }

    internal override async Task OnActionDown(bool isRight)
    {
        Initiate(isRight);
        await Task.CompletedTask;
    }
    
    internal override async Task OnActionUp()
    {
        Deinitiate();
        await Task.CompletedTask;
    }

    internal override async Task OnInitialization()
    {
        while (!XRGeneralSettings.Instance.Manager.isInitializationComplete || !TransitionManager.MainCamera.stereoEnabled)
        {
            await Task.Delay(1);
        }
    }
    
    internal override async Task OnDeinitialization()
    {
        while (TransitionManager.IsTransitioning)
        {
            await Task.Delay(1);
        }

        Deinitiate();
    }


    public override Context GetStartContext()
    {
        return _startContext;
    }

    private void Initiate(bool isRight)
    {
        if (_orb != null)
        {
            Object.Destroy(_orb.gameObject);
        }

        _orb = Object.Instantiate(_orbPrefab).GetComponent<Orb>();
        var controllerTransform = TransitionManager.XROrigin.GetComponentsInChildren<XRBaseController>().FirstOrDefault(controller =>
            controller.name.ToLower().Contains(isRight ? "right" : "left"))
            ?.transform;
        _orb.transform.parent = controllerTransform;
        _orb.transform.localPosition = new Vector3(0,0.15f,0);
        _orb.transform.localRotation = Quaternion.identity;
        _orb.Initialize(this);
    }

    private void Deinitiate()
    {
        if (_orb == null)
        {
            return;
        }

        Object.Destroy(_orb.gameObject);
        _orb = null;
    }
}
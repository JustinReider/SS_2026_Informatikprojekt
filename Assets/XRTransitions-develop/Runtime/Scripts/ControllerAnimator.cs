using System;
using System.Collections.Generic;
using Scripts;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerAnimator: MonoBehaviour
{
    private Renderer[] _renderers;

    public bool IsHidden { protected set; get; }
    
    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
    }

    private void OnEnable()
    {
        Transition.OnStartTransition += _ => Hide();
        Transition.OnEndTransition += _ => Show();
    }

    private void OnDisable()
    {
        Transition.OnStartTransition -= _ => Hide();
        Transition.OnEndTransition -= _ => Show();
    }

    public virtual void Hide()
    {
        foreach (var renderer in _renderers)
        {
            var c = renderer.material.color;
            c.a = 0;
            renderer.material.color = c;
            
        }
        IsHidden = true;
    }

    public virtual void Show()
    {
        foreach (var renderer in _renderers)
        {
            var c = renderer.material.color;
            c.a = 1;
            renderer.material.color = c;
            
        }
        IsHidden = false;
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

public class FadeTransition : Transition
    {

        [SerializeField] private Context _startContext;
        [SerializeField] private GameObject _fadePrefab;
        [SerializeField] private float _duration;
        

        private Fade _fade;
        
        internal override async Task OnInitialization()
        {
            await Task.CompletedTask;
        }

        internal override async Task OnDeinitialization()
        {
            while (TransitionManager.IsTransitioning)
            {
                await Task.Delay(1);
            }

            if (_fade != null)
            {
                Object.Destroy(_fade.gameObject);
            }
        }

        internal override async Task OnTriggerTransition()
        {
            _fade = Object.Instantiate(_fadePrefab).GetComponent<Fade>();
            _fade.Initialize(this);

            if (GetTargetContext().IsAR)
            {
                TransitionManager.XROrigin.transform.position = Destination.transform.position;
                await _fade.FadeOutAndIn(_duration);
            }
            else
            {
                await _fade.FadeOutAndIn(_duration);
                TransitionManager.XROrigin.transform.position = Destination.transform.position;
            }
            Object.Destroy(_fade.gameObject);
            _fade = null;
        }

        internal override async Task OnActionDown(bool isRight)
        {
            await TriggerTransition();
        }

        internal override async Task OnActionUp()
        {
            await Task.CompletedTask;
        }

        public override Context GetStartContext()
        {
            return _startContext;
        }
    }
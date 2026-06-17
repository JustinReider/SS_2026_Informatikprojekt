using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

namespace Scripts
{
    
    [Serializable]
    public class DissolveTransition: Transition
    {
        public float Duration => _duration;

        [SerializeField]
        private Context _startContext;
        
        [SerializeField] 
        private GameObject DissolvePrefab;

        [SerializeField] 
        private float _duration;

        private Dissolve _dissolve;

        internal override async Task OnTriggerTransition()
        {
            _dissolve = Object.Instantiate(DissolvePrefab).GetComponent<Dissolve>();
            _dissolve.Initialize(this);

            if (GetTargetContext().IsAR)
            {
                TransitionManager.XROrigin.transform.position = Destination.transform.position;
                await _dissolve.BlendForSeconds(Duration);
            }
            else
            {
                await _dissolve.BlendForSeconds(Duration);
                TransitionManager.XROrigin.transform.position = Destination.transform.position;
            }
            
            Object.Destroy(_dissolve.gameObject);
            _dissolve = null;
        }

        internal override async Task OnActionDown(bool isRight)
        {
            await TriggerTransition();
        }

        internal override async Task OnActionUp()
        {
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

            if (_dissolve != null)
            {
                Object.Destroy(_dissolve.gameObject);
            }
        }

        public override Context GetStartContext()
        {
            return _startContext;
        }
        
    }
}
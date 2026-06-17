using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;

namespace Scripts.Morph
{
    public class MorphTransition : Transition
    {
        [SerializeField] private Context _startContext;
        [SerializeField] private GameObject _morphPrefab;
        [SerializeField] private float _duration = 1.3f;

        private Morph _morph;
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
            
            if (_morph != null)
            {
                Object.Destroy(_morph);
            }
        }

        internal override async Task OnTriggerTransition()
        {
            _morph = Object.Instantiate(_morphPrefab).GetComponent<Morph>();
            _morph.Initialize(this);
            
            if (GetTargetContext().IsAR)
            {
                TransitionManager.XROrigin.transform.position = Destination.transform.position;
                await _morph.BlendForSeconds(_duration);
            }
            else
            {
                await _morph.BlendForSeconds(_duration);
                TransitionManager.XROrigin.transform.position = Destination.transform.position;
            }

            Object.Destroy(_morph.gameObject);
            _morph = null;
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
}
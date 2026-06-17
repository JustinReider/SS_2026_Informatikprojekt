using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using Object = UnityEngine.Object;

namespace Scripts
{
    [Serializable]
    public abstract class Transition
    {
        public static UnityAction<Transition> OnStartTransition;
        public static UnityAction<Transition> OnEndTransition;
        public static UnityAction<Transition,bool> OnActionPressed;
        public static UnityAction<Transition> OnActionReleased;

        [field: SerializeField] public Transform Destination { get; private set; }

        [field: SerializeField, DisableProperty]
        public bool IsInitialized { get; private set; }

        public TransitionManager TransitionManager { get; private set; }

        private Context _targetContext;
        internal abstract Task OnInitialization();
        internal abstract Task OnDeinitialization();
        internal abstract Task OnTriggerTransition();
        internal abstract Task OnActionDown(bool isRight);
        internal abstract Task OnActionUp();
        public abstract Context GetStartContext();

        public virtual Context GetTargetContext()
        {
            if (_targetContext == null)
            {
                _targetContext = Destination.GetComponentInParent<Context>();
            }

            return _targetContext;
        }

        public async Task Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            TransitionManager = Object.FindObjectOfType<TransitionManager>();
            await OnInitialization();
            IsInitialized = true;
        }

        public async Task Deinitialize()
        {
            if (!IsInitialized)
            {
                return;
            }
            
            await OnDeinitialization();
            IsInitialized = false;
        }

       

        public async Task TriggerTransition()
        {
            if (!IsInitialized || TransitionManager.CurrentContext != GetStartContext() ||
                TransitionManager.IsTransitioning)
            {
                return;
            }

            OnStartTransition?.Invoke(this);
            Context.OnExit?.Invoke(GetStartContext());

            Debug.Log("Transition");

            await OnTriggerTransition();

            Physics.SyncTransforms();

            await Task.Yield();

            Context.OnEnter?.Invoke(GetTargetContext());
            OnEndTransition?.Invoke(this);
        }
    }
}
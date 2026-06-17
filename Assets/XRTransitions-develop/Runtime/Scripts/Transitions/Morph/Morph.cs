using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Morph
{
    public class Morph : MonoBehaviour
    {
        private static readonly int Progress = Shader.PropertyToID("_Progress");

        public Renderer PlaneRenderer => planeRenderer;

        [SerializeField] private Renderer planeRenderer;
        private MorphCamera _leftPortalCamera;
        private MorphCamera _rightPortalCamera;
        private TransitionManager _transitionManager;
        private Transform _anchor;
        private bool _isTargetAR;
        private List<(Transform, Transform)> _dummyList = new();

        private void Awake()
        {
            if (planeRenderer == null)
            {
                planeRenderer = GetComponentInChildren<Renderer>();
            }

            _transitionManager = FindObjectOfType<TransitionManager>();
        }

        public void Initialize(MorphTransition transition)
        {
            _isTargetAR = transition.GetTargetContext().IsAR;
            if (_isTargetAR)
            {
                _anchor = new GameObject("MorphDestination").transform;
                var xrTransform = _transitionManager.XROrigin.transform;
                _anchor.position = xrTransform.position;
                _anchor.rotation = xrTransform.rotation;
            }
            else
            {
                _anchor = new GameObject("MorphDestination").transform;
                _anchor.position = transition.Destination.position;
                _anchor.rotation = transition.Destination.rotation;
            }
            _leftPortalCamera = new GameObject("LeftCamera").AddComponent<MorphCamera>();
            _leftPortalCamera.Initialize(this,_anchor, transition, Camera.StereoscopicEye.Left);

            _rightPortalCamera = new GameObject("RightCamera").AddComponent<MorphCamera>();
            _rightPortalCamera.Initialize(this,_anchor, transition, Camera.StereoscopicEye.Right);

            var camTransform = _transitionManager.MainCamera.transform;
            transform.parent = _transitionManager.XROrigin.transform;
            transform.position = _transitionManager.CenterEyePosition;
            transform.localRotation = Quaternion.LookRotation(camTransform.forward,
                camTransform.up);
        }

        public async Task BlendForSeconds(float seconds)
        {
            /*
            MeshFilter[] filters = _transitionManager.XROrigin.GetComponentsInChildren<MeshFilter>().Where(filter => filter.GetComponent<MeshRenderer>() != null && filter.GetComponentInParent<Dissolve>() == null).ToArray();
            foreach (MeshFilter filter in filters)
            {
                GameObject dummy = new GameObject(filter.gameObject.name + "-Dummy");
                MeshFilter dummyFilter = dummy.AddComponent<MeshFilter>();
                MeshRenderer dummyRenderer = dummy.AddComponent<MeshRenderer>();
                dummyFilter.GetCopyOf(filter);
                dummyRenderer.GetCopyOf(filter.GetComponent<MeshRenderer>());
                dummyRenderer.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
                dummy.transform.localScale = filter.transform.lossyScale;
                _dummyList.Add((filter.transform,dummy.transform));
            }
            */

            if (_isTargetAR)
            {
                PlaneRenderer.material.EnableKeyword("AR_TARGET");
            }
            else
            {
                PlaneRenderer.material.DisableKeyword("AR_TARGET");
            }
            
            var startTime = Time.time;
            PlaneRenderer.material.SetFloat(Progress, 0);
            while (Time.time <= startTime + seconds)
            {
                await Task.Yield();
                PlaneRenderer.material.SetFloat(Progress, (Time.time - startTime) / seconds);
            }

            PlaneRenderer.material.SetFloat(Progress,1);
            
            foreach (var (_, dummyTransform) in _dummyList)
            {
                Destroy(dummyTransform.gameObject);
            }
            _dummyList.Clear();
        }
        
        private void Update()
        {
            foreach (var (originalTransform, dummyTransform) in _dummyList)
            {
                dummyTransform.position =
                    _anchor.transform.TransformPoint(_transitionManager.XROrigin.transform.InverseTransformPoint(originalTransform.position));
                dummyTransform.rotation = _anchor.transform.TransformRotation(
                    _transitionManager.XROrigin.transform.InverseTransformRotation(originalTransform.rotation));
            }
        }

        private void OnDestroy()
        {
            if (_leftPortalCamera != null)
            {
                Destroy(_leftPortalCamera.gameObject);
            }

            if (_rightPortalCamera != null)
            {
                Destroy(_rightPortalCamera.gameObject);
            }
            Destroy(_anchor.gameObject);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Scripts;
using Scripts.Utils;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Dissolve : MonoBehaviour
{
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");

    public Renderer PlaneRenderer => _planeRenderer;

    [SerializeField] private Renderer _planeRenderer;
    private DissolveCamera _leftCamera;
    private DissolveCamera _rightCamera;
    private TransitionManager _transitionManager;
    private Transform _anchor;
    private bool _isTargetAR;
    private List<(Transform, Transform)> _dummyList = new();

    private void Awake()
    {
        if (_planeRenderer == null)
        {
            _planeRenderer = GetComponentInChildren<Renderer>();
        }

        _transitionManager = FindObjectOfType<TransitionManager>();
        //_localDummy.parent = _transitionManager.XROrigin.transform;
        var camPos = _transitionManager.MainCamera.transform.position;
        camPos.y = _transitionManager.XROrigin.transform.position.y;
    }

    public void Initialize(Scripts.DissolveTransition transition)
    {
        _isTargetAR = transition.GetTargetContext().IsAR;
        if (_isTargetAR)
        {
            _anchor = new GameObject("DissolveDestination").transform;
            var xrTransform = _transitionManager.XROrigin.transform;
            _anchor.position = xrTransform.position;
            _anchor.rotation = xrTransform.rotation;
        }
        else
        {
            _anchor = new GameObject("DissolveDestination").transform;
            _anchor.position = transition.Destination.position;
            _anchor.rotation = transition.Destination.rotation;
        }

        _leftCamera = new GameObject("LeftCamera").AddComponent<DissolveCamera>();
        _leftCamera.Initialize(this,_anchor, transition, Camera.StereoscopicEye.Left);

        _rightCamera = new GameObject("RightCamera").AddComponent<DissolveCamera>();
        _rightCamera.Initialize(this,_anchor, transition, Camera.StereoscopicEye.Right);

        /*
        _leftEyeCamera = new GameObject("LeftEyeCamera").AddComponent<DissolveEyeCamera>();
        _leftEyeCamera.Initialize(this,transition,Camera.StereoscopicEye.Left);
        
        _rightEyeCamera = new GameObject("RightEyeCamera").AddComponent<DissolveEyeCamera>();
        _rightEyeCamera.Initialize(this,transition,Camera.StereoscopicEye.Right);
        */

        transform.parent = _transitionManager.MainCamera.transform;
        transform.localPosition = new Vector3(0f, 0f, _transitionManager.MainCamera.nearClipPlane + 0.1f);
        transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);

        PlaneRenderer.material.SetFloat(Alpha, 0f);
    }

    public async Task BlendForSeconds(float seconds)
    {
        MeshFilter[] filters = _transitionManager.XROrigin.GetComponentsInChildren<MeshFilter>().Where(filter =>
            filter.GetComponent<MeshRenderer>() != null && filter.GetComponentInParent<Dissolve>() == null).ToArray();
        foreach (MeshFilter filter in filters)
        {
            GameObject dummy = new GameObject(filter.gameObject.name + "-Dummy");
            MeshFilter dummyFilter = dummy.AddComponent<MeshFilter>();
            MeshRenderer dummyRenderer = dummy.AddComponent<MeshRenderer>();
            dummyFilter.GetCopyOf(filter);
            dummyRenderer.GetCopyOf(filter.GetComponent<MeshRenderer>());
            dummyRenderer.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
            dummy.transform.localScale = filter.transform.lossyScale;
            _dummyList.Add((filter.transform, dummy.transform));
        }

        var startTime = Time.time;
        while (Time.time <= startTime + seconds)
        {
            await Task.Yield();
            var progress = Mathf.Min((Time.time - startTime) / seconds, 1f);
            PlaneRenderer.material.SetFloat(Alpha, _isTargetAR ? 1 - progress : progress);
        }

        PlaneRenderer.material.SetFloat(Alpha, _isTargetAR ? 0 : 1);

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
                _anchor.transform.TransformPoint(
                    _transitionManager.XROrigin.transform.InverseTransformPoint(originalTransform.position));
            dummyTransform.rotation = _anchor.transform.TransformRotation(
                _transitionManager.XROrigin.transform.InverseTransformRotation(originalTransform.rotation));
        }
    }

    private void OnDestroy()
    {
        Destroy(_leftCamera.gameObject);
        Destroy(_rightCamera.gameObject);
        Destroy(_anchor.gameObject);
    }
}
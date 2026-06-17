using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Scripts;
using Scripts.Utils;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [field: SerializeField]
    public Renderer PlaneRenderer { get; private set; }

    [SerializeField] private float _animationTime = 0.15f;

    private bool _isInitialized = false;
    
    private PortalCamera _leftPortalCamera;
    private PortalCamera _rightPortalCamera;

    private PortalTransition _transition;
    private TransitionManager _transitionManager;
    private Transform _destination;
    private Vector3 _lastPosition;
    private List<(Transform, Transform)> _dummyList = new();
    private bool _isPlayerInBounds = false;
    private void Awake()
    {
        if (PlaneRenderer == null)
        {
            PlaneRenderer = transform.Find("RenderPlane").GetComponent<MeshRenderer>();
        }
        _transitionManager = FindObjectOfType<TransitionManager>();
    }

    public async Task Initialize(PortalTransition transition)
    {
        _transition = transition;
        
        _leftPortalCamera = new GameObject("LeftCamera").AddComponent<PortalCamera>();
        _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

        _rightPortalCamera = new GameObject("RightCamera").AddComponent<PortalCamera>();
        _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);

        _destination = transition.Destination;
        _lastPosition = _transitionManager.CenterEyePosition;
        Context.OnEnter += context =>
        {
            if (context == _transition.GetStartContext())
            {
                _lastPosition = _transitionManager.CenterEyePosition;
            }
        };

        transform.localScale = Vector3.zero;
        
        var startTime = Time.time;
        while (Time.time < startTime + _animationTime)
        {
            transform.localScale =
                Vector3.Lerp(Vector3.zero, Vector3.one, (Time.time - startTime) / _animationTime);
            await Task.Delay(1);
        }
        transform.localScale = Vector3.one;

        MeshFilter[] filters = _transitionManager.XROrigin.GetComponentsInChildren<MeshFilter>().Where(filter => filter.GetComponent<MeshRenderer>() != null).ToArray();
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
        
        _isInitialized = true;
    }

    private bool FrontOfPortal(Vector3 pos)
    {
        Transform t = transform;
        return Math.Sign(Vector3.Dot(pos - t.position, t.forward)) > 0;
    }

    private async void Update()
    {
        if (!_isInitialized)
        {
            return;
        }

        if (_isPlayerInBounds)
        {
            if (FrontOfPortal(_lastPosition) && !FrontOfPortal(_transitionManager.CenterEyePosition))
            {
                await _transition.TriggerTransition();
                foreach (var (_, dummyTransform) in _dummyList)
                {
                    Destroy(dummyTransform.gameObject);
                }
                _dummyList.Clear();
            }
        }
        foreach (var (originalTransform, dummyTransform) in _dummyList)
        {
            var localToWorldMatrix = _destination.localToWorldMatrix * _transitionManager.XROrigin.transform.worldToLocalMatrix * originalTransform.localToWorldMatrix;
            dummyTransform.SetPositionAndRotation(localToWorldMatrix.GetColumn(3),localToWorldMatrix.rotation);
        }
    }

    private void LateUpdate()
    {
        _lastPosition = _transitionManager.CenterEyePosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == _transitionManager.MainCamera.transform)
        {
            _isPlayerInBounds = true;
            _lastPosition = _transitionManager.CenterEyePosition;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == _transitionManager.MainCamera.transform)
        {
            _isPlayerInBounds = false;
        }
    }

    private void OnEnable()
    {
        if (_leftPortalCamera != null)
        {
            _leftPortalCamera.gameObject.SetActive(true);
        }
        if (_rightPortalCamera != null)
        {
            _rightPortalCamera.gameObject.SetActive(true);
        }

        if (_isInitialized && _transition.GetStartContext() == _transitionManager.CurrentContext && Math.Sign(Vector3.Dot(_transitionManager.CenterEyePosition - transform.position, transform.forward)) < 0)
        {
            transform.rotation *= Quaternion.AngleAxis(180f,Vector3.up);
            _transition.Destination.rotation *= Quaternion.AngleAxis(180f,Vector3.up);
        }
    }
    
    private void OnDisable()
    {
        if (_leftPortalCamera != null)
        {
            _leftPortalCamera.gameObject.SetActive(false);
        }
        if (_rightPortalCamera != null)
        {
            _rightPortalCamera.gameObject.SetActive(false);
        }
    }

    public async Task Destroy()
    {
        transform.localScale = Vector3.one;
        
        var startTime = Time.time;
        while (Time.time < startTime + _animationTime)
        {
            transform.localScale =
                Vector3.Lerp(Vector3.one, Vector3.zero, (Time.time - startTime) / _animationTime);
            await Task.Delay(1);
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_leftPortalCamera)
        {
            Destroy(_leftPortalCamera.gameObject);
        }
        if (_rightPortalCamera)
        {
            Destroy(_rightPortalCamera.gameObject);
        }
        
        foreach (var (_, dummyTransform) in _dummyList)
        {
            if (dummyTransform != null)
            {
                Destroy(dummyTransform.gameObject);
            }
        }
        _dummyList.Clear();
    }
}
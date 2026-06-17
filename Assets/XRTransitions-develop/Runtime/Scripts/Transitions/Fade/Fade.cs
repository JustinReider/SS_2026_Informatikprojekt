using System.Linq;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;

public class Fade : MonoBehaviour
{
    private static readonly int Progress = Shader.PropertyToID("_Progress");

    public Renderer PlaneRenderer => _planeRenderer;

    [SerializeField] private Renderer _planeRenderer;
    private FadeCamera _leftCamera;
    private FadeCamera _rightCamera;
    private TransitionManager _transitionManager;
    private Transform _anchor;
    private bool _isTargetAR;

    private void Awake()
    {
        if (_planeRenderer == null)
        {
            _planeRenderer = GetComponentInChildren<Renderer>();
        }

        _transitionManager = FindObjectOfType<TransitionManager>();
    }

    public void Initialize(FadeTransition transition)
    {
        transform.parent = _transitionManager.MainCamera.transform;
        transform.localPosition = new Vector3(0f, 0f, _transitionManager.MainCamera.nearClipPlane+0.1f);
        transform.localRotation = Quaternion.AngleAxis(180,Vector3.up);
        
        _isTargetAR = transition.GetTargetContext().IsAR;
        if (_isTargetAR)
        {
            _anchor = new GameObject("FadeDestination").transform;
            var xrTransform = _transitionManager.XROrigin.transform;
            _anchor.position = xrTransform.position;
            _anchor.rotation = xrTransform.rotation;
        }
        else
        {
            _anchor = new GameObject("FadeDestination").transform;
            _anchor.position = transition.Destination.position;
            _anchor.rotation = transition.Destination.rotation;
        }

        _leftCamera = new GameObject("LeftCamera").AddComponent<FadeCamera>();
        _leftCamera.Initialize(this,_anchor, transition, Camera.StereoscopicEye.Left);

        _rightCamera = new GameObject("RightCamera").AddComponent<FadeCamera>();
        _rightCamera.Initialize(this,_anchor, transition, Camera.StereoscopicEye.Right);
        
        _planeRenderer.material.SetFloat(Progress,0f);
    }

    public async Task FadeOutAndIn(float seconds)
    {
        if (_isTargetAR)
        {
            _planeRenderer.material.EnableKeyword("AR_TARGET");
        }
        else
        {
            _planeRenderer.material.DisableKeyword("AR_TARGET");
        }
        _planeRenderer.material.SetFloat(Progress,0);
        var startTime = Time.time;
        while (Time.time <= startTime + seconds)
        {
            await Task.Yield();
            var progress = (Time.time - startTime) / seconds;
            _planeRenderer.material.SetFloat(Progress,progress);
        }
        _planeRenderer.material.SetFloat(Progress,1);
    }
    
    private void OnDestroy()
    {
        Destroy(_leftCamera.gameObject);
        Destroy(_rightCamera.gameObject);
        Destroy(_anchor.gameObject);
    }
    
}
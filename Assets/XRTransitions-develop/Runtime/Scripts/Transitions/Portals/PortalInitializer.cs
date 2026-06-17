using UnityEngine;
using Scripts;
using System.Linq;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class PortalInitializer : MonoBehaviour
{
    private PortalTransition _portalTransition;
    private TransitionManager _tManager;
    private Transform _portalTransform;
    private Camera _mainCamera;

    public async void InitializeAndSpawnPortals()
    {
        
        await SpawnPortal();

        if (_portalTransition != null)
        {
            Debug.Log("PT is " + _portalTransition.GetStartContext() + ", " + _portalTransition.GetTargetContext());
            Debug.Log("tM is " + _tManager.CurrentContext + ", " + _tManager.TargetContext);
            if (!_portalTransition.IsInitialized)
            {
                await _portalTransition.Initialize();
            }

            await _portalTransition.OnActionDown(true);
        }
        else
        {
            Debug.LogError("PortalTransition konnte nicht initialisiert werden.");
        }
    }

    public async Task SpawnPortal()
    {
        _tManager = FindObjectOfType<TransitionManager>();
        if (_tManager == null)
        {
            Debug.LogError("TransitionManager nicht gefunden!");
            return;
        }

        _mainCamera = Camera.main;
        while (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            await Task.Delay(50);
        }

        Context currentContext = _tManager.CurrentContext;
        GameObject contextObj = currentContext.gameObject;

        if (contextObj.transform.childCount <= 1)
        {
            Debug.LogError("Context1 hat kein Child(1)! Dies sollte das Portal oder dessen Platzhalter sein.");
            return;
        }
        _portalTransform = contextObj.transform.GetChild(1);

        await _tManager.InitializeTransitionType(typeof(PortalTransition));

        _portalTransition = _tManager.Transitions
        .FirstOrDefault(t => t is PortalTransition &&
                             t.GetStartContext() == currentContext &&
                             t.GetTargetContext() == _tManager.TargetContext) as PortalTransition;
        if( _portalTransition != null)
        {
            await _tManager.DisableTransitions();
            await _portalTransition.Initialize();
        }

        if (_portalTransition == null)
        {
            Debug.LogWarning("Keine PortalTransition gefunden.");
            return;
        }
    }

    public void ReSpawnPortals()
    {
        InitializeAndSpawnPortals();
    }
}

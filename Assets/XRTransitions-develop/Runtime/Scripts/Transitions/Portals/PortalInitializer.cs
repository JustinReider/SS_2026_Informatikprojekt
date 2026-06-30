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

        _portalTransition = _tManager.transition;

        if (_portalTransition == null)
        {
            return;
        }
    }

    public void ReSpawnPortals()
    {
        InitializeAndSpawnPortals();
    }
}

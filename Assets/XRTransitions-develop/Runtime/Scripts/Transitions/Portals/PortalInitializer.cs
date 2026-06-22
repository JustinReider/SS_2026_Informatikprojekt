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
        Debug.Log("SpawnPortalfirst");
        await SpawnPortal();
        Debug.Log("PortalSpawned");

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
        Debug.Log("1");
        _tManager = FindObjectOfType<TransitionManager>();
        Debug.Log("2");
        if (_tManager == null)
        {
            Debug.LogError("TransitionManager nicht gefunden!");
            return;
        }

        Debug.Log("3");
        _mainCamera = Camera.main;
        while (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            await Task.Delay(50);
        }

        Debug.Log("4");
        Context currentContext = _tManager.CurrentContext;
        Debug.Log("5");
        GameObject contextObj = currentContext.gameObject;

        Debug.Log("6");
        if (contextObj.transform.childCount <= 1)
        {
            Debug.LogError("Context1 hat kein Child(1)! Dies sollte das Portal oder dessen Platzhalter sein.");
            return;
        }
        Debug.Log("7");
        _portalTransform = contextObj.transform.GetChild(1);

        Debug.Log("9");
        _portalTransition = _tManager.transition;

        Debug.Log("11");
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

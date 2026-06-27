using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("Einstellungen")]
    public string loadingSceneName = "LoadingScreen";
    public float minStartLoadTime = 10f;
    public float minLoadTime = 2.5f;
    public float fadeTime = 0.5f;
    public string firstScene = "Terrain";
    public string firstEntranceId = "default";

    [Header("Player & XR")]
    public GameObject playerObject;
    public Camera mainCamera;
    public XROrigin xrOrigin;
    public TeleportationProvider teleportProvider;

    [Header("Global Volume")]
    public Volume globalVolume;

    private XRInteractionManager interactionManager;
    private CharacterController characterController;
    private bool isLoading = false;
    private LoadingScreenUI ui;
    private RandomBackgroundMusic music;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ui = FindFirstObjectByType<LoadingScreenUI>(FindObjectsInactive.Include);
        music = FindFirstObjectByType<RandomBackgroundMusic>(FindObjectsInactive.Include);

        if (xrOrigin == null) xrOrigin = FindFirstObjectByType<XROrigin>();
        if (teleportProvider == null) teleportProvider = FindFirstObjectByType<TeleportationProvider>();

        characterController = playerObject.GetComponent<CharacterController>();

        CreateInteractionManagerIfMissing();

        playerObject.SetActive(true);
        StartCoroutine(LoadFirstScene());
    }

    private void CreateInteractionManagerIfMissing()
    {
        interactionManager = FindFirstObjectByType<XRInteractionManager>();
        if (interactionManager == null)
        {
            GameObject managerObj = new GameObject("XR Interaction Manager");
            interactionManager = managerObj.AddComponent<XRInteractionManager>();
            DontDestroyOnLoad(managerObj);
        }
        else
        {
            DontDestroyOnLoad(interactionManager.gameObject);
        }
    }

    public void LoadScene(string targetScene, string entranceId = "default")
    {
        if (!isLoading) StartCoroutine(LoadSceneCoroutine(targetScene, entranceId));
    }

    public void SimpleLoadScene(string targetScene, string entranceId = "default")
    {
        if (!isLoading) StartCoroutine(SimpleLoadSceneCoroutine(targetScene, entranceId));
    }

    // =========================
    // FIRST SCENE LOAD
    // =========================
    IEnumerator LoadFirstScene()
    {
        yield return StartCoroutine(ui.FadeIn(fadeTime));
        if (music != null) StartCoroutine(music.FadeIn(fadeTime));

        var load = SceneManager.LoadSceneAsync(firstScene, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        float timer = 0f;
        while (timer < minStartLoadTime || load.progress < 0.9f)
        {
            timer += Time.deltaTime;
            ui?.SetProgress(Mathf.Clamp01(load.progress / 0.9f));
            yield return null;
        }

        ui?.SetProgress(1f);
        yield return new WaitForSeconds(0.3f);

        load.allowSceneActivation = true;
        yield return load;

        var loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        if (music != null) StartCoroutine(music.FadeOut(fadeTime));
        yield return StartCoroutine(ui.FadeOut(fadeTime));
        yield return new WaitForSeconds(fadeTime);

        SceneManager.SetActiveScene(loadedScene);
        yield return StartCoroutine(TeleportToEntrance(loadedScene, firstEntranceId));

        DisableLoadingScreen();
    }

    // =========================
    // NORMAL SCENE LOADS
    // =========================
    IEnumerator LoadSceneCoroutine(string targetScene, string entranceId)
    {
        isLoading = true;
        EnableLoadingScreen(false);

        PauseLocomotionScripts();
        yield return UnloadCurrentScene();

        TeleportPlayerToLoadingScreen();

        yield return StartCoroutine(PerformLoadSequence(targetScene, entranceId, true));
        isLoading = false;
    }

    IEnumerator SimpleLoadSceneCoroutine(string targetScene, string entranceId)
    {
        isLoading = true;
        EnablePostProcessing();
        EnableLoadingScreen(true);

        PauseLocomotionScripts();
        yield return UnloadCurrentScene();

        TeleportPlayerToLoadingScreen();

        yield return StartCoroutine(PerformLoadSequence(targetScene, entranceId, true, 2f));
        isLoading = false;
    }

    private IEnumerator PerformLoadSequence(string targetScene, string entranceId, bool useMusicFadeIn, float fadeInMultiplier = 1f)
    {
        if (useMusicFadeIn && music != null)
            StartCoroutine(music.FadeIn(fadeTime * fadeInMultiplier));

        var load = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        float timer = 0f;
        while (timer < minLoadTime || load.progress < 0.9f)
        {
            timer += Time.deltaTime;
            ui?.SetProgress(Mathf.Clamp01(load.progress / 0.9f));
            yield return null;
        }

        ui?.SetProgress(1f);
        load.allowSceneActivation = true;
        yield return load;

        var loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        if (music != null) StartCoroutine(music.FadeOut(fadeTime));
        yield return StartCoroutine(ui.FadeOut(fadeTime));
        yield return new WaitForSeconds(fadeTime);

        SceneManager.SetActiveScene(loadedScene);
        yield return StartCoroutine(TeleportToEntrance(loadedScene, entranceId));

        DisableLoadingScreen();
    }

    private IEnumerator UnloadCurrentScene()
    {
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }

    // =========================
    // TELEPORT
    // =========================
    private IEnumerator TeleportToEntrance(Scene scene, string entranceId)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning($"Scene '{scene.name}' nicht gültig.");
            yield break;
        }

        SceneEntrance target = FindEntrance(scene, entranceId);
        if (target == null)
        {
            Debug.LogWarning($"Kein SceneEntrance mit ID '{entranceId}' gefunden.");
            yield break;
        }

        PauseLocomotionScripts();

        var cc = characterController;
        if (cc != null) cc.enabled = false;

        // Einfache & zuverlässige Teleportation über den Provider
        var request = new TeleportRequest
        {
            destinationPosition = target.transform.position,
            destinationRotation = target.transform.rotation,
            requestTime = Time.time,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };

        teleportProvider.QueueTeleportRequest(request);

        yield return new WaitForSeconds(0.2f);

        ResetAllInteractors();
        ResumeLocomotionScripts();

        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
    }    

    private void TeleportPlayerToLoadingScreen()
    {
        StartCoroutine(TeleportToEntrance(SceneManager.GetSceneByName(loadingSceneName), "default"));
    }

    private SceneEntrance FindEntrance(Scene scene, string entranceId)
    {
        SceneEntrance fallback = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (var entrance in root.GetComponentsInChildren<SceneEntrance>(true))
            {
                if (entrance.entranceId == entranceId) return entrance;
                if (entrance.entranceId == "default") fallback = entrance;
            }
        }
        return fallback;
    }

    private void PauseLocomotionScripts()
    {
        if (characterController != null) characterController.enabled = false;

        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            string typeName = mb.GetType().Name;
            if (typeName.Contains("WallPreventer") || typeName.Contains("Locomotion"))
                mb.enabled = false;
        }
    }

    private void ResumeLocomotionScripts()
    {
        if (characterController != null) characterController.enabled = true;

        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            string typeName = mb.GetType().Name;
            if (typeName.Contains("WallPreventer") || typeName.Contains("Locomotion"))
                mb.enabled = true;
        }
    }

    private void ResetAllInteractors()
    {
        foreach (var interactor in FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None))
        {
            interactor.enabled = false;
            interactor.enabled = true;
        }

        foreach (var visual in FindObjectsByType<XRInteractorLineVisual>(FindObjectsSortMode.None))
        {
            visual.enabled = false;
            visual.enabled = true;
            if (visual.reticle != null) visual.reticle.gameObject.SetActive(true);
        }

        foreach (var nf in FindObjectsByType<NearFarInteractor>(FindObjectsSortMode.None))
        {
            nf.enabled = false;
            nf.enabled = true;
        }
    }

    // =========================
    // LOADING SCREEN
    // =========================
    void EnableLoadingScreen(bool simple)
    {
        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        foreach (GameObject go in loadingScene.GetRootGameObjects())
            go.SetActive(true);

        foreach (var script in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ILoadingScreenScript>())
            script.OnLoadingScreenActivated();
    }

    void EnablePostProcessing()
    {
        if (globalVolume != null)
            globalVolume.enabled = true;
    }

    void DisableLoadingScreen()
    {
        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        foreach (GameObject go in loadingScene.GetRootGameObjects())
        {
            if (go == playerObject) continue;
            go.SetActive(false);
        }
    }
}

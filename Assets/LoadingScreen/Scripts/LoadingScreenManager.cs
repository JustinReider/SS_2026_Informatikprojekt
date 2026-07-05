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

    [Header("Message Overlay")]
    [Tooltip("Layer, den die Overlay-Kamera für den Ladebildschirm-Text rendert. " +
             "Muss in den Project Settings als Layer existieren und darf sonst für nichts verwendet werden.")]
    public string messageOverlayLayer = "LoadingUI";

    private XRInteractionManager interactionManager;
    private CharacterController characterController;
    private bool isLoading = false;
    private LoadingScreenUI ui;
    private RandomBackgroundMusic music;
    private Camera messageOverlayCamera;
    private bool messageOverlayReady;

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
            return;
        }

        // Spiel startet komplett schwarz, bevor die erste Szene eingeblendet wird
        if (globalVolume != null)
            globalVolume.weight = 1f;
    }

    void Start()
    {
        ui = FindFirstObjectByType<LoadingScreenUI>(FindObjectsInactive.Include);
        music = FindFirstObjectByType<RandomBackgroundMusic>(FindObjectsInactive.Include);

        if (xrOrigin == null) xrOrigin = FindFirstObjectByType<XROrigin>();
        if (teleportProvider == null) teleportProvider = FindFirstObjectByType<TeleportationProvider>();

        characterController = playerObject.GetComponent<CharacterController>();

        CreateInteractionManagerIfMissing();
        SetupMessageOverlay();

        playerObject.SetActive(true);
        StartCoroutine(LoadFirstScene());
    }

    // =========================
    // MESSAGE OVERLAY CAMERA
    // Rendert den Text NACH dem Post-Processing, damit er über der Schwarzblende sichtbar bleibt.
    // =========================
    private void SetupMessageOverlay()
    {
        if (ui == null || ui.messageCanvasGroup == null) return;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        int layer = LayerMask.NameToLayer(messageOverlayLayer);
        if (layer < 0)
        {
            Debug.LogWarning($"[LoadingScreen] Layer '{messageOverlayLayer}' existiert nicht. " +
                             "Bitte in den Project Settings anlegen, sonst bleibt der Ladebildschirm-Text unsichtbar.");
            return;
        }

        // Text-Canvas (inkl. Kinder) auf den dedizierten Layer legen
        SetLayerRecursively(ui.messageCanvasGroup.gameObject, layer);

        // Hauptkamera soll den Text-Layer NICHT selbst rendern – sonst würde ihn das Post-Processing schlucken
        mainCamera.cullingMask &= ~(1 << layer);

        var baseData = mainCamera.GetUniversalAdditionalCameraData();

        var camObj = new GameObject("MessageOverlayCamera");
        camObj.transform.SetParent(mainCamera.transform, false);

        messageOverlayCamera = camObj.AddComponent<Camera>();
        messageOverlayCamera.clearFlags = CameraClearFlags.Depth;
        messageOverlayCamera.cullingMask = 1 << layer;
        messageOverlayCamera.depth = mainCamera.depth + 1;

        var overlayData = messageOverlayCamera.GetUniversalAdditionalCameraData();
        overlayData.renderType = CameraRenderType.Overlay;
        overlayData.renderPostProcessing = false;

        if (!baseData.cameraStack.Contains(messageOverlayCamera))
            baseData.cameraStack.Add(messageOverlayCamera);

        messageOverlayReady = true;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
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

    /// <summary>
    /// Startet den einfachen (durchgehend schwarzen) Ladebildschirm.
    /// </summary>
    /// <param name="targetScene">Zu ladende Szene.</param>
    /// <param name="entranceId">Ziel-Eingang in der neuen Szene.</param>
    /// <param name="message">Optionaler Text, der während des Ladens in VR angezeigt wird
    /// (z.B. "Dein Charakter wurde in die Mine geschickt"). Leer = kein Text.</param>
    /// <param name="minSimpleLoadTime">Mindestdauer des Ladebildschirms für diesen Durchlauf in Sekunden.
    /// Werte &lt; 0 verwenden den Standardwert <see cref="minLoadTime"/>.</param>
    public void SimpleLoadScene(string targetScene, string entranceId = "default", string message = "", float minSimpleLoadTime = -1f)
    {
        if (!isLoading) StartCoroutine(SimpleLoadSceneCoroutine(targetScene, entranceId, message, minSimpleLoadTime));
    }

    // =========================
    // FIRST SCENE LOAD
    // =========================
    IEnumerator LoadFirstScene()
    {
        StartCoroutine(ui.FadeIn(fadeTime));
        yield return StartCoroutine(FadeFromBlack(fadeTime));
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
        StartCoroutine(ui.FadeOut(fadeTime));
        yield return StartCoroutine(FadeToBlack(fadeTime));

        SceneManager.SetActiveScene(loadedScene);
        yield return StartCoroutine(TeleportToEntrance(loadedScene, firstEntranceId));

        DisableLoadingScreen();
        yield return StartCoroutine(FadeFromBlack(fadeTime));
    }

    // =========================
    // NORMAL SCENE LOADS
    // =========================
    IEnumerator LoadSceneCoroutine(string targetScene, string entranceId)
    {
        isLoading = true;
        PauseLocomotionScripts();

        // Übergang 1a: aktuelle Szene ausblenden (schwarz)
        yield return StartCoroutine(FadeToBlack(fadeTime));

        yield return UnloadCurrentScene();
        TeleportPlayerToLoadingScreen();

        // Übergang 1b: Ladebildschirm einblenden
        EnableLoadingScreen(false);
        StartCoroutine(ui.FadeIn(fadeTime));
        yield return StartCoroutine(FadeFromBlack(fadeTime));

        yield return StartCoroutine(PerformLoadSequence(targetScene, entranceId, true, true));
        isLoading = false;
    }

    IEnumerator SimpleLoadSceneCoroutine(string targetScene, string entranceId, string message, float minSimpleLoadTime)
    {
        isLoading = true;
        PauseLocomotionScripts();

        bool useMessageBlackout = UseMessageBlackout(message);

        // Abblenden: entweder über das opake Text-Overlay (liegt VOR dem Post-Processing,
        // damit der Text sichtbar bleibt) oder klassisch über die Post-Processing-Schwarzblende.
        if (useMessageBlackout)
            yield return StartCoroutine(ui.ShowMessage(message, fadeTime));
        else
            yield return StartCoroutine(FadeToBlack(fadeTime));

        yield return UnloadCurrentScene();
        TeleportPlayerToLoadingScreen();

        yield return StartCoroutine(PerformLoadSequence(targetScene, entranceId, true, false, 2f, minSimpleLoadTime, message));
        isLoading = false;
    }

    // Text-Overlay nur nutzen, wenn eine Nachricht vorliegt UND die Overlay-Kamera bereit ist.
    // Sonst Fallback auf die klassische Post-Processing-Schwarzblende (kein Regressionsrisiko).
    private bool UseMessageBlackout(string message)
    {
        return !string.IsNullOrEmpty(message) && messageOverlayReady && ui != null && ui.messageCanvasGroup != null;
    }

    private IEnumerator PerformLoadSequence(string targetScene, string entranceId, bool useMusicFadeIn, bool showVisuals, float fadeInMultiplier = 1f, float minTimeOverride = -1f, string message = "")
    {
        if (useMusicFadeIn && music != null)
            StartCoroutine(music.FadeIn(fadeTime * fadeInMultiplier));

        // Abblendquelle: opakes Text-Overlay (bereits vom Aufrufer eingeblendet) oder Post-Processing.
        bool useMessageBlackout = UseMessageBlackout(message);

        var load = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        float minTime = minTimeOverride >= 0f ? minTimeOverride : minLoadTime;
        float timer = 0f;
        while (timer < minTime || load.progress < 0.9f)
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

        // Übergang 2a: Ladebildschirm ausblenden, um den Szenenwechsel zu verdecken
        // (im "simple" Modus ist der Bildschirm bereits schwarz, daher kein erneuter Fade nötig)
        if (showVisuals)
        {
            StartCoroutine(ui.FadeOut(fadeTime));
            yield return StartCoroutine(FadeToBlack(fadeTime));
        }

        SceneManager.SetActiveScene(loadedScene);
        yield return StartCoroutine(TeleportToEntrance(loadedScene, entranceId));

        DisableLoadingScreen();

        // Übergang 2b: neue Szene einblenden – Text-Overlay bzw. Post-Processing wieder aufblenden
        if (useMessageBlackout)
            yield return StartCoroutine(ui.HideMessage(fadeTime));
        else
            yield return StartCoroutine(FadeFromBlack(fadeTime));
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

    // =========================
    // GLOBAL VOLUME FADE (schwarzblende zwischen Szenen)
    // =========================
    private IEnumerator FadePostProcess(float target, float duration)
    {
        if (globalVolume == null) yield break;

        float start = globalVolume.weight;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            globalVolume.weight = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        globalVolume.weight = target;
    }

    private IEnumerator FadeToBlack(float duration)
    {
        yield return FadePostProcess(1f, duration);
    }

    private IEnumerator FadeFromBlack(float duration)
    {
        yield return FadePostProcess(0f, duration);
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

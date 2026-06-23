using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;           
using UnityEngine.Rendering.Universal;

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

    [Header("Player")]
    public GameObject playerObject;

		[Header("Global Volume")]
		public Volume globalVolume;

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
            return;
        }
    }

    void Start()
    {
        ui = FindFirstObjectByType<LoadingScreenUI>(FindObjectsInactive.Include);
        music = FindFirstObjectByType<RandomBackgroundMusic>(FindObjectsInactive.Include);

        playerObject.SetActive(true);
        StartCoroutine(LoadFirstScene());
    }

    public void LoadScene(string targetScene, string entranceId = "default")
    {
        if (!isLoading)
            StartCoroutine(LoadSceneCoroutine(targetScene, entranceId));
    }
		public void SimpleLoadScene(string targetScene, string entranceId = "default")
    {
        if (!isLoading)
            StartCoroutine(SimpleLoadSceneCoroutine(targetScene, entranceId));
    }

    // =========================
    // FIRST SCENE LOAD
    // =========================
    IEnumerator LoadFirstScene()
    {
        yield return StartCoroutine(ui.FadeIn(fadeTime));

        if (music != null)
            StartCoroutine(music.FadeIn(fadeTime));

        AsyncOperation load = SceneManager.LoadSceneAsync(firstScene, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            ui?.SetProgress(Mathf.Clamp01(load.progress / 0.9f));

            if (load.progress >= 0.9f && timer >= minStartLoadTime)
                break;

            yield return null;
        }

        ui?.SetProgress(1f);
        yield return new WaitForSeconds(0.3f);

        load.allowSceneActivation = true;
        yield return load;

        if (music != null)
            StartCoroutine(music.FadeOut(fadeTime));

        yield return StartCoroutine(ui.FadeOut(fadeTime));

        yield return new WaitForSeconds(fadeTime);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(firstScene));
        TeleportPlayerToEntrance(firstScene, firstEntranceId);

        playerObject.SetActive(true);

        DisableLoadingScreen();
    }

    // =========================
    // GENERIC SCENE LOAD
    // =========================
    IEnumerator LoadSceneCoroutine(string targetScene, string entranceId)
    {
        isLoading = true;

        EnableLoadingScreen(false);

        Scene currentScene = SceneManager.GetActiveScene();
        yield return SceneManager.UnloadSceneAsync(currentScene);

        TeleportPlayerToEntrance(loadingSceneName, "default");

        yield return StartCoroutine(ui.FadeIn(fadeTime));

        if (music != null)
            StartCoroutine(music.FadeIn(fadeTime));

        AsyncOperation load = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            ui?.SetProgress(Mathf.Clamp01(load.progress / 0.9f));

            if (load.progress >= 0.9f && timer >= minLoadTime)
                break;

            yield return null;
        }

        ui?.SetProgress(1f);

        yield return PrewarmPhysics(SceneManager.GetSceneByName(targetScene));

        load.allowSceneActivation = true;
        yield return load;

        float warmupTime = 0f;
        float warmupDuration = 0.5f;

        while (warmupTime < warmupDuration)
        {
            warmupTime += Time.unscaledDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        yield return null;

        if (music != null)
            StartCoroutine(music.FadeOut(fadeTime));

        yield return StartCoroutine(ui.FadeOut(fadeTime));

        yield return new WaitForSeconds(fadeTime);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));
        TeleportPlayerToEntrance(targetScene, entranceId);

        playerObject.SetActive(true);

        DisableLoadingScreen();

        isLoading = false;
    }

		IEnumerator SimpleLoadSceneCoroutine(string targetScene, string entranceId)
    {
        isLoading = true;
				EnablePostProcessing();

        EnableLoadingScreen(true);

        Scene currentScene = SceneManager.GetActiveScene();
        yield return SceneManager.UnloadSceneAsync(currentScene);

        TeleportPlayerToEntrance(loadingSceneName, "default");

        if (music != null)
            StartCoroutine(music.FadeIn(fadeTime*2));

        AsyncOperation load = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            ui?.SetProgress(Mathf.Clamp01(load.progress / 0.9f));

            if (load.progress >= 0.9f && timer >= minLoadTime)
                break;

            yield return null;
        }

        ui?.SetProgress(1f);

        yield return PrewarmPhysics(SceneManager.GetSceneByName(targetScene));

        load.allowSceneActivation = true;
        yield return load;

        float warmupTime = 0f;
        float warmupDuration = 0.5f;

        while (warmupTime < warmupDuration)
        {
            warmupTime += Time.unscaledDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        yield return null;

        if (music != null)
            StartCoroutine(music.FadeOut(fadeTime));

        yield return new WaitForSeconds(fadeTime);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));
        TeleportPlayerToEntrance(targetScene, entranceId);

        playerObject.SetActive(true);

        DisableLoadingScreen();

        isLoading = false;
    }


    // =========================
    // PLAYER TELEPORT
    // =========================
    void TeleportPlayerToEntrance(string sceneName, string entranceId)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneEntrance targetEntrance = null;
        SceneEntrance fallback = null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (SceneEntrance entrance in root.GetComponentsInChildren<SceneEntrance>())
            {
                if (entrance.entranceId == entranceId)
                {
                    targetEntrance = entrance;
                    break;
                }

                if (entrance.entranceId == "default")
                    fallback = entrance;
            }

            if (targetEntrance != null) break;
        }

        SceneEntrance spawn = targetEntrance ?? fallback;

        if (spawn != null)
        {
            var cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerObject.transform.SetPositionAndRotation(
                spawn.transform.position,
                spawn.transform.rotation
            );

            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning($"Kein SceneEntrance mit id='{entranceId}' in '{sceneName}' gefunden.");
        }
    }

    // =========================
    // LOADING SCREEN CONTROL
    // =========================
    void EnableLoadingScreen(bool simple)
    {
        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);

        foreach (GameObject go in loadingScene.GetRootGameObjects()) {
            //if (go == playerObject && simple) go.SetActive(false);
            go.SetActive(true);
				}

				//if (!simple)
        foreach (var script in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ILoadingScreenScript>())
            script.OnLoadingScreenActivated();
    }

		void EnablePostProcessing()
    {
			if (globalVolume != null) {
				globalVolume.enabled = true;
			}
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

    // =========================
    // PHYSICS PREWARM
    // =========================
    IEnumerator PrewarmPhysics(Scene scene)
    {
        var meshColliders = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<MeshCollider>(true))
            .Where(mc => mc.sharedMesh != null);

        foreach (var mc in meshColliders)
        {
            mc.enabled = false;
            mc.enabled = true;

            yield return null;
        }
    }
}

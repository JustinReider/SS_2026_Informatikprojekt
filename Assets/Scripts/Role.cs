using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Scripts;
using UnityEngine.SceneManagement;

public class Role : MonoBehaviour
{
    public int role;
    public bool listening;
    private bool istAmLaden = false;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
    private TransitionManager _transitionManager;
    private PortalInitializer _portalInitializer;

    [Header("Kontext Zuordnungen")]
    // Hier ziehen wir im Inspector die Contexte aus deiner Hierarchie rein!
    [SerializeField] private Context contextLobby;
    [SerializeField] private Context contextSenatorRaum;
    [SerializeField] private Context contextMarkt;
    [SerializeField] private Context contextBakery;

    [Header("UI Komponenten")]
    [Tooltip("Zieh hier das übergeordnete Objekt 'RolesInfos' rein, das deine Schilder enthält")]
    public GameObject infoPanelsParent;

    void Start()
    {
        _transitionManager = FindObjectOfType<TransitionManager>();
        _portalInitializer = FindObjectOfType<PortalInitializer>();

        role = 0;
        listening = false;

        // 1. Sprachbefehle definieren
        keywords.Add("lobby", () => { WaehleRolleViaSprache(0); });
        keywords.Add("sklave", () => { WaehleRolleViaSprache(1); });
        keywords.Add("senator", () => { WaehleRolleViaSprache(2); });
        keywords.Add("händler", () => { WaehleRolleViaSprache(3); });

        // 2. Den Recognizer vorbereiten (aber noch NICHT starten)
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
    }

    // Diese Methode wird von deinem UI-Button aufgerufen
    public void startListen()
    {
        listening = true;
        if (keywordRecognizer != null && !keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Start();
            Debug.Log("Mikrofon aktiv... Bitte sprich jetzt!");
        }
    }

    // Wird aufgerufen, wenn ein passendes Wort erkannt wurde
    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (keywords.TryGetValue(args.text, out System.Action keywordAction))
        {
            Debug.Log("Sprachbefehl erkannt: " + args.text);
            keywordAction.Invoke();
        }
    }

    private async void WaehleRolleViaSprache(int roleID)
    {
        infoPanelsParent.SetActive(false);
        // 1. SCHUTZ VOR DOPPEL-TRIGGER: Bricht sofort ab, wenn die Methode bereits läuft
        if (istAmLaden)
        {
            Debug.LogWarning("[Portal-System] Sprachbefehl ignoriert: Es wird bereits geladen!");
            return;
        }

        if (role == roleID)
        {
            return;
        }

        // Sperre aktivieren und Mikrofon stummschalten während des Ladens
        istAmLaden = true;
        stopListen();
        int currentszene = role == 3 ? 1 : role;
        role = roleID;

        if (_transitionManager != null)
        {
            // Falls bei Händler (3) die Szene 1 geladen werden soll:
            int tatsaechlicherSzenenIndex = (roleID == 3) ? 1 : roleID;

            // ====================================================================
            // ERWEITERUNG A: Unnötige Szenen entladen (Schützt VR vor Memory-Overflow)
            // ====================================================================
            for (int i = SceneManager.sceneCount - 1; i > 0; i--)
            {
                Scene offeneSzene = SceneManager.GetSceneAt(i);
                if (offeneSzene.IsValid())
                {
                    // Entlade, wenn es nicht die Lobby (0), nicht die Zielszene und nicht die aktive Spieler-Szene ist
                    if (offeneSzene.buildIndex != currentszene &&
                        offeneSzene.buildIndex != tatsaechlicherSzenenIndex)
                    {
                        Debug.Log($"[Szenen-Manager] Entlade alte ungenutzte Szene: {offeneSzene.name}");
                        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(offeneSzene);
                        while (unloadOp != null && !unloadOp.isDone)
                        {
                            await System.Threading.Tasks.Task.Yield();
                        }
                    }
                }
            }
           
            Scene checkScene = SceneManager.GetSceneByBuildIndex(tatsaechlicherSzenenIndex);

            if (checkScene.IsValid() && checkScene.isLoaded)
            {
                Debug.Log($"[Portal-System] Szene {tatsaechlicherSzenenIndex} ist bereits geladen. Überspringe Ladevorgang.");
            }
            else
            {
                Debug.Log($"[Portal-System] Lade Szene {tatsaechlicherSzenenIndex} additiv...");
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(tatsaechlicherSzenenIndex, LoadSceneMode.Additive);

                // Sauberer Task-Warter ohne TaskCompletionSource-Müll
                while (!loadOp.isDone)
                {
                    await System.Threading.Tasks.Task.Yield();
                }
            }

            Scene neuGeladeneSzene = SceneManager.GetSceneByBuildIndex(tatsaechlicherSzenenIndex);
            Portal gefundenesPortal = null;

            if (neuGeladeneSzene.IsValid())
            {
                GameObject[] rootObjects = neuGeladeneSzene.GetRootGameObjects();
                foreach (GameObject go in rootObjects)
                {
                    gefundenesPortal = go.GetComponentInChildren<Portal>();
                    if (gefundenesPortal != null)
                    {
                        break; 
                    }
                }
            }

            if (gefundenesPortal != null)
            {
                Debug.Log($"<color=cyan>[Portal-Finder]</color> Erfolg! Portal-Komponente in der neuen Szene gefunden auf Objekt: {gefundenesPortal.name}");
                _transitionManager.RegisterTransition(gefundenesPortal);
            }
            else
            {
                Debug.LogError($"[Portal-Finder] Kritischer Fehler: In der geladenen Szene wurde kein 'Portal' gefunden!");
                // Reißleine ziehen bei Fehlern, um Hänger im Initializer zu vermeiden
                istAmLaden = false;
                startListen();
                return;
            }

            // INITIALIZER TRIGGERN
            if (_portalInitializer != null)
            {
                Debug.Log("CurrentContext ist " + _transitionManager.CurrentContext + ", TargetContext ist " + _transitionManager.TargetContext + ". Informiere PortalInitializer...");
                _portalInitializer.InitializeAndSpawnPortals();
            }
            else
            {
                Debug.LogWarning("PortalInitializer wurde in der Szene nicht gefunden!");
            }
        }

        // Lade-Sperre aufheben und Mikrofon wieder aktivieren
        istAmLaden = false;
    }

    private void stopListen()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            Debug.Log("Mikrofon wieder deaktiviert.");
        }
        listening = false;
    }

    public void OnDestroy()
    {
        if (keywordRecognizer != null)
        {
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            if (keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Stop();
            }
        }
    }
}
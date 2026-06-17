using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Scripts;

public class Role : MonoBehaviour
{
    public int role;
    public bool listening;

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
        startListen();
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

    private void WaehleRolleViaSprache(int roleID)
    {
        if (role == roleID)
        {
            Debug.Log("Rolle bereits current");
            return;
        }
        role = roleID;

        if (_transitionManager != null)
        {
            switch (roleID)
            {
                case 0: // lobby
                    _transitionManager.TargetContext = contextLobby;
                    Debug.Log("Portal-Ziel via Sprache geändert auf: Lobby");
                    break;
                case 1: // sklave
                    _transitionManager.TargetContext = contextMarkt; 
                    Debug.Log("Rolle Sklave gewählt (Ziel-Kontext muss noch zugewiesen werden)");
                    break;
                case 2: // senator
                    _transitionManager.TargetContext = contextSenatorRaum;
                    Debug.Log("Portal-Ziel via Sprache geändert auf: Senator Raum");
                    break;
                case 3: // händler
                    _transitionManager.TargetContext = contextBakery;
                    Debug.Log("Portal-Ziel via Sprache geändert auf: Markt");
                    break;
            }
            // 3. JETZT informieren wir den PortalInitializer, dass er das Portal öffnen soll!
            if (_portalInitializer != null)
            {
                Debug.Log("CurrentContext ist " + _transitionManager.CurrentContext + ", TargetContext ist " + _transitionManager.TargetContext + ". Informiere PortalInitializer: Öffne Portale für das neue Ziel...");
                _portalInitializer.InitializeAndSpawnPortals();
            }
            else
            {
                Debug.LogWarning("PortalInitializer wurde in der Szene nicht gefunden!");
            }
        }

        // Nach erfolgreicher Erkennung stoppen wir das Zuhören direkt wieder!
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
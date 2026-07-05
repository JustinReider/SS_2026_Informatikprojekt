using System;
using System.Text;
using UnityEngine;
using UnityEngine.Windows.Speech;

// Freies Diktat für die Senator-Schreibszene. Läuft über DictationRecognizer statt
// KeywordRecognizer (wie in Role.cs), weil beide nicht gleichzeitig laufen können -
// das Stop-Kommando wird deshalb direkt im erkannten Diktat-Text gesucht, statt über
// einen zweiten, parallelen KeywordRecognizer.
public class DiktatController : MonoBehaviour
{
    [Header("Stop-Erkennung")]
    [Tooltip("Wörter, die das Diktat sofort beenden, wenn sie als eigenes Wort in einem erkannten Satz vorkommen.")]
    public string[] stopWoerter = { "stop", "stopp" };

    public event Action<string> OnZwischenstand;       // Live-Vorschau (Hypothese), noch nicht endgültig
    public event Action<string> OnGesamtTextGeaendert; // Bisheriger endgültiger Gesamttext
    public event Action<string> OnDiktatBeendet;        // Endgültiger Gesamttext, Diktat ist vorbei

    public bool Laeuft { get; private set; }

    private DictationRecognizer recognizer;
    private readonly StringBuilder gesamtText = new StringBuilder();

    public void StartDiktat()
    {
        if (Laeuft) return;

        gesamtText.Clear();
        recognizer = new DictationRecognizer();
        recognizer.DictationHypothesis += OnHypothesis;
        recognizer.DictationResult += OnResult;
        recognizer.DictationComplete += OnComplete;
        recognizer.DictationError += OnError;
        recognizer.Start();
        Laeuft = true;
    }

    public void StopDiktat()
    {
        if (!Laeuft) return;
        Laeuft = false;
        BeendeRecognizer();
    }

    public string GetGesamtText() => gesamtText.ToString().Trim();

    private void OnHypothesis(string text)
    {
        string vorschau = gesamtText.Length > 0 ? gesamtText + " " + text : text;
        OnZwischenstand?.Invoke(vorschau);
    }

    private void OnResult(string text, ConfidenceLevel confidence)
    {
        if (EnthaeltStopWort(text))
        {
            Laeuft = false;
            string finalerText = gesamtText.ToString().Trim();
            OnDiktatBeendet?.Invoke(finalerText);
            BeendeRecognizer();
            return;
        }

        if (gesamtText.Length > 0) gesamtText.Append(" ");
        gesamtText.Append(text.Trim());
        OnGesamtTextGeaendert?.Invoke(gesamtText.ToString());
    }

    private bool EnthaeltStopWort(string text)
    {
        string[] worte = text.ToLowerInvariant().Split(
            new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string wort in worte)
            foreach (string stopWort in stopWoerter)
                if (wort == stopWort) return true;

        return false;
    }

    private void OnComplete(DictationCompletionCause cause)
    {
        // z.B. TimeoutExceeded: Recognizer hat sich bereits selbst beendet, Sequenz sauber abschließen.
        if (!Laeuft) return;
        Laeuft = false;
        OnDiktatBeendet?.Invoke(gesamtText.ToString().Trim());
    }

    private void OnError(string error, int hresult)
    {
        Debug.LogWarning($"[DiktatController] Fehler: {error} ({hresult})");
    }

    private void BeendeRecognizer()
    {
        if (recognizer == null) return;

        recognizer.DictationHypothesis -= OnHypothesis;
        recognizer.DictationResult -= OnResult;
        recognizer.DictationComplete -= OnComplete;
        recognizer.DictationError -= OnError;

        if (recognizer.Status == SpeechSystemStatus.Running)
            recognizer.Stop();

        recognizer.Dispose();
        recognizer = null;
    }

    void OnDestroy()
    {
        BeendeRecognizer();
    }
}

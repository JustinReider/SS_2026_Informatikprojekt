using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// BF1-style Death Card für "Das Dynamische Museum".
/// Einfach ein leeres GameObject erstellen, dieses Script draufziehen — fertig.
/// Der Canvas wird beim ersten Start automatisch gebaut, falls er noch nicht existiert.
/// Danach alles im Inspector frei editierbar.
///
/// Aufruf: SlaveDeathCard.Instance.Show();
///         SlaveDeathCard.Instance.Show(myData);
/// </summary>
public class SlaveDeathCard : MonoBehaviour
{
    public static SlaveDeathCard Instance { get; private set; }

    // ---------------------------------------------------------------
    // Daten-Struct
    // ---------------------------------------------------------------

    [System.Serializable]
    public struct DeathCardData
    {
        public string name;
        public string dates;
        public string role;
        [TextArea(3, 6)] public string bio;
        public string latin;
        public string translation;
    }

    // ---------------------------------------------------------------
    // Inspector-Felder  (werden per Code befüllt falls noch leer)
    // ---------------------------------------------------------------

    [Header("— Canvas (wird auto-erstellt wenn leer) —")]
    [SerializeField] private Canvas       canvas;
    [SerializeField] private CanvasGroup  canvasGroup;

    [Header("Lines")]
    [SerializeField] private Image topLine;
    [SerializeField] private Image dividerLine;
    [SerializeField] private Image bottomLine;

    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI datesText;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private TextMeshProUGUI bioText;
    [SerializeField] private TextMeshProUGUI latinText;
    [SerializeField] private TextMeshProUGUI translText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration  = 1.8f;
    [SerializeField] private float charDelay       = 0.055f;
    [SerializeField] private float blockPause      = 0.9f;
    [SerializeField] private float fadeOutDuration = 2.5f;
    [SerializeField] private float holdDuration    = 5.5f;
    [Tooltip("Fade-Dauer pro Bio-Zeile")]
    [SerializeField] private float bioLineFade     = 0.9f;
    [Tooltip("Pause zwischen Bio-Zeilen")]
    [SerializeField] private float bioLinePause    = 0.55f;

    [Header("Colors")]
    [SerializeField] private Color nameColor  = new Color(0.91f, 0.85f, 0.69f);
    [SerializeField] private Color datesColor = new Color(0.54f, 0.48f, 0.35f);
    [SerializeField] private Color roleColor  = new Color(0.48f, 0.42f, 0.29f);
    [SerializeField] private Color bioColor   = new Color(0.69f, 0.63f, 0.50f);
    [SerializeField] private Color latinColor = new Color(0.35f, 0.31f, 0.21f);
    [SerializeField] private Color lineColor  = new Color(0.54f, 0.42f, 0.16f, 0.6f);

    [Header("Default Card Data")]
    [SerializeField] private DeathCardData defaultData = new DeathCardData
    {
        name        = "Caius, Sohn des Brennus",
        dates       = "73 v. Chr.  —  50 v. Chr.",
        role        = "Damnatus ad Metalla  ·  Silbermine, Hispania",
        bio         = "Als Kriegsgefangener nach der Niederlage seines gallischen Stammes\n"
                    + "nach Rom verschleppt. Im Alter von dreiundzwanzig Jahren\n"
                    + "zur Arbeit in den Minen verurteilt.\n\n"
                    + "Er hinterließ keinen Namen in den Aufzeichnungen.\n"
                    + "Kein Grab. Keine Inschrift.\n"
                    + "Nur Stille unter dem Stein.",
        latin       = "Nemo in vita sua nomen reliquit",
        translation = "Niemand hinterließ in seinem Leben einen Namen"
    };

    private Coroutine _activeRoutine;

    // ---------------------------------------------------------------
    // Awake — Canvas nur bauen wenn Felder noch nicht gesetzt sind
    // ---------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Nur bauen wenn canvas noch nicht existiert/zugewiesen
        if (canvas == null)
            BuildCanvas();

        canvasGroup.alpha = 0f;
        canvas.enabled    = false;
        HideAllImmediate();
    }

		private void Start() {
				SlaveDeathCard.Instance.Show();
		}

    // ---------------------------------------------------------------
    // Auto-Build
    // ---------------------------------------------------------------

    private void BuildCanvas()
    {
        // Root-Canvas
        GameObject canvasGO = new GameObject("DeathCard_Canvas");
        canvasGO.transform.SetParent(transform);

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ((CanvasScaler)canvasGO.GetComponent<CanvasScaler>()).referenceResolution =
            new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        canvasGroup = canvasGO.AddComponent<CanvasGroup>();

        // Background
        GameObject bg = MakeRect("Background", canvasGO.transform);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.black;
        StretchFull(bg.GetComponent<RectTransform>());

        // Center-Container (schmaler als Screen, für das BF1-Layout)
        GameObject center = MakeRect("Center", canvasGO.transform);
        RectTransform centerRT = center.GetComponent<RectTransform>();
        centerRT.anchorMin        = new Vector2(0.5f, 0.5f);
        centerRT.anchorMax        = new Vector2(0.5f, 0.5f);
        centerRT.pivot            = new Vector2(0.5f, 0.5f);
        centerRT.sizeDelta        = new Vector2(700f, 600f);
        centerRT.anchoredPosition = Vector2.zero;

        // Vertikaler Layout-Offset — Elemente werden manuell positioniert
        float y = 220f;   // startet oben, geht nach unten

        topLine    = MakeLine("TopLine",    center.transform, 120f, 1f, y);      y -= 40f;
        nameText   = MakeTMP ("NameText",   center.transform, 600f, 60f,  y, 36f, true,  false); y -= 55f;
        datesText  = MakeTMP ("DatesText",  center.transform, 600f, 35f,  y, 16f, false, true);  y -= 50f;
        dividerLine= MakeLine("Divider",    center.transform,  40f, 1f,   y);     y -= 35f;
        roleText   = MakeTMP ("RoleText",   center.transform, 600f, 30f,  y, 11f, false, false); y -= 60f;
        bioText    = MakeTMP ("BioText",    center.transform, 600f, 160f, y, 15f, false, true);  y -= 175f;
        bioText.alignment = TextAlignmentOptions.Top;  // von oben wachsen lassen
        latinText  = MakeTMP ("LatinText",  center.transform, 600f, 30f,  y, 13f, false, false); y -= 35f;
        translText = MakeTMP ("TranslText", center.transform, 600f, 25f,  y, 11f, false, false); y -= 40f;
        bottomLine = MakeLine("BottomLine", center.transform,  80f, 1f,   y);

        Debug.Log("[SlaveDeathCard] Canvas automatisch erstellt. Du kannst alle Transforms jetzt im Inspector anpassen.");
    }

    // ---------------------------------------------------------------
    // Builder-Hilfsmethoden
    // ---------------------------------------------------------------

    private GameObject MakeRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private Image MakeLine(string name, Transform parent, float width, float height, float yPos)
    {
        GameObject go = MakeRect(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0f, yPos);

        Image img = go.AddComponent<Image>();
        img.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
        return img;
    }

    private TextMeshProUGUI MakeTMP(string name, Transform parent,
        float width, float height, float yPos,
        float fontSize, bool bold, bool italic)
    {
        GameObject go = MakeRect(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 1f);   // pivot oben, wächst nach unten
        rt.sizeDelta        = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0f, yPos);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.clear;

        tmp.fontStyle = FontStyles.Normal;
        if (bold)   tmp.fontStyle |= FontStyles.Bold;
        if (italic) tmp.fontStyle |= FontStyles.Italic;

        // Buchstabenabstand für Rolle/Latin (BF1-typisch)
        if (name == "RoleText" || name == "LatinText" || name == "TranslText")
            tmp.characterSpacing = 8f;

        go.SetActive(false);
        return tmp;
    }

    private void StretchFull(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
    }

    // ---------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------

    public void Show() => Show(defaultData);

    public void Show(DeathCardData data)
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(PlaySequence(data));
    }

    public void ForceClose()
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        StartCoroutine(FadeCanvas(1f, 0f, fadeOutDuration, () =>
        {
            canvas.enabled = false;
            HideAllImmediate();
        }));
    }

    // ---------------------------------------------------------------
    // Sequenz
    // ---------------------------------------------------------------

    private IEnumerator PlaySequence(DeathCardData data)
    {
        HideAllImmediate();
        canvas.enabled = true;

        yield return FadeCanvas(0f, 1f, fadeInDuration);

        yield return FadeGraphic(topLine, 0f, 1f, 0.4f);
        yield return new WaitForSeconds(blockPause * 0.5f);

        nameText.color = nameColor;
        nameText.gameObject.SetActive(true);
        yield return TypeWrite(nameText, data.name);
        yield return new WaitForSeconds(blockPause * 0.6f);

        datesText.color = datesColor;
        datesText.gameObject.SetActive(true);
        yield return TypeWrite(datesText, data.dates, charDelay * 1.5f);
        yield return new WaitForSeconds(blockPause);

        yield return FadeGraphic(dividerLine, 0f, 1f, 0.3f);
        yield return new WaitForSeconds(blockPause * 0.4f);

        roleText.color = roleColor;
        roleText.gameObject.SetActive(true);
        yield return TypeWrite(roleText, data.role, charDelay * 0.8f);
        yield return new WaitForSeconds(blockPause);

        bioText.gameObject.SetActive(true);
        yield return FadeLines(bioText, data.bio, bioColor);
        yield return new WaitForSeconds(blockPause * 1.5f);

        latinText.color = latinColor;
        latinText.gameObject.SetActive(true);
        yield return TypeWrite(latinText, data.latin, charDelay * 1.8f);
        yield return new WaitForSeconds(blockPause * 0.3f);

        translText.color = new Color(latinColor.r, latinColor.g, latinColor.b, 0f);
        translText.text  = data.translation;
        translText.gameObject.SetActive(true);
        yield return FadeGraphic(translText, 0f, 0.6f, 0.5f);

        yield return new WaitForSeconds(blockPause * 0.5f);
        yield return FadeGraphic(bottomLine, 0f, 1f, 0.4f);

        yield return new WaitForSeconds(holdDuration);

        yield return FadeCanvas(1f, 0f, fadeOutDuration);
        canvas.enabled = false;
        HideAllImmediate();
        _activeRoutine = null;
				//SceneManager.LoadScene("Lobby");
				LoadingScreenManager.Instance.SimpleLoadScene("Lobby");
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    [Header("Typewriter Fade")]
    [Tooltip("Wie lange jeder einzelne Buchstabe einblendet (Sekunden)")]
    [SerializeField] private float charFadeDuration = 0.12f;

    private IEnumerator TypeWrite(TextMeshProUGUI tmp, string fullText, float delay = -1f)
    {
        float d = delay < 0f ? charDelay : delay;

        // Gesamten Text setzen, aber alle Zeichen unsichtbar machen
        tmp.text = fullText;
        tmp.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmp.textInfo;
        int totalChars = textInfo.characterCount;

        // Alle Vertex-Alphas auf 0 setzen
        for (int i = 0; i < totalChars; i++)
            SetCharAlpha(tmp, i, 0);
        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // Buchstabe für Buchstabe einblenden
        for (int i = 0; i < totalChars; i++)
        {
            char c = fullText[i < fullText.Length ? i : fullText.Length - 1];

            // Leerzeichen und Zeilenumbrüche sofort, kein Fade
            if (c == ' ' || c == '\n')
            {
                SetCharAlpha(tmp, i, 255);
                tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                continue;
            }

            // Fade-in für diesen Buchstaben
            float elapsed = 0f;
            while (elapsed < charFadeDuration)
            {
                elapsed += Time.deltaTime;
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(elapsed / charFadeDuration) * 255f);
                SetCharAlpha(tmp, i, alpha);
                tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                yield return null;
            }
            SetCharAlpha(tmp, i, 255);
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // Pause bis zum nächsten Buchstaben
            yield return new WaitForSeconds(d);
        }
    }


    /// Blendet die Bio zeilenweise ein — jede Zeile fadet einzeln rein
    private IEnumerator FadeLines(TextMeshProUGUI tmp, string fullText, Color baseColor)
    {
        string[] lines = fullText.Split('\n');
        tmp.text  = "";
        tmp.color = baseColor;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Leerzeile: kurze Pause, keine Animation
            if (string.IsNullOrWhiteSpace(line))
            {
                tmp.text += "\n";
                yield return new WaitForSeconds(bioLinePause * 0.6f);
                continue;
            }

            // Zeile unsichtbar anhängen, dann per Vertex-Alpha einblenden
            int startChar = GetVisibleCharCount(tmp);
            tmp.text += (tmp.text.Length > 0 ? "\n" : "") + line;
            tmp.ForceMeshUpdate();

            int endChar = GetVisibleCharCount(tmp);

            // Alle neuen Chars auf Alpha 0
            for (int c = startChar; c < endChar; c++)
                SetCharAlpha(tmp, c, 0);
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // Fade-in für die gesamte Zeile gleichzeitig
            float elapsed = 0f;
            while (elapsed < bioLineFade)
            {
                elapsed += Time.deltaTime;
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(elapsed / bioLineFade) * 255f);
                for (int c = startChar; c < endChar; c++)
                    SetCharAlpha(tmp, c, alpha);
                tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                yield return null;
            }

            // Sicherstellen dass Zeile vollständig sichtbar
            for (int c = startChar; c < endChar; c++)
                SetCharAlpha(tmp, c, 255);
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // Pause vor nächster Zeile — letzte Zeile etwas länger
            float pause = (i == lines.Length - 1) ? bioLinePause * 1.5f : bioLinePause;
            yield return new WaitForSeconds(pause);
        }
    }

    /// Zählt sichtbare Zeichen im aktuellen TMP-Mesh (ignoriert Leerzeichen/Zeilenumbrüche)
    private int GetVisibleCharCount(TextMeshProUGUI tmp)
    {
        tmp.ForceMeshUpdate();
        return tmp.textInfo.characterCount;
    }

    /// Setzt das Alpha aller 4 Vertices eines einzelnen Zeichens
    private void SetCharAlpha(TextMeshProUGUI tmp, int charIndex, byte alpha)
    {
        TMP_TextInfo textInfo = tmp.textInfo;
        if (charIndex >= textInfo.characterCount) return;

        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        int meshIndex  = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        Color32[] colors = textInfo.meshInfo[meshIndex].colors32;
        colors[vertexIndex + 0].a = alpha;
        colors[vertexIndex + 1].a = alpha;
        colors[vertexIndex + 2].a = alpha;
        colors[vertexIndex + 3].a = alpha;
    }

    private IEnumerator FadeGraphic(Graphic graphic, float from, float to, float duration)
    {
        graphic.gameObject.SetActive(true);
        float elapsed = 0f;
        Color c = graphic.color;
        c.a = from; graphic.color = c;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            graphic.color = c;
            yield return null;
        }
        c.a = to; graphic.color = c;
    }

    private IEnumerator FadeCanvas(float from, float to, float duration, System.Action onDone = null)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
        onDone?.Invoke();
    }

    private void HideAllImmediate()
    {
        foreach (var t in new[] { nameText, datesText, roleText, bioText, latinText, translText })
            if (t != null) { t.text = ""; t.gameObject.SetActive(false); }

        foreach (var img in new[] { topLine, dividerLine, bottomLine })
            if (img != null) img.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
    }
}

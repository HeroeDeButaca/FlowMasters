using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Profiling;

public class PerformanceMonitor : MonoBehaviour
{
    public enum PerfMonitorLocation { LeftTopCorner, LeftBottomCorner, RightTopCorner, RightBottomCorner }

    [SerializeField] private PerfMonitorLocation _monitorLocation;
    private Vector2[] _anchorsPositions = new Vector2[4]
    {
        new Vector2(0,1), new Vector2(0,0), new Vector2(1,1), new Vector2(1,0)
    };

    private Vector2 _lastAnchoredPos = new Vector2(10, -10);

    [SerializeField] private bool _activateMonitor = true;

    private GameObject _canvasObject;

    [SerializeField]
    [Range(24f, 40f)]
    private float _monitorFontSize = 24f;

    [SerializeField] private bool _showFPS = true;
    [SerializeField] private bool _showConsumedRam = true;

    private TextMeshProUGUI _fpsText;
    private TextMeshProUGUI _ramText;

    [SerializeField] private float _updateStats = 0.5f;
    private float _timer;
    private int _frameCount;

    public static PerformanceMonitor Instance;

    void Awake()
    {
        if (!Debug.isDebugBuild || !_activateMonitor)
        {
            gameObject.SetActive(false);
            return;
        }

        if(Instance != this && Instance != null) { Destroy(gameObject); return; }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        CreatePerformanceUI();
    }

    void Update()
    {
        _timer += Time.unscaledDeltaTime;
        _frameCount++;

        if(_timer >= _updateStats)
        {
            if (_showFPS)
                UpdateFPS();

            if (_showConsumedRam)
                UpdateRAM();

            _frameCount = 0;
            _timer = 0f;
        }

        

    }

    private void CreatePerformanceUI()
    {
        _canvasObject = new GameObject("Performance Canvas");
        DontDestroyOnLoad(_canvasObject);

        Canvas canvas = _canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler canvasScaler = _canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        _canvasObject.AddComponent<GraphicRaycaster>();

        if (_showFPS)
            _fpsText = CreatePerformaceObject("FPS Text", "FPS: 0");

        if (_showConsumedRam)
            _ramText = CreatePerformaceObject("RAM Text", "RAM: 0 MB");
    }

    private TextMeshProUGUI CreatePerformaceObject(string gameObjectName, string initialText)
    {
        GameObject textObject = new GameObject(gameObjectName);
        textObject.transform.SetParent(_canvasObject.transform, false);

        TextMeshProUGUI perfText = textObject.AddComponent<TextMeshProUGUI>();
        perfText.text = initialText;
        perfText.fontSize = _monitorFontSize;
        perfText.color = Color.white;
        perfText.textWrappingMode = TextWrappingModes.NoWrap;

        int selectedPerfMonitor = (int)_monitorLocation;
        perfText.alignment = selectedPerfMonitor <= 1 ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.TopRight;

        RectTransform rect = perfText.rectTransform;
        rect.anchorMin = _anchorsPositions[selectedPerfMonitor];
        rect.anchorMax = _anchorsPositions[selectedPerfMonitor];
        rect.pivot = _anchorsPositions[selectedPerfMonitor];
        rect.anchoredPosition = _lastAnchoredPos;

        if(selectedPerfMonitor % 2 == 0)
            _lastAnchoredPos = rect.anchoredPosition - new Vector2(0, (_monitorFontSize - 4));
        else
            _lastAnchoredPos = rect.anchoredPosition + new Vector2(0, (_monitorFontSize - 4));

        return perfText;
    }

    private void UpdateFPS()
    {
        float fps = _frameCount / _timer;
        _fpsText.text = $"FPS: {(int)fps}";
    }

    private void UpdateRAM()
    {
        long bytes = Profiler.GetTotalAllocatedMemoryLong();
        float megabytes = bytes / (1024f * 1024f);

        _ramText.text = $"RAM: {megabytes:F0} MB";
    }
}

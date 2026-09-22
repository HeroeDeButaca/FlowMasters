using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerformanceMonitor : MonoBehaviour
{
    [SerializeField] private bool _activateMonitor = true;

    [SerializeField] private bool _showFPS = true;
    [SerializeField] private bool _showConsumedRam = true;
    [SerializeField] private bool _showConsumedCPU = true;

    private TextMeshProUGUI _fpsText;
    [SerializeField] private float _updateFps = 0.5f;
    private float _timer;
    private int _frameCount;

    void Awake()
    {
        if (!Debug.isDebugBuild || !_activateMonitor)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    void Start()
    {
        CreatePerformanceUI();
    }

    void Update()
    {
        if (_showFPS)
            FPSCount();

    }

    private void CreatePerformanceUI()
    {
        GameObject canvasObject = new GameObject("Performance Canvas");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("FPS Text");
        textObject.transform.SetParent(canvasObject.transform, false);

        if (_showFPS)
        {
            _fpsText = textObject.AddComponent<TextMeshProUGUI>();
            _fpsText.text = "FPS: 0";
            _fpsText.fontSize = 24;
            _fpsText.color = Color.white;

            RectTransform rect = _fpsText.rectTransform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
        }
    }

    private void FPSCount()
    {
        _frameCount++;
        _timer += Time.unscaledDeltaTime;

        if(_timer >= _updateFps)
        {
            float fps = _frameCount / _timer;
            _fpsText.text = $"FPS: {fps}";

            _frameCount = 0;
            _timer = 0f;
        }
    }
}

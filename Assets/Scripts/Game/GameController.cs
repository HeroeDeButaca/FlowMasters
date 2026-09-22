using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameController : MonoBehaviour
{
    private enum GameState { LoadingMap, InGame, Finished }
    private GameState _gameState = GameState.LoadingMap;

    private float _currentTime;
    private float _maxTime;
    [SerializeField]
    private TMP_Text _timeText;

    private int _maxNodes;

    private int _completedBoards;
    [SerializeField]
    private TMP_Text _completedBoardsText;

    public UnityEvent OnReset;

    [SerializeField]
    private AudioClip[] _bgmMusics;

    public static GameController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        NodesGenerator.Instance.OnBoardReady.AddListener(delegate
        {
            _gameState = GameState.InGame;
        });

        TouchDetector.Instance.OnTableFilled.AddListener(TableFilled);

        SetGame();

        SetMusic();
    }

    void Update()
    {
        if (_gameState == GameState.InGame)
        {
            _currentTime -= Time.deltaTime;
            _timeText.text = _currentTime.ToString("0");

            if (_currentTime <= 0f)
            {
                TouchDetector.Instance.CanTouch = false;
                _gameState = GameState.Finished;
                _timeText.text = "0";
                TimesOutManager.Instance.ShowPanel(_completedBoards);
            }

        }
    }

    private void SetGame()
    {
        CompetitiveMode mode = PlayerData.Instance.SelectedMode;
        int width = mode.TableWidth;
        int height = mode.TableHeight;
        _maxNodes = mode.NodesPerBoard;

        _maxTime = mode.TotalTime;
        _currentTime = _maxTime;
        _timeText.text = _currentTime.ToString("0");

        BoardGenerator.Instance.GenerateBoard(width, height, _maxNodes);
    }

    private void TableFilled()
    {
        _gameState = GameState.LoadingMap;
        OnReset?.Invoke();
        _completedBoards++;
        _completedBoardsText.text = _completedBoards.ToString("0");

        NodesGenerator.Instance.CreateCorrectBoard(_maxNodes);
    }

    private void SetMusic()
    {
        int randMusic = Random.Range(0, _bgmMusics.Length);
        AudioManager.Instance.PlayBGM(_bgmMusics[randMusic], true);
    }
}

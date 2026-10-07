using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CheckpointType
{
    Periodic, BoardCompleted, GameFinished
}

public class Checkpoint
{
    public float Time;
    public int BoardsCompleted;
    public CheckpointType Type;

    private Checkpoint() { }

    public Checkpoint(float time, int boardsCompleted, CheckpointType type)
    {
        Time = time;
        BoardsCompleted = boardsCompleted;
        Type = type;
    }
}

public class AntiCheat : MonoBehaviour
{
    private List<Checkpoint> _checkpoints;

    private bool _timeRuns = false;
    private float _timeElapsed = 0f;

    public static AntiCheat Instance;

    void Awake()
    {
        if(Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _checkpoints = new List<Checkpoint>();

        GameController.Instance.OnReset.AddListener(delegate
        {
            CreateCheckpoint(CheckpointType.BoardCompleted);
            _timeRuns = false;
        });

        NodesGenerator.Instance.OnBoardReady.AddListener(() => _timeRuns = true);

        StartAutoCheckpointCoroutine();
    }

    void Update()
    {
        if (_timeRuns)
        {
            _timeElapsed += Time.deltaTime;
        }
    }

    public void StartAutoCheckpointCoroutine()
    {
        StartCoroutine(AutoCheckpointCo());
    }

    public void StopAutoCheckpointCoroutine()
    {
        StopAllCoroutines();
    }

    private IEnumerator AutoCheckpointCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(8f);
            CreateCheckpoint(CheckpointType.Periodic);
        }
    }

    public void CreateCheckpoint(CheckpointType type)
    {
        int boardsCompleted = GameController.Instance.GetBoardsCompleted();
        Checkpoint checkpoint = new Checkpoint(_timeElapsed, boardsCompleted, type);
        _checkpoints.Add(checkpoint);

        Debug.Log($"Checkpoint tipo {type} creado. Value: {boardsCompleted}");
    }

    public bool CheckIsLegitRun()
    {
        bool isLegit = true;
        int lastScore = 0;

        for(int i = 0; i < _checkpoints.Count; i++)
        {
            int score = _checkpoints[i].BoardsCompleted;

            if(i > 0)
            {
                if((score-lastScore) > 1)
                {
                    isLegit = false;
                    break;
                }
            }

            lastScore = score;
        }

        Debug.Log($"The run is legit: {isLegit}");
        return isLegit;
    }
}

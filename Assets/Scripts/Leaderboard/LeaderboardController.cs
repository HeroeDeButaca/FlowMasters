using Newtonsoft.Json;
using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardData
{
    public int ModeId;

    public string PlayerName;
    public int IconId;
    public int TotalPoints;

    public LeaderboardData() { }

    public LeaderboardData(int modeId, string playerName, int iconId, int points)
    {
        ModeId = modeId;
        PlayerName = playerName;
        IconId = iconId;
        TotalPoints = points;
    }
}

public class LeaderboardController : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefabLeaderboard;

    [SerializeField]
    private Transform _contentLeaderboard;
    [SerializeField]
    private GameObject _loadingScreen;
    [SerializeField]
    private GameObject _textBeFirstGO;

    private LeaderboardBox[] _scoreBoxes;

    private LeaderboardData[,] _leaderboardData;
    private const int TOP_SHOW = 5;
    private const int TOTAL_GAMEMODES = 3;

    public static LeaderboardController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _scoreBoxes = new LeaderboardBox[_contentLeaderboard.childCount];
        for (int i = 0; i < _contentLeaderboard.childCount; i++)
            _scoreBoxes[i] = _contentLeaderboard.GetChild(i).GetComponent<LeaderboardBox>();

        _leaderboardData = new LeaderboardData[TOTAL_GAMEMODES, TOP_SHOW];
    }

    public void LoadAllScores()
    {
        for (int i = 0; i < TOTAL_GAMEMODES; i++)
            RetrieveFromDatabase(i);
    }

    private void RetrieveFromDatabase(int modeId)
    {
        string url = $"https://flowfreelikegameleaderboard-default-rtdb.europe-west1.firebasedatabase.app/{modeId}.json";

        RestClient.Get(url).Then(response =>
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, LeaderboardData>>(response.Text);

            if (dict == null || dict.Count == 0)
            {
                Debug.LogWarning("Leaderboard vacío");
                return;
            }

            var top5 = new List<LeaderboardData>(dict.Values);

            top5.Sort((a, b) => b.TotalPoints.CompareTo(a.TotalPoints));

            if (top5.Count > TOP_SHOW)
                top5 = top5.GetRange(0, TOP_SHOW);

            Data userData = PlayerData.Instance.UserData;
            for (int i = 0; i < top5.Count; i++)
            {
                var data = top5[i];
                //Debug.Log($"{i + 1}. {data.PlayerName} - {data.TotalPoints}");
                _leaderboardData[modeId, i] = data;

                if (i == 0)
                {
                    int lastScore = PlayerPrefs.GetInt("LastScore", 0);
                    if (data.PlayerName.Equals(userData.PlayerName) && lastScore == data.TotalPoints)
                        IconUnlocker.Instance.UnlockIcon(7);
                }
                else
                {
                    if (data.PlayerName.Equals(userData.PlayerName))
                        IconUnlocker.Instance.UnlockIcon(9);
                }
            }
        })
        .Catch(err =>
        {
            Debug.LogError(err);
        });
    }

    public void LoadLeaderboard(int modeId)
    {
        _textBeFirstGO.SetActive(false);
        _loadingScreen.SetActive(true);

        int totalDeactivatedBoxes = 0;
        for (int i = 0; i < _contentLeaderboard.childCount; i++)
        {
            bool deactivate = _leaderboardData[modeId, i] != null;

            _contentLeaderboard.GetChild(i).gameObject.SetActive(deactivate);
            if(!deactivate)
                totalDeactivatedBoxes++;

        }

        if (totalDeactivatedBoxes >= _contentLeaderboard.childCount)
        {
            _textBeFirstGO.SetActive(true);
            _loadingScreen.SetActive(false);
            return;
        }

        for (int i = 0; i < _scoreBoxes.Length; i++)
        {
            LeaderboardData data = _leaderboardData[modeId, i];
            if (data == null)
                break;

            Sprite iconSprite = IconManager.Instance?.GetIconSprite(data.IconId);
            _scoreBoxes[i].SetLeaderboardBox(i+1, iconSprite, data.PlayerName, data.TotalPoints);
        }

        _loadingScreen.SetActive(false);
    }

    public void HideScoreBoxes()
    {
        for (int i = 0; i < _contentLeaderboard.childCount; i++)
            _contentLeaderboard.GetChild(i).gameObject.SetActive(false);
    }
}

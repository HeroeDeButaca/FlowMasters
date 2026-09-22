using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.UI;
using TMPro;
using Proyecto26;
using System.Collections;

public class TimesOutManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _timesOutPanel;

    [SerializeField]
    private LocalizedString _completedBoardsTraduction; 
    [SerializeField]
    private TMP_Text _completedBoardsText;

    [SerializeField]
    private Button _returnMenuButton;

    private const string _databaseLink = "https://flowfreelikegameleaderboard-default-rtdb.europe-west1.firebasedatabase.app/";

    public static TimesOutManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _returnMenuButton.onClick.AddListener(ReturnToMenu);

        if (PlayerPrefs.GetInt("IsOffline") == 0)
            _returnMenuButton.interactable = false;

        _timesOutPanel.SetVisible(false);
    }

    public void ShowPanel(int completedBoards)
    {
        _timesOutPanel.SetVisible(true);

        string completedText = _completedBoardsTraduction.GetLocalizedString();
        completedText = completedText.Replace("x", completedBoards.ToString("0"));
        _completedBoardsText.text = completedText;

        if(PlayerPrefs.GetInt("IsOffline") == 0)
            PostPoints(completedBoards);

    }

    private void PostPoints(int score)
    {
        Data userData = PlayerData.Instance.UserData;
        int modeId = PlayerData.Instance.SelectedMode.ModeId;
        string playerName = userData.PlayerName;
        int iconId = userData.IconId;

        string key = $"{playerName}_Mode{modeId}";
        string url = $"{_databaseLink}{modeId}/{key}.json";

        RestClient.Get(url).Then(response =>
        {
            LeaderboardData data;

            if (response != null && !string.IsNullOrEmpty(response.Text) && response.Text != "null")
            {
                data = JsonUtility.FromJson<LeaderboardData>(response.Text);
            }
            else
            {
                data = new LeaderboardData(modeId, playerName, iconId, score);
            }

            if (data.TotalPoints < score)
                data.TotalPoints = score;

            data.PlayerName = playerName;
            data.ModeId = modeId;
            data.IconId = iconId;

            RestClient.Put(url, data)
                .Then(_ => Debug.Log("Score updated"))
                .Catch(err => Debug.LogError(err));

            _returnMenuButton.interactable = true;

        })
        .Catch(err =>
        {
            Debug.LogError($"GET error: {err}");
            _returnMenuButton.interactable = true;
        });
    }

    private void ReturnToMenu()
    {
        _returnMenuButton.interactable = false;
        AudioManager.Instance.StopBGM();
        StartCoroutine(ReturnToMenuCo());
    }

    private IEnumerator ReturnToMenuCo()
    {
        PlayerData.Instance.SavePlayerData();
        yield return new WaitForSeconds(1f);
        Destroy(PlayerData.Instance.gameObject);
        SceneManager.LoadScene(0);
    }

}

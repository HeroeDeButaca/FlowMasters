using Proyecto26;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class GameUpdateChecker : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _menuPanel;
    [SerializeField]
    private CanvasGroup _versionAvailablePanel;
    [SerializeField]
    private CanvasGroup _noInternetPanel;

    [SerializeField]
    private Sprite _connectionSpr;
    [SerializeField]
    private Sprite _noConnectionSpr;
    [SerializeField]
    private Image _connectionImage;

    public static GameUpdateChecker Instance;

    void Start()
    {
        GameIsUpdated((failedConnection, isUpdated) =>
        {
            PlayerPrefs.SetInt("IsOffline", (failedConnection || !isUpdated) ? 1 : 0);

            if (failedConnection)
            {
                _noInternetPanel.SetVisible(true);
                _connectionImage.sprite = _noConnectionSpr;
                return;
            }

            if (!isUpdated)
            {
                _connectionImage.sprite = _noConnectionSpr;
                _versionAvailablePanel.SetVisible(true);
            }
            else
            {
                LeaderboardController.Instance.LoadAllScores();
                _menuPanel.SetVisible(true);
            }
        });
    }

    private void GameIsUpdated(System.Action<bool, bool> callback)
    {
        CultureInfo en_US = CultureInfo.GetCultureInfo("en-US");
        float applicationVersion = float.Parse(Application.version, en_US);

        string url = "https://flowfreelikegameleaderboard-default-rtdb.europe-west1.firebasedatabase.app/GameVersion.json";

        RestClient.Get(url).Then(response =>
        {
            float serverVersion = 0f;

            if (response != null &&
                !string.IsNullOrEmpty(response.Text) &&
                response.Text != "null")
            {
                serverVersion = float.Parse(response.Text, en_US);
            }
            //Debug.Log($"App version: {applicationVersion}, Server version: {serverVersion}");

            bool isUpdated = serverVersion == applicationVersion;
            //Debug.Log($"IsUpdated: {isUpdated}");

            callback?.Invoke(false, isUpdated);
        })
        .Catch(err =>
        {
            Debug.LogError($"Error fetching version: {err}");
            callback?.Invoke(true, true);
        });
    }

    public void Exit() { Application.Quit();}
    public void OpenGamePage() { Application.OpenURL("https://javiersc.itch.io/flow-masters"); }
}

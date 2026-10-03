using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Components;

[System.Serializable]
public class CompetitiveMode
{
    public int ModeId;

    public LocalizedString LocalizedModeName;

    public int TotalTime;
    public int TableHeight;
    public int TableWidth;
    public int NodesPerBoard;
}

public class ModeManager : MonoBehaviour
{
    [Header("Modes")]
    [SerializeField]
    private CompetitiveMode[] _competitiveModes;

    [SerializeField]
    private Transform _scrollContentModes;

    [SerializeField]
    private GameObject _prefabButtonMode;

    [Space(10)]
    [Header("Mode Panel")]
    [SerializeField]
    private CanvasGroup _modePanelGroup;
    [SerializeField]
    private TMP_Text _modeTitleText;
    [SerializeField]
    private TMP_Text _modeTimeText;
    [SerializeField]
    private TMP_Text _modeHeightText;
    [SerializeField]
    private TMP_Text _modeWidthText;
    [SerializeField]
    private TMP_Text _modeNodesText;

    [SerializeField]
    private Button _playButton;

    [Header("Other")]
    [SerializeField]
    private GameObject _instructionsGO;

    void Start()
    {
        SetButtonModes();
        _instructionsGO.SetActive(true);
    }

    private void SetButtonModes()
    {
        for(int i = 0; i < _competitiveModes.Length; i++)
        {
            CompetitiveMode mode = _competitiveModes[i];

            GameObject modeButtonGO = Instantiate(_prefabButtonMode, _scrollContentModes);

            Button modeButton = modeButtonGO.GetComponent<Button>();
            modeButton.onClick.AddListener(delegate
            {
                SelectMode(mode);

                LeaderboardController.Instance.LoadLeaderboard(mode.ModeId);
                _playButton.onClick.AddListener(delegate
                {
                    StartGame(mode);
                });
            });

            LocalizedReference localizedReference = mode.LocalizedModeName;
            modeButtonGO.GetComponentInChildren<LocalizeStringEvent>().StringReference.SetReference(localizedReference.TableReference, localizedReference.TableEntryReference);
        }
    }

    private void SelectMode(CompetitiveMode mode)
    {
        ModePanelGroupControl(true);

        _modeTitleText.text = mode.LocalizedModeName.GetLocalizedString();
        _modeTimeText.text = mode.TotalTime.ToString("0");
        _modeHeightText.text = mode.TableHeight.ToString("0");
        _modeWidthText.text = mode.TableWidth.ToString("0");
        _modeNodesText.text = mode.NodesPerBoard.ToString("0");

        _instructionsGO.SetActive(false);
    }

    public void ModePanelGroupControl(bool show)
    {
        _modePanelGroup.alpha = show ? 1 : 0;
        _modePanelGroup.interactable = show;
        _modePanelGroup.blocksRaycasts = show;
    }

    public void StartGame(CompetitiveMode mode)
    {
        PlayerData.Instance.UserData.GamesPlayed++;

        if (PlayerData.Instance.UserData.GamesPlayed >= 10)
            IconUnlocker.Instance.UnlockIcon(1);
        else if (PlayerData.Instance.UserData.GamesPlayed >= 25)
            IconUnlocker.Instance.UnlockIcon(8);

        PlayerData.Instance.SelectedMode = mode;
        SceneManager.LoadScene(1);
    }

    public void ResetPlayButton() { _playButton.onClick.RemoveAllListeners(); }
}

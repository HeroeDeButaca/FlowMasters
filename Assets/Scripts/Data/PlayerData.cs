using System.IO;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class PlayerData : MonoBehaviour
{
    private string _saveDataFolder = "";
    private readonly string DATA_FILE_NAME = "PlayerData.save";

    private Data _data;
    public Data UserData => _data;
    public CompetitiveMode SelectedMode;

    [SerializeField]
    private CanvasGroup _userCreationGroup;
    [SerializeField]
    private CanvasGroup _mainMenuGroup;

    public static PlayerData Instance;

    void Awake()
    {
        if(Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        _saveDataFolder = Application.persistentDataPath + "/Saves/";
    }

    void Start()
    {
        string path = _saveDataFolder + DATA_FILE_NAME;

        if (File.Exists(path))
        {
            // Cargar datos
            _data = JsonCreator.LoadEncryptedData<Data>(path);
            IconManager.Instance.CheckListBoolData();
            IconManager.Instance.InitializeIcons();

            Sprite iconSprite = IconManager.Instance.GetIconSprite(_data.IconId);

            PlayerInfoManager.Instance.ChangeIcon(iconSprite);
            PlayerInfoManager.Instance.ChangeName(_data.PlayerName);

            _mainMenuGroup.SetVisible(true);
        }
        else
        {
            _mainMenuGroup.SetVisible(false);
            _userCreationGroup.SetVisible(true);
        }
    }

    void OnDestroy()
    {
        Instance = null;
    }

    public void ReadUsernameInput(TMP_InputField usernameInput)
    {
        _data = new Data(usernameInput.text);

        usernameInput.interactable = false;
        usernameInput.text = "Loading...";

        StartCoroutine(CreateDataCo());
    }

    private IEnumerator CreateDataCo()
    {
        Sprite iconSprite = IconManager.Instance.GetIconSprite(_data.IconId);

        PlayerInfoManager.Instance.ChangeIcon(iconSprite);
        PlayerInfoManager.Instance.ChangeName(_data.PlayerName);

        IconManager.Instance.CheckListBoolData();

        yield return StartCoroutine(SavePlayerDataCo());

        _userCreationGroup.SetVisible(false);
        _mainMenuGroup.SetVisible(true);
        IconManager.Instance.InitializeIcons();
    }

    public void SavePlayerData()
    {
        StartCoroutine(SavePlayerDataCo());
    }

    private IEnumerator SavePlayerDataCo()
    {
        bool saveCompleted = false;

        Task.Run(() =>
        {
            JsonCreator.EncryptSaveData(_data, _saveDataFolder, DATA_FILE_NAME);
            saveCompleted = true;
        });

        while (!saveCompleted)
        {
            yield return null;
        }
    }

    public void ShowFileFolder() { Application.OpenURL(_saveDataFolder); }

    public void DeleteData()
    {
        string path = _saveDataFolder + DATA_FILE_NAME;

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        Destroy(gameObject);
        SceneManager.LoadScene(0);
        
    }
}

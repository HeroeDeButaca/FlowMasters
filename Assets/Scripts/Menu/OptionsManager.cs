using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Linq;

public class ConfigData
{
    public bool IsFullscreen;
    public float MusicVolume;
    public float SfxVolume;
    public int LanguageSelected;

    public ConfigData()
    {
        IsFullscreen = true;
        MusicVolume = 0.5f;
        SfxVolume = 0.5f;
        LanguageSelected = 0;
    }
}

public class OptionsManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _optionsPanel;

    [SerializeField]
    private Toggle _fullscreenToggle;
    [SerializeField]
    private Slider _musicSlider;
    [SerializeField]
    private Slider _sfxSlider;
    [SerializeField]
    private TMP_Dropdown _languageDropdown;

    [SerializeField]
    private GameObject _fullscreenLabel;
    [SerializeField]
    private GameObject _showFileBtn;

    private ConfigData _configData;
    private string _saveDataFolder = "";
    private readonly string CONFIG_FILE_NAME = "config.json";

    private bool _initialized = false;

    private AudioManager _audioManager;

    void Start()
    {
        _audioManager = AudioManager.Instance;

        _saveDataFolder = Application.persistentDataPath + "/";
        string path = _saveDataFolder + CONFIG_FILE_NAME;

        if(File.Exists(path))
        {
            try
            {
                _configData = JsonCreator.LoadData<ConfigData>(path);
                Debug.Log($"MusicVolume: {_configData.MusicVolume}");
            }
            catch(System.Exception e)
            {
                Debug.LogError(e);
                _configData = new ConfigData();
                JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
            }
        }
        else
        {
            _configData = new ConfigData();
            JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
        }

        _optionsPanel.SetVisible(false);
        InitializeConfig();

#if PLATFORM_ANDROID
        HideAndroidSettings();
#endif
    }

    private void InitializeConfig()
    {
        _fullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);
        _musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        _sfxSlider.onValueChanged.AddListener(ChangeSfxVolume);
        _languageDropdown.onValueChanged.AddListener(ChangeLanguage);

        ConfigureLanguageDropdown();

        bool isFullscreen = _configData.IsFullscreen;
        float musicVolume = Mathf.Clamp01(_configData.MusicVolume);
        float sfxVolume = Mathf.Clamp01(_configData.SfxVolume);
        int languageSelected = Mathf.Clamp(_configData.LanguageSelected, 0, (_languageDropdown.options.Count-1));

        _fullscreenToggle.isOn = isFullscreen;
        _musicSlider.value = musicVolume;
        _sfxSlider.value = sfxVolume;
        _languageDropdown.value = languageSelected;

        ChangeFullscreen(isFullscreen);
        ChangeMusicVolume(musicVolume);
        ChangeSfxVolume(sfxVolume);
        ChangeLanguage(languageSelected);

        _initialized = true;
    }

    #region Change option values
    private void ChangeFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        _configData.IsFullscreen = fullscreen;

        if(_initialized)
            JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
    }

    private void ChangeMusicVolume(float volume)
    {
        _audioManager.ChangeBgmVolume(volume);
        _configData.MusicVolume = volume;

        if (volume >= 1f)
            IconUnlocker.Instance.UnlockIcon(2);
        else if (volume <= 0f)
            IconUnlocker.Instance.UnlockIcon(3);

        if (_initialized)
            JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
    }

    private void ChangeSfxVolume(float volume)
    {
        _audioManager.ChangeSfxVolume(volume);
        _configData.SfxVolume = volume;

        if (_initialized)
            JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
    }

    private void ChangeLanguage(int value)
    {
        _configData.LanguageSelected = value;
        IconUnlocker.Instance.UnlockIcon(value+4);
        StartCoroutine(ChangeLanguageCo(value));
    }

    private IEnumerator ChangeLanguageCo(int localeId)
    {
        _languageDropdown.interactable = false;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeId];

        _languageDropdown.interactable = true;

        if (_initialized)
            JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
    }
    #endregion

    private void ConfigureLanguageDropdown()
    {
        // Averiguamos los nombres de los locales que tiene el proyecto
        Locale[] locales = LocalizationSettings.AvailableLocales.Locales.ToArray();
        string[] languagesNames = new string[locales.Length];

        for (int i = 0; i < locales.Length; i++)
        {
            languagesNames[i] = locales[i].LocaleName;
        }

        // Seteamos el nombre de las opciones en el dropdown
        _languageDropdown.AddOptions(languagesNames.ToList());
    }

    private void HideAndroidSettings()
    {
        _fullscreenToggle.gameObject.SetActive(false);
        _fullscreenLabel.SetActive(false);
        _showFileBtn.SetActive(false);
    }
}

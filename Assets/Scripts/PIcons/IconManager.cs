using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;
using UnityEngine.Localization.Components;

public class IconManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("All the icon data should be here")]
    private IconData[] _iconsData;

    [SerializeField]
    private GameObject _prefabIcon;

    [SerializeField]
    private Transform _contentIcons;

    [SerializeField]
    private TMP_Text _totalIconsUnlockedText;

    [SerializeField]
    private LocalizedString _secretDescription;

    private int _totalIconsUnlocked = 0;

    public static IconManager Instance;

    void Awake()
    {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void InitializeIcons()
    {
        IconUnlocked[] playerIconsUnlocked = PlayerData.Instance.UserData.IconsUnlocked.ToArray();

        for(int i = 0; i < _iconsData.Length; i++)
        {
            IconData iconData = _iconsData[i];
            bool iconUnlocked = playerIconsUnlocked[i].Unlocked;

            if (!iconData.IsVisible && !iconUnlocked)
                continue;

            GameObject iconGO = Instantiate(_prefabIcon, _contentIcons);

            iconGO.transform.GetChild(0).GetComponent<Image>().sprite = iconData.IconSprite;
            iconGO.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate
            {
                PlayerInfoManager.Instance.ChangeIcon(iconData.IconSprite);
                PlayerData.Instance.UserData.IconId = iconData.IconId;
                PlayerData.Instance.SavePlayerData();
            });

            iconGO.transform.GetChild(1).gameObject.SetActive(iconData.IsLocked && !iconUnlocked);
            iconGO.transform.GetChild(2).gameObject.SetActive(iconData.IsSecret && !iconUnlocked);

            LocalizedReference obtentionReference = null;
            if (!iconUnlocked && iconData.IsSecret)
                obtentionReference = _secretDescription;
            else
                obtentionReference = iconData.RequerimentLocale;

            LocalizeStringEvent obtentionDescText = iconGO.transform.GetChild(3).GetComponentInChildren<LocalizeStringEvent>();
            obtentionDescText.StringReference.SetReference(obtentionReference.TableReference, obtentionReference.TableEntryReference);

            if (iconUnlocked)
                _totalIconsUnlocked++;
        }

        // El PIcon99 no debe contarse, por eso el "-1"
        _totalIconsUnlockedText.text = $"{_totalIconsUnlocked} / {_iconsData.Length-1}";
    }

    public void ResetIconsPanel()
    {
        _totalIconsUnlocked = 0;

        for(int i = 0; i < _contentIcons.childCount; i++)
        {
            GameObject go = _contentIcons.GetChild(i).gameObject;
            Destroy(go);
        }
    }

    public void CheckListBoolData()
    {
        int dataListLength = PlayerData.Instance.UserData.IconsUnlocked.Count;

        if (dataListLength < _iconsData.Length)
        {
            for(int i = (dataListLength); i < _iconsData.Length; i++)
            {
                IconUnlocked icon = new IconUnlocked(_iconsData[i].IconId, !_iconsData[i].IsLocked && !_iconsData[i].IsSecret);
                PlayerData.Instance.UserData.IconsUnlocked.Add(icon);
            }

            PlayerData.Instance.SavePlayerData();
        }
    }

    public Sprite GetIconSprite(int id)
    {
        Sprite iconSprite = null;

        if(_iconsData[id].IconId == id)
        {
            iconSprite = _iconsData[id].IconSprite;
        }
        else
        {
            for (int i = 0; i < _iconsData.Length; i++)
            {
                if (_iconsData[i].IconId == id)
                {
                    iconSprite = _iconsData[i].IconSprite;
                }
            }
        }

        return iconSprite;
    }
}

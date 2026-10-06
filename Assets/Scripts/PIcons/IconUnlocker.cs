using UnityEngine;
using UnityEngine.UI;

public class IconUnlocker : MonoBehaviour
{
    [SerializeField]
    private Animator _iconUnlockerAnim;
    [SerializeField]
    private AudioClip _iconUnlockedSfx;

    [SerializeField]
    private Image _iconUnlockedImage;

    public static IconUnlocker Instance;

    void Awake()
    {
        if(Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void UnlockIcon(int iconId)
    {
        if (PlayerData.Instance.UserData == null)
            return;

        bool isPreviouslyUnlocked = PlayerData.Instance.UserData.IconsUnlocked[iconId].Unlocked;

        if (!isPreviouslyUnlocked)
        {
            PlayerData.Instance.UserData.IconsUnlocked[iconId].Unlocked = true;
            _iconUnlockedImage.sprite = IconManager.Instance.GetIconSprite(iconId);
            _iconUnlockerAnim.SetTrigger("show");
            AudioManager.Instance.PlaySFX(_iconUnlockedSfx);
            PlayerData.Instance.SavePlayerData();

            if(IconManager.Instance != null)
            {
                IconManager.Instance.ResetIconsPanel();
                IconManager.Instance.InitializeIcons();
            }
            
        }
    }
}

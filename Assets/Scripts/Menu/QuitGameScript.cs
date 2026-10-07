using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitGameScript : MonoBehaviour
{
    private CanvasGroup _quitPanel;

    void Start()
    {
        _quitPanel = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _quitPanel.SetVisible(!_quitPanel.interactable);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

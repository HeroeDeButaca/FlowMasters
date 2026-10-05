using UnityEngine;
using UnityEngine.EventSystems;

public class ObtentionBubbleDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator _iconAnimator;

    void Start()
    {
        _iconAnimator = GetComponentInChildren<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _iconAnimator.SetBool("appear", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _iconAnimator.SetBool("appear", false);
    }
}

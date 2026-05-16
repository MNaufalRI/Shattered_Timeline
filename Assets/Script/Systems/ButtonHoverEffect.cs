using UnityEngine;
using UnityEngine.EventSystems; 

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Hover Visuals")]
    public GameObject leftOrnament;
    public GameObject rightOrnament;
    private void OnEnable()
    {
        SetHoverState(false);
    }

    private void OnDisable()
    {
        SetHoverState(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHoverState(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHoverState(false);

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetHoverState(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetHoverState(false);
    }

    private void SetHoverState(bool state)
    {
        if (leftOrnament != null) leftOrnament.SetActive(state);
        if (rightOrnament != null) rightOrnament.SetActive(state);
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour,IPointerDownHandler,IPointerClickHandler, IPointerUpHandler
{
    public TabGroup tabGroup;
    public Image backGround;
    public TextMeshProUGUI textComponent;

    public UnityEvent OnTabSelected;
    public UnityEvent OnTabDeSelected;
    
    void Start()
    {
        tabGroup.Subscribe(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void Select()
    {
        OnTabSelected?.Invoke();
    }

    public void DeSelect()
    {
        OnTabDeSelected?.Invoke();
    }
}

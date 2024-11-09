using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HomeScreen : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;
        UIManager.Instance.firstCategory.OnPointerClick(new PointerEventData(EventSystem.current));
    }
}

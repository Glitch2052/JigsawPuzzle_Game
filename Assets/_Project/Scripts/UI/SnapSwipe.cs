using UnityEngine;
using UnityEngine.UI;

public class SnapSwipe : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    public RectTransform content;
    public Color[] colors;
    
    private float scrollPos;
    float[] pos;
    private float time;
    private Button takeTheBtn;
    private float distance;
    private Image[] childImages;

    private float scrollTo;
    private float scrollToSoft;

    private bool isTapping = false;
    
    public int BtnNumber { get; private set; }
    
    void Start()
    {
        pos = new float[transform.childCount];
        childImages = new Image[pos.Length];
        distance = 1f / (pos.Length - 1f);
        
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
            childImages[i] = content.transform.GetChild(i).GetComponent<Image>();
        }

        // if (TryGetComponent(out HorizontalLayoutGroup horizontalLayoutGroup))
        // {
        //     horizontalLayoutGroup.padding.left = horizontalLayoutGroup.padding.right = (int)((RectTransform)scrollRect.transform).rect.width / 2 - 50;
        // }
        // scrollRect.onValueChanged.AddListener((a) =>
        // {
        //     Debug.Log(a);
        // });
    }

    void AddSnapEffect()
    {
        
    }

    void Update()
    {
        // Debug.Log($"Scroll Rect Velocity is {scrollRect.velocity.x}");
        if (Input.GetMouseButton(0) || Input.touchCount == 1)
        {
            if(!isTapping)
                scrollPos = scrollbar.value;
        }
        else
        {
            isTapping = false;
            for (int i = 0; i < pos.Length; i++)
            {
                if (scrollPos < pos[i] + (distance / 2) && scrollPos > pos[i] - (distance / 2))
                {
                    scrollTo = pos[i];
                    scrollToSoft = Mathf.Lerp(scrollToSoft, scrollTo, Time.deltaTime * 10f);
                    scrollbar.value = Mathf.Lerp(scrollbar.value, scrollToSoft, Time.deltaTime * 10f);
                }
            }
        }


        for (int i = 0; i < pos.Length; i++)
        {
            if (scrollPos < pos[i] + (distance / 2) && scrollPos > pos[i] - (distance / 2))
            {
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);
                childImages[i].color = colors[1];
                BtnNumber = i;
                UIManager.Instance.UpdateSelectedSizeIndex(BtnNumber);
                for (int j = 0; j < pos.Length; j++)
                {
                    if (j != i)
                    {
                        childImages[j].color = colors[0];
                        transform.GetChild(j).localScale = Vector2.Lerp(transform.GetChild(j).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
        
        // Debug.Log($"button number is {BtnNumber}");
    }

    public void SelectSizeOnTap(int index)
    {
        isTapping = true;
        scrollPos = (float)index / (pos.Length - 1);
    }
}
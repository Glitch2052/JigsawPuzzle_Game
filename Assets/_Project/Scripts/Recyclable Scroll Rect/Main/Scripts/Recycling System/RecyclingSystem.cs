using System.Collections;
using UnityEngine;
namespace PolyAndCode.UI
{
    /// <summary>
    /// Absract Class for creating a Recycling system.
    /// </summary>
    public abstract class RecyclingSystem
    {
        public IRecyclableScrollRectDataSource DataSource;

        protected RectTransform Viewport, Content;
        public RectTransform PrototypeCell;
        protected bool IsGrid;

        protected float MinPoolCoverage = 1.5f; // The recyclable pool must cover (viewPort * _poolCoverage) area.
        protected int MinPoolSize = 3; // Cell pool must have a min size
        protected float RecyclingThreshold = .2f; //Threshold for recycling above and below viewport
        
        //Assigned by constructor
        public int _coloumns;

        public abstract IEnumerator InitCoroutine(System.Action onInitialized = null);

        public abstract Vector2 OnValueChangedListener(Vector2 direction);

        public abstract void ClearData();

        #region Helper Methods

        public static void SetCenterAnchor(RectTransform rectTransform)
        {
            //Saving to reapply after anchoring. Width and height changes if anchoring is change. 
            var rect = rectTransform.rect;
            float width = rect.width;
            float height = rect.height;

            //Setting top anchor 
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            //Reapply size
            rectTransform.sizeDelta = new Vector2(width, height);
        }
        #endregion
    }
}
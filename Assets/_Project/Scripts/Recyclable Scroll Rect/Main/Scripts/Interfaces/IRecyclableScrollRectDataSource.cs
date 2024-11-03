namespace PolyAndCode.UI
{
    /// <summary>
    /// Interface for creating DataSource
    /// Recyclable Scroll Rect must be provided a Data source which must inherit from this.
    /// </summary>
    public interface IRecyclableScrollRectDataSource
    {
        int GetItemCount();
        
        void InitCell(ICell cell);
        
        void SetCell(ICell cell, int index);
    }
}

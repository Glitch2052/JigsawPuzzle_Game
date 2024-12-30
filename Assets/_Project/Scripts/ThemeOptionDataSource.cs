using System.Collections.Generic;
using PolyAndCode.UI;

public class ThemeOptionDataSource : IRecyclableScrollRectDataSource
{
    private readonly List<PuzzleCollectionData> themeCollectionList;

    public ThemeOptionDataSource(List<PuzzleCollectionData> themeCollectionList)
    {
        this.themeCollectionList = themeCollectionList;
    }

    public int GetItemCount()
    {
        return themeCollectionList.Count;
    }

    public void InitCell(ICell cell)
    {
        ((ThemeCategoryCell)cell).InitCell();
    }

    public void SetCell(ICell cell, int index)
    {
        ((ThemeCategoryCell)cell).SetCell(themeCollectionList[index]);
    }
}

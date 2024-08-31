using System.Diagnostics;
using System.Globalization;
using TMPro;
using UnityEngine;

public class GridUIButtons : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public Vector2Int[] gridSizes;

    public void LoadGrid(int index)
    {
        PuzzleGenerator.Instance.PuzzleGrid.IterateOverGridObjects((x,y, gridObj) =>
        {
            Destroy(gridObj.desiredPuzzlePiece.gameObject);
        });

        float startTime = Time.realtimeSinceStartup;
        
        PuzzleGenerator.Instance.GenerateGrid(gridSizes[index].x,gridSizes[index].y);
        
        timerText.text = ((Time.realtimeSinceStartup - startTime) * 1000).ToString("F2") + " millisec";
    }
}

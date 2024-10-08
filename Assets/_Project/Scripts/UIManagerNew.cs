using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerNew : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private PuzzleTextureDatabase puzzleTextureDatabase;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}

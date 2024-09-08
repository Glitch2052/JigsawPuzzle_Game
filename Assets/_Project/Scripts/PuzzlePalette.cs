using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePalette : IObject
{
    [SerializeField] private PaletteContent content;

    public override void Init()
    {
        content.Init(this);
    }
    
    public override void IUpdate()
    {
        content.UpdatePositions();
    }

    public override IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        return content.OnPointerDown(worldPos, pointerId);
    }

    public override IObject OnPointerDrag(Vector2 worldPos, int pointerId)
    {
        return content.OnPointerDrag(worldPos, pointerId);
    }

    public override IObject OnPointerUp(Vector2 worldPos, int pointerId)
    {
        return content.OnPointerUp(worldPos, pointerId);
    }
}

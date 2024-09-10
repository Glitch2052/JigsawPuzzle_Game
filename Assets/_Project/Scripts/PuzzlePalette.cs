using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePalette : IObject
{
    [SerializeField] private PaletteContent content;

    private float newOrthoSize;
    private float oldOrthoSize;

    public override void Init()
    {
        content.Init(this);
        newOrthoSize = oldOrthoSize = iSystem.Camera.orthographicSize;
    }
    
    public override void IUpdate()
    {
        ScalePaletteWithCamera();
        content.IUpdate();
    }

    private void ScalePaletteWithCamera()
    {
        newOrthoSize = iSystem.Camera.orthographicSize;
        LocalScale = newOrthoSize / oldOrthoSize * LocalScale.SetZ(1);
        oldOrthoSize = iSystem.Camera.orthographicSize;
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

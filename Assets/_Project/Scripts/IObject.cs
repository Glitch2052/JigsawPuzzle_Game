using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IObject : MonoBehaviour
{
    [HideInInspector] public InteractiveSystem iSystem;

    protected const int EMPTY = -999;
    protected int draggingPointerId = EMPTY;
    private Vector2 offset, speed, prevPos, currPos, touchPoint, speedMultiplier = new (20f, 20f);
    [field: SerializeField] public BoxCollider2D MainCollider { get; private set; }

    public Vector3 Position
    {
        get => transform.position;
        set => OnPositionUpdate(value);
    }
    public virtual void Init()
    {
        
    }

    public virtual bool OnTap(Vector2 worldPos, int pointerId)
    {
        return true;
    }

    public virtual void IUpdate()
    {
        if (draggingPointerId != EMPTY && iSystem.inputSystem.GetPointerEvent(draggingPointerId).iObject == this)
        {
            touchPoint = iSystem.inputSystem.GetCurrentWorldPosition(draggingPointerId);
            currPos = touchPoint + offset;
            speed = currPos - prevPos;
            Vector3 position = transform.position;
            prevPos = position;

            Vector3 pos = position;
            pos.x += speed.x * speedMultiplier.x * Time.deltaTime;
            pos.y += speed.y * speedMultiplier.y * Time.deltaTime;

            Position = pos;
        }
    }

    protected void OnPositionUpdate(Vector3 value)
    {
        transform.position = value;
    }
    
    public virtual IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        if (draggingPointerId == EMPTY)
        {
            touchPoint = worldPos;
            offset = transform.position - new Vector3(worldPos.x, worldPos.y, 0);
            currPos = prevPos = transform.position;
            draggingPointerId = pointerId;
            
            OnSelected();

            return this;
        }

        return null;
    }
    
    public virtual IObject OnPointerDrag(Vector2 worldPos, int pointerId)
    {
        if (draggingPointerId == pointerId)
        {
            touchPoint = worldPos;
            return this;
        }

        return null;
    }
    
    public virtual IObject OnPointerUp(Vector2 worldPos, int pointerId)
    {
        if (draggingPointerId == pointerId)
        {
            OnReleased();
            draggingPointerId = EMPTY;
            return this;
        }

        return null;
    }
    
    public void SetISystem(InteractiveSystem iSys)
    {
        if (iSystem == iSys) return;
        if (iSystem) iSystem.UnRegisterIEntity(this);
        iSystem = iSys;
        if (iSystem) iSystem.RegisterIEntity(this);
    }

    protected virtual void OnSelected()
    {
        
    }

    protected virtual void OnReleased()
    {
        
    }

    public virtual void OnRegistered()
    {
        iSystem.RegisterTouchCollider(MainCollider,this);
    }

    public virtual void OnUnRegistered()
    {
        iSystem.UnRegisterTouchCollider(MainCollider);
    }
    
}

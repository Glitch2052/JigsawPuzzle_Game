using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class IObject : MonoBehaviour
{
    [HideInInspector] public InteractiveSystem iSystem;

    protected const int EMPTY = -999;
    protected int draggingPointerId = EMPTY;
    private Vector2 offset, speed, prevPos, currPos, touchPoint, speedMultiplier = new (18f, 18f);
    [field: SerializeField] public BoxCollider2D MainCollider { get; private set; }
    
    [HideInInspector] public IObject parent;

    private Coroutine localScaleCoroutine;
    

    public Vector3 Position
    {
        get => transform.position;
        set => OnPositionUpdate(value);
    }

    public Vector3 LocalPosition
    {
        get => transform.localPosition;
        set => transform.localPosition = value;
    }

    public Vector3 LocalScale
    {
        get => transform.localScale;
        set => transform.localScale = value;
    }
    
    public Vector3 LocalScaleLerped
    {
        get => transform.localScale;
        set
        {
            if (localScaleCoroutine != null) StopCoroutine(localScaleCoroutine);
            if (gameObject.activeInHierarchy)
                localScaleCoroutine = StartCoroutine(_LocalScaleLerp(transform.localScale, value));
            else transform.localScale = value;
        }
    }
    
    IEnumerator _LocalScaleLerp(Vector3 fromValue, Vector3 toValue)
    {
        
        var alpha = 0f;
        while (true)
        {
            alpha = Mathf.Clamp01(alpha + Time.deltaTime * 5f);
            transform.localScale = Vector3.Lerp(fromValue, toValue, alpha);
            if (alpha >= 1) break;
            yield return null;
        }

        localScaleCoroutine = null;
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
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, currPos.x, Time.deltaTime * speedMultiplier.x);
            pos.y = Mathf.Lerp(pos.y, currPos.y, Time.deltaTime * speedMultiplier.y);
            Position = pos;
            // speed = currPos - prevPos;
            // Vector3 position = transform.position;
            // prevPos = position;
            //
            // Vector3 pos = position;
            // pos.x += speed.x * speedMultiplier.x * Time.deltaTime;
            // pos.y += speed.y * speedMultiplier.y * Time.deltaTime;
            //
            // Position = pos;
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
        //Set Position On Pointer Down
        Position = Position.SetZ(InteractiveSystem.DRAG_Z_ORDER);
    }

    protected virtual void OnReleased()
    {
        Position = Position.SetZ(0);
    }

    public virtual void OnRegistered()
    {
        iSystem.RegisterTouchCollider(MainCollider,this);
    }

    public virtual void OnUnRegistered()
    {
        iSystem.UnRegisterTouchCollider(MainCollider);
    }
    
    public virtual void SetParent(IObject parent, Transform parentTransform = null)
    {
        if (parent)
        {
            transform.SetParent(parentTransform ? parentTransform : parent.transform);
        }
        else
        {
            if (iSystem == null)
            {
                SetISystem(iSystem);
            }

            transform.SetParent(iSystem.transform);
        }

        if (this.parent) this.parent.OnChildRemoved(this);

        this.parent = parent;

        if (this.parent) this.parent.OnChildAttached(this);
    }
    
    protected virtual void OnChildAttached(IObject child)
    {
        // Debug.Log("OnChildAttached(" + child.name + ")", gameObject);
    }

    protected virtual void OnChildRemoved(IObject child)
    {
        // Debug.Log("OnChildRemoved(" + child.name + ")", gameObject);
    }
    
    protected void SortColliders(int count, Collider2D[] colliders)
    {
        float threshold = 0.05f;
        // Array is already sorted by depth
        // Sort array according to position (Nearest to farthest)
        Vector2 pos = Position;
        int from = -1, to = -1;

        for (int i = 1; i < count; i++)
        {
            if (colliders[i].transform.position.z - colliders[i - 1].transform.position.z < threshold)
            {
                if (from == -1) from = i - 1;
                to = i;
            }
            else if (from != -1)
            {
                SortList(colliders, from, to);
                from = to = -1;
            }
        }

        if (from != -1) SortList(colliders, from, to);
    }

    void SortList(Collider2D[] colliders, int from, int to)
    {
        for (int i = from; i <= to; i++)
        {
            for (int j = i + 1; j <= to; j++)
            {
                float dist_i = (Position - colliders[i].transform.position).sqrMagnitude;
                float dist_j = (Position - colliders[j].transform.position).sqrMagnitude;

                if (dist_j < dist_i)
                {
                    (colliders[i], colliders[j]) = (colliders[j], colliders[i]);
                }
            }
        }
    }
    
    public virtual JSONNode ToJson(JSONNode node = null)
    {
        if (node == null)
            node = new JSONObject();

        node["pos"] = Position;
        node["rot"] = transform.rotation;
        return node;
    }

    public virtual void FromJson(JSONNode node)
    {
        if(node == null) return;
        
        Position = node["pos"];
        transform.rotation = node["rot"];
    }
}

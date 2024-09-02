using System;
using UnityEngine;

public class Grid<T>
{
    private int width;
    private int height;
    private float cellSize;
    private Vector2 origin;
    private T[,] gridArray;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public Vector2 Origin => origin;

    public Vector2 startPoint => origin;
    public Vector2 cellOffset;
    public Vector2 EndPoint { get; }

    public Grid(int width,int height,float cellSize,Vector2 origin, Vector2 cellOffset,Func<Grid<T>,int,int,T> CreateGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;
        this.cellOffset = cellOffset;
        
        EndPoint = origin + new Vector2(width, height) * cellSize;
        gridArray = new T[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                gridArray[i, j] = CreateGridObject(this,i,j);
            }
        }

// #if UNITY_EDITOR
//         DrawDebugData();
// #endif
    }

    public void IterateOverGridObjects(Action<int,int,T> action)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                action?.Invoke(i,j,GetGridObject(i,j));
            }
        }
    }

    public T GetGridObject(int x, int y)
    {
        if(x >= 0 && x < width && y >= 0 && y < height)
            return gridArray[x, y];
        return default;
    }

    public bool GetGridObject(int x, int y, out T gridObject)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            gridObject = gridArray[x, y];
            return true;
        }
        gridObject = default;
        return false;
    }

    public T GetGridObject(Vector2 worldPosition)
    {
        (int x, int y) = GetXY(worldPosition);
            return GetGridObject(x,y);
    }

    public void SetGridObject(int x, int y,T cell)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            gridArray[x, y] = cell;
    }

    private (int,int) GetXY(Vector2 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.y - origin.y) / cellSize);
        return (x, y);
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(x, y) * cellSize + origin;
    }
    
    public Vector2 GetWorldPositionWithCellOffset(int x, int y)
    {
        return new Vector2(x, y) * cellSize + origin + cellOffset;
    }

#if UNITY_EDITOR
    private void DrawDebugData()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Debug.DrawLine(GetWorldPosition(i,j), GetWorldPosition(i,j + 1),Color.white,3600f);
                Debug.DrawLine(GetWorldPosition(i,j), GetWorldPosition(i + 1,j),Color.white,3600f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0,height), GetWorldPosition(width,height),Color.white,3600f);
        Debug.DrawLine(GetWorldPosition(width,0), GetWorldPosition(width,height),Color.white,3600f);
    }
#endif
}
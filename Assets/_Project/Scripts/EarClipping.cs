using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class EarClipping
{ 
    public static int[] Triangulate(List<float3> vertices)
    {
        List<int> indexList = new List<int>(vertices.Count);
        for (int i = 0; i < vertices.Count; i++)
        {
            indexList.Add(i);
        }

        int totalTrisCount = vertices.Count - 2;
        int totalTriangleIndexCount = totalTrisCount * 3;
        
        int[] triangles = new int[totalTriangleIndexCount];

        int currentTriangleIndex = 0;
        while (indexList.Count > 3)
        {
            bool earFound = false;

            for (int i = 0; i < indexList.Count; i++)
            {
                int prevIndex = indexList[GetLoopedIndex(i - 1, indexList.Count)];
                int currIndex = indexList[GetLoopedIndex(i, indexList.Count)];
                int nextIndex = indexList[GetLoopedIndex(i + 1, indexList.Count)];
                    
                float3 prev = vertices[prevIndex];
                float3 curr = vertices[currIndex];
                float3 next = vertices[nextIndex];

                if (IsEar(prev,curr,next, prevIndex, currIndex, nextIndex, vertices))
                {
                    triangles[currentTriangleIndex++] = prevIndex;
                    triangles[currentTriangleIndex++] = currIndex;
                    triangles[currentTriangleIndex++] = nextIndex;
                    
                    indexList.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            if (!earFound)
            {
                Debug.LogError("No ear found. Possible non-simple polygon.");
                return null;
            }
        }

        if (indexList.Count == 3)
        {
            triangles[currentTriangleIndex++] = indexList[0];
            triangles[currentTriangleIndex++] = indexList[1];
            triangles[currentTriangleIndex] = indexList[2];
        }
        
        return triangles;
    }
    
    private static bool IsEar(float3 prev, float3 curr, float3 next, int prevIndex, int currIndex, int nextIndex ,List<float3> vertices2D)
    {
        if (!Utilities.IsConvex(prev-curr,next - curr))
            return false;

        for (int i = 0; i < vertices2D.Count; i++)
        {
            float3 point = vertices2D[i];
            
            if(i == prevIndex || i == currIndex || i == nextIndex)
                continue;
            
            if (Utilities.IsPointInsideTriangleArea(point,prev,curr,next))
                return false;
        }
        return true;
    }

    private static int GetLoopedIndex(int index, int count)
    {
        return (count + index) % count;
    }
}

[BurstCompile]
public struct VertexTriangulationJob : IJob
{
    //Used For Mesh Triangulation
    public NativeList<int> indexList;
    [ReadOnly] public NativeArray<float3> vertices;
    [WriteOnly] public NativeArray<int> triangles;
    
    //Used For Uv Calculation
    [ReadOnly] public float2 minBoardPosition;
    [ReadOnly] public float2 maxBoardPosition;
    [ReadOnly] public float4x4 localToWorldMatrix;
    [WriteOnly] public NativeArray<float2> boardUvs;
    [WriteOnly] public NativeArray<float2> meshUvs;

    private float3 minObjectPos;
    private float3 maxObjectPos;
    
    
    public void Execute()
    {
        //Assign Index List
        //Get Min Max ObjectPos
        //Calculate Uvs with respect to Puzzle Board
        for (int i = 0; i < vertices.Length; i++)
        {
            float3 vertexPos = vertices[i];
            boardUvs[i] = GetPuzzleBoardUvData(vertexPos);
            indexList.Add(i);

            minObjectPos = math.min(minObjectPos, vertexPos);
            maxObjectPos = math.max(maxObjectPos, vertexPos);
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            meshUvs[i] = GetLocalUvData(vertices[i]);
        }
        
        int currentTriangleIndex = 0;
        while (indexList.Length > 3)
        {
            for (int i = 0; i < indexList.Length; i++)
            {
                int prevIndex = indexList[GetLoopedIndex(i - 1, indexList.Length)];
                int currIndex = indexList[GetLoopedIndex(i, indexList.Length)];
                int nextIndex = indexList[GetLoopedIndex(i + 1, indexList.Length)];
                    
                float3 prev = vertices[prevIndex];
                float3 curr = vertices[currIndex];
                float3 next = vertices[nextIndex];

                if (IsEar(prev,curr,next, prevIndex, currIndex, nextIndex, vertices))
                {
                    triangles[currentTriangleIndex++] = prevIndex;
                    triangles[currentTriangleIndex++] = currIndex;
                    triangles[currentTriangleIndex++] = nextIndex;
                    
                    indexList.RemoveAt(i);
                    break;
                }
            }
        }

        if (indexList.Length == 3)
        {
            triangles[currentTriangleIndex++] = indexList[0];
            triangles[currentTriangleIndex++] = indexList[1];
            triangles[currentTriangleIndex] = indexList[2];
        }
    }
    
    private static bool IsEar(float3 prev, float3 curr, float3 next, int prevIndex, int currIndex, int nextIndex ,NativeArray<float3> vertices2D)
    {
        if (!IsConvex(prev-curr,next - curr))
            return false;

        for (int i = 0; i < vertices2D.Length; i++)
        {
            float3 point = vertices2D[i];
            
            if(i == prevIndex || i == currIndex || i == nextIndex)
                continue;
            
            if (IsPointInsideTriangleArea(point,prev,curr,next))
                return false;
        }
        return true;
    }
    
    private static int GetLoopedIndex(int index, int count)
    {
        return (count + index) % count;
    }
    
    private static bool IsConvex(float3 prev, float3 next)
    {
        return prev.x * next.y - prev.y * next.x > 0;
    }
    
    private static bool IsPointInsideTriangleArea(float3 p, float3 p0, float3 p1, float3 p2)
    {
        float dX = p.x - p0.x;
        float dY = p.y - p0.y;
        float dX20 = p2.x - p0.x;
        float dY20 = p2.y - p0.y;
        float dX10 = p1.x - p0.x;
        float dY10 = p1.y - p0.y;

        float sp = (dY20 * dX) - (dX20 * dY);
        float tp = (dX10 * dY) - (dY10 * dX);
        float d = (dX10 * dY20) - (dY10 * dX20);

        if (d > 0)
        {
            return (sp >= 0) && (tp >= 0) && (sp + tp <= d);
        }
        return (sp <= 0) && (tp <= 0) && (sp + tp >= d);
    }

    private float2 GetPuzzleBoardUvData(float3 vertex)
    {
        float4 vertexPos = new float4(vertex,1.0f);
        float4 worldPos = math.mul(localToWorldMatrix, vertexPos);
        return math.unlerp(minBoardPosition, maxBoardPosition, new float2(worldPos.x, worldPos.y));
    }

    private float2 GetLocalUvData(float3 vertex)
    {
        float3 uv = math.unlerp(minObjectPos, maxObjectPos, vertex);
        return new float2(uv.x,uv.y);
    }
}

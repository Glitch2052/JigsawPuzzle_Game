using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utilities
{
    public static Vector2 GetRandomPointInArc(float startAngle, float endAngle, float radius)
    {
        float angle = Random.Range(startAngle, endAngle) * Mathf.Deg2Rad;
        float distance = Random.Range(0f, radius);
        return new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
    }
    
    public static Vector2 GetNormalizedRandomPointInArc(float startAngle, float endAngle,out float angle)
    {
        angle = Random.Range(startAngle, endAngle) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    
    public static T[] ShuffleArray<T>(T[] array,int seed)
    {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length-1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            (array[randomIndex], array[i]) = (array[i], array[randomIndex]);
        }
        return array;
    }
    
    public static List<T> ShuffleArray<T>(List<T> array,int seed)
    {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Count-1; i++)
        {
            int randomIndex = prng.Next(i, array.Count);
            (array[randomIndex], array[i]) = (array[i], array[randomIndex]);
        }
        return array;
    }
    
    // public static bool IsConvex(float3 prev, float3 next)
    // {
    //     return prev.x * next.y - prev.y * next.x > 0;
    // }

    /// <summary>
    /// Determines whether a given point is lies in the triangle defined by point a, b and c
    /// </summary>
    /// <param name="p">point to determine</param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns>TRUE if point lies inside triangle area</returns>
    //In some cases if a and c lie on horizontal line then there is divide by zero exception
    [Obsolete("Use IsPointInsideTriangleArea")]
    public static bool IsPointInsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float s1 = c.y - a.y;
        float s2 = c.x - a.x;
        float s3 = b.y - a.y;
        float s4 = p.y - a.y;
        
        float w1 = ((s2 * s4) - (s1 * (p.x - a.x)))/ ((s2 * s3) - s1 * (b.x - a.x));
        float w2 = (s4 - (w1 * s3)) / s1;
        return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
    }
    
    /// <summary>
    /// Returns a bool indicating that the angle between two vectors is less than 180 degrees
    /// or in other words IT IS CONVEX
    /// </summary>
    /// <param name="prev">direction vector from A to B</param>
    /// <param name="next">direction vector from A to C</param>
    /// <returns></returns>
    [BurstCompile]
    public static bool IsConvex(float3 prev, float3 next)
    {
        return prev.x * next.y - prev.y * next.x > 0;
    }
    
    [BurstCompile]
    public static bool IsPointInsideTriangleArea(float3 p, float3 p0, float3 p1, float3 p2)
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
}

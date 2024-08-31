using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extension
{
    public static Vector3 SetX(this Vector3 pos,float x)
    {
        Vector3 newPos = pos;
        newPos.x = x;
        return newPos;
    }
    
    public static Vector3 SetY(this Vector3 pos,float y)
    {
        Vector3 newPos = pos;
        newPos.y = y;
        return newPos;
    }
    
    public static Vector3 SetZ(this Vector3 pos,float z)
    {
        Vector3 newPos = pos;
        newPos.z = z;
        return newPos;
    }
}

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[CreateAssetMenu(menuName = "Puzzle Data/Edge Profile",fileName = "Edge Profile Info")]
public class EdgeShapeSO : ScriptableObject
{
    public SplineContainer knobProfile;
    public float xLength;
    private float scaleFactor;

    private List<float3> flatCornerPositions;
    private Dictionary<string, List<float3>> profileVertices;

    public float3 BottomLeft => flatCornerPositions[0];
    public float3 TopLeft => flatCornerPositions[1];
    public float3 TopRight => flatCornerPositions[2];
    public float3 BottomRight => flatCornerPositions[3];

    private static readonly int curveResolution = 40;

    
    public void Init(float cellSize)
    {
        SetScaleFactor(cellSize);
        
        float halfCellSize = cellSize * 0.5f;
        flatCornerPositions = new List<float3>
        {
            new (-halfCellSize, -halfCellSize, 0),
            new (-halfCellSize, halfCellSize, 0),
            new (halfCellSize, halfCellSize, 0),
            new (halfCellSize, -halfCellSize, 0)
        };

        profileVertices = new Dictionary<string, List<float3>>();
    }
    
    private void SetScaleFactor(float cellSize)
    {
        scaleFactor = cellSize / xLength;
    }

    public void EvaluateAllPossibleSplinesCombinationOnMainThread()
    {
        float3 scale = new float3(scaleFactor,scaleFactor,1);
        float3 negativeScale = new float3(scaleFactor, -scaleFactor, 1);
        
        //Left Side
        float3 translation = flatCornerPositions[0];
        quaternion rotation = quaternion.EulerXYZ(0,0,math.radians(90));
        profileVertices.Add(EdgeName.LeftKnob, GetSplineVertices(float4x4.TRS(translation,rotation,scale)));
        profileVertices.Add(EdgeName.LeftSocket, GetSplineVertices(float4x4.TRS(translation,rotation,negativeScale)));
        
        //Top Side
        translation = flatCornerPositions[1];
        rotation = quaternion.identity;
        profileVertices.Add(EdgeName.TopKnob, GetSplineVertices(float4x4.TRS(translation,rotation,scale)));
        profileVertices.Add(EdgeName.TopSocket, GetSplineVertices(float4x4.TRS(translation,rotation,negativeScale)));
        
        //Right Side
        translation = flatCornerPositions[2];
        rotation = quaternion.EulerXYZ(0,0,math.radians(270));
        profileVertices.Add(EdgeName.RightKnob, GetSplineVertices(float4x4.TRS(translation,rotation,scale)));
        profileVertices.Add(EdgeName.RightSocket, GetSplineVertices(float4x4.TRS(translation,rotation,negativeScale)));
        
        //Bottom Side
        translation = flatCornerPositions[3];
        rotation = quaternion.EulerXYZ(0,0,math.radians(180));
        profileVertices.Add(EdgeName.BottomKnob, GetSplineVertices(float4x4.TRS(translation,rotation,scale)));
        profileVertices.Add(EdgeName.BottomSocket, GetSplineVertices(float4x4.TRS(translation,rotation,negativeScale)));
    }
    
    private List<float3> GetSplineVertices(float4x4 trsMatrix)
    {
        NativeSpline nativeSpline = new NativeSpline(knobProfile.Spline, trsMatrix);
        
        List<float3> splinePoints = new List<float3>(curveResolution);
        for (int i = 0; i < curveResolution; i ++)
        {
            float3 p = nativeSpline.EvaluatePosition((float)i/ (curveResolution));
            splinePoints.Add(p);
        }
        nativeSpline.Dispose();
        
        return splinePoints;
    }

    public List<float3> GetEvaluatedVertices(string key)
    {
        profileVertices.TryGetValue(key, out List<float3> value);
        return value;
    }

    //I Overdid it in this method. There was no need for job system here
    //we could simply evaluate spline with normal spline evaluation method since we are only doing it for 8 times at start of game
    /*public void EvaluateAllPossibleSplinesCombination()
    {
        int splinesCount = 8;
        NativeArray<JobHandle> splineEvaluationJobHandles = new NativeArray<JobHandle>(splinesCount,Allocator.Temp);
        List<NativeSpline> tempNativeSplinesList = new List<NativeSpline>(splinesCount);
        Dictionary<string, NativeArray<float3>> tempSplinePosDictionary = new Dictionary<string, NativeArray<float3>>();

        float3 scale = new float3(scaleFactor, scaleFactor, 1);
        float3 negativeScale = new float3(scaleFactor, -scaleFactor, 1);
        
        //Left Side
        float3 translation = flatCornerPositions[0];
        quaternion rotation = quaternion.EulerXYZ(0,0,90);
        splineEvaluationJobHandles[0] = EvaluateSpline(EdgeName.LeftKnob, float4x4.TRS(translation,rotation,scale), tempSplinePosDictionary, tempNativeSplinesList);
        splineEvaluationJobHandles[1] = EvaluateSpline(EdgeName.LeftSocket, float4x4.TRS(translation,rotation,negativeScale), tempSplinePosDictionary, tempNativeSplinesList);
        
        //Top Side
        translation = flatCornerPositions[1];
        rotation = quaternion.identity;
        splineEvaluationJobHandles[2] = EvaluateSpline(EdgeName.TopKnob, float4x4.TRS(translation,rotation,scale), tempSplinePosDictionary, tempNativeSplinesList);
        splineEvaluationJobHandles[3] = EvaluateSpline(EdgeName.TopSocket, float4x4.TRS(translation,rotation,negativeScale), tempSplinePosDictionary, tempNativeSplinesList);
        
        //Right Side
        translation = flatCornerPositions[2];
        rotation = quaternion.EulerXYZ(0,0,270);
        splineEvaluationJobHandles[4] = EvaluateSpline(EdgeName.RightKnob, float4x4.TRS(translation,rotation,scale), tempSplinePosDictionary, tempNativeSplinesList);
        splineEvaluationJobHandles[5] = EvaluateSpline(EdgeName.RightSocket, float4x4.TRS(translation,rotation,negativeScale), tempSplinePosDictionary, tempNativeSplinesList);
        
        //Bottom Side
        translation = flatCornerPositions[3];
        rotation = quaternion.EulerXYZ(0,0,180);
        splineEvaluationJobHandles[6] = EvaluateSpline(EdgeName.BottomKnob, float4x4.TRS(translation,rotation,scale), tempSplinePosDictionary, tempNativeSplinesList);
        splineEvaluationJobHandles[7] = EvaluateSpline(EdgeName.BottomSocket, float4x4.TRS(translation,rotation,negativeScale), tempSplinePosDictionary, tempNativeSplinesList);
        
        JobHandle.CompleteAll(splineEvaluationJobHandles);

        //dispose all native splines
        foreach (NativeSpline spline in tempNativeSplinesList)
        {
            spline.Dispose();
        }
        //dispose all native containers in dictionary
        foreach (var keyValuePair in tempSplinePosDictionary)
        {
            profileVertices.Add(keyValuePair.Key, new List<float3>(keyValuePair.Value));
            keyValuePair.Value.Dispose();
        }
        
        //dispose all handles and native containers
        splineEvaluationJobHandles.Dispose();
    }

    private JobHandle EvaluateSpline(string key, float4x4 trsMatrix , Dictionary<string, NativeArray<float3>> posDictionary, List<NativeSpline> splinesList)
    {
        NativeArray<float3> nativeSplinePoints = new NativeArray<float3>(40, Allocator.TempJob);
        NativeSpline nativeSpline = new NativeSpline(knobProfile.Spline,trsMatrix,Allocator.TempJob);
        
        SplineEvaluationJob splineEvaluationJob = new SplineEvaluationJob
        {
            nativeSpline = nativeSpline,
            splinePoints = nativeSplinePoints
        };
        
        splinesList.Add(nativeSpline);
        posDictionary.Add(key,nativeSplinePoints);
        
        return splineEvaluationJob.Schedule();
    }
        
    public struct SplineEvaluationJob : IJob
    {
        [ReadOnly] public NativeSpline nativeSpline;
        public NativeArray<float3> splinePoints;
        
        public void Execute()
        {
            float resolution = splinePoints.Length;
            for (int i = 0; i < splinePoints.Length; i++)
            {
                splinePoints[i] = nativeSpline.EvaluatePosition(i / resolution);
            }
        }
    }*/
}


public static class EdgeName
{
    public static readonly string LeftKnob = "LeftKnob";
    public static readonly string LeftSocket = "LeftSocket";
    
    public static readonly string TopKnob = "TopKnob";
    public static readonly string TopSocket = "TopSocket";
    
    public static readonly string RightKnob = "RightKnob";
    public static readonly string RightSocket = "RightSocket";
    
    public static readonly string BottomKnob = "BottomKnob";
    public static readonly string BottomSocket = "BottomSocket";
}

public enum EdgeType
{
    Flat,
    Knob,
    Socket
}
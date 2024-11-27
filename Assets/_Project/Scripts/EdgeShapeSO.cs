using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
#if UNITY_EDITOR
using System.Linq;
using Object = UnityEngine.Object;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Puzzle Data/Edge Profile",fileName = "Edge Profile Info")]
public class EdgeShapeSO : ScriptableObject
{
    public SplineContainer knobProfile;
    public float xLength;
    public Material puzzleMaterial;

    private float scaleFactor;
    private List<float3> flatCornerPositions;
    private Dictionary<string, List<float3>> profileVertices;
    
    
    private static readonly int Width = Shader.PropertyToID("_Width");
    private static readonly int Height = Shader.PropertyToID("_Height");
    private static readonly int GridCellSize = Shader.PropertyToID("_GridCellSize");
    private static readonly int HalfObjectSize = Shader.PropertyToID("_HalfObjectSize");
    public static readonly int NormalStrength = Shader.PropertyToID("_NormalStrength");
    public static float normalStrengthValue = 0.65f;

    #region Mesh Data

    [Space(40), Header("Mesh Data For All Possible Puzzle Piece Combination")] 
    [SerializeField] private List<MeshData> allPossibleMeshCacheList;
    private Dictionary<string, MeshData> meshCacheDictionary;
    
    #endregion
    
    private static readonly int CurveResolution = 40;
    
    public void Init()
    {
        meshCacheDictionary = new Dictionary<string, MeshData>();
        foreach (MeshData data in allPossibleMeshCacheList)
        {
            meshCacheDictionary[data.edgeProfile] = data;
        }
    }

    public void UpdatePuzzleMaterial(Texture2D puzzleTexture, int width, int height , float gridCellSize)
    {
        puzzleMaterial.mainTexture = puzzleTexture;
        puzzleMaterial.SetFloat(Width,width);
        puzzleMaterial.SetFloat(Height,height);
        puzzleMaterial.SetFloat(GridCellSize,gridCellSize);
        puzzleMaterial.SetVector(HalfObjectSize,Vector2.one * (gridCellSize * 0.5f));
        Shader.SetGlobalFloat(NormalStrength, normalStrengthValue);
    }

    public MeshData GetMeshData(string key)
    {
        return meshCacheDictionary[key];
    }

#if UNITY_EDITOR
    [Space(60), Header("Editor Only Data")] 
    [SerializeField] private float cellSize;
    [SerializeField] private PuzzlePiece puzzlePrefab;
    [SerializeField] private bool generateGameObjects;

    public Object normalMapsFolder;
    
    private EditorCoroutine meshDataCalculationCoroutine;
    public float3 BottomLeft => flatCornerPositions[0];
    public float3 TopLeft => flatCornerPositions[1];
    public float3 TopRight => flatCornerPositions[2];
    public float3 BottomRight => flatCornerPositions[3];
    

    [ContextMenu("Calculate All Possible MeshData")]
    public void CalculateAllPossibleMeshData()
    {
        if (meshDataCalculationCoroutine != null)
        {
            Debug.LogError("Editor Coroutine is Still Running");
            return;
        }
        meshDataCalculationCoroutine = EditorCoroutineUtility.StartCoroutine(CalculateMeshData(), this);
    }

    private IEnumerator CalculateMeshData()
    {
        SetUpScriptableEditorOnly(cellSize);
        
        yield return null;
        
        //here length is 81 because there are 4 edges each with 3 different combination
        //so 3^4 possible combinations
        for (int i = 4; i < 81; i++)
        {
            if(i == 6 || i == 9 || i == 18 || i == 27 || i == 54) continue;
            string ternaryNum = ConvertDecimalToTernary(i);
            MeshData generatedData = EarClipping.GenerateMeshDataEditorOnly(this, ternaryNum);
            allPossibleMeshCacheList.Add(generatedData);
            yield return null;
        }

        if (generateGameObjects)
        {
            GameObject parentObject = new GameObject("Parent")
            {
                transform =
                {
                    position = Vector3.zero
                }
            };
            float x = 0;
            float y = 0;
            for (int i = 0; i < allPossibleMeshCacheList.Count; i++)
            {
                x = i % 9;
                var piece = Instantiate(puzzlePrefab, parentObject.transform);
                if (i % 9 == 0)
                {
                    y += 3;
                }
                piece.transform.position = new Vector3(x * 3, y, 0);
                piece.SetMeshData(allPossibleMeshCacheList[i]);
                yield return null;
            }
        }

        
        meshDataCalculationCoroutine = null;
    }

    private string ConvertDecimalToTernary(int num)
    {
        string ternary = string.Empty;
        while (num > 0)
        {
            ternary = (num % 3) + ternary;
            num /= 3;
        }
        
        return ternary.PadLeft(4,'0');    
    }
    
    private void SetUpScriptableEditorOnly(float cellSize)
    {
        allPossibleMeshCacheList.Clear();
        scaleFactor = cellSize / xLength;
        
        float halfCellSize = cellSize * 0.5f;
        flatCornerPositions = new List<float3>
        {
            new (-halfCellSize, -halfCellSize, 0),
            new (-halfCellSize, halfCellSize, 0),
            new (halfCellSize, halfCellSize, 0),
            new (halfCellSize, -halfCellSize, 0)
        };

        profileVertices = new Dictionary<string, List<float3>>();
        EvaluateAllPossibleSplinesCombinationOnMainThread();
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
        
        List<float3> splinePoints = new List<float3>(CurveResolution);
        for (int i = 0; i < CurveResolution; i ++)
        {
            float3 p = nativeSpline.EvaluatePosition((float)i/ (CurveResolution));
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

    [ContextMenu("Assign Normal Maps")]
    private void AssignNormalMaps()
    {
        string folderPath = AssetDatabase.GetAssetPath(normalMapsFolder);
        string[] assetGuids = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });
        string[] assetPaths = new string[assetGuids.Length];
        for (var i = 0; i < assetGuids.Length; i++)
        {
            var guid = assetGuids[i];
            assetPaths[i] = AssetDatabase.GUIDToAssetPath(guid);
        }

        List<MeshData> updatedMeshData = new List<MeshData>();
        foreach (MeshData meshData in allPossibleMeshCacheList)
        {
            MeshData newMeshData = meshData;
            string path = assetPaths.FirstOrDefault(p => p.Contains(meshData.edgeProfile));
            if (path == String.Empty || path == "") continue;
            newMeshData.normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            updatedMeshData.Add(newMeshData);
        }
        allPossibleMeshCacheList.Clear();
        allPossibleMeshCacheList = updatedMeshData;
        EditorUtility.SetDirty(this);
    }
    
#endif
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

[Serializable]
public struct MeshData
{
    public string edgeProfile;
    public Texture2D normalTex;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
}

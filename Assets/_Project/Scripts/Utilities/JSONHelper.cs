using SimpleJSON;

public static class JSONHelper
{
    private static readonly string NextSceneType = "nextSceneType";
    
    public static SceneType GetNextSceneType(this JSONNode configData)
    {
        if (SceneType.GameScene.ToString() == configData[NextSceneType])
            return SceneType.GameScene;
        if (SceneType.LevelSelect.ToString() == configData[NextSceneType])
            return SceneType.LevelSelect;
        return SceneType.None;
    }
    
    public static void SetNextSceneType(this JSONNode configData, SceneType sceneType)
    {
        if (configData[NextSceneType] != null) configData.Remove(NextSceneType);
        configData.Add(NextSceneType, sceneType.ToString());
    }
}

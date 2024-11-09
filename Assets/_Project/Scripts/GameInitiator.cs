using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;

public class GameInitiator : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    
    private async void Start()
    {
        Application.targetFrameRate = 60;
        
        BindObjects();
        //Show Loading Panel
        await InitializeObjects();

        BeginGame();
    }

    private void BindObjects()
    {
        gameManager = Instantiate(gameManager);
        uiManager = Instantiate(uiManager);
    }

    private async Task InitializeObjects()
    {
        // Wait Till Initialization Of Objects
        // like ads handler or analytics services
        await Task.Yield();
        
        gameManager.Init();
        uiManager.Init();
    }

    private void BeginGame()
    {
        JSONNode node = new JSONObject();
        node.SetNextSceneType(SceneType.LevelSelect);
        gameManager.LoadScene(StringID.LevelSelectScene,node);
    }
}

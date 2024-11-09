using System.Threading.Tasks;
using UnityEngine;

public class GameInitiator : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    private async void Start()
    {
        Application.targetFrameRate = 60;
        
        BindObjects();
        await InitializeObjects();
    }

    private void BindObjects()
    {
        gameManager = Instantiate(gameManager);
        uiManager = Instantiate(uiManager);
    }

    private async Task InitializeObjects()
    {
        await Task.Yield();
        // Wait Till Initialization Of Objects
        // like ads handler or analytics services
    }
}

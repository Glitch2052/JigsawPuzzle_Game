using System.Collections;
using SimpleJSON;
using TMPro;
using UnityEngine;

public class HomeScenePlayButton : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    // public TMPAnimator tmpAnimator;
    // public TMPWriter tmpWriter;
    
    public IEnumerator Start()
    {
        // tmpWriter.StopWriter();
        // yield return new WaitForSeconds(0.5f);
        // tmpAnimator.StartAnimating();
        // tmpWriter.StartWriter();
        yield return new WaitForSeconds(0.5f);
        // tmpAnimator.StopAnimating();
    }

    public void LoadLevelSelectScene()
    {
        JSONNode node = new JSONObject();
        node.SetNextSceneType(SceneType.LevelSelect);
        GameManager.Instance.LoadScene(StringID.LevelSelectScene,node);
    }
}

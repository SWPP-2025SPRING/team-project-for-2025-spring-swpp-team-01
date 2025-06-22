using UnityEngine;

public class StartSceneController : MonoBehaviour
{
    public GameObject startPanel;      // StartImage 오브젝트
    public GameObject cutscenePanel;   // CutscenePanel 오브젝트

    public void OnStartClicked()
    {
        startPanel.SetActive(false);
        cutscenePanel.SetActive(true);
    }
}

using UnityEngine;

public class Stage1Loader : MonoBehaviour
{
    public GameObject stage1UI;

    public void HideUIAndStartGame()
    {
        stage1UI.SetActive(false);
    }
}

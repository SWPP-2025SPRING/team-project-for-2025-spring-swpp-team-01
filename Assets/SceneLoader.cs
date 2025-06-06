using UnityEngine;
using UnityEngine.SceneManagement;

// 다른 스크립트에서 씬을 전환할 때 사용하는 유틸리티
// Utility script for loading scenes from other components
public class SceneLoader : MonoBehaviour
{
    // 지정한 이름의 씬을 로드한다
    // Loads a scene with the given name
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

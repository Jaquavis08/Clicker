using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagersc : MonoBehaviour
{
   public void LoadSceneByName(string sceneName)
   {
       SceneManager.LoadScene(sceneName);
    }
}

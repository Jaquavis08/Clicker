using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagersc : MonoBehaviour
{
   public void LoadSceneByName(string sceneName)
   {
       SceneManager.LoadScene(sceneName);
   }

//    public void LoadSceneByName(string sceneName)
//    {
//#if PLATFORM_ANDROID

//        SceneManager.LoadScene(sceneName);

//#else

//        SceneManager.LoadScene(sceneName);

//#endif
//    }
}

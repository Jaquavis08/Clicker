using UnityEditor;
using UnityEngine;

public class MissingScriptFinder
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void FindMissingScripts()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        int count = 0;

        foreach (GameObject go in allObjects)
        {
            var components = go.GetComponents<MonoBehaviour>();
            foreach (var c in components)
            {
                if (c == null)
                {
                    Debug.Log($"Missing script found on GameObject: {go.name}", go);
                    count++;
                }
            }
        }

        Debug.Log($"Total missing scripts found: {count}");
    }
}

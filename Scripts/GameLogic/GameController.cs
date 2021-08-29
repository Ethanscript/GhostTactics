using UnityEditor;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public void exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

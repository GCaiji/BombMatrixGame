using UnityEngine;

public class QuitGameHandler : MonoBehaviour
{
    // 退出游戏方法
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        
        // 在编辑器中停止播放
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在发布版本中退出应用
        Application.Quit();
#endif
    }
}
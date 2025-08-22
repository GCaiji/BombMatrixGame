// SceneLoader.cs（挂载在每个场景的跳转按钮上）
using UnityEngine;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] string targetScene;
    
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(LoadTargetScene);
    }

    // 公开的重新绑定方法
    public void RebindToSingleton()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(LoadTargetScene);
    }

    void LoadTargetScene()
    {
        // 通过单例实例跳转
        LoadingManager.Instance.LoadSceneAsync(targetScene); 
    }
}
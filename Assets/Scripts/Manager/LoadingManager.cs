using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LoadingManager : MonoBehaviour
{
    private static LoadingManager _instance;
    private bool isLoading = false; 

    // 存储所有场景中的SceneLoader引用
    private List<SceneLoader> sceneLoaders = new List<SceneLoader>();

    public static LoadingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LoadingManager>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("LoadingManager_Instance");
                    _instance = singletonObject.AddComponent<LoadingManager>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        // 确保单例唯一性
        if (_instance != null && _instance != this)
        {
            Debug.Log($"[LoadingManager] Duplicate instance destroyed in {gameObject.scene.name}");
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log($"[LoadingManager] Initialized in {gameObject.scene.name}");
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[LoadingManager] Scene {scene.name} loaded with mode {mode}");
        isLoading = false;
        
        // 场景加载后立即触发UI重绑定
        StartCoroutine(RebindSceneUI());
    }

    // 注册SceneLoader
    public void RegisterSceneLoader(SceneLoader loader)
    {
        if (!sceneLoaders.Contains(loader))
        {
            sceneLoaders.Add(loader);
            Debug.Log($"[LoadingManager] Registered SceneLoader in {loader.gameObject.scene.name}");
        }
    }

    // 注销SceneLoader
    public void UnregisterSceneLoader(SceneLoader loader)
    {
        if (sceneLoaders.Contains(loader))
        {
            sceneLoaders.Remove(loader);
        }
    }

    // UI重绑定协程
    private IEnumerator RebindSceneUI()
    {
        // 等待一帧确保所有UI元素初始化完成
        yield return null;
        
        Debug.Log($"[LoadingManager] Rebinding UI in {SceneManager.GetActiveScene().name}");
        
        // 收集当前场景的所有SceneLoader
        SceneLoader[] currentSceneLoaders = FindObjectsOfType<SceneLoader>();
        
        foreach (SceneLoader loader in currentSceneLoaders)
        {
            if (!sceneLoaders.Contains(loader))
            {
                loader.RebindToSingleton();
                sceneLoaders.Add(loader);
            }
        }
    }

    // 直接加载场景
    public void LoadSceneDirect(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning($"[LoadingManager] Scene {sceneName} is already loading");
            return;
        }
        
        Debug.Log($"[LoadingManager] Loading scene directly: {sceneName}");
        
        isLoading = true;
        SceneManager.LoadScene(sceneName);
        
        // 清理前一个场景的loader引用
        ClearDestroyedLoaders();
    }
    
    // 异步加载场景
    public void LoadSceneAsync(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning($"[LoadingManager] Scene {sceneName} is already loading");
            return;
        }
        
        if (this == null) 
        {
            Debug.LogWarning("[LoadingManager] Instance destroyed before loading");
            return;
        }
        
        Debug.Log($"[LoadingManager] Loading scene async: {sceneName}");
        
        isLoading = true;
        StartCoroutine(LoadAsyncRoutine(sceneName));
        
        // 清理前一个场景的loader引用
        ClearDestroyedLoaders();
    }

    private IEnumerator LoadAsyncRoutine(string sceneName)
    {
        Debug.Log($"[LoadingManager] Starting async load: {sceneName}");
        
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;
        
        while (!op.isDone)
        {
            Debug.Log($"[LoadingManager] Loading progress: {op.progress * 100}%");
            yield return null;
        }
        
        Debug.Log($"[LoadingManager] {sceneName} loaded successfully");
        isLoading = false;
    }

    // 清理已被销毁的SceneLoader引用
    private void ClearDestroyedLoaders()
    {
        for (int i = sceneLoaders.Count - 1; i >= 0; i--)
        {
            if (sceneLoaders[i] == null)
            {
                sceneLoaders.RemoveAt(i);
            }
        }
    }
}
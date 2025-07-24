using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private BombStats baseBombStats;
    
    [Header("Character Settings")]
    [SerializeField] private CharacterStats baseCharacterStats;    // 新增：角色基础配置
    
    [Header("Game Settings")]
    [SerializeField] private MapStats mapStats;           // 新增：地图配置引用
    [SerializeField] private float timeLimit = 180f;      // 游戏时间限制（秒）
    
    [Header("Debug")]
    [SerializeField] private float currentDestructionPercentage; // 当前破坏百分比
    [SerializeField] private float remainingTime;                // 剩余游戏时间
    
    private RuntimeBombStats currentBombStats;
    private RuntimeCharacterStats currentCharacterStats;    // 新增：角色运行时数据
    private bool gameEnded;
    
    // 单例实例
    public static GameManager Instance { get; private set; }
    
    // 公开属性
    public RuntimeBombStats CurrentBombStats => currentBombStats;
    public RuntimeCharacterStats CurrentCharacterStats => currentCharacterStats;    // 新增：角色数据访问器
    public float CurrentDestructionPercentage => currentDestructionPercentage;
    public float RemainingTime => remainingTime;
    public bool IsGameEnded => gameEnded;

    private void Awake()
    {
        // 单例模式处理
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 初始化基础数据和验证
        if (baseBombStats == null)
        {
            Debug.LogError("baseBombStats 未设置！");
        }
        
        if (baseCharacterStats == null)    // 新增：检查角色配置
        {
            Debug.LogError("baseCharacterStats 未设置！");
        }
        
        // 检查并加载默认地图配置
        if (mapStats == null)
        {
            // 尝试加载默认配置
            mapStats = Resources.Load<MapStats>("DefaultMapStats");
            
            if (mapStats == null)
            {
                Debug.LogWarning("未找到默认地图配置，创建临时配置");
                mapStats = ScriptableObject.CreateInstance<MapStats>();
            }
        }
        
        ResetToDefaultStats();
        
        // 确保在场景加载时重置游戏状态
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 当加载新场景时重置游戏状态
        ResetGameState();
    }

    private void Update()
    {
        if (gameEnded) return;
        
        // 更新游戏计时器
        remainingTime -= Time.deltaTime;
        
        // 检查时间耗尽
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            GameOver(false); // 时间耗尽 - 失败
        }
    }

    // 重置所有游戏状态
    public void ResetGameState()
    {
        gameEnded = false;
        currentDestructionPercentage = 0f;
        remainingTime = timeLimit;
        ResetToDefaultStats();
        
        // 启用游戏控制相关的对象
        Time.timeScale = 1f;
    }

    // 重置数据（包括炸弹和角色属性）
    public void ResetToDefaultStats()
    {
        // 重置炸弹数据
        if (baseBombStats != null)
        {
            currentBombStats = baseBombStats.CreateRuntimeStats();
            Debug.Log($"炸弹数值重置: 伤害={currentBombStats.Damage}");
        }
        else
        {
            Debug.LogWarning("使用默认炸弹数值");
            currentBombStats = new RuntimeBombStats
            {
                Damage = 1,
                FuseTime = 3f,
                ExplosionRadius = 2f
            };
        }

        // 重置角色数据
        if (baseCharacterStats != null)
        {
            currentCharacterStats = baseCharacterStats.CreateRuntimeStats();
            Debug.Log($"角色数值重置: 生命值={currentCharacterStats.CurrentHealth}");
        }
        else
        {
            Debug.LogWarning("使用默认角色数值");
            currentCharacterStats = new RuntimeCharacterStats
            {
                MaxHealth = 5,
                CurrentHealth = 5,
                MoveSpeed = 5f,
                MaxBombs = 3,
                BombCooldown = 1f,
                InvincibleDuration = 2f
            };
        }
    }

    // 开始新游戏
    public void StartNewGame()
    {
        ResetToDefaultStats();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 更新破坏进度
    public void UpdateDestructionProgress(int destroyedTileCount, int totalTiles)
    {
        if (gameEnded || mapStats == null) return;
        
        if (totalTiles <= 0)
        {
            Debug.LogWarning("totalTiles不能为0或负数");
            return;
        }
        
        currentDestructionPercentage = Mathf.Clamp01((float)destroyedTileCount / totalTiles);
        
        // 使用 MapStats 中的配置，添加null检查
        if (mapStats != null && currentDestructionPercentage >= mapStats.DestructionGoal)
        {
            GameOver(true); // 达成破坏目标 - 胜利
        }
    }

    // 游戏结束处理
    public void GameOver(bool isVictory)
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Time.timeScale = 0f; // 暂停游戏
        
        Debug.Log(isVictory ? "游戏胜利！" : "游戏失败！");
        
        // 这里可以添加游戏结束UI显示逻辑
        // UIManager.Instance.ShowGameOverScreen(isVictory, currentDestructionPercentage);
        
        // 保存进度或进行其他处理
    }

    // 获取格式化的时间字符串（分钟:秒）
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}

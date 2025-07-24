using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawnManager : MonoBehaviour
{
    [Header("生成设置")]
    public float spawnInterval = 5f;
    public float warningTime = 2f;
    
    [Header("对象池设置")]
    public int warningPoolSize = 10;
    public int monsterPoolSize = 20;
    
    [Header("引用")]
    public GameObject monsterPrefab;
    public GameObject warningIndicatorPrefab;
    public GameObject mapPlane;
    [Tooltip("拖入场景中的怪物容器对象")]
    public GameObject monstersContainer;
    [Tooltip("拖入场景中的预警圈容器对象")]
    public GameObject warningsContainer;

    private Coroutine spawnCoroutine;
    private Renderer planeRenderer;
    private Queue<GameObject> warningPool = new Queue<GameObject>();
    private Queue<GameObject> monsterPool = new Queue<GameObject>();
    private List<GameObject> activeWarnings = new List<GameObject>();
    private List<GameObject> activeMonsters = new List<GameObject>();

    private void Start()
    {
        if (mapPlane == null)
        {
            Debug.LogError("未分配地图平面对象！");
            return;
        }

        planeRenderer = mapPlane.GetComponent<Renderer>();
        if (planeRenderer == null)
        {
            Debug.LogError("地图平面对象没有Renderer组件！");
            return;
        }
        
        // 自动创建容器对象（如果未手动拖入）
        if (monstersContainer == null)
        {
            monstersContainer = new GameObject("MonstersContainer");
        }
        if (warningsContainer == null)
        {
            warningsContainer = new GameObject("WarningsContainer");
        }
        
        InitializePools();
        StartSpawning();
    }
    
    private void InitializePools()
    {
        // 初始化预警圈对象池
        for (int i = 0; i < warningPoolSize; i++)
        {
            CreateNewWarningInstance();
        }
        
        // 初始化怪物对象池
        for (int i = 0; i < monsterPoolSize; i++)
        {
            CreateNewMonsterInstance();
        }
    }
    
    private void CreateNewWarningInstance()
    {
        GameObject warning = Instantiate(
            warningIndicatorPrefab, 
            Vector3.zero,
            Quaternion.Euler(90f, 0f, 0f)
        );
        
        // 设置父对象为预警圈容器
        warning.transform.SetParent(warningsContainer.transform);
        warning.SetActive(false);
        warningPool.Enqueue(warning);
    }
    
    private void CreateNewMonsterInstance()
    {
        GameObject monster = Instantiate(monsterPrefab, Vector3.zero, Quaternion.identity);
        
        // 设置父对象为怪物容器
        monster.transform.SetParent(monstersContainer.transform);
        monster.SetActive(false);
        monsterPool.Enqueue(monster);
    }

    public void StartSpawning()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
            
        spawnCoroutine = StartCoroutine(SpawnCycle());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            Vector3 spawnPosition = GetRandomSpawnPosition();
            ShowWarning(spawnPosition);
            yield return new WaitForSeconds(warningTime);
            SpawnMonster(spawnPosition);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (planeRenderer == null) return Vector3.zero;

        Bounds bounds = planeRenderer.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x), 
            0.5f, 
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private void ShowWarning(Vector3 position)
    {
        if (warningIndicatorPrefab == null) return;
        
        GameObject warning = null;
        if (warningPool.Count > 0)
        {
            warning = warningPool.Dequeue();
        }
        else
        {
            CreateNewWarningInstance();
            warning = warningPool.Dequeue();
            Debug.LogWarning("预警圈对象池为空，动态创建新实例。考虑增大对象池大小。");
        }
        
        warning.transform.position = position;
        warning.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        warning.SetActive(true);
        activeWarnings.Add(warning);
        
        StartCoroutine(ReturnWarningToPool(warning, warningTime));
    }
    
    private IEnumerator ReturnWarningToPool(GameObject warning, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!activeWarnings.Contains(warning)) yield break;
        
        warning.SetActive(false);
        activeWarnings.Remove(warning);
        warningPool.Enqueue(warning);
    }

    private void SpawnMonster(Vector3 position)
    {
        if (monsterPrefab == null) return;
        
        GameObject monster = null;
        if (monsterPool.Count > 0)
        {
            monster = monsterPool.Dequeue();
        }
        else
        {
            CreateNewMonsterInstance();
            monster = monsterPool.Dequeue();
            Debug.LogWarning("怪物对象池为空，动态创建新实例。考虑增大对象池大小。");
        }
        
        Vector3 spawnPos = new Vector3(position.x, 10f, position.z);
        monster.transform.position = spawnPos;
        monster.SetActive(true);
        activeMonsters.Add(monster);
        
        MonsterController monsterController = monster.GetComponent<MonsterController>();
        if (monsterController != null)
        {
            // 注意：这里使用的是 Action<GameObject> 回调
            monsterController.StartFalling(position, ReturnMonsterToPool);
        }
        else
        {
            StartCoroutine(DelayedReturnMonster(monster));
        }
    }
    
    private IEnumerator DelayedReturnMonster(GameObject monster)
    {
        yield return new WaitForSeconds(5f);
        ReturnMonsterToPool(monster);
    }
    
    // 注意：此方法必须匹配 Action<GameObject> 委托
    public void ReturnMonsterToPool(GameObject monster)
    {
        if (monster == null) return;
        if (!activeMonsters.Contains(monster)) return;
        
        monster.SetActive(false);
        activeMonsters.Remove(monster);
        monsterPool.Enqueue(monster);
    }

    private void OnDrawGizmosSelected()
    {
        if (mapPlane == null) return;
        
        Renderer renderer = mapPlane.GetComponent<Renderer>();
        if (renderer == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
    }
    
    // 添加清理所有生成对象的方法
    public void CleanUpAllSpawnedObjects()
    {
        // 停止生成
        StopSpawning();
        
        // 清空所有活动对象
        foreach (var warning in activeWarnings)
        {
            if (warning != null)
            {
                warning.SetActive(false);
                warningPool.Enqueue(warning);
            }
        }
        activeWarnings.Clear();
        
        foreach (var monster in activeMonsters)
        {
            if (monster != null)
            {
                monster.SetActive(false);
                monsterPool.Enqueue(monster);
            }
        }
        activeMonsters.Clear();
    }
}
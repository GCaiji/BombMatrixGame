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
    public GameObject monstersContainer;
    public GameObject warningsContainer;

    private Coroutine spawnCoroutine;
    private Renderer planeRenderer;
    private Queue<GameObject> warningPool = new Queue<GameObject>();
    private Queue<GameObject> monsterPool = new Queue<GameObject>();
    private List<GameObject> activeWarnings = new List<GameObject>();
    private List<GameObject> activeMonsters = new List<GameObject>();

    private void Start()
    {
        if (mapPlane != null)
        {
            planeRenderer = mapPlane.GetComponent<Renderer>();
        }

        // 自动创建容器对象
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

        warning.transform.SetParent(warningsContainer.transform);
        warning.SetActive(false);
        warningPool.Enqueue(warning);
    }

    private void CreateNewMonsterInstance()
    {
        GameObject monster = Instantiate(monsterPrefab, Vector3.zero, Quaternion.identity);

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
        }

        warning.transform.position = position;
        warning.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        warning.SetActive(true);
        activeWarnings.Add(warning);
        
        // 初始化预警圈控制器
        WarningIndicatorController indicator = warning.GetComponent<WarningIndicatorController>();
        if (indicator != null)
        {
            indicator.Initialize(this);
        }
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
        }

        Vector3 spawnPos = new Vector3(position.x, 10f, position.z);
        monster.transform.position = spawnPos;
        monster.SetActive(true);
        activeMonsters.Add(monster);

        MonsterController monsterController = monster.GetComponent<MonsterController>();
        if (monsterController != null)
        {
            monsterController.StartFalling(position, ReturnMonsterToPool);
        }
        else
        {
            StartCoroutine(DelayedReturnMonster(monster));
        }
    }

    // 公共方法供预警圈控制器调用
    public void ReturnWarningToPool(GameObject warning)
    {
        if (warning == null) return;

        warning.SetActive(false);
        if (activeWarnings.Contains(warning))
            activeWarnings.Remove(warning);

        warningPool.Enqueue(warning);
    }

    public void ReturnMonsterToPool(GameObject monster)
    {
        if (monster == null) return;
        if (!activeMonsters.Contains(monster)) return;

        monster.SetActive(false);
        activeMonsters.Remove(monster);
        monsterPool.Enqueue(monster);
    }

    private IEnumerator DelayedReturnMonster(GameObject monster)
    {
        yield return new WaitForSeconds(5f);
        ReturnMonsterToPool(monster);
    }

    public void CleanUpAllSpawnedObjects()
    {
        StopSpawning();

        // 清理所有活动的预警圈
        foreach (var warning in new List<GameObject>(activeWarnings))
        {
            ReturnWarningToPool(warning);
        }
        activeWarnings.Clear();

        // 清理所有活动的怪物
        foreach (var monster in new List<GameObject>(activeMonsters))
        {
            ReturnMonsterToPool(monster);
        }
        activeMonsters.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (mapPlane == null) return;

        Renderer renderer = mapPlane.GetComponent<Renderer>();
        if (renderer == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
    }
}
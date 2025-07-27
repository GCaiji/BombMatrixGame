using UnityEngine;

public class WarningIndicatorController : MonoBehaviour
{
    private MonsterSpawnManager spawnManager;

    public void Initialize(MonsterSpawnManager manager)
    {
        spawnManager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检查碰撞对象是否是怪物
        if (other.CompareTag("Monster"))
        {
            // 通知生成管理器回收此预警圈
            if (spawnManager != null)
            {
                spawnManager.ReturnWarningToPool(gameObject);
            }
        }
    }
}
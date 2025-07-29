using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MonsterController))]
public class SlimeController : MonoBehaviour
{
    // 添加动画触发器常量
    private static readonly int IdleTrigger = Animator.StringToHash("Idle");
    private static readonly int HitTrigger = Animator.StringToHash("Hit");
    private static readonly int DieTrigger = Animator.StringToHash("Die");

    private Animator animator;
    private MonsterController monsterController;
    private bool hasLanded = false;  // 新增：标记是否已经落地
    
    void Start()
    {
        animator = GetComponent<Animator>();
        monsterController = GetComponent<MonsterController>();
        
        // 安全订阅事件 - 使用协程等待Stats初始化
        StartCoroutine(SubscribeToEventsAfterInitialization());
    }
    
    private IEnumerator SubscribeToEventsAfterInitialization()
    {
        // 等待直到Stats被初始化
        int maxAttempts = 10;
        int attempts = 0;
        
        while (monsterController.Stats == null && attempts < maxAttempts)
        {
            attempts++;
            yield return null; // 等待一帧
        }
        
        // 安全订阅死亡事件
        if (monsterController.Stats != null)
        {
            monsterController.Stats.OnMonsterDeath += HandleMonsterDeath;
            monsterController.OnMonsterLanded += OnSlimeLanded;  // 新增：订阅落地事件
        }
        else
        {
            Debug.LogWarning($"无法订阅事件: Stats未初始化 ({gameObject.name})");
        }
    }

    private void OnSlimeLanded()
    {
        if (hasLanded) return;  // 如果已经落地过，直接返回
        
        hasLanded = true;  // 标记已落地
        
        if (monsterController.Stats != null)
        {
            Debug.Log($"[Slime状态] {gameObject.name}已落地 | " +
                     $"生命值: {monsterController.Stats.CurrentHealth}/{monsterController.Stats.MaxHealth} | " +
                     $"攻击力: {monsterController.Stats.AttackDamage}");
        }
    }
    
    void OnDisable()
    {
        // 在禁用时执行清理
        SafeUnsubscribeEvents();
    }
    
    void OnDestroy()
    {
        SafeUnsubscribeEvents();
    }
    
    private void SafeUnsubscribeEvents()
    {
        // 安全取消订阅
        if (monsterController != null)
        {
            if (monsterController.Stats != null)
            {
                monsterController.Stats.OnMonsterDeath -= HandleMonsterDeath;
            }
            monsterController.OnMonsterLanded -= OnSlimeLanded;  // 新增：取消订阅落地事件
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 检查是否与玩家碰撞且怪物状态正常
        if (other.CompareTag("Player") && monsterController != null && monsterController.Stats != null)
        {
            // 受到伤害
            monsterController.Stats.TakeDamage(1);

            // 如果怪物死亡，直接播放死亡动画
            if (monsterController.Stats.IsDead)
            {
                animator.SetTrigger(DieTrigger);
            }
            // 否则播放受击动画
            else
            {
                animator.SetTrigger(HitTrigger);
            }
        }
    }
    
    private void HandleMonsterDeath()
    {
        // 播放死亡动画
        animator.SetTrigger(DieTrigger);
        
        // ===== 新增：死亡信息输出 =====
        Debug.Log($"[Slime死亡] {gameObject.name}已被消灭！");
        
        StartCoroutine(DelayedReturnToPool(1.0f)); // 增加延迟时间以确保死亡动画播放完成
    }
    
    IEnumerator DelayedReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 安全返回对象池
        if (monsterController != null)
        {
            monsterController.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

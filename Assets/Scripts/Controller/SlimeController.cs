using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MonsterController))]
public class SlimeController : MonoBehaviour, IDamageable
{
    // 动画参数定义
    private static readonly int IsHitParam = Animator.StringToHash("IsHit");
    private static readonly int IsDeadParam = Animator.StringToHash("IsDead");

    private Animator animator;
    private MonsterController monsterController;
    private Coroutine _hitAnimationCoroutine; // 受击动画协程控制
    private bool hasLanded = false; // 标记是否已经落地
    
    void Start()
    {
        animator = GetComponent<Animator>();
        monsterController = GetComponent<MonsterController>();
        StartCoroutine(SubscribeToEventsAfterInitialization());
    }
    
    private IEnumerator SubscribeToEventsAfterInitialization()
    {
        int maxAttempts = 10;
        int attempts = 0;
        
        while (monsterController.Stats == null && attempts < maxAttempts)
        {
            attempts++;
            yield return null;
        }
        
        if (monsterController.Stats != null)
        {
            monsterController.Stats.OnMonsterDeath += HandleMonsterDeath;
            monsterController.OnMonsterLanded += OnSlimeLanded;
        }
    }

    private void OnSlimeLanded()
    {
        if (hasLanded) return;
        hasLanded = true;
    }
    
    void OnDisable()
    {
        SafeUnsubscribeEvents();
    }
    
    void OnDestroy()
    {
        SafeUnsubscribeEvents();
    }
    
    private void SafeUnsubscribeEvents()
    {
        if (monsterController != null)
        {
            if (monsterController.Stats != null)
            {
                monsterController.Stats.OnMonsterDeath -= HandleMonsterDeath;
            }
            monsterController.OnMonsterLanded -= OnSlimeLanded;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && monsterController != null && monsterController.Stats != null)
        {
            monsterController.Stats.TakeDamage(1);
            PlayHitAnimation(0.5f); // 统一使用动画协程处理
        }
    }
    
    // 删除OnTriggerExit方法，由动画协程统一控制结束
    
    // 动画事件回调方法（保留用于可能的其他用途）
    public void OnHitAnimationComplete()
    {
        // 可能不需要具体实现，保留接口
    }
    
    private void HandleMonsterDeath()
    {
        if (animator != null)
        {
            animator.SetBool(IsHitParam, false); // 确保停止受击状态
            animator.SetBool(IsDeadParam, true);
        }
        
        // 取消所有可能正在进行的动画协程
        if (_hitAnimationCoroutine != null)
        {
            StopCoroutine(_hitAnimationCoroutine);
            _hitAnimationCoroutine = null;
        }
        
        StartCoroutine(DelayedReturnToPool(1.0f));
    }
    
    IEnumerator DelayedReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (monsterController != null)
        {
            monsterController.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsAlive => monsterController != null && monsterController.Stats != null && !monsterController.Stats.IsDead;

    public void TakeDamage(int damageAmount)
    {
        if (monsterController != null && monsterController.Stats != null)
        {
            monsterController.Stats.TakeDamage(damageAmount);
            PlayHitAnimation(0.5f); // 统一使用动画协程处理
        }
    }

    // 统一处理受击动画的方法（适用于所有伤害源）
    private void PlayHitAnimation(float duration)
    {
        if (!IsAlive || animator == null) return;
        
        // 停止之前的动画协程（防止重复叠加）
        if (_hitAnimationCoroutine != null)
        {
            StopCoroutine(_hitAnimationCoroutine);
        }
        
        // 设置为受击状态
        animator.SetBool(IsHitParam, true);
        
        // 启动新的协程控制动画时间
        _hitAnimationCoroutine = StartCoroutine(StopHitAnimationAfterDelay(duration));
    }
    
    // 受击动画协程（控制动画播放时间）
    private IEnumerator StopHitAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (animator != null && IsAlive)
        {
            animator.SetBool(IsHitParam, false);
        }
        _hitAnimationCoroutine = null;
    }
}
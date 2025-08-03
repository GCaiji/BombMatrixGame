using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ActorController : MonoBehaviour, IDamageable
{
    [Header("Character Stats")]
    [SerializeField] private CharacterStats baseStats;
    [SerializeField] private int monsterDamage = 1;
    [SerializeField] private int bombDamage = 1;
    [SerializeField] private float damageCooldown = 0.5f;
    
    [Header("Hit Animation")]
    [SerializeField] private float bombHitDuration = 0.5f;
    
    private RuntimeCharacterStats runtimeStats;
    private Animator animator;
    private bool isDead = false;
    private bool canTakeDamage = true;
    private int affectedLayerIndex = 1;
    private Coroutine _hitAnimationCoroutine;
    
    public bool IsInvincible => runtimeStats.IsInvincible;
    
    private static readonly int HitTrigger = Animator.StringToHash("IsHit");
    private static readonly int DeathBool = Animator.StringToHash("IsDead");
    private static readonly int Speed = Animator.StringToHash("Speed");

    public CharacterStats BaseStats => baseStats;
    public RuntimeCharacterStats RuntimeStats => runtimeStats;
    public float MoveSpeed => runtimeStats.MoveSpeed;

    void Awake()
    {
        ConfigureCollisionComponents();
        animator = GetComponent<Animator>();
        InitializeStats();
        InitializeAnimator();
    }
    
    void OnEnable()
    {
        // 订阅爆炸事件
        if (BombManager.Instance != null)
        {
            BombManager.OnExplosion += HandleBombExplosion;
        }
    }
    
    void OnDisable()
    {
        // 取消订阅爆炸事件
        if (BombManager.Instance != null)
        {
            BombManager.OnExplosion -= HandleBombExplosion;
        }
    }

    private void ConfigureCollisionComponents()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CapsuleCollider>();
        }
        collider.isTrigger = true;
        collider.enabled = true;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (isDead) return;
        runtimeStats.UpdateInvincibleState(Time.deltaTime);
    }

    private void InitializeStats()
    {
        if (baseStats == null)
        {
            Debug.LogError("请指定CharacterStats");
            runtimeStats = new RuntimeCharacterStats();
        }
        else
        {
            runtimeStats = baseStats.CreateRuntimeStats();
        }
    }

    private void InitializeAnimator()
    {
        animator.SetBool(DeathBool, false);
        animator.SetFloat(Speed, 0);

        if (animator.layerCount > affectedLayerIndex)
        {
            animator.SetLayerWeight(affectedLayerIndex, 0f);
        }
        else
        {
            Debug.LogError("受击层不存在！请检查Animator");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Monster") && !isDead && canTakeDamage)
        {
            TakeDamage(monsterDamage);
            StartCoroutine(DamageCooldown());
        }
        // 炸弹接触伤害
        else if (other.gameObject.CompareTag("Bomb") && !isDead && canTakeDamage)
        {
            BombController bomb = other.GetComponent<BombController>();
            if (bomb != null && bomb.IsAboutToExplode())
            {
                TakeDamage(bombDamage);
                StartCoroutine(DamageCooldown());
            }
        }
    }
    
    // 处理炸弹爆炸伤害
    private void HandleBombExplosion(Vector3 explosionPosition, float explosionRadius)
    {
        if (isDead || !canTakeDamage) return;
        
        // 计算距离并检查是否在爆炸范围内
        float distance = Vector3.Distance(transform.position, explosionPosition);
        if (distance <= explosionRadius)
        {
            // 根据距离计算伤害衰减
            float damageMultiplier = Mathf.Clamp01(1 - (distance / explosionRadius));
            int calculatedDamage = Mathf.CeilToInt(bombDamage * damageMultiplier);
            
            TakeDamage(calculatedDamage);
            StartCoroutine(DamageCooldown());
        }
    }

    private System.Collections.IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || !canTakeDamage) return;
        
        bool isDamaged = runtimeStats.TakeDamage(damage);
        if (!isDamaged) return;
        
        PlayHitAnimation();
        
        if (runtimeStats.CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void PlayHitAnimation()
    {
        if (animator == null || animator.layerCount <= affectedLayerIndex || isDead) return;
        
        // 停止之前的动画协程
        if (_hitAnimationCoroutine != null)
        {
            StopCoroutine(_hitAnimationCoroutine);
        }
        
        // 播放受击动画
        animator.SetLayerWeight(affectedLayerIndex, 1f);
        animator.SetTrigger(HitTrigger);
        
        // 启动新的协程控制动画结束
        _hitAnimationCoroutine = StartCoroutine(ResetHitLayerAfterDelay());
    }
    
    private System.Collections.IEnumerator ResetHitLayerAfterDelay()
    {
        yield return new WaitForSeconds(bombHitDuration);
        
        if (!isDead && animator != null && animator.layerCount > affectedLayerIndex)
        {
            animator.SetLayerWeight(affectedLayerIndex, 0f);
        }
        
        _hitAnimationCoroutine = null;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        animator.SetBool(DeathBool, true);
        
        if (TryGetComponent<Collider>(out var collider))
            collider.enabled = false;
            
        // 停止所有协程
        if (_hitAnimationCoroutine != null)
        {
            StopCoroutine(_hitAnimationCoroutine);
            _hitAnimationCoroutine = null;
        }
        
        // 移除事件订阅
        if (BombManager.Instance != null)
        {
            BombManager.OnExplosion -= HandleBombExplosion;
        }
    }
}
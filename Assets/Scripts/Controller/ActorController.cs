using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ActorController : MonoBehaviour, IDamageable
{
    [Header("Character Stats")]
    [SerializeField] private CharacterStats baseStats;
    [SerializeField] private int monsterDamage = 1;
    [SerializeField] private int bombDamage = 1;
    [SerializeField] private float invincibleDuration = 1f;
    
    [Header("Hit Animation")]
    [SerializeField] private float bombHitDuration = 0.5f;
    
    [Header("Hit Stun Settings")]
    [SerializeField] private float hitStunDuration = 1f;

    private RuntimeCharacterStats runtimeStats;
    private Animator animator;
    private bool isDead = false;
    private bool isInHitStun = false;
    private bool isPlayingHitAnimation = false;

    private int affectedLayerIndex = 1;
    
    public bool IsInvincible => runtimeStats.IsInvincible;
    public bool IsInHitStun => isInHitStun;
    public bool IsActionDisabled => isInHitStun || isDead || isPlayingHitAnimation;
    
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
        if (BombManager.Instance != null)
        {
            BombManager.OnExplosion += HandleBombExplosion;
        }
    }
    
    void OnDisable()
    {
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
        if (isDead || IsActionDisabled) return;
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
        if (animator == null) return;
        
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
        if (IsActionDisabled || IsInvincible) return;

        if (other.gameObject.CompareTag("Monster"))
        {
            TakeDamage(monsterDamage);
        }
        else if (other.gameObject.CompareTag("Bomb"))
        {
            BombController bomb = other.GetComponent<BombController>();
            if (bomb != null && bomb.IsAboutToExplode())
            {
                TakeDamage(bombDamage);
            }
        }
    }
    
    private void HandleBombExplosion(Vector3 explosionPosition, float explosionRadius)
    {
        if (IsActionDisabled) return;
        
        float distance = Vector3.Distance(transform.position, explosionPosition);
        if (distance <= explosionRadius)
        {
            float damageMultiplier = Mathf.Clamp01(1 - (distance / explosionRadius));
            int calculatedDamage = Mathf.CeilToInt(bombDamage * damageMultiplier);
            
            TakeDamage(calculatedDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsActionDisabled) return;

        bool isDamaged = runtimeStats.TakeDamage(damage, invincibleDuration);
        if (!isDamaged) return;

        // 启动僵直状态
        StartCoroutine(HitStunRoutine());
        
        // 播放受击动画
        PlayHitAnimation();

        if (runtimeStats.CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    // 僵直状态协程
    private IEnumerator HitStunRoutine()
    {
        // 设置状态
        isInHitStun = true;
        Debug.Log($"进入僵直状态，持续时间: {hitStunDuration}秒");
        
        // 等待僵直时间
        yield return new WaitForSeconds(hitStunDuration);
        
        // 清理状态
        isInHitStun = false;
        Debug.Log("僵直状态结束，恢复行动能力");
    }

    // 播放受击动画
    private void PlayHitAnimation()
    {
        if (animator == null || animator.layerCount <= affectedLayerIndex || isDead) return;

        isPlayingHitAnimation = true;
        animator.SetLayerWeight(affectedLayerIndex, 1f);
        animator.SetTrigger(HitTrigger);
        animator.SetFloat(Speed, 0);
        animator.SetBool("IsMoving", false);
        
        Debug.Log($"受击动画开始播放");
        
        // 启动协程重置动画状态
        StartCoroutine(ResetHitAnimation());
    }
    
    // 重置受击动画状态
    private IEnumerator ResetHitAnimation()
    {
        // 等待完整的受击动画时间
        yield return new WaitForSeconds(bombHitDuration);
        
        // 重置动画状态
        if (!isDead && animator != null && animator.layerCount > affectedLayerIndex)
        {
            animator.SetLayerWeight(affectedLayerIndex, 0f);
        }
        
        isPlayingHitAnimation = false;
        Debug.Log("受击动画结束");
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if (animator != null)
        {
            animator.SetBool(DeathBool, true);
        }
        
        if (TryGetComponent<Collider>(out var collider))
            collider.enabled = false;
            
        // 重置所有状态
        isInHitStun = false;
        isPlayingHitAnimation = false;
        
        // 移除事件订阅
        if (BombManager.Instance != null)
        {
            BombManager.OnExplosion -= HandleBombExplosion;
        }
    }
}

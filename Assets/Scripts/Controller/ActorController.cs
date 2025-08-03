using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ActorController : MonoBehaviour
{
    [SerializeField] private CharacterStats baseStats;
    [SerializeField] private int monsterDamage = 1;
    [SerializeField] private float damageCooldown = 0.5f;
    private RuntimeCharacterStats runtimeStats;
    private Animator animator;
    private bool isDead = false;
    private bool canTakeDamage = true;
    private int affectedLayerIndex = 1;
    private bool isHitAnimationPlaying = false;

    // 修改：只判断是否处于无敌状态
    public bool IsInvincible => runtimeStats.IsInvincible;
    public bool IsHitAnimationPlaying => isHitAnimationPlaying;

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
        Debug.Log($"角色碰撞配置完成 - 触发器状态: {GetComponent<Collider>().isTrigger}");
    }

    private void ConfigureCollisionComponents()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CapsuleCollider>();
            Debug.LogWarning("自动添加CapsuleCollider");
        }
        collider.isTrigger = true;
        collider.enabled = true;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.LogWarning("自动添加Rigidbody");
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
            Debug.Log($"初始生命值: {runtimeStats.CurrentHealth}");
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
            Debug.Log("检测到Monster碰撞");
            TakeDamage(monsterDamage);
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
        if (isDead) return;
        
        bool isDamaged = runtimeStats.TakeDamage(damage);
        if (!isDamaged)
        {
            Debug.Log("处于无敌状态，未受伤害");
            return;
        }

        Debug.Log($"受到 {damage} 点伤害! 当前生命值: {runtimeStats.CurrentHealth}/{runtimeStats.MaxHealth}");

        PlayHitAnimation();

        if (runtimeStats.CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void PlayHitAnimation()
    {
        if (animator.layerCount <= affectedLayerIndex) return;

        isHitAnimationPlaying = true;
        animator.SetLayerWeight(affectedLayerIndex, 1f);
        animator.SetTrigger(HitTrigger);

        StopCoroutine(ResetHitLayer());
        StartCoroutine(ResetHitLayer());
    }

    private System.Collections.IEnumerator ResetHitLayer()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (!isDead)
        {
            animator.SetLayerWeight(affectedLayerIndex, 0f);
            isHitAnimationPlaying = false;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("角色死亡");

        animator.SetBool(DeathBool, true);
        if (animator.layerCount > affectedLayerIndex)
        {
            animator.SetLayerWeight(affectedLayerIndex, 1f);
        }

        if (TryGetComponent<Collider>(out var collider))
            collider.enabled = false;
    }

    public void OnHitAnimationEnd()
    {
        if (!isDead && animator.layerCount > affectedLayerIndex)
        {
            animator.SetLayerWeight(affectedLayerIndex, 0f);
            isHitAnimationPlaying = false;
        }
    }
}


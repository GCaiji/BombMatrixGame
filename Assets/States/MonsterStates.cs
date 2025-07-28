using UnityEngine;

[CreateAssetMenu(menuName = "Game/Monster Stats")]
public class MonsterStats : ScriptableObject
{
    [Header("Base Settings")]
    [SerializeField] [Range(1, 3)] private int maxHealth = 1;
    [SerializeField] [Range(1, 5)] private int attackDamage = 1;
    [SerializeField] [Range(1, 20)] private float moveSpeed = 5f;
    [SerializeField] [Range(0.1f, 5f)] private float attackCooldown = 1f;

    [Header("Runtime Values")]
    [SerializeField] private int currentHealth;
    
    public int CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = Mathf.Clamp(value, 0, MaxHealth);
    }

    // 创建运行时状态的方法
    public RuntimeMonsterStats CreateRuntimeStats()
    {
        RuntimeMonsterStats runtimeStats = new RuntimeMonsterStats
        {
            MaxHealth = this.MaxHealth,
            CurrentHealth = this.MaxHealth,
            AttackDamage = this.AttackDamage,
            MoveSpeed = this.MoveSpeed,
            AttackCooldown = this.AttackCooldown,
        };
        return runtimeStats;
    }

    // 初始化方法
    public void Initialize()
    {
        CurrentHealth = MaxHealth;
    }
    
    public int MaxHealth => Mathf.Max(1, maxHealth);
    public int AttackDamage => Mathf.Max(1, attackDamage);
    public float MoveSpeed => Mathf.Clamp(moveSpeed, 1f, 20f);
    public float AttackCooldown => Mathf.Clamp(attackCooldown, 0.1f, 5f);
  
}

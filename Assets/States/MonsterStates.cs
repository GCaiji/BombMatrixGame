using UnityEngine;

[CreateAssetMenu(menuName = "Game/Monster Stats")]
public class MonsterStats : ScriptableObject
{
    [Header("Base Settings")]
    [SerializeField] [Range(1, 3)] private int maxHealth = 1;
    [SerializeField] [Range(1, 5)] private int attackDamage = 1;
    [SerializeField] [Range(1, 20)] private float moveSpeed = 5f;
    [SerializeField] [Range(0.1f, 5f)] private float attackCooldown = 1f;
    
    public int MaxHealth => maxHealth;
    public int AttackDamage => attackDamage;
    public float MoveSpeed => moveSpeed;
    public float AttackCooldown => attackCooldown;

    // 创建运行时状态的方法
    public RuntimeMonsterStats CreateRuntimeStats()
    {
        return new RuntimeMonsterStats
        {
            MaxHealth = MaxHealth,
            CurrentHealth = MaxHealth,
            AttackDamage = AttackDamage,
            MoveSpeed = MoveSpeed,
            AttackCooldown = AttackCooldown,
        };
    }
}

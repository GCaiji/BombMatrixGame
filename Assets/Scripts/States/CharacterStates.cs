using UnityEngine;

[CreateAssetMenu(menuName = "Game/Character Stats")]
public class CharacterStats : ScriptableObject
{
    
    
    [Header("Base Settings")]
    [SerializeField] [Range(1, 10)] private int maxHealth = 5;
    [SerializeField] [Range(3, 8)] private float moveSpeed = 5f;
    [SerializeField] [Range(1, 10)] private int maxBombs = 10; 
    [SerializeField] [Range(0.5f, 2f)] private float bombCooldown = 1f;
    [SerializeField] [Range(1f, 3f)] private float invincibleDuration = 2f;

    [Header("Runtime Values")]
    [SerializeField] private int currentHealth;
    
    public int CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = Mathf.Clamp(value, 0, MaxHealth);
    }

    // 初始化方法
    public void Initialize()
    {
        CurrentHealth = MaxHealth;
    }
    
    public int MaxHealth => Mathf.Max(1, maxHealth);
    public float MoveSpeed => Mathf.Clamp(moveSpeed, 3f, 8f);
    public int MaxBombs => Mathf.Clamp(maxBombs, 1, 5);
    public float BombCooldown => Mathf.Clamp(bombCooldown, 0.5f, 2f);
    public float InvincibleDuration => Mathf.Clamp(invincibleDuration, 1f, 3f);
}

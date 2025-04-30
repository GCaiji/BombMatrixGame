using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private CharacterStats stats;
    
    public float MoveSpeed => stats.MoveSpeed;
    private void Awake()
    {
        stats.Initialize(); // 初始化生命值
        InvokeRepeating(nameof(LogStatus), 0f, 3f);
    }

    private void LogStatus()
    {
        Debug.Log($"当前生命值: {stats.CurrentHealth}, 移动速度: {stats.MoveSpeed}");
    }

    // 示例：添加生命值修改方法
    public void TakeDamage(int damage)
    {
        stats.CurrentHealth -= damage;
    }

    public void Heal(int amount)
    {
        stats.CurrentHealth += amount;
    }
}
using UnityEngine;

// 基础伤害接口
public interface IDamageable 
{
    void TakeDamage(int damageAmount);  // 接受伤害
    bool IsAlive { get; }               // 是否存活（可选）
}
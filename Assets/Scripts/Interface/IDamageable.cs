using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// 接收伤害的接口方法
    /// </summary>
    /// <param name="damage">基础伤害值</param>
    /// <param name="damageSource">伤害来源（可选）</param>
    void TakeDamage(int damage, GameObject damageSource = null);
}
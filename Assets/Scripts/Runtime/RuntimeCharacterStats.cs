using UnityEngine;

[System.Serializable]
public class RuntimeCharacterStats
{
    // 基础属性
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    public float MoveSpeed { get; set; }
    public int MaxBombs { get; set; }
    public float BombCooldown { get; set; }
    public float InvincibleDuration { get; set; }

    // 当前状态
    public bool IsInvincible { get; private set; }
    public float InvincibleTimer { get; private set; }
    public int CurrentBombCount { get; private set; }

    // 克隆方法
    public RuntimeCharacterStats Clone()
    {
        return new RuntimeCharacterStats
        {
            MaxHealth = this.MaxHealth,
            CurrentHealth = this.CurrentHealth,
            MoveSpeed = this.MoveSpeed,
            MaxBombs = this.MaxBombs,
            BombCooldown = this.BombCooldown,
            InvincibleDuration = this.InvincibleDuration,
            IsInvincible = this.IsInvincible,
            InvincibleTimer = this.InvincibleTimer,
            CurrentBombCount = this.CurrentBombCount
        };
    }

    // 受伤方法
    public bool TakeDamage(int damage)
    {
        if (IsInvincible) return false;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        if (damage > 0)
        {
            SetInvincible();
            Debug.Log($"角色受到 {damage} 点伤害! 当前生命值: {CurrentHealth}");
        }
        return true;
    }

    // 治疗方法
    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        Debug.Log($"角色恢复 {amount} 点生命值! 当前生命值: {CurrentHealth}");
    }

    // 设置无敌状态
    public void SetInvincible()
    {
        IsInvincible = true;
        InvincibleTimer = InvincibleDuration;
    }

    // 更新无敌状态
    public void UpdateInvincibleState(float deltaTime)
    {
        if (!IsInvincible) return;

        InvincibleTimer -= deltaTime;
        if (InvincibleTimer <= 0)
        {
            IsInvincible = false;
            Debug.Log("无敌状态结束");
        }
    }

    // 炸弹计数管理
    public bool TryPlaceBomb()
    {
        if (CurrentBombCount >= MaxBombs) return false;
        
        CurrentBombCount++;
        return true;
    }

    public void OnBombExploded()
    {
        CurrentBombCount = Mathf.Max(0, CurrentBombCount - 1);
    }
}

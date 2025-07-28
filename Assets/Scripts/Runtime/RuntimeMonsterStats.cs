using UnityEngine;

[System.Serializable]
public class RuntimeMonsterStats
{
    // 基础属性 - 从MonsterStats获取
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    public int AttackDamage { get; set; }
    public float MoveSpeed { get; set; }
    public float AttackCooldown { get; set; }
    public int ExperienceValue { get; set; }
    
    // 怪物类型（可选）
    public MonsterType MonsterType { get; set; }
    
    // 当前状态
    public bool IsStunned { get; private set; }
    public float StunTimer { get; private set; }
    public float AttackTimer { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public Vector2 MovementDirection { get; private set; }

    // 克隆方法
    public RuntimeMonsterStats Clone()
    {
        return new RuntimeMonsterStats
        {
            MaxHealth = this.MaxHealth,
            CurrentHealth = this.CurrentHealth,
            AttackDamage = this.AttackDamage,
            MoveSpeed = this.MoveSpeed,
            AttackCooldown = this.AttackCooldown,
            ExperienceValue = this.ExperienceValue,
            MonsterType = this.MonsterType,
            IsStunned = this.IsStunned,
            StunTimer = this.StunTimer,
            AttackTimer = this.AttackTimer,
            MovementDirection = this.MovementDirection
        };
    }

    // 初始化方法
    public void Initialize(MonsterStats baseStats, MonsterType monsterType = MonsterType.Normal)
    {
        MaxHealth = baseStats.MaxHealth;
        CurrentHealth = baseStats.MaxHealth;
        AttackDamage = baseStats.AttackDamage;
        MoveSpeed = baseStats.MoveSpeed;
        AttackCooldown = baseStats.AttackCooldown;
        MonsterType = monsterType;
        
        IsStunned = false;
        StunTimer = 0f;
        AttackTimer = 0f;
        MovementDirection = Vector2.zero;
    }

    // 受伤方法
    public void TakeDamage(int damage)
    {
        if (IsStunned || IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        Debug.Log($"怪物受到 {damage} 点伤害! 当前生命值: {CurrentHealth}/{MaxHealth}");
        
        // 受到伤害时可能会触发特殊状态
        if (CurrentHealth > 0 && damage > 0)
        {
            // 如果是特殊怪物类型，有概率进入狂怒状态
            if (MonsterType == MonsterType.Elite && Random.value > 0.7f)
            {
                EnterFuryState();
            }
        }
    }

    // 进入狂怒状态（特殊怪物）
    public void EnterFuryState()
    {
        if (IsStunned) return;
        
        Debug.Log("怪物进入狂怒状态!");
        AttackDamage = (int)(AttackDamage * 1.5f);
        MoveSpeed *= 1.3f;
    }

    // 被击晕
    public void Stun(float duration)
    {
        IsStunned = true;
        StunTimer = duration;
        Debug.Log($"怪物被击晕 {duration} 秒");
    }

    // 更新状态
    public void UpdateState(float deltaTime)
    {
        // 更新击晕状态
        if (IsStunned)
        {
            StunTimer -= deltaTime;
            if (StunTimer <= 0)
            {
                IsStunned = false;
                Debug.Log("怪物恢复行动");
            }
        }
        
        // 更新攻击冷却
        if (AttackTimer > 0)
        {
            AttackTimer -= deltaTime;
        }
    }

    // 设置移动方向
    public void SetMovementDirection(Vector2 direction)
    {
        MovementDirection = direction.normalized;
    }

    // 尝试攻击
    public bool TryAttack()
    {
        if (IsStunned || IsDead || AttackTimer > 0) return false;
        
        AttackTimer = AttackCooldown;
        Debug.Log($"怪物发动攻击! 伤害: {AttackDamage}");
        return true;
    }

    // 获取移动向量（考虑状态）
    public Vector2 GetMovementVector()
    {
        if (IsStunned || IsDead) return Vector2.zero;
        return MovementDirection * MoveSpeed;
    }

    // 复活方法（如需要）
    public void Revive(int reviveHealthPercent = 50)
    {
        CurrentHealth = Mathf.Max(1, MaxHealth * reviveHealthPercent / 100);
        IsStunned = false;
        StunTimer = 0f;
        AttackTimer = 0f;
        Debug.Log($"怪物复活! 生命值: {CurrentHealth}/{MaxHealth}");
    }
}

// 怪物类型枚举（扩展用）
public enum MonsterType
{
    Normal, // 普通
    Elite,  // 精英
    Boss    // BOSS
}
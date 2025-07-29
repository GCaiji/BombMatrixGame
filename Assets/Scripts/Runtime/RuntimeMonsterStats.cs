    using UnityEngine;
    using System;

    [System.Serializable]
    public class RuntimeMonsterStats
    {
        // 基础属性
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int AttackDamage { get; set; }
        public float MoveSpeed { get; set; }
        public float AttackCooldown { get; set; }
        
        // 当前状态
        public bool IsStunned { get; private set; }
        public float AttackTimer { get; private set; }
        public bool IsDead => CurrentHealth <= 0;
        public event Action OnMonsterDeath;

        // 受伤方法
        public void TakeDamage(int damage)
        {
            if (IsStunned || IsDead) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            if (IsDead)
            {
                OnMonsterDeath?.Invoke();
            }
        }

        // 更新状态
        public void UpdateState(float deltaTime)
        {
            if (AttackTimer > 0)
            {
                AttackTimer -= deltaTime;
            }
        }

        // 尝试攻击
        public bool TryAttack()
        {
            if (IsStunned || IsDead || AttackTimer > 0) return false;
            
            AttackTimer = AttackCooldown;
            return true;
        }
        
        public void Initialize(int maxHealth, int attackDamage, float moveSpeed, float attackCooldown)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            AttackDamage = attackDamage;
            MoveSpeed = moveSpeed;
            AttackCooldown = attackCooldown;
        }
    }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimeBombStats 
{
    public float ExplosionRadius { get; set; }
    public float FuseTime { get; set; }
    public int Damage { get; set; }

    public RuntimeBombStats Clone()
    {
        return new RuntimeBombStats
        {
            ExplosionRadius = this.ExplosionRadius,
            FuseTime = this.FuseTime,
            Damage = this.Damage
        };
    }

    public void IncreaseExplosionRadius(float amount)
    {
        ExplosionRadius = Mathf.Clamp(ExplosionRadius + amount, 0.2f, 5f);
        Debug.Log($"炸弹爆炸范围已增加! 当前范围: {ExplosionRadius}");
    }
}

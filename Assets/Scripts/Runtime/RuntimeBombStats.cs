using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimeBombStats 
{
    public float ExplosionRadius { get;  set; }
    public float FuseTime { get;  set; }
    public int Damage { get;  set; }

    public RuntimeBombStats Clone()
    {
        return new RuntimeBombStats
        {
            ExplosionRadius = this.ExplosionRadius,
            FuseTime = this.FuseTime,
            Damage = this.Damage
        };
    }
    
}

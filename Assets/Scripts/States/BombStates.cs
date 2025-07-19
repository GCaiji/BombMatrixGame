using UnityEngine;

[CreateAssetMenu(menuName = "Game/Bomb Stats")]
public class BombStats : ScriptableObject
{
    [Header("Explosion Settings")]
    [SerializeField] [Range(0.2f, 5f)] private float explosionRadius = 0.2f;
    [SerializeField] [Range(1f, 5f)] private float fuseTime = 3f;
    [SerializeField] [Range(1, 3)] private int damage = 1;

    public float ExplosionRadius => Mathf.Clamp(explosionRadius, 0.2f, 5f);
    public float FuseTime => Mathf.Clamp(fuseTime, 1f, 5f);
    public int Damage => Mathf.Clamp(damage, 1, 3);

    public RuntimeBombStats CreateRuntimeStats()
    {
        return new RuntimeBombStats
        {
            ExplosionRadius = Mathf.Clamp(explosionRadius, 0.2f, 5f), // 修改最小值为 0.2f
            FuseTime = Mathf.Clamp(fuseTime, 1f, 5f),
            Damage = Mathf.Clamp(damage, 1, 3)
        };
    }
}


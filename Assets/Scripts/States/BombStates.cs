using UnityEngine;

[CreateAssetMenu(menuName = "Game/Bomb Stats")]
public class BombStats : ScriptableObject
{
    [Header("Explosion Settings")]
    [SerializeField] [Range(1f, 5f)] private float explosionRadius = 2f;
    [SerializeField] [Range(1f, 5f)] private float fuseTime = 3f;
    [SerializeField] [Range(1, 3)] private int damage = 1;

    public float ExplosionRadius => Mathf.Clamp(explosionRadius, 1f, 5f);
    public float FuseTime => Mathf.Clamp(fuseTime, 1f, 5f);
    public int Damage => Mathf.Clamp(damage, 1, 3);
}

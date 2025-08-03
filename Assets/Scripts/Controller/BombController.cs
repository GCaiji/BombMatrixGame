using UnityEngine;
using System.Collections; 

public class BombController : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem spark;
    [SerializeField] private ParticleSystem smoke;
    [SerializeField] private ParticleSystem explosion;
    
    [Header("Damage Settings")]
    [SerializeField] private LayerMask damageableLayers;
    
    private RuntimeBombStats _runtimeBombStats;
    private Animator _bombAnimator;
    private float _timer;
    private bool _hasExploded;
    private bool _isDestroyed;

    public bool IsAboutToExplode() => _timer < 0.5f;

    void Start()
    {
        // 初始组件检查
        if (_bombAnimator == null) _bombAnimator = GetComponent<Animator>();
    }

    public void Initialize(RuntimeBombStats runtimeStats)
    {
        if (runtimeStats == null)
        {
            Debug.LogError("RuntimeBombStats未初始化！");
            enabled = false;
            return;
        }

        _runtimeBombStats = runtimeStats;
        _timer = _runtimeBombStats.FuseTime;

        if (_bombAnimator == null)
        {
            _bombAnimator = GetComponent<Animator>();
            if (_bombAnimator == null)
            {
                Debug.LogError("缺少Animator组件");
                enabled = false;
                return;
            }
        }

        _bombAnimator.Play("Ignite");
        PlayParticles();
    }

    void Update()
    {
        if (!_hasExploded && _runtimeBombStats != null)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                TriggerExplosion();
            }
        }
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _runtimeBombStats.ExplosionRadius,
            damageableLayers,
            QueryTriggerInteraction.Collide // 包含触发器
        );

        Vector3 explosionPos = transform.position;
        float explosionRadius = _runtimeBombStats.ExplosionRadius;

        foreach (Collider hit in hits)
        {
            // 处理可破坏物体
            DestructibleTile destructibleTile = hit.GetComponent<DestructibleTile>();
            
            // 检查是否是怪物
            if (hit.CompareTag("Monster"))
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(1);
                }
                continue;
            }

            // 处理其他可受伤物体
            IDamageable otherDamageable = hit.GetComponent<IDamageable>();
            if (otherDamageable != null)
            {
                otherDamageable.TakeDamage(1);
            }
        }

        // 通知GroundManager处理地形破坏
        if (GroundManager.Instance != null)
        {
            GroundManager.Instance.DestroyTilesInRadius(explosionPos, explosionRadius);
        }
        else
        {
            Debug.LogError("[BombController] GroundManager实例未找到！");
        }

        // 触发全局爆炸事件
        if (BombManager.Instance != null)
        {
            BombManager.Instance.TriggerExplosionEvent(explosionPos, explosionRadius);
        }

        StartCoroutine(DestroyAndUpdateGround(explosionPos, explosionRadius));
    }

    private IEnumerator DestroyAndUpdateGround(Vector3 explosionPos, float explosionRadius)
    {
        _isDestroyed = true;
        
        if(explosion != null)
        {
            explosion.transform.SetParent(transform.parent);
            explosion.Play();
        }

        Destroy(gameObject);

        yield return null;

        GroundManager.Instance?.DestroyTilesInRadius(explosionPos, explosionRadius);

        if(explosion != null)
        {
            float duration = explosion.main.duration;
            yield return new WaitForSeconds(duration);
            Destroy(explosion.gameObject);
        }
    }

    public void TriggerExplosion()
    {
        if(_isDestroyed) return;
    
        _hasExploded = true;
        Explode();
        
        _bombAnimator?.SetTrigger("Explode");
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_runtimeBombStats != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _runtimeBombStats.ExplosionRadius);
        }
    }
    #endif

    public void OnExplosionEnd()
    {
        if (_isDestroyed || _bombAnimator == null) 
        {
            return;
        }
        _bombAnimator.SetTrigger("Destroy");
    }

    public void OnDestroyEnd()
    {
        if (_isDestroyed) return;
        StartCoroutine(DestroyAfterParticles());
    }

    private IEnumerator DestroyAfterParticles()
    {
        _isDestroyed = true;
    
        if(explosion != null)
        {
            explosion.transform.SetParent(transform.parent);
            explosion.Play();
        }

        Destroy(gameObject); 

        if(explosion != null)
        {
            float duration = explosion.main.duration;
            yield return new WaitForSeconds(duration);
            Destroy(explosion.gameObject);
        }
    }

    public void PlayParticles()
    {
        if(_isDestroyed) return;
    
        if(spark != null && spark.isStopped) 
            spark.Play();
        if(smoke != null && smoke.isStopped)
            smoke.Play();
    }

    public void StopParticles()
    {
        if(spark != null) spark.Stop();
        if(smoke != null) smoke.Stop();
    }

    public void PlayExplosion()
    {
        if (_isDestroyed || explosion == null) return;

        explosion.Play();
    }
}
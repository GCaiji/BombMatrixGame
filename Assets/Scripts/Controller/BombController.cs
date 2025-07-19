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
    
    private RuntimeBombStats _runtimeBombStats; // 修改为运行时炸弹数据
    private Animator _bombAnimator;
    private float _timer;
    private bool _hasExploded;
    private bool _isDestroyed;

    void Start()
    {
        // 仅保留其他组件的初始化逻辑
    }

    public void Initialize(RuntimeBombStats runtimeStats) // 新的 Initialize 方法
    {
        if (runtimeStats == null)
        {
            Debug.LogError("RuntimeBombStats未初始化！");
            enabled = false;
            return;
        }

        _runtimeBombStats = runtimeStats; // 赋值运行时炸弹数据
        _timer = _runtimeBombStats.FuseTime; // 设置计时器

        // 在此处获取 Animator 组件，确保动画状态已设置完成
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

        _bombAnimator.Play("Ignite"); // 播放点燃动画
        PlayParticles(); // 播放粒子效果
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
        Debug.Log($"[BombController] 开始爆炸检测 - 位置: {transform.position}, 半径: {_runtimeBombStats.ExplosionRadius}");

        // 获取范围内物体
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _runtimeBombStats.ExplosionRadius,
            damageableLayers
        );

        Debug.Log($"[BombController] 检测到 {hits.Length} 个物体在爆炸范围内");

        Vector3 explosionPos = transform.position;
        float explosionRadius = _runtimeBombStats.ExplosionRadius;

        foreach (Collider hit in hits)
        {
            Debug.Log($"[BombController] 检测到物体: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");
            
            // 处理可破坏物体
            DestructibleTile destructibleTile = hit.GetComponent<DestructibleTile>();
            if (destructibleTile != null)
            {
                Debug.Log($"[BombController] 发现可破坏瓦片: {hit.gameObject.name}");
            }

            // 处理可受伤物体
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log($"[BombController] 对物体 {hit.gameObject.name} 造成伤害: {_runtimeBombStats.Damage}");
                damageable.TakeDamage(_runtimeBombStats.Damage);
            }
        }

        // 通知GroundManager处理地形破坏
        if (GroundManager.Instance != null)
        {
            Debug.Log($"[BombController] 通知GroundManager处理地形破坏");
            GroundManager.Instance.DestroyTilesInRadius(explosionPos, explosionRadius);
        }
        else
        {
            Debug.LogError("[BombController] GroundManager实例未找到！");
        }

        StartCoroutine(DestroyAndUpdateGround(explosionPos, explosionRadius));
    }

    private IEnumerator DestroyAndUpdateGround(Vector3 explosionPos, float explosionRadius)
    {
        // 开始销毁流程
        _isDestroyed = true;
        
        if(explosion != null)
        {
            explosion.transform.SetParent(transform.parent);
            explosion.Play();
        }

        // 销毁炸弹对象
        Destroy(gameObject);

        // 等待一帧确保对象被完全销毁
        yield return null;

        // 更新地形和导航网格
        GroundManager.Instance?.DestroyTilesInRadius(explosionPos, explosionRadius);

        // 处理剩余的爆炸特效
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
            Gizmos.DrawWireSphere(transform.position, _runtimeBombStats.ExplosionRadius); // 使用运行时数据
        }
    }
    #endif

    // 动画事件方法
    public void OnExplosionEnd()
    {
        if (_isDestroyed || _bombAnimator == null) 
        {
            Debug.LogWarning("尝试触发已销毁对象的动画事件");
            return;
        }
        _bombAnimator.SetTrigger("Destroy");
    }

    public void OnDestroyEnd()
    {
        if (_isDestroyed) return;
        Debug.Log($"触发销毁流程 - 实例ID: {gameObject.GetInstanceID()}");
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

    public void PlayExplosion() // 修正方法名大写
    {
        if (_isDestroyed || explosion == null) return;

        Debug.Log($"爆炸粒子状态 - 时长: {explosion.main.duration}秒");    
        explosion.Play();
    }
}

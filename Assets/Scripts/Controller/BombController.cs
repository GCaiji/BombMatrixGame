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
    
    private BombStats _bombStats;
    private Animator _bombAnimator;
    private float _timer;
    private bool _hasExploded;
    private bool _isDestroyed;

    void Start()
    {
        // 添加Animator组件获取
        _bombAnimator = GetComponent<Animator>();
        
        if (_bombStats == null)
        {
            Debug.LogError("BombStats未初始化！");
            enabled = false;
            return;
        }

        _timer = _bombStats.FuseTime;
        
        if (_bombAnimator != null)
            _bombAnimator.Play("Ignite");
        else
            Debug.LogError("缺少Animator组件");
    }

    void Update()
    {
        if (!_hasExploded && _bombStats != null)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                TriggerExplosion();
            }
        }
    }

    public void Initialize(BombStats stats)
    {
        _bombStats = stats;
    }

    private void Explode()
    {
        Debug.Log($"炸弹爆炸参数 - 伤害: {_bombStats.Damage} " +
                 $"半径: {_bombStats.ExplosionRadius} " +
                 $"引燃时间: {_bombStats.FuseTime}");

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _bombStats.ExplosionRadius,
            damageableLayers
        );

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            damageable?.TakeDamage(_bombStats.Damage);
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
        if (_bombStats != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _bombStats.ExplosionRadius);
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
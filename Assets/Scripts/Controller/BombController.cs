using UnityEngine;
using System.Collections; 

public class BombController : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem spark;
    [SerializeField] private ParticleSystem smoke;
    [SerializeField] private ParticleSystem explosion;
    
    public float fuseTime = 3f;  // 倒计时时间
    public Animator bombAnimator;

    private float timer;
    private bool hasExploded;
    private bool isDestroyed = false;

    void Start()
    {
        timer = fuseTime;
        bombAnimator.Play("Ignite");
    }

    void Update()
    {
        if (!hasExploded)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                TriggerExplosion();
            }
        }
    }

    public void TriggerExplosion()
    {
        if(isDestroyed) return;
    
        hasExploded = true;
        if(bombAnimator != null)
            bombAnimator.SetTrigger("Explode");
    }

    // 动画事件：爆炸结束后进入摧毁状态
    public void OnExplosionEnd()
    {
        if (isDestroyed || bombAnimator == null) 
        {
            Debug.LogWarning("尝试触发已销毁对象的动画事件");
            return;
        }
        bombAnimator.SetTrigger("Destroy");
    }

    // 动画事件：摧毁结束后销毁对象
    public void OnDestroyEnd()
    {
        if (isDestroyed) return;
        Debug.Log($"触发销毁流程 - 实例ID: {gameObject.GetInstanceID()}");
        StartCoroutine(DestroyAfterParticles());
    }
    // 修改BombController.cs
    private IEnumerator DestroyAfterParticles()
    {
        isDestroyed = true;
    
        // 分离爆炸粒子父级
        if(explosion != null)
        {
            explosion.transform.SetParent(transform.parent); // 使粒子独立于炸弹存在
            explosion.Play();
        }

        // 立即销毁炸弹对象
        Destroy(gameObject); 

        // 等待粒子播放完成
        if(explosion != null)
        {
            float duration = explosion.main.duration;
            yield return new WaitForSeconds(duration);
            Destroy(explosion.gameObject); // 最后销毁粒子
        }
    }
    public void PlayParticles()
    {
        if(isDestroyed) return; // 增加销毁状态检查
    
        if(spark != null && spark.isStopped) 
            spark.Play();
        if(smoke != null && smoke.isStopped)
            smoke.Play();
    }

    public void StopParticles()
    {
        spark.Stop();
        smoke.Stop();
    }
    public void playExplosion()
    {
        // 添加更详细的空引用检查
        if (isDestroyed || explosion == null)
        {
            //Debug.LogError("粒子系统引用丢失或对象已销毁");
            return;
        }

        // 检查粒子系统是否有效
        if (!explosion.IsAlive(true))
        {
            //Debug.LogError("粒子系统不可用");
            return;
        }

        // 打印关键参数
        Debug.Log($"爆炸粒子状态 - 时长: {explosion.main.duration}秒 | 循环: {explosion.main.loop} | 播放状态: {explosion.isPlaying}");
    
        explosion.Play();
    }
}
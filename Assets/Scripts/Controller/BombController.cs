using UnityEngine;

public class BombController : MonoBehaviour {
    public ParticleSystem spark; // 拖入SparkEffect
    public ParticleSystem smoke; // 拖入SmokeEffect

    // 动画事件调用：开始燃烧
    public void PlayPagitrticles() {
        spark.Play();
        smoke.Play();
    }

    // 动画事件调用：停止燃烧
    public void StopParticles() {
        spark.Stop();
        smoke.Stop();
    }
}
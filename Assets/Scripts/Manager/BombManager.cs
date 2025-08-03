using UnityEngine;
using System;

public class BombManager : MonoBehaviour
{
    public static BombManager Instance { get; private set; }
    
    // 爆炸事件 (位置, 半径)
    public static event Action<Vector3, float> OnExplosion;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    // 触发爆炸事件的方法
    public void TriggerExplosionEvent(Vector3 position, float radius)
    {
        OnExplosion?.Invoke(position, radius);
    }
}
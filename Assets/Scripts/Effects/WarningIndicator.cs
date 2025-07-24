using UnityEngine;

public class WarningIndicator : MonoBehaviour
{
    [Tooltip("预警圈缩放动画速度")]
    public float pulseSpeed = 2f;
    [Tooltip("最大缩放尺寸")]
    public float maxScale = 1.5f;
    [Tooltip("最小缩放尺寸")]
    public float minScale = 0.8f;

    private SpriteRenderer spriteRenderer;
    private float currentScale;
    private float scaleDirection = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // 如果没有SpriteRenderer组件，尝试添加
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 初始设置
        currentScale = minScale;
        transform.localScale = Vector3.one * currentScale;
    }

    private void Update()
    {
        // 缩放动画
        currentScale += scaleDirection * pulseSpeed * Time.deltaTime;
        
        // 反转方向
        if (currentScale >= maxScale)
        {
            currentScale = maxScale;
            scaleDirection = -1f;
        }
        else if (currentScale <= minScale)
        {
            currentScale = minScale;
            scaleDirection = 1f;
        }
        
        // 应用缩放
        transform.localScale = Vector3.one * currentScale;
        
        // 闪烁效果（可选）
        float alpha = Mathf.PingPong(Time.time * 2, 0.5f) + 0.5f;
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
using UnityEngine;

public class WarningIndicator : MonoBehaviour
{
    [Header("Shader 参数控制")]
    [Tooltip("初始进度 (1 = 满进度, 0 = 结束)")]
    public float startProgress = 1f;
    [Tooltip("总预警时间 (秒)")]
    public float warningDuration = 3f;
    [Tooltip("最大闪烁速度")]
    public float maxFlashSpeed = 10f;
    [Tooltip("最小闪烁速度")]
    public float minFlashSpeed = 2f;
    
    [Header("缩放动画设置")]
    [Tooltip("预警圈缩放动画速度")]
    public float pulseSpeed = 2f;
    [Tooltip("最大缩放尺寸")]
    public float maxScale = 1.5f;
    [Tooltip("最小缩放尺寸")]
    public float minScale = 0.8f;
    [Tooltip("是否启用缩放动画")]
    public bool enablePulsing = true;

    private Material warningMaterial;
    private float currentProgress;
    private float currentScale;
    private float scaleDirection = 1f;
    private float elapsedTime;

    private void Awake()
    {
        // 获取材质实例
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            warningMaterial = Application.isPlaying ? 
                renderer.material : renderer.sharedMaterial;
        }
        else
        {
            Debug.LogError("警告指示器没有渲染器组件！");
            enabled = false;
            return;
        }
        
        // 初始化材质参数
        currentProgress = startProgress;
        warningMaterial.SetFloat("_Progress", currentProgress);
        
        // 初始化缩放
        currentScale = enablePulsing ? minScale : 1f;
        transform.localScale = Vector3.one * currentScale;
        
        // 初始化闪烁速度
        UpdateFlashSpeed();
    }

    private void Update()
    {
        // 更新倒计时进度
        elapsedTime += Time.deltaTime;
        currentProgress = Mathf.Clamp01(1 - (elapsedTime / warningDuration));
        warningMaterial.SetFloat("_Progress", currentProgress);
        
        // 更新闪烁速度 (随时间加快)
        UpdateFlashSpeed();
        
        // 缩放动画
        if (enablePulsing)
        {
            PulseAnimation();
        }
        
        // 结束检测
        if (elapsedTime >= warningDuration)
        {
            HandleWarningComplete();
        }
    }
    
    private void UpdateFlashSpeed()
    {
        // 根据进度调整闪烁速度 (进度越小速度越快)
        float flashSpeed = Mathf.Lerp(maxFlashSpeed, minFlashSpeed, currentProgress);
        warningMaterial.SetFloat("_FlashSpeed", flashSpeed);
    }
    
    private void PulseAnimation()
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
    }
    
    private void HandleWarningComplete()
    {
        // 这里添加警告完成后的逻辑
        Debug.Log("预警结束！");
        
        // 示例：完成后延迟销毁
        Destroy(gameObject, 0.5f);
        
        // 禁用脚本防止进一步更新
        enabled = false;
    }
    
    // 用于外部重置或设置进度
    public void SetProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        warningMaterial.SetFloat("_Progress", currentProgress);
        elapsedTime = warningDuration * (1 - progress);
    }
    
    // 重置倒计时
    public void ResetWarning()
    {
        elapsedTime = 0f;
        currentProgress = startProgress;
        warningMaterial.SetFloat("_Progress", currentProgress);
        enabled = true;
    }
    
    private void OnDestroy()
    {
        // 确保在销毁时清理材质实例
        if (Application.isPlaying && warningMaterial != null)
        {
            Destroy(warningMaterial);
        }
    }
}
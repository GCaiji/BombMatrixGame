using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class InvincibilityController : MonoBehaviour
{
    [Header("闪烁效果设置")]
    public float flashSpeed = 10f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.8f;
    public bool useFlashColor = false;
    public Color flashColor = new Color(1f, 0.5f, 0.5f, 1f);
    
    private Renderer targetRenderer;
    private MaterialPropertyBlock propertyBlock;
    private ActorController actorController;
    private RuntimeCharacterStats runtimeStats;
    
    void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        
        actorController = GetComponentInParent<ActorController>();
        if (actorController == null)
        {
            Debug.LogError("ActorController not found in parent hierarchy!");
            enabled = false;
            return;
        }
    }

    void Start()
    {
        StartCoroutine(InitializeRuntimeStats());
    }

    private IEnumerator InitializeRuntimeStats()
    {
        int maxAttempts = 3;
        int currentAttempt = 0;
        float retryDelay = 0.1f;

        while (currentAttempt < maxAttempts)
        {
            runtimeStats = actorController.RuntimeStats;
            if (runtimeStats != null)
            {
                break;
            }

            Debug.LogWarning($"等待RuntimeStats初始化，尝试次数：{currentAttempt + 1}/{maxAttempts}");
            yield return new WaitForSeconds(retryDelay);
            currentAttempt++;
        }

        if (runtimeStats == null)
        {
            Debug.LogError("无法获取RuntimeStats，组件将被禁用");
            enabled = false;
        }
    }
    
    void Update()
    {
        UpdateInvincibilityEffect();
    }
    
    private void UpdateInvincibilityEffect()
    {
        if (runtimeStats == null) return;
        
        targetRenderer.GetPropertyBlock(propertyBlock);
        float currentInvincible = propertyBlock.GetFloat("_Invincible");
        
        if (runtimeStats.IsInvincible)
        {
            if (currentInvincible != 1f)
            {
                propertyBlock.SetFloat("_Invincible", 1f);
                propertyBlock.SetFloat("_FlashSpeed", flashSpeed);
                propertyBlock.SetFloat("_MinAlpha", minAlpha);
                propertyBlock.SetFloat("_MaxAlpha", maxAlpha);
                
                if (useFlashColor)
                {
                    propertyBlock.SetFloat("_UseCustomColor", 1f);
                    propertyBlock.SetColor("_FlashColor", flashColor);
                }
                else
                {
                    propertyBlock.SetFloat("_UseCustomColor", 0f);
                }
                
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }
        else if (currentInvincible != 0f)
        {
            propertyBlock.SetFloat("_Invincible", 0f);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}

using UnityEngine;

public class MonsterController : MonoBehaviour
{
    // 添加落地事件
    public event System.Action OnMonsterLanded;
    
    // 使用属性确保安全访问
    private RuntimeMonsterStats _stats;
    public RuntimeMonsterStats Stats {
        get => _stats;
        private set {
            _stats = value;
            // 确保Stats不为空
            if (_stats == null)
            {
                Debug.LogWarning($"MonsterController.Stats设置为空 ({gameObject.name})");
            }
        }
    }
    
    [Header("掉落设置")]
    public float fallGravity = 9.8f;
    public float bounceForce = 1f;
    public LayerMask groundLayer;
    
    private Rigidbody rb;
    private Vector3 targetLandPosition;
    private bool isFalling = false;
    private bool hasLanded = false;
    private bool isGrounded = false;
    private Collider monsterCollider;

    private void Awake()
    {
        // 确保标签
        gameObject.tag = "Monster";
        
        // 获取组件
        rb = GetComponent<Rigidbody>();
        monsterCollider = GetComponent<Collider>();
        
        // 自动添加Rigidbody（如果缺失）
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.LogWarning($"为 {gameObject.name} 自动添加了 Rigidbody 组件");
        }
        
        // 初始化刚体设置
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        // 初始禁用碰撞器
        if (monsterCollider != null)
            monsterCollider.enabled = false;
        else
            Debug.LogWarning($"怪物缺少碰撞器组件 ({gameObject.name})");
    }
    
    public void Initialize(RuntimeMonsterStats stats)
    {
        this.Stats = stats;
    }
    
    public void StartFalling(Vector3 targetPosition, RuntimeMonsterStats stats)
    {
        // 设置运行时状态
        this.Stats = stats;
        
        targetLandPosition = targetPosition;
        isFalling = true;
        hasLanded = false;
        isGrounded = false;
        
        // 启用碰撞器
        if (monsterCollider != null)
            monsterCollider.enabled = true;
    }

    private void FixedUpdate()
    {
        if (isFalling && !hasLanded)
        {
            // 安全访问Rigidbody
            if (rb != null)
            {
                rb.velocity += Vector3.down * fallGravity * Time.fixedDeltaTime;
            }
            
            CheckGrounded();
            if (isGrounded)
            {
                Land();
            }
        }
    }

    private void CheckGrounded()
    {
        float checkDistance = 0.3f;
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGrounded = Physics.Raycast(ray, out RaycastHit hit, checkDistance, groundLayer);
        
        if (isGrounded)
        {
            targetLandPosition = hit.point;
        }
    }

    private void Land()
    {
        hasLanded = true;
        isFalling = false;
        
        // 设置位置
        transform.position = new Vector3(
            targetLandPosition.x,
            targetLandPosition.y + 0.01f,
            targetLandPosition.z
        );
        
        // 安全访问Rigidbody
        if (rb != null)
        {
            // 弹跳逻辑
            rb.velocity = Vector3.zero;
            if (bounceForce > 0)
            {
                rb.velocity = Vector3.up * bounceForce;
                rb.useGravity = true;
            }
            else
            {
                FreezeRigidbody();
            }
        }
        
        // 触发落地事件
        OnMonsterLanded?.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded && !isFalling && (groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            FreezeRigidbody();
        }
    }

    private void FreezeRigidbody()
    {
        // 安全访问Rigidbody
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
    
    // 返回对象池方法
    public void ReturnToPool()
    {
        // 重置状态
        isFalling = false;
        hasLanded = false;
        isGrounded = false;
        
        // 重置刚体
        FreezeRigidbody();
        
        // 禁用碰撞器
        if (monsterCollider != null)
            monsterCollider.enabled = false;
        
        // 清除所有事件订阅者
        OnMonsterLanded = null;
        
        // 通知生成管理器
        if (MonsterSpawnManager.Instance != null)
        {
            MonsterSpawnManager.Instance.ReturnMonsterToPool(gameObject);
        }
        else
        {
            Debug.LogWarning("MonsterSpawnManager 实例为空，直接销毁怪物");
            Destroy(gameObject);
        }
    }
}
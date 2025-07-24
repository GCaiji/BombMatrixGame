using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("掉落设置")]
    [Tooltip("掉落时的重力")]
    public float fallGravity = 9.8f;
    [Tooltip("落地时的弹跳力")]
    public float bounceForce = 1f;
    [Tooltip("地面检测层级（需在Layers中设置Ground层）")]
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Vector3 targetLandPosition;
    private bool isFalling = false;
    private bool hasLanded = false;
    private bool isGrounded = false; // 新增：检测是否接触地面

    private void Awake()
    {
        // 确保Rigidbody组件存在且不为空
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        // 初始化刚体设置
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        // 移除可能导致冲突的动画组件（如果存在）
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            Destroy(anim); // 避免未配置的动画器导致空引用
        }
    }

    public void StartFalling(Vector3 targetPosition, System.Action<GameObject> returnCallback)
    {
        targetLandPosition = targetPosition;
        isFalling = true;
        hasLanded = false;
        isGrounded = false;
        
        // 启用碰撞器
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;
    }

    private void FixedUpdate()
    {
        if (isFalling && !hasLanded)
        {
            // 应用下落重力
            rb.velocity += Vector3.down * fallGravity * Time.fixedDeltaTime;
            
            // 检测是否接触地面（使用射线检测更可靠）
            CheckGrounded();
            if (isGrounded)
            {
                Land();
            }
        }
    }

    // 射线检测：判断是否接触地面
    private void CheckGrounded()
    {
        // 从怪物位置向下发射短射线检测地面
        float checkDistance = 0.3f; // 检测距离（根据怪物碰撞体大小调整）
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGrounded = Physics.Raycast(ray, out RaycastHit hit, checkDistance, groundLayer);
        
        // 如果检测到地面，更新目标落地位置为地面接触点
        if (isGrounded)
        {
            targetLandPosition = hit.point;
        }
    }

    private void Land()
    {
        hasLanded = true;
        isFalling = false;
        
        // 强制设置位置到地面接触点
        transform.position = new Vector3(
            targetLandPosition.x,
            targetLandPosition.y + 0.01f, // 轻微抬高避免与地面穿插
            targetLandPosition.z
        );
        
        // 清除速度，应用弹跳
        rb.velocity = Vector3.zero;
        if (bounceForce > 0)
        {
            rb.velocity = Vector3.up * bounceForce;
            // 弹跳后重新启用重力，让其再次下落
            rb.useGravity = true;
        }
        else
        {
            FreezeRigidbody();
        }
        
        OnLanded();
    }

    // 碰撞事件：当弹跳后再次落地时冻结
    private void OnCollisionEnter(Collision collision)
    {
        // 仅在弹跳后检测到地面碰撞时冻结
        if (hasLanded && !isFalling && (groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            FreezeRigidbody();
        }
    }

    private void FreezeRigidbody()
    {
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll; // 完全冻结
    }

    protected virtual void OnLanded()
    {
        Debug.Log($"怪物已落地：{transform.position}");
    }
}

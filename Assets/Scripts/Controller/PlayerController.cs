using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private float runSpeed = 5.5f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject destinationMarker;
    [SerializeField] private ParticleSystem clickEffect;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float stopThreshold = 0.1f;
    [SerializeField] private float speedSmoothTime = 0.08f;  // 缩短默认平滑时间
    [SerializeField] private float speedTriggerThreshold = 0.3f;

    [Header("Animation Optimization")]
    [SerializeField] public float speedBufferTime = 0.1f;  // 从0.5改为0.1秒
    private float speedBufferTimer;
    
    private Camera mainCamera;
    private float currentSpeed;
    private float speedSmoothVelocity;

    void Awake()
    {
        mainCamera = Camera.main;
        InitializeComponents();
    }

    void Update()
    {
        HandleMovementInput();
        UpdateDestinationMarker();
        UpdateAnimationState();
        HandleRotation();
    }

    private void InitializeComponents()
    {  
        destinationMarker.SetActive(false);
        agent.speed = runSpeed;
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsMoving", false);  // 初始化状态
    }

    private void HandleMovementInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (RaycastGround(out Vector3 hitPoint))
            {
                agent.SetDestination(hitPoint);
                PlayClickEffect(hitPoint);
            }
        }
        else if (Input.GetKeyDown(KeyCode.S)) // 示例：按S键急停
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    private bool RaycastGround(out Vector3 hitPoint)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayer))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                hitPoint = navHit.position + Vector3.up * 0.1f;
                return true;
            }
        }
        hitPoint = Vector3.zero;
        return false;
    }

    private void PlayClickEffect(Vector3 position)
    {
        if (clickEffect != null)
        {
            clickEffect.transform.position = position;
            clickEffect.Play();
        }
    }

    private void UpdateDestinationMarker()
    {
        bool shouldShow = agent.remainingDistance > stopThreshold && !agent.pathPending;
        destinationMarker.SetActive(shouldShow);
        if (shouldShow) destinationMarker.transform.position = agent.destination;
    }

    private void UpdateAnimationState()
    {
        // 获取导航系统状态
        bool isAgentStopped = agent.isStopped || agent.remainingDistance <= stopThreshold;
    
        // 优化1：使用更精准的速度判断逻辑
        float actualSpeed = isAgentStopped ? 0 : agent.velocity.magnitude;
        bool isActuallyMoving = actualSpeed > speedTriggerThreshold;

        // 优化2：分层缓冲控制
        if (isActuallyMoving)
        {
            // 移动时立即重置计时器（无缓冲）
            speedBufferTimer = 0;
            animator.SetBool("IsMoving", true);
        }
        else
        {
            // 停止时启动缓冲倒计时
            speedBufferTimer += Time.deltaTime;
            if (speedBufferTimer >= speedBufferTime)
            {
                animator.SetBool("IsMoving", false);
            }
        }

        // 优化3：动态速度计算
        float targetSpeed = animator.GetBool("IsMoving") ? 
            Mathf.Clamp01(actualSpeed / runSpeed) : 
            0f;

        currentSpeed = Mathf.SmoothDamp(
            currentSpeed, 
            targetSpeed, 
            ref speedSmoothVelocity, 
            speedSmoothTime
        );
    
        animator.SetFloat("Speed", currentSpeed);
    }

    private void HandleRotation()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }
}
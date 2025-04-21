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
    [SerializeField] private float speedSmoothTime = 0.1f;
    [SerializeField] private float speedTriggerThreshold = 0.3f; // 新增速度触发阈值
    
    [Header("Animation Optimization")]
    [SerializeField] private float speedBufferTime = 0.5f; // 状态保持时间
    private float speedBufferTimer;
    
    private Camera mainCamera;
    private float currentSpeed;
    private float speedSmoothVelocity;
    private bool isMoving; // 新增移动状态跟踪

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
        animator.ResetTrigger("Run"); // 初始化触发器状态
        animator.ResetTrigger("Idle");
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
        float actualSpeed = agent.velocity.magnitude;
        bool isActuallyMoving = actualSpeed > speedTriggerThreshold;

        // 速度缓冲逻辑
        if (isActuallyMoving)
        {
            speedBufferTimer = speedBufferTime; // 重置计时器
        }
        else
        {
            speedBufferTimer -= Time.deltaTime;
        }

        // 有效移动状态 = 当前移动或缓冲期内
        bool validMovingState = speedBufferTimer > 0;

        // 状态切换逻辑
        if (!isMoving && validMovingState)
        {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Run");
            isMoving = true;
        }
        else if (isMoving && !validMovingState)
        {
            animator.ResetTrigger("Run");
            animator.SetTrigger("Idle");
            isMoving = false;
        }
    
        // 平滑速度参数（即使实际速度为0）
        float targetSpeed = validMovingState ? actualSpeed / runSpeed : 0;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);
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
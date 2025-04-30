using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject destinationMarker;
    [SerializeField] private ParticleSystem clickEffect;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float stopThreshold = 0.1f;
    [SerializeField] private float speedSmoothTime = 0.08f;
    [SerializeField] private float speedTriggerThreshold = 0.3f;
    [SerializeField] private float speedBufferTime = 0.1f;

    [Header("Bomb Settings")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private BombStats bombStats;
    [SerializeField] private int maxBombs = 3;
    [SerializeField] private Transform bombContainer;
    
    [Header("Character Reference")]
    [SerializeField] private CharacterController characterController;
    
    
    private List<GameObject> activeBombs = new List<GameObject>();
    private Camera mainCamera;
    private float currentSpeed;
    private float speedSmoothVelocity;
    private float speedBufferTimer;

    void Awake()
    {
        mainCamera = Camera.main;
        InitializeComponents();
        agent.speed = characterController.MoveSpeed;
    }

    void Update()
    {
        HandleMovementInput();
        UpdateDestinationMarker();
        UpdateAnimationState();
        HandleRotation();
        HandleBombPlacement();
    }

    private void InitializeComponents()
    {  
        destinationMarker.SetActive(false);
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsMoving", false);
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
        float characterRunSpeed = characterController.MoveSpeed;
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
            Mathf.Clamp01(actualSpeed / characterRunSpeed) : 
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
    private void HandleBombPlacement()
    {
        // 清理已销毁的炸弹引用
        activeBombs.RemoveAll(b => b == null);

        if (Input.GetMouseButtonDown(0) && activeBombs.Count < maxBombs)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.y = 0;
        
            GameObject newBomb = Instantiate(
                bombPrefab, 
                spawnPos, 
                Quaternion.identity, 
                bombContainer  // 添加父容器参数
            );
            
            BombController bombController = newBomb.GetComponent<BombController>();
            if(bombController != null)
            {
                bombController.Initialize(bombStats); // 传入配置参数
            }
            
            activeBombs.Add(newBomb);

            // 获取回调的正确方式
            Animator bombAnimator = newBomb.GetComponent<Animator>();
            if(bombAnimator != null)
            {
                StateMachineBehaviour[] behaviours = bombAnimator.GetBehaviours<BombDestroyCallback>();
                if(behaviours.Length > 0)
                {
                    BombDestroyCallback callback = (BombDestroyCallback)behaviours[0];
                    callback.onDestroyComplete = () => 
                    {
                        // 安全移除
                        if(activeBombs.Contains(newBomb))
                            activeBombs.Remove(newBomb);
                    };
                }
            }
        }
    }
}
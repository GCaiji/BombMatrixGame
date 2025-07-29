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
    [SerializeField] private Transform bombContainer;

    [Header("Character Reference")]
    [SerializeField] private ActorController characterController;
   
    private List<GameObject> activeBombs = new List<GameObject>();
    private Camera mainCamera;
    private float currentSpeed;
    private float speedSmoothVelocity;
    private float speedBufferTimer;
    private bool isEmergencyStop = false;  // 新增急停状态标志位

    void Awake()
    {
        mainCamera = Camera.main;
        InitializeComponents();
    }

    void Update()
    {
        HandleEmergencyStop();  
        HandleMovementInput();
        UpdateDestinationMarker();
        UpdateAnimationState();
        HandleRotation();
        HandleBombPlacement();
        HandleBombRadiusIncrease(); // 新增：处理炸弹范围增加
    }

    void Start()
    {
        // 改为在Start中进行检查
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager 实例未找到！");
            enabled = false;
            return;
        }

        // 移动速度设置也移到Start
        agent.speed = characterController.Stats.MoveSpeed;
    }

    private void InitializeComponents()
    {  
        destinationMarker.SetActive(false);
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsMoving", false);
    }

    private void HandleEmergencyStop()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            isEmergencyStop = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            isEmergencyStop = false;
        }
    }

    private void HandleMovementInput()
    {
        if (isEmergencyStop) return;  // 如果处于急停状态，禁止移动操作

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
        // 获取导航系统状态
        bool isAgentStopped = agent.isStopped || agent.remainingDistance <= stopThreshold;
    
        // 优化1：使用更精准的速度判断逻辑
        float actualSpeed = isAgentStopped ? 0 : agent.velocity.magnitude;
        bool isActuallyMoving = actualSpeed > speedTriggerThreshold;
        float characterRunSpeed =agent.speed;
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

        // 检查是否按下左键或A键
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.A))
        {
            if (activeBombs.Count >= characterController.Stats.MaxBombs)
            {
                Debug.Log($"炸弹数量已达上限 (当前时间戳: {Time.time:F2})");
                return;
            }

            Debug.Log($"放置炸弹 (当前时间戳: {Time.time:F2})");

            Vector3 spawnPos = transform.position;
            spawnPos.y = 0;

            GameObject newBomb = Instantiate(
                bombPrefab, 
                spawnPos, 
                Quaternion.identity, 
                bombContainer
            );

            BombController bombController = newBomb.GetComponent<BombController>();
            if (bombController != null)
            {
                bombController.Initialize(GameManager.Instance.CurrentBombStats.Clone());
            }

            activeBombs.Add(newBomb);

            Animator bombAnimator = newBomb.GetComponent<Animator>();
            if (bombAnimator != null)
            {
                StateMachineBehaviour[] behaviours = bombAnimator.GetBehaviours<BombDestroyCallback>();
                if (behaviours.Length > 0)
                {
                    BombDestroyCallback callback = (BombDestroyCallback)behaviours[0];
                    callback.onDestroyComplete = () =>
                    {
                        if (activeBombs.Contains(newBomb))
                            activeBombs.Remove(newBomb);
                    };
                }
            }
        }
    }

    private void HandleBombRadiusIncrease()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentBombStats != null)
            {
                GameManager.Instance.CurrentBombStats.IncreaseExplosionRadius(0.5f);
            }
        }
    }
}
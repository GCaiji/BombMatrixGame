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
    private bool isEmergencyStop = false;
    private bool wasActionDisabled = false;

    void Awake()
    {
        mainCamera = Camera.main;
        InitializeComponents();
    }

    void Update()
    {
        if (characterController == null) return;
        
        // 检查是否可行动
        bool isActionDisabled = characterController.IsActionDisabled;
        
        // 状态变化检测
        if (wasActionDisabled && !isActionDisabled)
        {
            // 状态恢复时重置代理
            agent.isStopped = false;
            Debug.Log("角色行动恢复，重置NavMeshAgent");
        }
        
        wasActionDisabled = isActionDisabled;
        
        if (isActionDisabled)
        {
            StopMovement();
            return;
        }

        HandleEmergencyStop();  
        HandleMovementInput();
        UpdateDestinationMarker();
        UpdateAnimationState();
        HandleRotation();
        HandleBombPlacement();
        HandleBombRadiusIncrease();
    }

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager 实例未找到！");
            enabled = false;
            return;
        }

        if (characterController == null)
        {
            Debug.LogError("ActorController未赋值！");
            enabled = false;
            return;
        }

        agent.speed = characterController.RuntimeStats.MoveSpeed;
    }

    private void InitializeComponents()
    {  
        if (destinationMarker != null) destinationMarker.SetActive(false);
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsMoving", false);
        }
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
        if (isEmergencyStop || characterController.IsActionDisabled)
        {
            return;
        }

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
        if (destinationMarker == null) return;
        
        bool shouldShow = agent.remainingDistance > stopThreshold && !agent.pathPending;
        destinationMarker.SetActive(shouldShow);
        if (shouldShow) destinationMarker.transform.position = agent.destination;
    }

    private void StopMovement()
    {
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetFloat("Speed", 0f);
        }
        
        currentSpeed = 0f;
        speedBufferTimer = 0f;
        
        if (destinationMarker != null) destinationMarker.SetActive(false);
    }

    private void UpdateAnimationState()
    {
        if (characterController.IsActionDisabled)
        {
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetFloat("Speed", 0f);
            }
            return;
        }

        bool isAgentStopped = agent.isStopped || agent.remainingDistance <= stopThreshold;
        float actualSpeed = isAgentStopped ? 0 : agent.velocity.magnitude;
        bool isActuallyMoving = actualSpeed > speedTriggerThreshold;
        float characterRunSpeed = agent.speed;

        if (isActuallyMoving)
        {
            speedBufferTimer = 0;
            if (animator != null) animator.SetBool("IsMoving", true);
        }
        else
        {
            speedBufferTimer += Time.deltaTime;
            if (speedBufferTimer >= speedBufferTime && animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }

        float targetSpeed = (animator != null && animator.GetBool("IsMoving")) ? 
            Mathf.Clamp01(actualSpeed / characterRunSpeed) : 
            0f;

        currentSpeed = Mathf.SmoothDamp(
            currentSpeed, 
            targetSpeed, 
            ref speedSmoothVelocity, 
            speedSmoothTime
        );
    
        if (animator != null) animator.SetFloat("Speed", currentSpeed);
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
        activeBombs.RemoveAll(b => b == null);
        
        // 检查是否可行动
        if (characterController.IsActionDisabled)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.A))
        {
            if (activeBombs.Count >= characterController.BaseStats.MaxBombs)
            {
                return;
            }

            Vector3 spawnPos = transform.position;
            spawnPos.y = 0;

            GameObject newBomb = Instantiate(
                bombPrefab, 
                spawnPos, 
                Quaternion.identity, 
                bombContainer
            );

            BombController bombController = newBomb.GetComponent<BombController>();
            if (bombController != null && GameManager.Instance != null)
            {
                bombController.Initialize(GameManager.Instance.CurrentBombStats.Clone());
            }

            activeBombs.Add(newBomb);
        }
    }

    private void HandleBombRadiusIncrease()
    {
        if (characterController.IsActionDisabled) return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentBombStats != null)
            {
                GameManager.Instance.CurrentBombStats.IncreaseExplosionRadius(0.5f);
            }
        }
    }
}
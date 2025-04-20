using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class PlayerController : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 100f;

    [Header("Visual")]
    [SerializeField] private GameObject destinationMarker;
    [SerializeField] private ParticleSystem clickEffect;

    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        destinationMarker.SetActive(false);
    }

    void Update()
    {
        HandleMovementInput();
        UpdateDestinationMarker();
    }

    private void HandleMovementInput()
    {
        if (Input.GetMouseButtonDown(1)) // 右键点击
        {
            if (RaycastGround(out Vector3 hitPoint))
            {
                agent.SetDestination(hitPoint);
                if (clickEffect != null) 
                {
                    clickEffect.transform.position = hitPoint + Vector3.up * 0.1f;
                    clickEffect.Play();
                }
            }
        }
    }

    private bool RaycastGround(out Vector3 hitPoint)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayer))
        {
            // 确保目标点在双层地板的可行走区域
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                hitPoint = navHit.position;
                return true;
            }
        }
        hitPoint = Vector3.zero;
        return false;
    }

    private void UpdateDestinationMarker()
    {
        if (agent.pathPending || agent.remainingDistance < 0.1f)
        {
            destinationMarker.SetActive(false);
            return;
        }

        destinationMarker.transform.position = agent.destination;
        destinationMarker.SetActive(true);
    }
}

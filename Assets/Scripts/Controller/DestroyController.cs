using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshSurface))]
public class DestoryController : MonoBehaviour
{
    [SerializeField] private float destroyHeight = -0.5f;
    [SerializeField] private float destroyDuration = 1f;
    
    private NavMeshSurface surface;
    private Vector3 originalPosition;

    void Awake()
    {
        surface = GetComponent<NavMeshSurface>();
        originalPosition = transform.position;
    }

    public void DestroySection(Vector3 explosionPos, float radius)
    {
        // 模拟地板破坏效果
        foreach (Transform child in transform)
        {
            if (Vector3.Distance(child.position, explosionPos) < radius)
            {
                StartCoroutine(DestroyTile(child.gameObject));
            }
        }

        // 更新所有导航网格
        NavMeshBaker navMeshBaker = FindObjectOfType<NavMeshBaker>();
        if (navMeshBaker != null)
        {
            navMeshBaker.BakeNavMesh();
        }
    }

    private System.Collections.IEnumerator DestroyTile(GameObject tile)
    {
        float elapsed = 0;
        Vector3 startPos = tile.transform.position;

        while (elapsed < destroyDuration)
        {
            tile.transform.position = Vector3.Lerp(
                startPos, 
                originalPosition + Vector3.up * destroyHeight, 
                elapsed / destroyDuration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        tile.SetActive(false);
    }
}

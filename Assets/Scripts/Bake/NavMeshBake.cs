using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshBaker : MonoBehaviour
{
    [SerializeField] private NavMeshSurface[] surfaces;

    void Start()
    {
        BakeNavMesh();
    }

    public void BakeNavMesh()
    {
        foreach (var surface in surfaces)
        {
            if (surface != null)
            {
                surface.BuildNavMesh();
            }
        }
    }
}

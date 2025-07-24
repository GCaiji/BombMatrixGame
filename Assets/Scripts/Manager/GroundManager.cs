using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshSurface))]
public class GroundManager : MonoBehaviour
{
    public static GroundManager Instance { get; private set; }
    
    private NavMeshSurface _navMeshSurface;
    private readonly List<DestructibleTile> _allTiles = new();
    private int _initialTileCount;
    private int _destroyedTiles;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        
        _navMeshSurface = GetComponent<NavMeshSurface>();
        
        // 配置NavMeshSurface
        _navMeshSurface.collectObjects = CollectObjects.Volume; // 只收集Volume内的对象
        _navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders; // 使用碰撞体而不是网格
        
        // 初始化所有可破坏瓦片
        foreach (Transform child in transform)
        {
            var tile = child.gameObject.AddComponent<DestructibleTile>();
            _allTiles.Add(tile);
        }
        
        _initialTileCount = _allTiles.Count;
        _destroyedTiles = 0;
        BakeNavMesh();
    }

    public void DestroyTilesInRadius(Vector3 explosionPos, float radius)
    {
        // Debug.Log($"[GroundManager] 开始处理范围内地形破坏 - 中心点: {explosionPos}, 半径: {radius}");
        // Debug.Log($"[GroundManager] 当前总瓦片数: {_allTiles.Count}, 已销毁: {_destroyedTiles}");

        bool destroyedAny = false;
        int tilesDestroyed = 0;
        
        foreach (var tile in _allTiles)
        {
            if (tile == null)
            {
                // Debug.LogWarning("[GroundManager] 发现空瓦片引用");
                continue;
            }

            if (tile.IsDestroyed)
            {
                //Debug.Log($"[GroundManager] 瓦片已被销毁: {tile.gameObject.name}");
                continue;
            }
            
            float distance = Vector3.Distance(tile.transform.position, explosionPos);
            if (distance <= radius)
            {
                // Debug.Log($"[GroundManager] 销毁瓦片: {tile.gameObject.name}, 距离: {distance}");
                tile.DestroyTile();
                _destroyedTiles++;
                tilesDestroyed++;
                destroyedAny = true;
            }
        }
        
        // Debug.Log($"[GroundManager] 本次爆炸共销毁 {tilesDestroyed} 个瓦片");

        if (destroyedAny)
        {
            GameManager.Instance?.UpdateDestructionProgress(_destroyedTiles, _initialTileCount);
            StartCoroutine(DelayedNavMeshBake());
        }
    }

    private IEnumerator DelayedNavMeshBake()
    {
        yield return null;
        BakeNavMesh();
    }

    public void BakeNavMesh()
    {
        if (_navMeshSurface != null) 
            _navMeshSurface.BuildNavMesh();
    }
}

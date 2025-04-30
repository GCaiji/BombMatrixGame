using UnityEngine;

[CreateAssetMenu(menuName = "Game/Map Stats")]
public class MapStats : ScriptableObject
{
    [Header("Game Rules")]
    [SerializeField] [Range(0.5f, 1f)] private float destructionGoal = 0.8f;
    [SerializeField] [Range(60f, 300f)] private float timeLimit = 180f;
    
    [Header("Tile Settings")]
    [SerializeField] private Vector2 tileSize = Vector2.one;

    public float DestructionGoal => Mathf.Clamp(destructionGoal, 0.5f, 1f);
    public float TimeLimit => Mathf.Clamp(timeLimit, 60f, 300f);
    public Vector2 TileSize => tileSize;
}

using UnityEditor;
using UnityEngine;

public class TileGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Destructible Tiles")]
    private static void ShowWindow() => GetWindow<TileGenerator>("瓦片生成器");

    public int width = 5;
    public int height = 5;
    public float tileSize = 1f;
    public float yPosition = 0.01f;
    public Material tileMaterial;
    public bool showGizmos = true; // 显示辅助线

    void OnGUI()
    {
        EditorGUILayout.LabelField("瓦片设置", EditorStyles.boldLabel);
        width = Mathf.Max(1, EditorGUILayout.IntField("横向数量", width));
        height = Mathf.Max(1, EditorGUILayout.IntField("纵向数量", height));
        tileSize = Mathf.Max(0.1f, EditorGUILayout.FloatField("瓦片大小", tileSize));
        yPosition = EditorGUILayout.FloatField("Y轴位置", yPosition);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("材质设置", EditorStyles.boldLabel);
        tileMaterial = (Material)EditorGUILayout.ObjectField("瓦片材质", tileMaterial, typeof(Material), false);
        
        showGizmos = EditorGUILayout.Toggle("显示辅助线", showGizmos);

        EditorGUILayout.Space();
        if (GUILayout.Button("生成瓦片", GUILayout.Height(30)))
        {
            GenerateTiles();
        }

        EditorGUILayout.HelpBox(
            "如果瓦片不可见，请检查：\n" +
            "1. 摄像机是否对准瓦片区域\n" +
            "2. 材质是否正确设置\n" +
            "3. 场景中是否有其他物体遮挡", 
            MessageType.Warning
        );
    }

    private void GenerateTiles()
    {
        // 清除已有的瓦片父物体
        var existing = GameObject.Find("DestructibleLayer");
        if (existing != null)
            DestroyImmediate(existing);

        // 创建新的父物体
        GameObject destructibleLayer = new GameObject("DestructibleLayer");
        destructibleLayer.transform.position = new Vector3(
            (width * tileSize) / 2 - tileSize / 2,
            yPosition, 
            (height * tileSize) / 2 - tileSize / 2
        );

        // 生成瓦片
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject tile = new GameObject($"Tile_{x}_{z}");
                tile.transform.parent = destructibleLayer.transform;
                tile.transform.localPosition = new Vector3(x * tileSize, 0, z * tileSize);

                // 添加网格和渲染器组件
                MeshFilter meshFilter = tile.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = tile.AddComponent<MeshRenderer>();
                meshFilter.mesh = CreateTileMesh(x, z, tileSize, width, height);
                meshRenderer.material = EnsureValidMaterial();

                // 添加碰撞体
                BoxCollider collider = tile.AddComponent<BoxCollider>();
                collider.size = new Vector3(tileSize, 0.01f, tileSize);
                collider.center = new Vector3(tileSize / 2, 0, tileSize / 2);

                // 添加破坏脚本
                tile.AddComponent<DestructibleTile>();
            }
        }

        // 选中生成的物体
        Selection.activeObject = destructibleLayer;
        // 聚焦到物体
        SceneView.FrameLastActiveSceneView();

        Debug.Log($"生成完成：{width * height}个瓦片，位置：{destructibleLayer.transform.position}");
    }

    private Material EnsureValidMaterial()
    {
        if (tileMaterial != null)
            return tileMaterial;

        // 创建高可见度的默认材质
        Material defaultMat = new Material(Shader.Find("Unlit/Color"));
        defaultMat.color = new Color(0.8f, 0.4f, 0.4f); // 亮红色，确保可见
        defaultMat.name = "DefaultVisibleTileMaterial";
        return defaultMat;
    }

    private Mesh CreateTileMesh(int tileX, int tileZ, float tileSize, int totalWidth, int totalHeight)
    {
        Mesh mesh = new Mesh();
        
        // 顶点
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(tileSize, 0, 0),
            new Vector3(0, 0, tileSize),
            new Vector3(tileSize, 0, tileSize)
        };
        
        // 三角形
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        // UV坐标
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2((float)tileX / totalWidth, (float)tileZ / totalHeight),
            new Vector2((float)(tileX + 1) / totalWidth, (float)tileZ / totalHeight),
            new Vector2((float)tileX / totalWidth, (float)(tileZ + 1) / totalHeight),
            new Vector2((float)(tileX + 1) / totalWidth, (float)(tileZ + 1) / totalHeight)
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        return mesh;
    }
}

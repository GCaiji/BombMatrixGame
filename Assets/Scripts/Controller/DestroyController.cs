using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DestroyController.cs
public class DestroyController : MonoBehaviour {
    public RenderTexture dissolveRT; // 引用Shader中的_DissolveMap
    public Texture2D brushTexture;   // 笔刷形状（圆形渐变）
    public float brushSize = 0.1f;  // 笔刷半径

    private Texture2D editableTexture;

    void Start() {
        // 初始化可编辑纹理
        editableTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        Graphics.CopyTexture(dissolveRT, editableTexture);
    }

    // 在指定位置添加破坏
    public void AddDestruction(Vector3 worldPos) {
        // 转换坐标到UV空间
        Vector2 uv = WorldToUV(worldPos);
        
        // 应用笔刷
        int centerX = (int)(uv.x * editableTexture.width);
        int centerY = (int)(uv.y * editableTexture.height);
        int radius = (int)(brushSize * editableTexture.width);

        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                if (x*x + y*y > radius*radius) continue;
                
                int px = Mathf.Clamp(centerX + x, 0, editableTexture.width-1);
                int py = Mathf.Clamp(centerY + y, 0, editableTexture.height-1);
                
                Color pixel = brushTexture.GetPixelBilinear(
                    (x + radius) / (2f * radius), 
                    (y + radius) / (2f * radius)
                );
                
                editableTexture.SetPixel(px, py, pixel);
            }
        }
        
        editableTexture.Apply();
        Graphics.Blit(editableTexture, dissolveRT);
    }

    // 世界坐标转UV
    private Vector2 WorldToUV(Vector3 worldPos) {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        return new Vector2(
            (localPos.x + 5) / 10f,  // Plane默认尺寸10x10
            (localPos.z + 5) / 10f
        );
    }
}

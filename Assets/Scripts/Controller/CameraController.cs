using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;       // 角色模型
    public Transform lookAtPoint; // 你创建的lookat空物体

    [Header("摄像机参数")]
    [SerializeField] private float height = 10f;     // 摄像机高度
    [SerializeField] private float distance = 5f;    // 水平距离
    [SerializeField] private float smoothSpeed = 5f; // 跟随平滑度

    [Header("视角控制")]
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 20f;
    [SerializeField] private float zoomSpeed = 10f;

    private Vector3 offset;
    private Vector3 targetOffset; // 新增：目标偏移量

    void Start()
    {
        // 初始化偏移量
        offset = new Vector3(0, height, -distance);
        targetOffset = offset;
    }

    void LateUpdate()
    {
        if (target == null || lookAtPoint == null) return;

        // 平滑过渡到目标偏移量
        offset = Vector3.Lerp(offset, targetOffset, Time.deltaTime * smoothSpeed);
        
        // 更新摄像机位置
        transform.position = target.position + offset;
        
        // 始终看向目标点
        transform.LookAt(lookAtPoint.position);

        // 处理缩放
        HandleZoom();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            height = Mathf.Clamp(height - scroll * zoomSpeed, minHeight, maxHeight);
            targetOffset = new Vector3(0, height, -distance); // 更新目标偏移量
        }
    }
}

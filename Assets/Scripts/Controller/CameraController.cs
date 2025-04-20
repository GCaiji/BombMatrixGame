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

    void Start()
    {
        // 初始化偏移量
        offset = new Vector3(0, height, -distance);
    }

    void LateUpdate()
    {
        if (target == null || lookAtPoint == null) return;

        // 摄像机位置计算
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            smoothSpeed * Time.deltaTime
        );
        
        transform.position = smoothedPosition;

        // 始终看向目标点
        transform.LookAt(lookAtPoint.position);

        // 可选：鼠标滚轮缩放
        HandleZoom();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        height = Mathf.Clamp(height - scroll * zoomSpeed, minHeight, maxHeight);
        offset = new Vector3(0, height, -distance);
    }
}
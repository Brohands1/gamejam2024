using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 要跟随的目标
    public Vector3 offset = new Vector3(0, 5, -10); // 相机的偏移量
    public float smoothSpeed = 0.125f; // 平滑移动速度

    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;

        // 平滑移动相机
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 更新相机位置
        transform.position = smoothedPosition;

        // 保持相机朝向目标
        transform.LookAt(target);
    }
}

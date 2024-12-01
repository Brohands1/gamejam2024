using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Ҫ�����Ŀ��
    public Vector3 offset = new Vector3(0, 5, -10); // �����ƫ����
    public float smoothSpeed = 0.125f; // ƽ���ƶ��ٶ�

    void LateUpdate()
    {
        if (target == null) return;

        // ����Ŀ��λ��
        Vector3 desiredPosition = target.position + offset;

        // ƽ���ƶ����
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // �������λ��
        transform.position = smoothedPosition;

        // �����������Ŀ��
        transform.LookAt(target);
    }
}

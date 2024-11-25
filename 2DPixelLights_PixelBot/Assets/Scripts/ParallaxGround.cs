using UnityEngine;

public class ParallaxScrolling : MonoBehaviour
{
    public Transform player;          // ���/��ɫ�� Transform
    public float parallaxSpeed;       // ���������ٶ�
    public float length;              // �����Ŀ�ȣ����ڼ���ѭ����

    private Vector3 startPosition;    // �����ĳ�ʼλ��
    private float startPlayerPositionX; // ��ҳ�ʼλ��

    void Start()
    {
        // ��ʼ�������ĳ�ʼλ�ú���ҳ�ʼλ��
        startPosition = transform.position;
        startPlayerPositionX = player.position.x;
    }

    void Update()
    {
        // �����������ڳ�ʼλ�õ��ƶ�
        float playerMovement = player.position.x - startPlayerPositionX;

        // �����Ĺ�����ʹ�����λ�ƺ��ٶȼ����������
        float newPosition = playerMovement * parallaxSpeed;

        // ���±���λ�ã�ʹ�����
        transform.position = startPosition + Vector3.left * newPosition;

        // ��鱳���Ƿ��ƶ�����һ���������
        if (Mathf.Abs(transform.position.x - startPosition.x) >= length)
        {
            // �������ƶ���������ʱ������λ������
            startPosition.x += length * Mathf.Sign(playerMovement);
        }
    }
}

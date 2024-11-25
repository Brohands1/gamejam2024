using UnityEngine;

public class ParallaxScrolling : MonoBehaviour
{
    public Transform player;          // 玩家/角色的 Transform
    public float parallaxSpeed;       // 背景滚动速度
    public float length;              // 背景的宽度，用于计算循环点

    private Vector3 startPosition;    // 背景的初始位置
    private float startPlayerPositionX; // 玩家初始位置

    void Start()
    {
        // 初始化背景的初始位置和玩家初始位置
        startPosition = transform.position;
        startPlayerPositionX = player.position.x;
    }

    void Update()
    {
        // 计算玩家相对于初始位置的移动
        float playerMovement = player.position.x - startPlayerPositionX;

        // 背景的滚动（使用相对位移和速度计算滚动量）
        float newPosition = playerMovement * parallaxSpeed;

        // 更新背景位置，使其滚动
        transform.position = startPosition + Vector3.left * newPosition;

        // 检查背景是否移动超过一个背景宽度
        if (Mathf.Abs(transform.position.x - startPosition.x) >= length)
        {
            // 当背景移动超出长度时，将其位置重置
            startPosition.x += length * Mathf.Sign(playerMovement);
        }
    }
}

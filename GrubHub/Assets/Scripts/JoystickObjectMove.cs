using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickObjectMove : MonoBehaviour
{
    // Start is called before the first frame update
    public Joystick joystick;

    public Rigidbody2D rb;

    public float speed;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void Update()
    {
        if(joystick.Direction.y != 0)
        {
            rb.velocity = new Vector2(joystick.Direction.x * speed, joystick.Direction.y * speed);
        }else
        {
            rb.velocity = Vector2.zero;
        }
    }
}

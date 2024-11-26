using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MoveController : MonoBehaviour {

    public float speed;         //速度
    [Header("距离地面的最小高度")]
    public float distance;      //距离地面的高度
    public Vector3 moveSpeed;   //每一帧的移动速度
    public PlayDir nowDir;      //现在的玩家的方向
    public Animator playAnimator;

    public float gravity;       //受到的重力
    public bool gravityEnable;  //重力开关
    public bool inputEnable;    //接受输入开关  true 游戏接受按键输入  false不接受按键输入
    public float jumpPower;     //向上跳跃的力
    public bool isGround;       //是否在地面  true在地面 false不在地面
    public float jumpTime;      //跳跃的蓄力时间
    [Header("玩家是否存活")]
    public bool isAlive;
    [Header("爬强状态")]
    public bool isClimb;
    [Header("跳跃状态")]
    public bool jumpState;
    [Header("暗影冲刺持续时间")]
    public float DashTime;
    [Header("是否能够使用暗影冲刺")]
    public bool isCanDash;
    float timeJump;             //跳跃当前的蓄力时间
    public Vector2 boxSize;
    int playerLayerMask;
    private PlayState state;
    public Image dieMaskImage;
    public Vector3 startPoint;
    void Start()
    {
        isAlive = true;
        nowDir = PlayDir.Right;
        boxSize = new Vector2(0.66f, 1.32f);    //设置盒子射线的大小
        startPoint = transform.position;
        //startPoint.y = -2.56f;      //初始高度修正

        playAnimator = GetComponent<Animator>();

        gravityEnable = true;
        inputEnable = true;
        jumpState = false;
        isCanDash = true;   //状态初始化

        playerLayerMask = LayerMask.GetMask("Player");
        playerLayerMask = ~playerLayerMask;             //获得当前玩家层级的mask值，并使用~运算，让射线忽略玩家层检测

    }

    void Update() {
        if (!isAlive) return; //角色死亡无法操作

        LRMove();
        UDMpve();
        Jump();
        DashFunc();

        //更新状态和动画
        UpdateAnimtorState();
        playAnimator.SetInteger("state", (int)state);

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.contacts!=null)
        { }
    }

    private void FixedUpdate()
    {
        JumpUpdate();
    }

    /// <summary>
    ///根据落地状态更新动画以及玩家的状态信息
    /// </summary>
    public void UpdateAnimtorState()
    {
        if (isGround && isClimb)
        {
            state = PlayState.Climb;
            isClimb = false;
        }
        else if (isGround && Mathf.Abs(moveSpeed.x) > 0)
        {
            state = PlayState.Run;
        }
        else if (isGround)
        {
            state = PlayState.Normal;
        }
        else if (jumpState)
        {
            state = PlayState.Jump;
        }
        else if (moveSpeed.y < 0)
        {
            state = PlayState.Fall;
        }        

    }


    /// <summary>
    /// 左右移动
    /// </summary>
    public void LRMove()
    {
        if (!inputEnable)
        {
            return;
        }
        float h = Input.GetAxis("Horizontal");
        moveSpeed.x = h * speed;

        if (!isClimb)   //爬墙状态不能通过按键转向
        {
            DirToRotate();
        }

        if (h == 0)//停止按键输入
        {
            playAnimator.SetTrigger("stopTrigger");
            playAnimator.ResetTrigger("IsRotate");
            playAnimator.SetBool("IsRun", false);
        }
        else
        {
            playAnimator.ResetTrigger("stopTrigger");
        }
    }

    /// <summary>
    /// 根据方向进行旋转
    /// </summary>
    public void DirToRotate()
    {
        if (nowDir == PlayDir.Left && moveSpeed.x > 0)
        {
            transform.Rotate(0, 180, 0);
            nowDir = PlayDir.Right;
            if (isGround)
            {
                playAnimator.SetTrigger("IsRotate");
            }

        }
        else if (nowDir == PlayDir.Right && moveSpeed.x < 0)
        {
            transform.Rotate(0, -180, 0);
            nowDir = PlayDir.Left;
            if (isGround)//在地面才播放转向动画
            {
                playAnimator.SetTrigger("IsRotate");
            }
        }
        else if (nowDir == PlayDir.Right && moveSpeed.x > 0)
        {
            playAnimator.SetBool("IsRun", true);
        }
        else if (nowDir == PlayDir.Left && moveSpeed.x < 0)
        {
            playAnimator.SetBool("IsRun", true);
        }

    }

    /// <summary>
    /// 重力更新
    /// </summary>
    public void UDMpve()
    {
        if (!gravityEnable)
        {
           // moveSpeed.y = 0;
            return;
        }

        if (isGround)   //在地面
        {
            moveSpeed.y = 0;
        }
        else
        {
            if (isClimb)
            {
                JumpUpdate(-1.0f);
            }
            else
            {
                JumpUpdate(-1 * gravity * Time.fixedDeltaTime);
            }

        }
    }

    /// <summary>
    /// 检测是否在地面
    /// </summary>
    /// <returns></returns>
    public bool CheckIsGround()
    {
        float aryDistance = boxSize.y * 0.5f + 0.1f;
        RaycastHit2D hit2D = Physics2D.BoxCast(transform.position, boxSize, 0, Vector2.down, aryDistance, playerLayerMask);

        if (hit2D.collider != null)
        {
            return Vector3.Distance(transform.position, hit2D.point) <= (boxSize.y * 0.5f + distance);
        }
        return false;
    }

    void JumpUpdate(float power = 0)
    {
        moveSpeed.y += power;
    }
    /// <summary>
    /// 跳跃
    /// </summary>
    public void Jump()
    {
        if (!inputEnable)
        {
            return;
        }
        if (isClimb && Input.GetKeyDown(InputManager.Instance.Jump))
        {
            StartCoroutine(ClimpJumpMove());
            return;
        }
        if (Input.GetKeyDown(InputManager.Instance.Jump) && isGround)
        {
            jumpState = true;
            JumpUpdate(jumpPower);
            //moveSpeed.y += jumpPower;
            playAnimator.SetBool("IsJump", true);   //播放一段跳动画
            timeJump = 0;
        }
        else if (Input.GetKey(InputManager.Instance.Jump) && jumpState)
        {
            timeJump += Time.deltaTime;
            if (timeJump < jumpTime)
            {
                JumpUpdate(jumpPower);
            }
        }
        else if (Input.GetKeyUp(InputManager.Instance.Jump))
        {
            jumpState = false;
             timeJump = 0;
        }
        //进入上跳减速状态，但还在上升
        if (moveSpeed.y > 0 && moveSpeed.y < 1.5f)
        {
            playAnimator.SetBool("IsSlowUp", true);   
        }
        else
        {
            playAnimator.SetBool("IsSlowUp", false);   
        }
        //进入下落状态
        if (moveSpeed.y < 0)
        {

            playAnimator.SetBool("IsStopUp", true);   
        }
        else
        {
            playAnimator.SetBool("IsStopUp", false);   

        }
    }

    /// <summary>
    /// 冲刺函数
    /// </summary>
    public void DashFunc()
    {
        if (!inputEnable)
        {
            return;
        }
        if (Input.GetKeyDown(InputManager.Instance.Dash) && isCanDash)
        {
            if (isClimb)
            {
                ClimpRotate();  //如果是爬墙状态冲刺，先转向在进行冲刺
            }
            StartCoroutine(DashMove(DashTime));
            playAnimator.SetTrigger("IsDash");//播放冲刺动画
            isCanDash = false;
        }
    }

    /// <summary>
    /// 玩家死亡
    /// </summary>
    public void Die()
    {
        isAlive = false;
    }

    IEnumerator DashMove(float time)
    {
        inputEnable = false;
        gravityEnable = false;
        moveSpeed.y = 0;
        if (nowDir == PlayDir.Left)
        {
            moveSpeed.x = 15*-1;
        }
        else
        {
            moveSpeed.x = 15;
        }

        yield return new WaitForSeconds(time);
        inputEnable = true;
        gravityEnable = true;
        //moveSpeed.y = 0;
        //moveSpeed.x = jumpPower;
    }

    /// <summary>
    /// 墙上跳跃的移动
    /// </summary>
    /// <returns></returns>
    IEnumerator ClimpJumpMove()
    {
        inputEnable = false;    //此时不接受其余输入
        gravityEnable = false;
        isClimb = false;
        playAnimator.SetTrigger("IsStopClimpJump");

        playAnimator.ResetTrigger("IsClimb");
        if (nowDir == PlayDir.Left)
        {
            moveSpeed.x = 8;
        }
        else
        {
            moveSpeed.x = -8;
        }

        moveSpeed.y =  6;
        yield return new WaitForSeconds(0.15f);
        inputEnable = true;
        gravityEnable = true;

    }

    /// <summary>
    /// 爬墙跳后的转向
    /// </summary>
    public void ClimpRotate()
    {
        if (nowDir == PlayDir.Left)
        {
            nowDir = PlayDir.Right;
            transform.Rotate(0, 180, 0);
        }
        else
        {
            nowDir = PlayDir.Left;
            transform.Rotate(0, -180, 0);
        }
    }

    /// <summary>
    /// 检测下一帧的位置是否能够移动，并进行修正
    /// </summary>
    public void CheckNextMove()
    {
        Vector3 moveDistance = moveSpeed * Time.deltaTime;
        int dir = 0;//确定下一帧移动的左右方向
        if (moveSpeed.x > 0)
        {
            dir = 1;
        }
        else if (moveSpeed.x < 0)
        {
            dir = -1;
        }
        else
        {
            dir = 0;
        }
        if (dir != 0)//当左右速度有值时
        {
            RaycastHit2D lRHit2D = Physics2D.BoxCast(transform.position, boxSize, 0, Vector2.right * dir, 5.0f, playerLayerMask);
            if (lRHit2D.collider != null)//如果当前方向上有碰撞体
            {
                float tempXVaule = (float)Math.Round(lRHit2D.point.x, 1);                   //取X轴方向的数值，并保留1位小数精度。防止由于精度产生鬼畜行为
                Vector3 colliderPoint = new Vector3(tempXVaule, transform.position.y);      //重新构建射线的碰撞点
                float tempDistance = Vector3.Distance(colliderPoint, transform.position);   //计算玩家与碰撞点的位置
                if (tempDistance > (boxSize.x * 0.5f + distance))   //如果距离大于 碰撞盒子的高度的一半+最小地面距离
                {
                    transform.position += new Vector3(moveDistance.x, 0, 0); //说明此时还能进行正常移动，不需要进行修正
                    if (isClimb)        //如果左右方向没有碰撞体了，退出爬墙状态
                    {
                        isClimb = false;
                        playAnimator.ResetTrigger("IsClimb"); //重置触发器  退出
                        playAnimator.SetTrigger("exitClimp");
                    }
                }
                else//如果距离小于  根据方向进行位移修正
                {
                    float tempX = 0;//新的X轴的位置
                    if (dir > 0)
                    {
                        tempX = tempXVaule - boxSize.x * 0.5f - distance + 0.05f; //多加上0.05f的修正距离，防止出现由于精度问题产生的鬼畜行为
                    }
                    else
                    {
                        tempX = tempXVaule + boxSize.x * 0.5f + distance - 0.05f;
                    }
                    transform.position = new Vector3(tempX, transform.position.y, 0);//修改玩家的位置
                    if (!lRHit2D.collider.CompareTag("Trap"))    //如果左右不是陷阱
                    {
                        EnterClimpFunc(transform.position); //检测当前是否能够进入爬墙状态
                        playAnimator.ResetTrigger("exitClimp");
                    }
                    else
                    {
                        Die();
                    }

                }

            }
            else
            {
                transform.position += new Vector3(moveDistance.x, 0, 0);
                if (isClimb)
                {
                    isClimb = false;
                    playAnimator.SetTrigger("exitClimp");
                    playAnimator.ResetTrigger("IsClimb"); //重置触发器  退出
                }

            }
        }
        else
        {
            if (isClimb)    //当左右速度无值时且处于爬墙状态时
            {
                ExitClimpFunc();
            }
        }
        //更新方向信息，上下轴
        if (moveSpeed.y > 0)
        {
            dir = 1;
        }
        else if (moveSpeed.y < 0)
        {
            dir = -1;
        }
        else
        {
            dir = 0;
        }
        //上下方向进行判断
        if (dir != 0)
        {
            RaycastHit2D uDHit2D = Physics2D.BoxCast(transform.position, boxSize, 0, Vector3.up * dir, 5.0f, playerLayerMask);
            if (uDHit2D.collider != null)
            {
                float tempYVaule = (float)Math.Round(uDHit2D.point.y, 1);
                Vector3 colliderPoint = new Vector3(transform.position.x, tempYVaule);
                float tempDistance = Vector3.Distance(transform.position, colliderPoint);

                if (tempDistance > (boxSize.y * 0.5f + distance))
                {

                    float tempY = 0;
                    float nextY = transform.position.y + moveDistance.y;
                    if (dir > 0)
                    {
                        tempY = tempYVaule - boxSize.y * 0.5f - distance;
                        if (nextY > tempY)
                        {
                            transform.position = new Vector3(transform.position.x, tempY+0.1f, 0);
                        }
                        else
                        {
                            transform.position += new Vector3(0, moveDistance.y, 0);
                        }
                    }
                    else
                    {
                        tempY = tempYVaule + boxSize.y * 0.5f + distance;
                        if (nextY < tempY)
                        {
                            transform.position = new Vector3(transform.position.x, tempY-0.1f, 0); //上下方向多减少0.1f的修正距离，防止鬼畜
                        }
                        else
                        {
                            transform.position += new Vector3(0, moveDistance.y, 0);
                        }
                    }
                    isGround = false;   //更新在地面的bool值
                }
                else
                {
                    float tempY = 0;
                    if (dir > 0)//如果是朝上方向移动，且距离小于规定距离，就说明玩家头上碰到了物体，反之同理。
                    {
                        tempY = uDHit2D.point.y - boxSize.y * 0.5f - distance + 0.05f;
                        isGround = false;
                        Debug.Log("头上碰到了物体");
                    }
                    else
                    {
                        tempY = uDHit2D.point.y + boxSize.y * 0.5f + distance - 0.05f;
                        Debug.Log("着地");
                        isGround = true;
                    }
                    moveSpeed.y = 0;
                    transform.position = new Vector3(transform.position.x, tempY, 0);
                    if (uDHit2D.collider.CompareTag("Trap"))    //如果头上是陷阱  死亡
                    {
                        Die();
                    }
                }
            }
            else
            {
                isGround = false;
                transform.position += new Vector3(0, moveDistance.y, 0);
            }
        }
        else
        {
            isGround = CheckIsGround();//更新在地面的bool值
        }
    }

    /// <summary>
    /// 进入爬墙的函数
    /// </summary>
    public void EnterClimpFunc(Vector3 rayPoint)
    {
        //设定碰到墙 且  从碰撞点往下 玩家碰撞盒子高度内  没有碰撞体  就可进入碰撞状态。
        RaycastHit2D hit2D = Physics2D.BoxCast(rayPoint, boxSize, 0, Vector2.down, boxSize.y, playerLayerMask);
        if (hit2D.collider != null)
        {
            Debug.Log("无法进入爬墙状态  "+ hit2D.collider.name);
        }
        else
        {
            //如果上方是异形碰撞体，那么就无法进入爬墙状态
            hit2D = Physics2D.BoxCast(rayPoint, boxSize, 0, Vector2.up, boxSize.y*0.8f, playerLayerMask);
            if (hit2D.collider == null || hit2D.collider.gameObject.tag != "Arc")
            {
                playAnimator.SetTrigger("IsClimb");//动画切换
                isClimb = true;
                isCanDash = true; //爬墙状态，冲刺重置
            }


        }
    }

    /// <summary>
    /// 退出爬墙状态检测
    /// </summary>
    public void ExitClimpFunc()
    {
        RaycastHit2D hit2D = new RaycastHit2D();
        switch (nowDir)
        {
            case PlayDir.Left:
                hit2D = Physics2D.Raycast(transform.position, Vector3.left, boxSize.x);
                break;
            case PlayDir.Right:
                hit2D = Physics2D.Raycast(transform.position, Vector3.right, boxSize.x);
                break;
        }

        if (hit2D.collider == null)
        {
            Invoke("ExitClimb", 0.1f);
            playAnimator.SetTrigger("exitClimp");
            playAnimator.ResetTrigger("IsClimb"); //重置触发器  退出
        }
    }

    void ExitClimb()
    {
        isClimb = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Controller : MonoBehaviour
{
    [Header("Movement")]
    public float Speed;
    float DesiredMoveSpeed;
    public float JumpForce;
    public float JumpColldownTime;
    public bool IsGrounded;
    public float AirMod;
    public float GroundDrag;
    public bool ReadyToJump;
    public LayerMask WhatIsGround;
    public KeyCode JumpButton;
    public bool CanMove;

    [Header("Climbing")]
    public Transform Camera;
    public float MaxWallLookAngle;
    public bool IsClimbing;
    public float MaxClimbTime;
    public float ClimbSpeed;
    public float DetectionLength;
    public LayerMask WhatIsWall;
    private bool WallFront;
    private RaycastHit FrontWallHit;
    private float WallLoockAngle;
    private float ClimbTime;

    [Header("Sliding")]
    public float PlayerHeight;
    public bool IsSliding;

    [Header("Wall Running")]
    public float WallJumpUpForce;
    public float WallJumpSideForce;
    public float WallRunSpeed;
    public float MinJumpHeight;
    public float WallRunForce;
    public float MaxWallRunTime;
    private float WallRunTimer;
    public float WallCheckDistance;
    private RaycastHit LeftWallHit;
    private RaycastHit RightWallHit;
    bool WallLeft;
    bool WallRight;
    bool WallRunning;



    [Header("Components")]
    public Rigidbody rb;
    Combat_Controller CombatController;
    Vector3 MoveDirection;
    float Hor;
    float Ver;
    Camera_Controller CamController;

    private void Start()
    {
        CamController = GetComponentInChildren<Camera_Controller>();
        rb = GetComponent<Rigidbody>();
        CombatController = GetComponent<Combat_Controller>();
        CanMove = true;
    }
    private void Update()
    {
        MyInput();
        StateMachine();
        GroundCheck();
        if (!IsSliding)
        {
            SpeedControll();
            WallCheck();
            CheckForWall();
        }
        PlayerHeightController();
    }
    private void FixedUpdate()
    {
        if (CanMove)
            MovementLogic(MoveDirection);
        if (IsClimbing)
            Climbing();
        if (WallRunning)
            WallRunningMovement();
    }
    void MyInput()
    {
        Hor = Input.GetAxisRaw("Horizontal");
        Ver = Input.GetAxisRaw("Vertical");

        MoveDirection = transform.forward * Ver + transform.right * Hor;

        if (Input.GetKey(JumpButton) && (IsGrounded) && ReadyToJump)
        {
            ReadyToJump = false;
            Jump();
            Invoke(nameof(JumpCooldown), JumpColldownTime);
        }

        if (Input.GetKey(KeyCode.C) && !CombatController.InAction)
            Slide();
        else
        {
            CanMove = true;
            PlayerHeight = 2;
            IsSliding = false;
        }
        if (Input.GetKeyDown(KeyCode.C) && IsGrounded)
        {
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            rb.AddForce(transform.forward * Speed, ForceMode.Impulse);
        }
    }
    private void StateMachine()
    {
        if (WallFront && Input.GetKey(KeyCode.W) && WallLoockAngle < MaxWallLookAngle && !IsGrounded)
        {
            if (!IsClimbing && ClimbTime > 0)
                StartClimbing();
            if (ClimbTime > 0)
                ClimbTime -= Time.deltaTime;
            if (ClimbTime <= 0)
                StopClimbing();
        }
        else
            if (IsClimbing)
            StopClimbing();

        if ((WallLeft || WallRight) && Ver > 0 && !IsSliding && AboveGround())
        {
            if (!WallRunning)
                StartWallRun();
            if (Input.GetKeyDown(JumpButton))
                WallJump();
        }
        else
        {
            if(WallRunning)
                StopWallRun();
        }

        if (WallRunning)
        {
            if (WallRight)
            {
                CamController.TiltMod = 1;
            }
            else if (WallLeft)
            {
                CamController.TiltMod = -1;
            }
        }
        else
            CamController.TiltMod = 0;
    }
    #region Movement
    void MovementLogic(Vector3 MoveDir)
    {
        if (IsGrounded)
            rb.AddForce(MoveDir.normalized * Speed * 10, ForceMode.Force);
        else if (!IsGrounded)
        {
            rb.AddForce(MoveDir.normalized * AirMod * 10f * Speed, ForceMode.Force);
            rb.AddForce(Vector3.down * 2, ForceMode.Force);
        }
    }
    void SpeedControll()
    {
        if (IsGrounded && !IsSliding)
            rb.drag = GroundDrag;
        else
            rb.drag = 0;
        if (WallRunning)
            DesiredMoveSpeed = WallRunSpeed;
        else if (!IsGrounded)
            DesiredMoveSpeed = 100;
        else
            DesiredMoveSpeed = Speed;

        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (flatVelocity.magnitude > DesiredMoveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * DesiredMoveSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }
    public void Grappling(Transform GrabPoint,float GrabForce)
    {
        rb.velocity = new Vector3(0, 0, 0);
        rb.AddForce((GrabPoint.position - transform.position) * GrabForce, ForceMode.Impulse);
    }
    #endregion
    #region Slide
    void PlayerHeightController()
    {
        transform.localScale = new Vector3(transform.localScale.x, PlayerHeight / 2, transform.localScale.z);
    }
    void Slide()
    {
        IsSliding = true;
        CanMove = false;
        rb.drag = 0;
        PlayerHeight = 1;
    }
    #endregion
    #region Jumping
    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (IsSliding)
            rb.AddForce(transform.forward * 4, ForceMode.Force);
        else
            rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);
    }
    void JumpCooldown()
    {
        ReadyToJump = true;
    }
    void GroundCheck()
    {
        IsGrounded = Physics.CheckSphere(transform.position - transform.up, 0.4f, WhatIsGround);
    }

    #endregion
    #region Climbing
    void StartClimbing()
    {
        IsClimbing = true;
    }
    void Climbing()
    {
        //rb.velocity = new Vector3(rb.velocity.x, ClimbSpeed, rb.velocity.z);
        rb.velocity = new Vector3(0, ClimbSpeed, 0);

    }
    void StopClimbing()
    {
        IsClimbing = false;
    }
    void WallCheck()
    {
        WallFront = Physics.SphereCast(transform.position, 0.25f, transform.forward, out FrontWallHit, DetectionLength, WhatIsWall);
        WallLoockAngle = Vector3.Angle(transform.forward, -FrontWallHit.normal);
        if (IsGrounded)
            ClimbTime = MaxClimbTime;
        if (IsClimbing)
            CanMove = false;
        else
            CanMove = true;
    }
    #endregion
    #region WallRunning
    void CheckForWall()
    {
        WallRight = Physics.Raycast(transform.position, transform.right, out RightWallHit, WallCheckDistance, WhatIsWall);
        WallLeft = Physics.Raycast(transform.position, -transform.right, out LeftWallHit, WallCheckDistance, WhatIsWall);
    }
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, MinJumpHeight, WhatIsGround);
    }
    void StartWallRun()
    {
        WallRunning = true;
    }
    void StopWallRun()
    {
        WallRunning = false;
        rb.useGravity = true;
    }
    void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 WallNormal = WallRight ? RightWallHit.normal : LeftWallHit.normal;
        Vector3 WallForward = Vector3.Cross(WallNormal, transform.up);
        if ((transform.forward - WallForward).magnitude > (transform.forward - -WallForward).magnitude)
            WallForward = -WallForward;
        rb.AddForce(WallForward * WallRunForce, ForceMode.Force);
        if (!(WallLeft && Hor > 0) && !(WallRight && Hor < 0))
            rb.AddForce(-WallNormal * 100, ForceMode.Force);
    }
    void WallJump()
    {
        Vector3 WallNormal = WallRight ? RightWallHit.normal : LeftWallHit.normal;
        Vector3 ForceToApply = transform.up * WallJumpUpForce + WallNormal * WallJumpSideForce;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(ForceToApply, ForceMode.Impulse);
    }
    #endregion
}

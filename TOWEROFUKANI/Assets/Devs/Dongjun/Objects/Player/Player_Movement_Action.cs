﻿using UnityEngine;

public class Player_Movement_Action : CLA_Action,
    ICanDetectGround
{
    #region Var: Inspector
    [Header("Ref")]
    [SerializeField] private Transform spriteRoot;
    [SerializeField] private BoxCollider2D oneWayCollider;

    [Header("GroundDetection")]
    [SerializeField] private GroundDetectionData groundDetectionData;

    [Header("Gravity")]
    [SerializeField] private GravityData gravityData;

    [Header("Walk")]
    [SerializeField] private PlayerWalkData walkData;

    [Header("Jump")]
    [SerializeField] private JumpData jumpData;
    #endregion

    #region Var: Ground Detection
    private bool isGrounded = false;
    private GroundInfo curGroundInfo = new GroundInfo();
    #endregion

    #region Var: Jump
    private bool jumpKeyPressed = false;
    private bool isJumping = false;
    private bool canPlayJumpAnim = true;
    #endregion

    #region Var: Fall Through
    private bool fallThroughKeyPressed = false;
    #endregion

    #region Var: Components
    private Animator animator;
    private Rigidbody2D rb2D;
    #endregion

    #region Var: Properties
    public JumpData JumpData => jumpData;
    #endregion


    #region Method: Unity
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();

        groundDetectionData.Size = oneWayCollider.size;
    }
    #endregion

    #region Method: CLA_Action
    public override void OnExit()
    {
        // Reset Ground Data
        isGrounded = false;
        curGroundInfo.Reset();

        // Reset Jump Data
        isJumping = false;

        // Reset Velocity
        rb2D.velocity = new Vector2(rb2D.velocity.x, 0);
    }
    public override void OnLateUpdate()
    {
        // Look At Mouse
        spriteRoot.LookAtMouseY(Global.Inst.MainCam, transform);
    }
    public override void OnFixedUpdate()
    {
        // Detect Ground
        GroundDetection_Logic.DetectGround(!isJumping, rb2D, transform, groundDetectionData, ref isGrounded, ref curGroundInfo);
        GroundDetection_Logic.ExecuteOnGroundMethod(this, isGrounded, ref groundDetectionData);

        // Fall Through
        fallThroughKeyPressed = PlayerInputManager.Inst.Input_FallThrough;
        GroundDetection_Logic.FallThrough(ref fallThroughKeyPressed, isGrounded, rb2D, transform, oneWayCollider, groundDetectionData);

        // Walk
        PlayerWalk_Logic.Walk(PlayerInputManager.Inst.Input_WalkDir, rb2D, ref walkData, isJumping);

        // Jump
        jumpKeyPressed = PlayerInputManager.Inst.Input_Jump;
        Jump_Logic.Jump(ref jumpKeyPressed, ref isJumping, ref jumpData, rb2D, transform);

        // Gravity
        Gravity_Logic.ApplyGravity(rb2D, 
            isGrounded ? new GravityData(false, 0, 0) : 
            !isJumping ? gravityData : 
            new GravityData(true, jumpData.jumpGravity, 0));

        // Animation
        UpdateAnimation();
    }
    #endregion

    #region Method: Animation
    private void UpdateAnimation()
    {
        const string
        Idle = "Player_Idle",
        Walk_Forward = "Player_Walk_Forward",
        Walk_Backward = "Player_Walk_Backward",
        Airborne = "Player_Airborne",
        Jump = "Player_Jump",
        AirJump = "Player_AirJump";

        string jumpAnim = jumpData.canJump ? Jump : AirJump;

        if (isGrounded)
        {
            if (PlayerInputManager.Inst.Input_WalkDir == 0)
            {
                animator.Play(Idle);
            }
            else
            {
                if ((PlayerInputManager.Inst.Input_WalkDir == 1 && spriteRoot.rotation.eulerAngles.y == 0) ||
                    (PlayerInputManager.Inst.Input_WalkDir == -1 && spriteRoot.rotation.eulerAngles.y == 180))
                    animator.Play(Walk_Forward);
                else
                    animator.Play(Walk_Backward);
            }
        }
        else
        {
            if (!isJumping)
            {
                animator.Play(Airborne);
            }
            else if (canPlayJumpAnim)
            {
                animator.Play(jumpAnim);

                if (PlayerInputManager.Inst.Input_Jump)
                    animator.Play(jumpAnim);
            }
            canPlayJumpAnim = jumpData.canJump;
        }
    }
    #endregion

    #region Interface: ICanDetectGround
    public void OnGroundEnter()
    {
        // Reset Jump
        Jump_Logic.ResetJumpCount(ref jumpData);
    }
    public void OnGroundStay()
    {
    }
    public void OnGroundExit()
    {
    }
#endregion
}
﻿using Dongjun.Helper;
using UnityEngine;

public class Player : SSM_Main
{
    #region Var: Inspector
    [Header("Component")]
    public Transform spriteRoot;
    public SpriteRenderer bodySpriteRenderer;
    public BoxCollider2D oneWayCollider;

    [Header("GroundDetection")]
    public GroundDetectionData groundDetectionData;

    [Header("Gravity")]
    public GravityData gravityData;
    #endregion

    #region Var: Player
    public bool CanMelee = true;
    public bool CanDash = true;
    public bool CanKick = true;
    #endregion

    #region Var: Properties
    public Rigidbody2D rb2D
    { get; private set; }
    public Animator animator
    { get; private set; }
    public int Dir => bodySpriteRenderer.flipX ? -1 : 1;
    public bool IsDashing => CurrentState == state_Dash;
    #endregion

    #region Var: States
    private Player_Stunned state_Stunned;
    private Player_Normal state_Normal;
    private Player_Dash state_Dash;
    private Player_Kick state_Kick;
    #endregion

    #region Method: Unity
    protected override void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        base.Awake();
    }
    #endregion

    #region Method: Init States
    protected override void InitStates()
    {
        SetLogic(ref state_Stunned, () =>
        {
            if (!PlayerStatus.Inst.IsStunned)
                return state_Normal;

            return null;
        });

        SetLogic(When.AnyAction, () =>
        {
            if (PlayerStatus.Inst.IsStunned)
                return state_Stunned;

            return null;
        });

        SetLogic(ref state_Normal, () => 
        {
            if (CanDash && PlayerInputManager.Inst.Input_DashDir != 0 && PlayerStats.Inst.UseStamina(1))
                return state_Dash;

            if (CanKick && Input.GetKeyDown(PlayerActionKeys.Kick))
                return state_Kick;

            return null;
        });

        SetLogic(ref state_Dash, () => 
        {
            // Cancle On Jump
            if (state_Normal.JumpData.canJump && PlayerInputManager.Inst.Input_Jump)
                return state_Normal;

            // Cancle On End
            if (state_Dash.DashDone)
                return state_Normal;

            return null;
        });

        SetLogic(ref state_Kick, () => 
        {
            if (!state_Kick.IsKicking)
                return state_Normal;

            return null;
        });

        SetDefaultState(state_Normal);
    }
    #endregion
}
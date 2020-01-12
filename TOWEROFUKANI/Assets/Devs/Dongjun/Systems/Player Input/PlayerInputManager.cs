﻿using UnityEngine;

public sealed class PlayerInputManager : SingletonBase<PlayerInputManager>
{
    #region Var: Walk
    private int input_WalkDir = 0;
    public int Input_WalkDir => input_WalkDir;
    #endregion

    #region Var: Jump
    public bool Input_Jump { get; private set; } = false;
    #endregion

    #region Var: FallThrough
    public bool Input_FallThrough { get; private set; } = false;
    #endregion

    #region Var: Dash
    [HideInInspector]
    private readonly float dashInputInterval = 0.2f;
    private float dashInputTime = 0;
    private int dashInputCount = 0;
    private int oldDashInput = 0;
    public int Input_DashDir
    { get; private set; }
    #endregion

    #region Var: Weapon
    public bool CanUseWeapon
    { get; private set; } = true;
    #endregion

    #region Method: Unity
    private void Update()
    {
        // Movement
        GetInput_Walk();
        GetInput_Jump();
        GetInput_FallThrough();

        // Action
        GetInput_Dash();

        // Weapon
        UpdateWeaponKey();
    }
    #endregion

    #region Method: Reset Input
    public void ResetInput()
    {
        Input_Jump = false;
        Input_FallThrough = false;
        Input_DashDir = 0;
    }
    #endregion

    #region Method: Walk
    private void GetInput_Walk()
    {
        void GetDir(ref int dir, KeyCode plusKey, KeyCode minusKey)
        {
            if (Input.GetKeyDown(plusKey))
                dir = 1;
            if (Input.GetKeyDown(minusKey))
                dir = -1;

            if (dir == 1 && Input.GetKeyUp(plusKey))
                dir = -1;
            if (dir == -1 && Input.GetKeyUp(minusKey))
                dir = 1;

            if (!Input.GetKey(plusKey) && !Input.GetKey(minusKey))
                dir = 0;
        }

        GetDir(ref input_WalkDir, PlayerMovementKeys.WalkRight, PlayerMovementKeys.WalkLeft);
    }
    #endregion

    #region Method: FallThrough
    private void GetInput_FallThrough()
    {
        if (Input.GetKeyDown(PlayerMovementKeys.FallThrough))
            Input_FallThrough = true;
    }
    #endregion

    #region Method: Jump
    private void GetInput_Jump()
    {
        if (Input.GetKeyDown(PlayerMovementKeys.Jump))
            Input_Jump = true;
    }
    #endregion

    #region Method: Dash
    private void GetInput_Dash()
    {
        if (Input.GetKeyDown(PlayerActionKeys.Dash))
        {
            Input_DashDir = Input_WalkDir;
            dashInputCount = 0;
            dashInputTime = 0;
            return;
        }

        if (Input.GetKeyDown(PlayerMovementKeys.WalkRight) || 
            Input.GetKeyDown(PlayerMovementKeys.WalkLeft))
        {
            // First Tap
            if (dashInputCount == 0)
                dashInputCount++;

            // After First Tap
            if (oldDashInput == Input_WalkDir && dashInputTime <= dashInputInterval)
                dashInputCount++;
            else
                dashInputCount = 0;

            // Check Tap Count
            if (dashInputCount == 2)
            {
                Input_DashDir = Input_WalkDir;
                dashInputCount = 0;
            }

            // Reset Data
            dashInputTime = 0;
            oldDashInput = Input_WalkDir;
        }

        dashInputTime += Time.deltaTime;
    }
    #endregion

    #region Method: Weapon
    private void UpdateWeaponKey()
    {
        // UI 위에서 무기 버튼을 누르면 무기 사용 못함.
        if (UI_Utility.IsMouseOverUI() 
        && (Input.GetKeyDown(PlayerWeaponKeys.MainAbility) || Input.GetKeyDown(PlayerWeaponKeys.SubAbility)))
        {
            CanUseWeapon = false;
            return;
        }

        // UI 위에서 무기 버튼을 눌렀을 때 이후에도 버튼을 계속 누르고 있으면 무기 사용 못함.
        if (!CanUseWeapon && Input.GetKey(PlayerWeaponKeys.MainAbility))
            return;

        // 다시 무기 사용 가능.
        CanUseWeapon = true;
    }
    #endregion
}
﻿using Dongjun.Helper;
using UnityEngine;

public class OBB_Gun_Reload_Base<D, W> : AimedWeapon_State_Base<D, W>
    where D : OBB_Data_Animator
    where W : GunItem
{
    public override void OnEnter()
    {
        // Timer
        weaponItem.Timer_Reload.SetActive(true);

        // Animation
        data.Animator.Play(weaponItem.ANIM_Reload);
    }
    public override void OnLateEnter()
    {
        // Animation
        data.Animator.SetDuration(weaponItem.Timer_Reload.EndTime.Value);
    }
    public override void OnExit()
    {
        if (weaponItem.Timer_Reload.IsEnded)
            weaponItem.ReloadFull();

        // Timer
        weaponItem.Timer_Reload.SetActive(false);
        weaponItem.Timer_Reload.Reset();

        // Animation
        data.Animator.ResetSpeed();
    }
}
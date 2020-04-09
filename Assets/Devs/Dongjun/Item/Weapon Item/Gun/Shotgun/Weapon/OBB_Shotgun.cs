﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class OBB_Data_Shotgun : OBB_Data_Animator
{

}

class OBB_Shotgun : OBB_Controller_Weapon<OBB_Data_Shotgun, ShotgunItem>
{
    // States
    private OBB_Gun_Idle state_Idle;
    private OBB_Shotgun_Main state_Main;
    private OBB_Gun_Reload state_Reload;
    private OBB_Gun_SwapMagazine state_SwapMagazine;

    // Behaviours
    private Single bvr_Idle;
    private Continue bvr_AutoReload;
    private Sequence bvr_ManualReload;
    private Choice bvr_Normal;

    protected override void InitStates()
    {
        GetState(ref state_Idle);
        GetState(ref state_Main);
        GetState(ref state_Reload);
        GetState(ref state_SwapMagazine);
        SetDefaultState(state_Idle, EMPTY_STATE_ACTION);
    }
    protected override void InitBehaviours()
    {
        bvr_Idle = new Single(
            state_Idle,
            EMPTY_STATE_ACTION,
            () => false);

        bvr_AutoReload = new Continue(
            (state_SwapMagazine,
            EMPTY_STATE_ACTION,
            () => weaponItem.Dur_Main_SwapMagazine.IsEnded),
            (state_Reload,
            EMPTY_STATE_ACTION,
            () => weaponItem.Dur_Main_Reload.IsEnded));

        bvr_ManualReload = new Sequence(
            (state_SwapMagazine,
            EMPTY_STATE_ACTION,
            () => weaponItem.Dur_Main_SwapMagazine.IsEnded),
            (state_Reload,
            EMPTY_STATE_ACTION,
            () => weaponItem.Dur_Main_Reload.IsEnded));

        bvr_Normal = new Choice(
            (state_Idle,
            EMPTY_STATE_ACTION,
            () =>
            {
                if (PlayerWeaponKeys.GetKeyDown(PlayerWeaponKeys.MainAbility) && weaponItem.CD_Main_Shoot.IsEnded)
                    return state_Main;

                return state_Idle;
            }),
            (state_Main,
            EMPTY_STATE_ACTION,
            () =>
            {
                if (weaponItem.CD_Main_Shoot.IsEnded)
                    return END_BEHAVIOUR;

                return state_Main;
            }));
    }
    protected override void InitObjectives()
    {
        // Pause
        NewObjective(
            () => !weaponItem.IsSelected || PlayerStatus.Incapacitated, true)
            .AddBehaviour(bvr_Idle);

        // Auto Reload
        NewObjective(
            () => weaponItem.LoadedBullets == 0 && weaponItem.CD_Main_Shoot.IsEnded)
            .AddBehaviour(bvr_AutoReload);

        // Manual Reload
        NewObjective(
            () => PlayerWeaponKeys.GetKeyDown(PlayerWeaponKeys.Reload)
            && weaponItem.LoadedBullets < weaponItem.MagazineSize.Value, true)
            .AddBehaviour(bvr_ManualReload, true);

        // Normal
        NewObjective(
            () => weaponItem.LoadedBullets > 0)
            .AddBehaviour(bvr_Normal, true);

        // Default
        SetDefaultObjective()
            .AddBehaviour(bvr_Idle);
    }
}

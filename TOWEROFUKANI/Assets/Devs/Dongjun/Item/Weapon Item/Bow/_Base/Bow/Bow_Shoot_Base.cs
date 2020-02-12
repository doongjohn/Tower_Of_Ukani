﻿using Dongjun.Helper;
using UnityEngine;

public abstract class Bow_Shoot_Base<T> : Bow_State_Base<T>
    where T : BowItem
{
    #region Var: Inspector
    [Header("Shoot")]
    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected Arrow arrowPrefab;

    [Header("Shoot Animation")]
    [SerializeField] protected float maxShootAnimTime;

    [Header("Camera Shake")]
    [SerializeField] protected CameraShake.Data camShakeData_Shoot;
    #endregion

    #region Method: SSM
    public override void OnEnter()
    {
        weapon.shootTimer.SetActive(true);
        weapon.shootTimer.Restart();

        weapon.animator.Play(weapon.ANIM_Shoot, 0, 0);
    }
    public override void OnExit()
    {
        weapon.shootTimer.SetActive(false);
        weapon.shootTimer.ToZero();

        weapon.animator.ResetSpeed();
    }
    public override void OnLateUpdate()
    {
        if (!weapon.IsSelected)
            return;

        // Look At Mouse
        transform.AimMouse(Global.Inst.MainCam, transform);

        // Animation Speed
        weapon.animator.SetDuration(weapon.shootTimer.EndTime.Value, maxShootAnimTime, weapon.ANIM_Shoot);
    }
    #endregion

    #region Method: Shoot
    protected virtual void Shoot()
    {
        SpawnArrow();
        ShootEffects();

        // Trigger Item Effect
        ActionEffectManager.Trigger(PlayerActions.WeaponMain);
        ActionEffectManager.Trigger(PlayerActions.BowShoot);
    }
    protected virtual void SpawnArrow()
    {
        // Set Attack Data
        AttackData curAttackData = weapon.attackData;
        curAttackData.damage = new FloatStat(Mathf.Max(weapon.attackData.damage.Value * weapon.drawPower, 1));

        // Set Projectile Data
        ProjectileData curArrowData = weapon.arrowData;
        curArrowData.moveSpeed.Base *= weapon.drawPower;

        // Spawn Arrow
        Arrow arrow = arrowPrefab.Spawn(shootPoint.position, transform.rotation);

        // Set Arrow Data
        arrow.InitData(arrow.transform.right, curArrowData, curAttackData);
    }
    protected virtual void ShootEffects()
    {
        // Cam Shake
        CamShake_Logic.ShakeBackward(camShakeData_Shoot, transform);
    }
    #endregion

    #region Method: Anim Event
    protected virtual void OnAnim_ShootArrow()
    {
        Shoot();
    }
    #endregion
}
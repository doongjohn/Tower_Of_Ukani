﻿using UnityEngine;

public class MachineGun_Main_Action : CLA_Action
{
    #region Var: Inspector
    [Header("Shoot")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private PoolingObj bulletPrefab;
    [SerializeField] private float maxShootAnimTime;

    [Header("Accuracy")]
    [SerializeField] private float acry_YPosOffset;
    [SerializeField] private float acry_ZRotOffset;

    [Header("Muzzle Flash")]
    [SerializeField] private Transform muzzleFlashParent;
    [SerializeField] private PoolingObj muzzleFlashPrefab;

    [Header("Empty Shell")]
    [SerializeField] private Transform emptyShellSpawnPos;
    [SerializeField] private PoolingObj emptyShellPrefab;

    [Header("Ammo Belt")]
    [SerializeField] private Transform ammoBelt;
    [SerializeField] private float ammoBeltAmmoCount;

    [Header("Camera Shake")]
    [SerializeField] private CameraShake.Data camShakeData_Shoot;
    #endregion

    #region Var: Components
    private Animator animator;
    private MachineGun gun_Main;
    #endregion


    #region Method: Unity
    private void Awake()
    {
        animator = GetComponent<Animator>();
        gun_Main = GetComponent<MachineGun>();
    }
    #endregion

    #region Method: CLA_Action
    public override void OnStart()
    {
        if (gun_Main.gunData.loadedBullets == gun_Main.gunData.magazineSize)
            ammoBelt.localPosition = Vector3.zero;
    }
    public override void OnEnd()
    {
        animator.speed = 1;
        animator.ResetTrigger("Shoot");
    }
    public override void OnUpdate()
    {
        if (!gun_Main.IsSelected)
            return;

        if (gun_Main.gunData.shootTimer.IsTimerAtMax && Input.GetKey(PlayerInputManager.Inst.Keys.MainAbility))
        {
            // Restart Timer
            gun_Main.gunData.shootTimer.Restart();

            // Spawn Bullet
            Transform bullet = bulletPrefab.Activate(shootPoint.position, transform.rotation).transform;
            bullet.position += shootPoint.up * Random.Range(-acry_YPosOffset, acry_YPosOffset);
            bullet.rotation = Quaternion.Euler(0, 0, bullet.eulerAngles.z + Random.Range(-acry_ZRotOffset, acry_ZRotOffset));

            // Consume Bullet
            gun_Main.gunData.loadedBullets -= 1;

            // Update Ammo Belt Pos
            ammoBelt.localPosition 
                = new Vector2(0, Mathf.Lerp(0, 0.0625f * ammoBeltAmmoCount, 1 - ((float)gun_Main.gunData.loadedBullets / gun_Main.gunData.magazineSize)));

            // Empty Shell
            emptyShellPrefab.Activate(emptyShellSpawnPos.position, transform.rotation);

            // Muzzle Flash
            muzzleFlashPrefab.Activate(muzzleFlashParent, new Vector2(0, 0), Quaternion.identity).transform.position 
                = bullet.position;

            // Animation
            animator.SetTrigger("Shoot");

            // Cam Shake
            CamShake_Logic.ShakeBackward(camShakeData_Shoot, transform);
        }
    }
    public override void OnLateUpdate()
    {
        // Lool At Mouse
        LookAtMouse_Logic.Rotate(Global.Inst.MainCam, transform, transform);
        LookAtMouse_Logic.FlipX(Global.Inst.MainCam, gun_Main.SpriteRoot.transform, transform);

        if (gun_Main.IsSelected)
        {
            AnimSpeed_Logic.SetAnimSpeed(animator, gun_Main.gunData.shootTimer.endTime, maxShootAnimTime, gun_Main.WeaponNameTrimed + "_Shoot");
        }
    }
    #endregion
}

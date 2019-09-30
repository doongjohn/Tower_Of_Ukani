﻿using UnityEngine;

public class MachineGun_Main_Action : CLA_Action
{
    #region Var: Inspector
    [Header("Points")]
    [SerializeField] private Transform shootPoint;

    [Header("Prefabs")]
    [SerializeField] private PoolingObj bulletPrefab;

    [Header("Accuracy")]
    [SerializeField] private float acry_YPosOffset;
    [SerializeField] private float acry_ZRotOffset;

    [Header("Animation")]
    [SerializeField] private float maxShootAnimTime;

    [Header("Effects")]
    [SerializeField] private Transform shootParticleParent;
    [SerializeField] private PoolingObj shootParticlePrefab;
    [SerializeField] private CameraShake.Data camShakeData_Shoot;
    #endregion

    #region Var: Properties
    public bool AnimEnd_Shoot { get; private set; } = false;
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
    public override void OnEnd()
    {
        AnimEnd_Shoot = false;
        animator.speed = 1;
        animator.ResetTrigger("Shoot");
    }
    public override void OnUpdate()
    {
        if (!gun_Main.IsSelected)
            return;

        if (gun_Main.gunData.shootTimer.IsTimerAtMax && Input.GetKey(PlayerInputManager.Inst.Keys.MainAbility))
        {
            // Spawn Bullet
            Transform bullet = ObjPoolingManager.Activate(bulletPrefab, shootPoint.position, transform.rotation).transform;
            bullet.position += shootPoint.up * Random.Range(-acry_YPosOffset, acry_YPosOffset);
            bullet.rotation = Quaternion.Euler(0, 0, bullet.eulerAngles.z + Random.Range(-acry_ZRotOffset, acry_ZRotOffset));

            // Use Bullet
            gun_Main.gunData.loadedBullets -= 1;

            // Continue Timer
            gun_Main.gunData.shootTimer.Restart();

            // Animation
            AnimEnd_Shoot = false;
            animator.SetTrigger("Shoot");

            // Particle Effect
            ObjPoolingManager.Activate(shootParticlePrefab, shootParticleParent, new Vector2(0, 0), Quaternion.identity);

            // Cam Shake Effect
            CamShake_Logic.ShakeBackward(camShakeData_Shoot, transform);
        }
    }
    public override void OnLateUpdate()
    {
        AnimSpeed_Logic.SetAnimSpeed(animator, gun_Main.gunData.shootTimer.endTime, maxShootAnimTime, "Pistol_Shoot");
        LookAtMouse_Logic.Rotate(Global.Inst.MainCam, transform, transform);
        LookAtMouse_Logic.FlipX(Global.Inst.MainCam, gun_Main.SpriteRoot.transform, transform);
    }
    #endregion

    #region Method: AnimEvent
    private void OnAnimEnd_Shoot()
    {
        AnimEnd_Shoot = true;
    }
    #endregion
}

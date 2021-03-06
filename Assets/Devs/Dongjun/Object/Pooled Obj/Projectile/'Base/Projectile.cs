﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ProjectileData
{
    public FloatStat moveSpeed;
    public FloatStat travelDist;
    public FloatStat gravity;

    public void Reset()
    {
        moveSpeed.Reset();
        travelDist.Reset();
        gravity.Reset();
    }
}

public class Projectile : PoolingObj
{
    #region Var: Inspector
    [Header("Detection: Tag")]
    [SerializeField] protected string[] ignoreTags;

    [Header("Detection: Creature")]
    [SerializeField] protected Rigidbody2D creatureDetectRB;

    [Header("Detection: Wall")]
    [SerializeField] protected Rigidbody2D wallDetectRB;

    [Header("Visual")]
    [SerializeField] protected bool rotateToMovingDir = true;

    [Header("Effects")]
    [SerializeField] protected PoolingObj particle_Hit;
    [SerializeField] protected float particle_HitOffset;
    #endregion

    #region Var: Data
    protected Vector2 velocity;
    protected ProjectileData projectileData;
    protected AttackData attackData;
    #endregion

    #region Method: Init PoolingObj
    public override void OnSpawn()
    {
        // 왜 리지드 바디 위치가 아래를 보고 발사하면 0, 0 인건지는 모르겠음....
        // 아무튼 여기서 이니셜라이즈 해야함.
        if (creatureDetectRB != null) creatureDetectRB.position = transform.position;
        if (wallDetectRB != null) wallDetectRB.position = transform.position;

        velocity = Vector2.zero;
        projectileData.travelDist.Reset();
    }
    #endregion

    #region Method: Init Data
    public void InitData(Vector2 startDir, ProjectileData projectileData, AttackData attackData = new AttackData())
    {
        // Init Data
        this.projectileData = projectileData;
        this.attackData = attackData;

        // Init Velocity
        velocity = projectileData.moveSpeed.Value * startDir;
    }
    #endregion

    #region Method: Unity
    protected virtual void FixedUpdate()
    {
        Move();
    }
    #endregion

    #region Method: Move
    protected virtual void Move()
    {
        // Set Velocity
        velocity.y -= projectileData.gravity.Value * Time.fixedDeltaTime;

        // Move
        transform.Translate(velocity * Time.fixedDeltaTime, Space.World);

        // Update Travle Dist
        float dist = velocity.magnitude * Time.fixedDeltaTime;
        projectileData.travelDist.ModFlat += dist;

        // Detect Object
        if (!DetectCreature(dist)) DetectWall(dist);

        // On Maxt Distance
        if (projectileData.travelDist.Value >= projectileData.travelDist.Max)
        {
            OnMaxDist();
            ObjPoolingManager.Sleep(this);
        }

        // Rotate Towards Moving Dir
        if (rotateToMovingDir) transform.right = velocity.normalized;
    }
    protected virtual void OnMaxDist()
    {

    }
    #endregion

    #region Method: Detect Object
    protected bool IsVaildTag(Component target)
    {
        foreach (var tag in ignoreTags)
            if (target.CompareTag(tag))
                return false;

        return true;
    }
    protected virtual bool DamageCreature(GameObject hit)
    {
        return false;
    }
    protected virtual bool DetectCreature(float dist)
    {
        if (creatureDetectRB == null)
            return false;

        // Check Creature
        List<RaycastHit2D> creatureHits = new List<RaycastHit2D>();
        creatureDetectRB.Cast(velocity.normalized, creatureHits, dist);

        // Run Logic from Closest Creature
        foreach (var hit in creatureHits.OrderByDescending(o => Vector2.SqrMagnitude((Vector2)creatureDetectRB.transform.position - o.point)).Reverse())
        {
            if (!IsVaildTag(hit.collider))
                continue;

            if (!DamageCreature(hit.collider.gameObject))
                continue;

            OnHit(hit.point);
            return true;
        }

        return false;
    }
    protected virtual bool DetectWall(float dist)
    {
        if (wallDetectRB == null)
            return false;

        // Check Wall
        List<RaycastHit2D> wallHits = new List<RaycastHit2D>();
        wallDetectRB.Cast(velocity.normalized, wallHits, dist);

        // Run Logic from Closest Wall
        foreach (var hit in wallHits.OrderByDescending(o => Vector2.SqrMagnitude((Vector2)wallDetectRB.transform.position - o.point)).Reverse())
        {
            if (!IsVaildTag(hit.collider))
                continue;

            OnHit(hit.point);
            return true;
        }

        return false;
    }
    protected virtual void OnHit(Vector2 hitPos)
    {
        // Sleep
        this.Sleep();

        // Spawn Hit Effect
        if (particle_Hit == null)
            return;

        Transform hitParticle = particle_Hit.Spawn(hitPos, Quaternion.identity).transform;
        hitParticle.right = -velocity.normalized;
        hitParticle.position -= (Vector3)velocity.normalized * particle_HitOffset;
    }
    #endregion
}

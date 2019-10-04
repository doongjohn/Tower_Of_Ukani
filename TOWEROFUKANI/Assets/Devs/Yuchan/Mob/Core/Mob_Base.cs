﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum eMobAniST
{
    Attack,
    Hit,
    Walk,
    Idle,
    AirborneDown,
    AirborneUp,
    Last,
}

[Serializable]
public struct AniSpeedData
{
    public eMobAniST ST;
    public float t;
}

public class Mob_Base : MonoBehaviour, IHurt, ICanDetectGround
{
    #region Var: Inspector

    [Header("Collision")]
    [SerializeField] BoxCollider2D m_OneWayCollider;

    [SerializeField] eMobAniST m_CurAniST;

    [Header("Attack")]
    [SerializeField] float m_AttackRange;

    [SerializeField] AniSpeedData[] m_AniSpeedData;
    [SerializeField] MobMoveData m_MoveData;
    [SerializeField] FollowData m_followData;
    [SerializeField] CorpseData m_compseData;
    [SerializeField] JumpData m_jumpData;
    [SerializeField] GravityData m_gravityData;
    [SerializeField] GroundDetectionData m_groundDetectionData;

    #endregion

    #region Var: Component
    protected Rigidbody2D m_rb;
    protected Animator m_ani;
    #endregion

    #region Var:
    Dictionary<eMobAniST, (string, float)> m_Ani = new Dictionary<eMobAniST, (string, float)>();
    GroundInfo mCurGroundInfo = new GroundInfo();

    protected bool m_bGrounded = false;
    protected bool m_bJumping = false;
    protected bool m_bJumpStart = false;
    protected bool m_bFallStart = false;
    protected bool m_bHurting = false;
    protected bool m_bAniStart = false;
    protected bool m_bYMoveCoolTime = false;
    protected bool m_bMoveAble = true;
    protected bool m_bAttacking = false;

    protected int m_bPrevDir = 0;
    #endregion

    #region Var: Properties
    float VelX =>
     m_bAttacking ? 0 :
     CanAttack ? 0 :
     CanFollow ? m_MoveData.Speed * m_MoveData.Dir :
     m_bHurting || !m_bGrounded ? 0
    : !CliffDetect_Logic.CanFall(m_MoveData.FallHeight, transform, m_MoveData.Dir * m_groundDetectionData.Size.x, m_groundDetectionData.GroundLayers) ? 0
    : m_MoveData.Speed * m_MoveData.Dir;

    float VelY => m_rb.velocity.y;
    public bool CanFollow => ((GM.PlayerPos - transform.position).magnitude < m_followData.dis);
    public bool CanAttack => m_bAttacking ? true : m_bAttacking = ((GM.PlayerPos - transform.position).magnitude < m_AttackRange);

    #endregion

    #region Method:
    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_ani = GetComponent<Animator>();
        if (m_MoveData.State == MobMoveData.eState.Move) OnMoveRandom();
        if (m_MoveData.State == MobMoveData.eState.Idle) OnIdleRandom();

        for (int i = 0; i < (int)eMobAniST.Last; i++)
            m_Ani[(eMobAniST)i] = ($"{gameObject.name}_{((eMobAniST)i).ToString()}", 1);
        foreach (var i in m_AniSpeedData)
            m_Ani[i.ST] = ($"{gameObject.name}_{i.ST.ToString()}", i.t);

        m_groundDetectionData.Size = m_OneWayCollider.size;
    }
    private void FixedUpdate()
    {
        GroundDetection_Logic.DetectGround(!m_bJumping, m_rb, transform, m_groundDetectionData, ref m_bGrounded, ref mCurGroundInfo);
        GroundDetection_Logic.ExecuteOnGroundMethod(this, m_bGrounded, ref m_groundDetectionData);

        m_rb.velocity = new Vector2(VelX, VelY);

        GroundDetection_Logic.FallThrough(ref m_bFallStart, m_bGrounded, m_rb, transform, m_OneWayCollider, m_groundDetectionData);

        Jump_Logic.Jump(ref m_bJumpStart, ref m_bJumping, ref m_jumpData, m_rb, transform);

        Gravity_Logic.ApplyGravity(m_rb, m_bGrounded ? new GravityData(false, 0, 0) : !m_bJumping ? m_gravityData : new GravityData(true, m_jumpData.jumpGravity, 0));

        Anim_Logic.SetAnimSpeed(m_ani, m_Ani[m_CurAniST].Item2);
        if (m_bAniStart) { m_ani.Play(m_Ani[m_CurAniST].Item1, 0, 0); m_bAniStart = false; }
        else m_ani.Play(m_Ani[m_CurAniST].Item1);
    }
    private void OnDestroy()
    {
        ATimer.Pop("JumpFall" + GetInstanceID());
    }
    #endregion

    #region Method: Move
    public void OnMoveRandom()
    {
        ATimer.Set(this, m_MoveData.MoveT.Get, OnIdleRandom);
        m_MoveData.State = MobMoveData.eState.Move;
        m_MoveData.Dir = ARandom.Dir;
    }
    public void OnIdleRandom()
    {
        ATimer.Set(this, m_MoveData.IdleT.Get, OnMoveRandom);
        m_MoveData.State = MobMoveData.eState.Idle;
    }
    public bool MoveRandom()
    {
        if (!m_bGrounded) return false;
        m_MoveData.State = MobMoveData.eState.Move;
        m_CurAniST = eMobAniST.Walk;
        ATimer.Tick(this);
        if (!m_bYMoveCoolTime)
        {
            m_bYMoveCoolTime = ARandom.Get(70) ? true : ARandom.Get(50) ? Jump() : Fall();
            m_bYMoveCoolTime = true;
            ATimer.Set("JumpFall" + GetInstanceID(), m_MoveData.CoolTime, () => { m_bYMoveCoolTime = false; });
        }
        return true;
    }
    public bool IdleRandom()
    {
        if (m_MoveData.State == MobMoveData.eState.Idle)
        {
            m_CurAniST = eMobAniST.Idle;
            m_MoveData.Dir = 0;
        }
        return true;
    }
    public bool Falling() { if (!m_bJumping) m_CurAniST = eMobAniST.AirborneDown; return true; }
    public bool Jump()
    {
        if (m_bJumpStart = MobYMoveDetect_Logic.UP(transform.position, ref m_jumpData, ref m_MoveData))
            m_CurAniST = eMobAniST.AirborneUp;

        return m_bJumpStart;
    }
    public bool Fall()
    {
        if (m_bFallStart = MobYMoveDetect_Logic.Down(transform.position, m_groundDetectionData.Size, ref m_MoveData))
            m_CurAniST = eMobAniST.AirborneDown;

        return m_bFallStart;
    }

    #endregion

    // TODO : 


    public bool Follow()
    {
        var act = Follow_Logic.Follow(ref m_followData, ref m_MoveData, ref m_jumpData, m_groundDetectionData.Size, m_MoveData.Dir, transform.position);
        m_CurAniST = eMobAniST.Walk;
        switch (act)
        {
            case eMoveAction.JumpProcess:               return m_bJumpStart = true;
            case eMoveAction.Jump:                      return Jump(); 
            case eMoveAction.DownJump:                  return Fall(); 
            case eMoveAction.Left: m_MoveData.Dir = -1; return true;
            case eMoveAction.Right: m_MoveData.Dir = 1; return true;
            default: break;
        }
        return true;
    }
    public bool Attack()
    {
        m_CurAniST = eMobAniST.Attack;
        return m_bAttacking;
    }
    private void AttackEnd()
    {
        m_bAttacking = false;
    }


    public virtual void OnHurt(/* 총알 위치 */)
    {
        if (!m_bHurting)
        {
            m_bPrevDir = m_MoveData.SprDir;
            m_MoveData.SprDir *= (Mathf.Sign(GM.PlayerPos.x - transform.position.x) == m_bPrevDir) ? -1 : 1;
        }
        m_bHurting = true;
        m_bAniStart = true;

        m_CurAniST = eMobAniST.Hit;
    }
    public bool Hurting() => m_bHurting;
    private void HurtEnd() { m_bHurting = false; m_MoveData.SprDir = m_bPrevDir; }

    #region Interface: IHurt

    public virtual void OnDead()
    {
        Destroy(gameObject);
        CorpseMgr.CreateCorpseOrNull(transform, m_compseData);
    }
    #endregion

    #region Interface: ICanDetectGround
    virtual public void OnGroundEnter()
    {
        Jump_Logic.ResetJumpCount(ref m_jumpData);
        m_CurAniST = eMobAniST.Idle;
    }
    virtual public void OnGroundStay()
    {
    }
    virtual public void OnGroundExit()
    {
    }
    #endregion
}
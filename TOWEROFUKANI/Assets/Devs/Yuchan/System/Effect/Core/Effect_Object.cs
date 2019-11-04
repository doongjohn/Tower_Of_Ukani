﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Object : MonoBehaviour
{
    [SerializeField] Transform feetPos;
    [SerializeField] SpriteRenderer Pspr;
    void CreateAirJump(  )
    {
        AirJumpEft.Create(feetPos.position);
    }

    void CreateWalkDust(  )
    {
        var obj = PlayerWalkDustEft.Create(feetPos.position);


        obj.GetComponentInChildren<SpriteRenderer>().flipX = Global.Inst.PlayerRB2D.velocity.x >= 0 ? false : true;
    }



}

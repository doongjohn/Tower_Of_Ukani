﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour
{
    static GM Inst;

    Player player;

    public static Player Player => Inst.player;
    public static Vector2 PlayerSize => new Vector2(0.5f,0.8f);
    public static GameObject PlayerObj => Inst.player.gameObject;
    public static Vector3 PlayerPos => Inst.player.transform.position;
    public static Dictionary<string, Texture2D> MapSize = new Dictionary<string, Texture2D>();
    public static Texture2D CurMapSize => MapSize[CurMapName];
    public static string CurMapName;
    public static Vector2 CurMapCenter;
    public static Vector2 CurMapWorldPoint => CurMapCenter + new Vector2((CurMapSize.width / 2) + ((CurMapSize.width % 2 == 0) ? 0.5f : 0), (CurMapSize.height / 2) + ((CurMapSize.height % 2 == 0) ? 0.5f : 0));

    public float pos;


    void Awake()
    {
        Inst = this;
        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        pos = (GM.CurMapWorldPoint.x) - CurMapSize.width;
    }
}

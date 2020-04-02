﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct RangeInt
{
    public int min;
    public int max;
}

[System.Serializable]
public struct ItemDropInfo
{
    public Item item;
    public float drop_chance;
    public RangeInt DropCount;
}


public class Mob_DropItem : MonoBehaviour
{
    [SerializeField] int DropCount;
    [SerializeField] ItemDropInfo[] DropInfos;


    public void OnDead()
    {
        List<ItemDropInfo> r = new List<ItemDropInfo>();
        for (int i = 0; i < DropCount; i++)
        {
            List<ItemDropInfo> pick = new List<ItemDropInfo>();
            foreach(var ii in DropInfos) 
                if (ARandom.Get(ii.drop_chance)) pick.Add(ii);
            if (pick.Count == 0) continue;
            r.Add(pick[Random.Range(0, pick.Count)]);
        }
        foreach (var i in r)
            ItemDB.Inst.SpawnDroppedItem(
                transform.position, 
                i.item.Info.ItemName, 
                Random.Range(i.DropCount.min, i.DropCount.max)
                ).GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle * Random.Range(100f,200f));
    }
}
﻿using Dongjun.Helper;
using UnityEngine;

public class UI_PlayerStaminaBar : MonoBehaviour
{
    [SerializeField] private Transform staminaBarParent;
    [SerializeField] private UI_Fill staminaBarPrefab;

    private UI_Fill[] staminaBars;

    private void Awake()
    {
        staminaBarParent.ClearChildren();
    }
    private void Start()
    {
        SetUp_StaminaBar();
        PlayerStats.Inst.AddEvent_OnStaminaChange(gameObject, Update_StaminaBar);
    }

    private void SetUp_StaminaBar()
    {
        staminaBars = new UI_Fill[(int)PlayerStats.Inst.Stamina.Max];

        for (int i = 0; i < staminaBars.Length; i++)
        {
            staminaBars[i] = Instantiate(staminaBarPrefab, staminaBarParent).GetComponent<UI_Fill>();
            staminaBars[i].Value = PlayerStats.Inst.Stamina.Value >= i + 1 ? 1 : PlayerStats.Inst.Stamina.Value - i;
        }
    }

    private void Update_StaminaBar(FloatStat stamina)
    {
        for (int i = 0; i < staminaBars.Length; i++)
        {
            staminaBars[i].Value = stamina.Value >= i + 1 ? 1 : stamina.Value - i;
        }
    }
}

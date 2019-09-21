﻿using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CLA_Main : MonoBehaviour
{
    [SerializeField]
    private CLA_Action defaultAction;
    protected CLA_Action DefaultAction => defaultAction;
    protected CLA_Action CurrentAction { get; private set; }

    protected Dictionary<CLA_Action, Action> ConditionLogics = new Dictionary<CLA_Action, Action>();


    private void Awake()
    {
        Init();
    }
    private void Start()
    {
        if (defaultAction == null)
            Debug.LogError("It Needs Default Action!");

        CurrentAction = defaultAction;
        CurrentAction?.OnStart();
    }
    private void Update()
    {
        CurrentAction?.OnUpdate();

        if (CurrentAction != null && ConditionLogics.ContainsKey(CurrentAction))
            ConditionLogics[CurrentAction]?.Invoke();
    }
    private void FixedUpdate()
    {
        CurrentAction?.OnFixedUpdate();
    }

    protected abstract void Init();
    protected void ChangeAction(CLA_Action action)
    {
        CurrentAction?.OnEnd();
        CurrentAction = action;
        CurrentAction?.OnStart();
    }
}

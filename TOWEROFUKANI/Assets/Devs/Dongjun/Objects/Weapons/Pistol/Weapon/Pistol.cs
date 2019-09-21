﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Weapon
{
    private Pistol_Main_Action pistol_Main_Action;

    protected override void Init()
    {
        pistol_Main_Action = GetComponent<Pistol_Main_Action>();

        ConditionLogics.Add(pistol_Main_Action, CL_MainAction);
    }

    private void CL_MainAction()
    {
        // Write Condition Logic Here
    }
}

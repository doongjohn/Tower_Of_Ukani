﻿using UnityEngine;

public class Equilibrium : PassiveItem
{
    private ItemEffect onDamagedEffect;
    private ItemEffect onHitEffect;

    private float effectPercent = 30f;

    #region Method: Initialize
    public override void InitStats()
    {

    }
    #endregion

    public override void OnAdd(InventoryBase inventory)
    {
        base.OnAdd(inventory);

        onDamagedEffect = new ItemEffect(GetType(), OnDamaged);
        onHitEffect = new ItemEffect(GetType(), OnHit);

        ItemEffectManager.AddEffect(PlayerActions.Damaged, onDamagedEffect);
        ItemEffectManager.AddEffect(PlayerActions.Hit, onHitEffect);
    }
    public override void OnDrop()
    {
        base.OnDrop();
        ItemEffectManager.RemoveEffect(PlayerActions.Damaged, onDamagedEffect);
        ItemEffectManager.RemoveEffect(PlayerActions.Hit, onHitEffect);
    }

    public override void ApplyBonusStats(WeaponItem weapon)
    {

    }

    private void OnDamaged()
    {
        PlayerStats.Inst.DamageReceived += MathD.Round(PlayerStats.Inst.DamageReceived * (effectPercent * 0.01f));
    }
    private void OnHit()
    {
        PlayerStats.Inst.DamageToDeal += MathD.Round(PlayerStats.Inst.DamageToDeal * (effectPercent * 0.01f));
    }
}
﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Bloodseeker : ActiveItem
{
    #region Var: Inspector
    [SerializeField] private GameObject shieldEffectPrefab;
    #endregion

    #region Var: Stats
    private TimerData durationTimer = new TimerData();
    private FloatStat shieldHealth;
    #endregion

    #region Var: Visual Effect
    private GameObject shieldEffect;
    #endregion

    #region Var: Item Effect
    private PlayerActionEvent onKill;
    private PlayerActionEvent onDamageReceived;

    int toAbsorb = 0;
    List<GameObject> coprsePrefabs = new List<GameObject>();
    Coroutine checkAllCorpseAbsorbed;
    #endregion

    #region Method: Unity
    protected override void Awake()
    {
        base.Awake();

        // Player Action Event
        onKill = this.NewPlayerActionEvent(() =>
        {
            var corpseSpawner = PlayerStats.Inst.KilledMob.GetComponent<CorpseSpawner>();
            if (corpseSpawner == null)
                return;

            // Add Corpse Count
            toAbsorb += corpseSpawner.CorpseCount;

            // On Corpse Absorb
            corpseSpawner.SetCorpseMode(eCorpseSpawnMode.Absorb, coprpsePrefab =>
            {
                if (!IsActive)
                    return;

                coprsePrefabs.Add(coprpsePrefab);
                toAbsorb -= 1;
                shieldHealth.ModFlat += 5;

                // Show Effect
                shieldEffect.SetActive(true);
            });
        });

        onDamageReceived = this.NewPlayerActionEvent(() =>
        {
            // Calculate Overkill Damage
            float overkillDmg = Mathf.Max(PlayerStats.Inst.DamageReceived - shieldHealth.Value, 0);

            // Damage Shield
            shieldHealth.ModFlat -= PlayerStats.Inst.DamageReceived;

            // Damage Player
            PlayerStats.Inst.DamageReceived = overkillDmg;

            // Hide Effect
            if (shieldHealth.Value == 0)
                shieldEffect.SetActive(false);
        });
    }
    private IEnumerator CheckAllCorpseAbsorbed()
    {
        while(true)
        {
            yield return new WaitForEndOfFrame();

            if (IsActive && durationTimer.IsEnded && toAbsorb == 0)
            {
                PlayerStats.Inst.Heal(shieldHealth.Value * 0.6f);
                Deactivate();

                checkAllCorpseAbsorbed = null;
                yield return null;
            }
        }
    }
    #endregion

    #region Method: Stats
    public override void InitStats()
    {
        // Init Cooldown
        CooldownTimer.EndTime = 10f;

        // Init Mana Usage
        ManaUsage = new FloatStat(20, min: 0);

        // Init Duration
        durationTimer.EndTime = 10f;
        durationTimer
            .SetTick(gameObject)
            .SetAction(
                onEnd: () => 
                {
                    checkAllCorpseAbsorbed = StartCoroutine(CheckAllCorpseAbsorbed());
                    PlayerActionEventManager.RemoveEvent(PlayerActions.Kill, onKill);
                })
            .SetActive(false);

        // Init Shield HP
        shieldHealth = new FloatStat(0, min: 0, max: 20);
    }
    #endregion

    #region Method: Item
    public override void OnAdd(InventoryBase inventory)
    {
        base.OnAdd(inventory);

        // Spawn Effect
        shieldEffect = Instantiate(shieldEffectPrefab, GM.Player.transform.GetChild(0));
        shieldEffect.SetActive(false);
    }
    protected override void OnRemovedFromInventory()
    {
        base.OnRemovedFromInventory();

        // Destroy Effect
        Destroy(shieldEffect);
    }
    #endregion

    #region Method: Activate / Deactivate
    protected override void OnActivate()
    {
        // Stop Cooldown Timer
        CooldownTimer.SetActive(false);
        CooldownTimer.Reset();

        // Start Duration Timer
        durationTimer.SetActive(true);
        durationTimer.Reset();

        PlayerActionEventManager.AddEvent(PlayerActions.Kill, onKill);
        PlayerActionEventManager.AddEvent(PlayerActions.HealthDamaged, onDamageReceived);
    }
    public override void Deactivate()
    {
        base.Deactivate();

        // Start Cooldown Timer
        CooldownTimer.SetActive(true);
        CooldownTimer.Reset();

        // Stop Duration Timer
        durationTimer.SetActive(false);
        durationTimer.Reset();

        // Reset Shield HP
        shieldHealth.Reset();

        toAbsorb = 0;
        coprsePrefabs.Clear();

        if (checkAllCorpseAbsorbed != null)
            StopCoroutine(checkAllCorpseAbsorbed);

        // Hide Effect
        shieldEffect.SetActive(false);

        PlayerActionEventManager.RemoveEvent(PlayerActions.Kill, onKill);
        PlayerActionEventManager.RemoveEvent(PlayerActions.HealthDamaged, onDamageReceived);
    }
    #endregion
}

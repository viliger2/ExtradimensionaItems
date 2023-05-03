using IL.RoR2.UI;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Items.ItemBehaviors
{
    // attach it to master for it to last through stages
    // use NetworkWeaver after build to patch dll so it actually works
    public class ReturnalAdrenalineItemBehavior : NetworkBehaviour
    {
        public CharacterMaster master;

        [SyncVar]
        public int adrenalineLevel;

        [SyncVar]
        public float adrenalinePerLevel;

        private float previousHp;

        private float stopwatch;

        public void Awake()
        {
            enabled = false;
        }

        public void OnEnable()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            if (master && master.GetBody())
            {
                previousHp = master.GetBody().healthComponent.health;
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body.master == master)
            {
                var itemCount = master.inventory.GetItemCount(Content.Items.ReturnalAdrenaline);
                args.attackSpeedMultAdd += ((ReturnalAdrenaline.AttackSpeedBonus.Value / 100) + ((ReturnalAdrenaline.AttackSpeedBonusPerStack.Value / 100) * (itemCount - 1))) * ((adrenalineLevel >= (adrenalinePerLevel * 1)) ? 1 : 0);
                args.moveSpeedMultAdd += ((ReturnalAdrenaline.MovementSpeedBonus.Value / 100) + ((ReturnalAdrenaline.MovementSpeedBonusPerStack.Value / 100) * (itemCount - 1))) * ((adrenalineLevel >= (adrenalinePerLevel * 2)) ? 1 : 0);
                args.baseHealthAdd += ((ReturnalAdrenaline.HealthBonus.Value) + ((ReturnalAdrenaline.HealthBonusPerStack.Value) * (itemCount - 1))) * ((adrenalineLevel >= (adrenalinePerLevel * 3)) ? 1 : 0);
                args.baseShieldAdd += (body.maxHealth * (ReturnalAdrenaline.ShieldBonus.Value / 100) + (body.maxHealth * (ReturnalAdrenaline.ShieldBonusPerStack.Value / 100) * (itemCount - 1))) * ((adrenalineLevel >= (adrenalinePerLevel * 4)) ? 1 : 0);
                args.critAdd += ((ReturnalAdrenaline.CritBonus.Value) + ((ReturnalAdrenaline.CritBonusPerStack.Value) * (itemCount - 1))) * ((adrenalineLevel >= (adrenalinePerLevel * 5)) ? 1 : 0);
            }
        }

        public void OnDisable()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            adrenalineLevel = 0;
        }

        public void OnDestroy()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (enabled && adrenalineLevel < adrenalinePerLevel * 5)
            {
                CharacterBody attackerBody = damageReport.attackerBody;
                if (attackerBody && attackerBody == master.GetBody())
                {
                    if (damageReport.victimIsElite)
                    {
                        adrenalineLevel += ReturnalAdrenaline.EliteEnemyReward.Value;
                    }
                    else if (damageReport.victimIsChampion)
                    {
                        adrenalineLevel += ReturnalAdrenaline.BossEnemyReward.Value;
                    }
                    else
                    {
                        adrenalineLevel += ReturnalAdrenaline.NormalEnemyReward.Value;
                    }
                    if (adrenalineLevel >= adrenalinePerLevel * 5)
                    {
                        adrenalineLevel = (int)(adrenalinePerLevel * 5);

                        if (ReturnalAdrenaline.MaxLevelProtection.Value)
                        {
                            master.GetBody().AddBuff(Content.Buffs.ReturnalMaxLevelProtection);
                        }
                    }
                    MyLogger.LogMessage("new stack number {0}", adrenalineLevel.ToString());
                }
            }
        }

        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch < ReturnalAdrenaline.HealthCheckFrequency.Value)
            {
                return;
            }

            var body = master.GetBody();

            stopwatch -= ReturnalAdrenaline.HealthCheckFrequency.Value;

            if (body)
            {
                if ((previousHp - body.healthComponent.health) > body.healthComponent.fullHealth * 0.2)
                {
                    if (body.HasBuff(Content.Buffs.ReturnalMaxLevelProtection))
                    {
                        body.RemoveBuff(Content.Buffs.ReturnalMaxLevelProtection);
                        MyLogger.LogMessage("saved by max level buff");
                    }
                    else
                    {
                        adrenalineLevel = 0;
                        MyLogger.LogMessage("lost all stacks");
                    }
                }

                previousHp = body.healthComponent.health;
            }
        }

        public void RecalculatePerLevelValue(int count)
        {
            adrenalinePerLevel = ReturnalAdrenaline.KillsPerLevel.Value * ((100f - Util.ConvertAmplificationPercentageIntoReductionPercentage(ReturnalAdrenaline.KillsPerLevelPerStack.Value * (count - 1))) / 100f);
            MyLogger.LogMessage("Current per level exp {0}", adrenalinePerLevel.ToString());
        }
    }

}

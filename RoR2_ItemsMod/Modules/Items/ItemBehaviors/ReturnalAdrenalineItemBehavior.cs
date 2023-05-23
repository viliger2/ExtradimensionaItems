using ExtradimensionalItems.Modules.UI;
using IL.RoR2.UI;
using R2API;
using RoR2;
using RoR2.Audio;
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

        public CharacterBody body;

        [SyncVar]
        public int adrenalineLevel;

        [SyncVar]
        public float adrenalinePerLevel;

        private float previousHp;

        private float stopwatch;

        private int currentLevel = 0; // just for the sound

        private GameObject glow;

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
                SetupBody();
            }
        }

        private void SetupBody()
        {
            body = master.GetBody();
            previousHp = body.healthComponent.health;
            glow = FindGlow();
            if (glow)
            {
                glow.SetActive(false);
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
            currentLevel = 0;
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
                if (attackerBody && attackerBody == body)
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
                            body.AddBuff(Content.Buffs.ReturnalMaxLevelProtection);
                            MyLogger.LogMessage("Player {0}({1}) reached max level of ReturnalAdrenaline, adding ReturnalMaxLevelProtection buff", body.GetUserName(), body.name);
                        }
                    }

                    if(currentLevel != (int)(adrenalineLevel / adrenalinePerLevel))
                    {
                        EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_Returnal_LevelUp", body.gameObject);
                        currentLevel = (int)(adrenalineLevel / adrenalinePerLevel);
                        RpcSetGlow();
                    }
                }
            }
        }

        [ServerCallback]
        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch < ReturnalAdrenaline.HealthCheckFrequency.Value)
            {
                return;
            }

            stopwatch -= ReturnalAdrenaline.HealthCheckFrequency.Value;

            if (body)
            {
                if ((previousHp - body.healthComponent.health) > body.healthComponent.fullHealth * 0.2)
                {
                    if (body.HasBuff(Content.Buffs.ReturnalMaxLevelProtection))
                    {
                        body.RemoveBuff(Content.Buffs.ReturnalMaxLevelProtection);
                        MyLogger.LogMessage("Player {0}({1}) has been damaged equal to threshold, removing ReturnalMaxLevelProtection buff", body.GetUserName(), body.name);
                    }
                    else
                    {
                        adrenalineLevel = 0;
                        currentLevel = 0;
                        RpcSetGlow();
                        EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_Returnal_Break", body.gameObject);
                        MyLogger.LogMessage("Player {0}({1}) has been damaged equal to threshold, losing all item's levels", body.GetUserName(), body.name);
                    }
                }

                previousHp = body.healthComponent.health;
            } else if (master.GetBody())
            {
                // it is here since on stage transitions master can exist without a body
                // so when we awake together with master body simply wont be there
                 SetupBody();
            }
        }

        public void Update()
        {
            if(master)
            {
                var instance = ReturnalAdrenalineUI.FindInstance(master);
                if (instance && !instance.gameObject.activeSelf && master.hasEffectiveAuthority)
                {
                    instance.Enable();
                }
            }
        }

        public void RecalculatePerLevelValue(int count)
        {
            adrenalinePerLevel = ReturnalAdrenaline.KillsPerLevel.Value * ((100f - Util.ConvertAmplificationPercentageIntoReductionPercentage(ReturnalAdrenaline.KillsPerLevelPerStack.Value * (count - 1))) / 100f);
            if (body)
            {
                MyLogger.LogMessage("Player {0}({1}) has picked up additional stack of ReturnalAdrenaline, new per level requirement is {2}", body.GetUserName(), body.name, adrenalinePerLevel.ToString());
            }
        }

        private GameObject FindGlow()
        {
            if (body)
            {
                var modelLocator = body.GetComponent<ModelLocator>();
                if (modelLocator)
                {
                    var characterModel = modelLocator.modelTransform.GetComponent<CharacterModel>();
                    if(characterModel)
                    {
                        List<GameObject> list = characterModel.GetItemDisplayObjects(Content.Items.ReturnalAdrenaline.itemIndex);
                        if(list.Count > 0)
                        {
                            var glowTransform = list[0].transform.Find("Effect");
                            if (glowTransform)
                            {
                                return glowTransform.gameObject;
                            }
                        }
                    }
                }
            }

            return null;
        }

        [ClientRpc]
        private void RpcSetGlow()
        {
            if (glow)
            {
                glow.SetActive(currentLevel > 0);

                var particles = glow.GetComponent<ParticleSystem>();

                if (particles) {
                    particles.Clear();
                    var main = particles.main;
                    switch (currentLevel)
                    {
                        case 1:
                            main.startColor = Color.yellow;
                            break;
                        case 2:
                            main.startColor = Color.gray;
                            break;
                        case 3:
                            main.startColor = Color.white;
                            break;
                        case 4:
                            main.startColor = Color.blue;
                            break;
                        case 5:
                            main.startColor = Color.red;
                            break;
                    }
                    particles.Play();
                }
            }
        }

    }

}

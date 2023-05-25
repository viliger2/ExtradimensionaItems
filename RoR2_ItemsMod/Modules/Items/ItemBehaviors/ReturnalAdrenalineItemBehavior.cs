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
using static RoR2.CharacterMaster;

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

        private int currentLevel = 0;

        private float previousHp = 0f;

        private float stopwatch;

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
                master.onBodyStart += Master_onBodyStart;
                SetupBody();
            }
        }

        public void OnDisable()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            if (master)
            {
                master.onBodyStart -= Master_onBodyStart;
            }
            adrenalineLevel = 0;
            currentLevel = 0; // so if the player picks it up again sound doesn't play
        }

        public void OnDestroy()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            if (master)
            {
                master.onBodyStart -= Master_onBodyStart;
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
                        //currentLevel = 0;
                        EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_Returnal_Break", body.gameObject);
                        MyLogger.LogMessage("Player {0}({1}) has been damaged equal to threshold, losing all item's levels", body.GetUserName(), body.name);
                    }
                }

                previousHp = body.healthComponent.health;
            }
        }

        public void Update()
        {
            if (!ReturnalAdrenaline.DisableHUD.Value)
            {
                if (master)
                {
                    var instance = ReturnalAdrenalineUI.FindInstance(master);
                    if (instance && !instance.gameObject.activeSelf && master.hasEffectiveAuthority)
                    {
                        instance.Enable();
                    }
                }
            }
            if (glow)
            {
                if (currentLevel != (int)(adrenalineLevel / adrenalinePerLevel))
                {
                    Util.PlaySound("EI_Returnal_LevelUp", body.gameObject);
                    currentLevel = (int)(adrenalineLevel / adrenalinePerLevel);
                    SetGlow();
                }
            }
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
                }
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

        private void Master_onBodyStart(CharacterBody body)
        {
            this.body = body;
            SetupBody();
        }

        private void SetupBody()
        {
            if (!body)
            {
                body = master.GetBody();
            }
            if (body)
            {
                if (body.healthComponent)
                {
                    previousHp = body.healthComponent.health;
                }
                glow = FindGlow();
                SetGlow();
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
                    if (characterModel)
                    {
                        List<GameObject> list = characterModel.GetItemDisplayObjects(Content.Items.ReturnalAdrenaline.itemIndex);
                        if (list.Count > 0)
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

        private void SetGlow()
        {
            if (glow)
            {
                glow.SetActive(currentLevel > 0);

                var particles = glow.GetComponent<ParticleSystem>();

                if (particles)
                {
                    particles.Clear();
                    var main = particles.main;
                    switch (currentLevel)
                    {
                        case 1:
                            main.startColor = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f);
                            break;
                        case 2:
                            main.startColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.5f);
                            break;
                        case 3:
                            main.startColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f);
                            break;
                        case 4:
                            main.startColor = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.5f);
                            break;
                        case 5:
                            main.startColor = new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
                            break;
                    }
                    particles.Play();
                }
            }
        }

    }

}

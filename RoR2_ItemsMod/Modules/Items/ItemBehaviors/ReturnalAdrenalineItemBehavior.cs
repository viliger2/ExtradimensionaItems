using ExtradimensionalItems.Modules.UI;
using R2API;
using RoR2;
using RoR2.Audio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Items.ItemBehaviors
{
    // attach it to master for it to last through stages
    // use NetworkWeaver after build to patch dll so it actually works
    public class ReturnalAdrenalineItemBehavior : NetworkBehaviour
    {
        [SyncVar]
        public int adrenalineLevel;

        [SyncVar]
        public float adrenalinePerLevel;

        public int itemCount;

        public CharacterMaster master;

        private CharacterBody body;

        private int currentLevel = 0;

        private float previousHp = 0f;

        private float stopwatch;

        private GameObject glow;

        private GameObject maxLevelEffect;

        // since buffs are cleared on stage transition 
        // we need some way to keep them between stages
        private bool hasProtectionBuff;

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
            if (body && ReturnalAdrenaline.MaxLevelProtection.Value)
            {
                body.RemoveBuff(Content.Buffs.ReturnalMaxLevelProtection);
            }
            adrenalineLevel = 0;
            currentLevel = 0; // so if the player picks it up again sound doesn't play
            itemCount = 0; // so when we pick it up again checks actually work
        }

        public void OnDestroy()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            if (master)
            {
                master.onBodyStart -= Master_onBodyStart;
            }
            if (body && ReturnalAdrenaline.MaxLevelProtection.Value)
            {
                body.RemoveBuff(Content.Buffs.ReturnalMaxLevelProtection);
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
                if ((previousHp - body.healthComponent.health) > body.healthComponent.fullHealth * (ReturnalAdrenaline.CriticalDamage.Value / 100f))
                {
                    if (body.HasBuff(Content.Buffs.ReturnalMaxLevelProtection))
                    {
                        EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_Returnal_Break", body.gameObject);
                        body.RemoveBuff(Content.Buffs.ReturnalMaxLevelProtection);
                        hasProtectionBuff = false;
                        MyLogger.LogMessage("Player {0}({1}) has been damaged equal to threshold, removing ReturnalMaxLevelProtection buff", body.GetUserName(), body.name);
                    }
                    else if(adrenalineLevel > 0)
                    {
                        adrenalineLevel = 0;
                        EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_Returnal_LevelDown", body.gameObject);
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
            if (currentLevel != (int)(adrenalineLevel / adrenalinePerLevel))
            {
                currentLevel = (int)(adrenalineLevel / adrenalinePerLevel);
                if (currentLevel > 0) Util.PlaySound("EI_Returnal_LevelUp", body.gameObject);
                if (glow)
                {
                    SetGlow();
                    // we don't need glow for "bubble" effect
                    // but checking for glow ensures that item display exists
                    if (ReturnalAdrenaline.MaxLevelProtection.Value)
                    {
                        ManageMaxLevelProtectionEffect();
                    }
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
                    if (damageReport.victimIsChampion)
                    {
                        adrenalineLevel += ReturnalAdrenaline.BossEnemyReward.Value;
                    }
                    else if (damageReport.victimIsElite)
                    {
                        adrenalineLevel += ReturnalAdrenaline.EliteEnemyReward.Value;
                    }
                    else
                    {
                        adrenalineLevel += ReturnalAdrenaline.NormalEnemyReward.Value;
                    }
                    if (adrenalineLevel >= adrenalinePerLevel * 5)
                    {
                        adrenalineLevel = Mathf.CeilToInt(adrenalinePerLevel * 5);

                        if (ReturnalAdrenaline.MaxLevelProtection.Value)
                        {
                            body.AddBuff(Content.Buffs.ReturnalMaxLevelProtection);
                            hasProtectionBuff = true;
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
                if (hasProtectionBuff && ReturnalAdrenaline.MaxLevelProtection.Value)
                {
                    body.AddBuff(Content.Buffs.ReturnalMaxLevelProtection);
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
            var transform = FindItemDisplayTransform();
            if (transform)
            {
                var glowTransform = transform.Find("Effect");
                if (glowTransform)
                {
                    return glowTransform.gameObject;
                }
            }

            return null;
        }

        private Transform FindItemDisplayTransform()
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
                            return list[0].transform;
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
                            main.startColor = new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, 0.5f);
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

        private void ManageMaxLevelProtectionEffect()
        {
            bool hasProtectionBuff = body.HasBuff(Content.Buffs.ReturnalMaxLevelProtection);
            if (hasProtectionBuff && !maxLevelEffect)
            {
                maxLevelEffect = Instantiate(RoR2.CharacterBody.AssetReferences.bearVoidTempEffectPrefab, FindItemDisplayTransform());
                maxLevelEffect.transform.localPosition = new Vector3(0, 0.1745f, 0);
                maxLevelEffect.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                // removing TVE component since it automatically destroys the object
                // on the next frame
                var component = maxLevelEffect.gameObject.GetComponent<TemporaryVisualEffect>();
                if (component)
                {
                    Destroy(component);
                }
            }
            else if (maxLevelEffect && !hasProtectionBuff)
            {
                Destroy(maxLevelEffect);
            }
        }

    }

}

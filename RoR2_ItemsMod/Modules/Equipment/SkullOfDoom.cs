﻿using BepInEx.Configuration;
using ExtradimensionalItems.Modules.Effects;
using R2API;
using RoR2;
using RoR2.Audio;
using SimpleJSON;
using System;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static RoR2.CharacterBody;

namespace ExtradimensionalItems.Modules.Equipment
{
    public class SkullOfDoom : EquipmentBase<SkullOfDoom>
    {
        // using DotController is kinda pointless since it seems we are limited to dots that the game has
        // and we have to write our own controller that will inflict DoT AND at the end of the day
        // it is still just HealthComponent.TakeDamage
        public class SkullOfDoomBehavior : MonoBehaviour
        {
            public CharacterBody body;

            private float stopwatch;

            private float damageTimer = DamageFrequency.Value;

            public void Awake()
            {
                this.enabled = false;
            }

            public void FixedUpdate()
            {
                if (!NetworkServer.active)
                {
                    return;
                }

                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > damageTimer && body.HasBuff(Content.Buffs.SkullOfDoom))
                {
                    stopwatch -= damageTimer;
                    DealDamage(body);
                }
            }
        }

        public static ConfigEntry<float> SpeedBuff;
        public static ConfigEntry<float> DamageOverTime;
        public static ConfigEntry<float> DamageFrequency;
        public static ConfigEntry<bool> EnableFuelCellInteraction;
        public static ConfigEntry<float> FuelCellSpeedBuff;
        public static ConfigEntry<float> FuelCellDamageOverTime;

        public override string EquipmentName => "SkullOfDoom";

        public override string EquipmentLangTokenName => "SKULL_OF_DOOM";

        public override GameObject EquipmentModel => AssetBundle.LoadAsset<GameObject>("SkullOfDoom");

        public override Sprite EquipmentIcon => AssetBundle.LoadAsset<Sprite>("texSkullOfDoomIcon");

        public override string BundleName => "skullofdoom";

        public override bool EnigmaCompatible => false;

        public override float Cooldown => 1f;

        public override bool IsLunar => true;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var ItemBodyModelPrefab = AssetBundle.LoadAsset<GameObject>("SkullOfDoom");
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();

            // to fix item fade enable "Dither" on hopoo shader in Unity
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Stomach",
                    localPos = new Vector3(-0.18344F, 0.09567F, -0.07979F),
                    localAngles = new Vector3(345.6406F, 245.6418F, 357.0431F),
                    localScale = new Vector3(0.2684F, 0.2684F, 0.2684F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Stomach",
                    localPos = new Vector3(0.12679F, 0.08065F, -0.0143F),
                    localAngles = new Vector3(348.8699F, 116.6115F, 13.81139F),
                    localScale = new Vector3(0.36207F, 0.36207F, 0.36207F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(0.07624F, 3.89845F, 0.41577F),
                    localAngles = new Vector3(278.5278F, 193.6384F, 347.2162F),
                    localScale = new Vector3(3.03086F, 3.03086F, 3.03086F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Stomach",
                    localPos = new Vector3(-0.1407F, -0.02355F, 0.16146F),
                    localAngles = new Vector3(356.7233F, 324.9602F, 354.826F),
                    localScale = new Vector3(0.3983F, 0.3983F, 0.3983F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Chest",
                    localPos = new Vector3(-0.00451F, 0.27833F, 0.06226F),
                    localAngles = new Vector3(355.1013F, 357.1098F, 1.42237F),
                    localScale = new Vector3(0.17004F, 0.17004F, 0.17004F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Stomach",
                    localPos = new Vector3(0.18282F, -0.05672F, 0.09466F),
                    localAngles = new Vector3(354.4527F, 82.68719F, 357.8358F),
                    localScale = new Vector3(0.32309F, 0.32309F, 0.32309F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "FlowerBase",
                    localPos = new Vector3(0.66577F, -1.27998F, -0.15682F),
                    localAngles = new Vector3(334.4954F, 153.8183F, 6.51623F),
                    localScale = new Vector3(0.80493F, 0.80493F, 0.80493F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "MechBase",
                    localPos = new Vector3(0.24075F, 0.0859F, 0.31343F),
                    localAngles = new Vector3(348.4964F, 85.43927F, 0.06247F),
                    localScale = new Vector3(0.36744F, 0.36744F, 0.36744F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Chest",
                    localPos = new Vector3(0.67091F, -0.71493F, 6.87573F),
                    localAngles = new Vector3(45.40445F, 27.58954F, 32.31604F),
                    localScale = new Vector3(3.58299F, 3.58299F, 3.58299F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Stomach",
                    localPos = new Vector3(0.10521F, 0.12538F, 0.16917F),
                    localAngles = new Vector3(4.82773F, 21.6987F, 10.16348F),
                    localScale = new Vector3(0.44293F, 0.44293F, 0.44293F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Hat",
                    localPos = new Vector3(-0.07336F, 0.06832F, 0.0972F),
                    localAngles = new Vector3(333.7668F, 322.4415F, 16.41724F),
                    localScale = new Vector3(0.18695F, 0.18695F, 0.18695F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Pelvis",
                    localPos = new Vector3(-0.14623F, 0.18664F, 0.39209F),
                    localAngles = new Vector3(318.1043F, -0.00001F, 78.44671F),
                    localScale = new Vector3(0.68419F, 0.68419F, 0.68419F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Backpack",
                    localPos = new Vector3(0.33549F, -0.43196F, 0.02816F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(0.28088F, 0.28088F, 0.28088F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "LargeExhaust2L",
                    localPos = new Vector3(-0.04229F, 0.04201F, -0.0088F),
                    localAngles = new Vector3(350.3206F, 192.9503F, 149.9679F),
                    localScale = new Vector3(0.23255F, 0.23255F, 0.23255F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Pelvis",
                    localPos = new Vector3(-0.00165F, 0.00045F, 0.0012F),
                    localAngles = new Vector3(11.55748F, 326.889F, 345.8238F),
                    localScale = new Vector3(0.00373F, 0.00373F, 0.00373F)
                }
            });
            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Hammer",
                    localPos = new Vector3(-0.00093F, 0.01148F, 0.0099F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.02445F, 0.02445F, 0.02445F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Shield",
                    localPos = new Vector3(0.30286F, -0.81682F, 0.20925F),
                    localAngles = new Vector3(15.57833F, 120.7215F, 355.1023F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "UpperArmL",
                    localPos = new Vector3(0.18666F, 0.32715F, -0.0234F),
                    localAngles = new Vector3(332.9864F, 93.54329F, 174.718F),
                    localScale = new Vector3(0.62114F, 0.62114F, 0.62114F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Chest",
                    localPos = new Vector3(-0.41525F, 0.07911F, 0.13884F),
                    localAngles = new Vector3(64.61535F, 66.48531F, 47.00687F),
                    localScale = new Vector3(0.44591F, 0.44591F, 0.44591F)
                }
            });
            rules.Add("mdlSniper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Pelvis",
                    localPos = new Vector3(-0.12341F, 0.17137F, -0.14276F),
                    localAngles = new Vector3(356.4608F, 202.8583F, 7.02963F),
                    localScale = new Vector3(0.35544F, 0.35544F, 0.35544F)
                }
            });
            // EXAMPLE
            //rules.Add("body", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Chest",
            //        localPos = new Vector3(0, 0, 0),
            //        localAngles = new Vector3(0, 0, 0),
            //        localScale = new Vector3(1, 1, 1)
            //    }
            //});
            // END EXAMPLE
            return rules;
        }

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            LoadSoundBank();
            SetLogbookCameraPosition();
            CreateConfig(config);
            LoadLanguageFile();
            CreateBuffs();
            CreateEquipment(ref Content.Equipment.SkullOfDoom);
            CreateVisualEffects();
            Hooks();
        }

        protected override void LoadSoundBank()
        {
            base.LoadSoundBank();
            Utils.RegisterNetworkSound("EI_SkullOfDoom_Use");
        }

        public override string GetOverlayDescription(string value, JSONNode tokensNode)
        {
            return string.Format(value, (SpeedBuff.Value / 100).ToString("###%"), (DamageOverTime.Value / 100).ToString("###%"), DamageFrequency.Value,
                        EnableFuelCellInteraction.Value ? string.Format(tokensNode["EQUIPMENT_SKULL_OF_DOOM_FUEL_CELL"].Value, (FuelCellSpeedBuff.Value / 100).ToString("###%"), (FuelCellDamageOverTime.Value / 100).ToString("###%")) : "");
        }

        protected override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            RoR2.CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (NetworkServer.active)
            {
                if (body)
                {
                    bool hasSkullOfDoom = EquipmentCatalog.GetEquipmentDef(body.inventory.currentEquipmentIndex) == Content.Equipment.SkullOfDoom;
                    if (hasSkullOfDoom && !body.gameObject.TryGetComponent<SkullOfDoomBehavior>(out _))
                    {
                        var component = body.gameObject.AddComponent<SkullOfDoomBehavior>();
                        component.body = body;
                    }
                    if (!hasSkullOfDoom)
                    {
                        if (body.gameObject.TryGetComponent<SkullOfDoomBehavior>(out var component))
                        {
                            UnityEngine.Object.Destroy(component);
                        }
                        if (body.HasBuff(Content.Buffs.SkullOfDoom))
                        {
                            MyLogger.LogMessage("Player {0}({1}) picked up another equipment while having {2} buff, removing the buff.", body.GetUserName(), body.name, Content.Buffs.SkullOfDoom.name);
                            body.RemoveBuff(Content.Buffs.SkullOfDoom);
                        }
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body.inventory && body.HasBuff(Content.Buffs.SkullOfDoom))
            {
                args.moveSpeedMultAdd += (SpeedBuff.Value / 100) + ((FuelCellSpeedBuff.Value / 100) * (EnableFuelCellInteraction.Value ? body.inventory.GetItemCount(RoR2Content.Items.EquipmentMagazine) : 0));
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!NetworkServer.active)
            {
                MyLogger.LogWarning("[Server] function Modules.Equipment.SkullOfDoom::ActivateEquipment(RoR2.EquipmentSlot) called on client.");
                return false;
            }

            var body = slot.characterBody;

            if (body.TryGetComponent<SkullOfDoomBehavior>(out var behavior))
            {
                if (body.HasBuff(Content.Buffs.SkullOfDoom))
                {
                    MyLogger.LogMessage("Player {0}({1}) used {2}, removing damage DoT and movement speed buff.", body.GetUserName(), body.name, EquipmentName);
                    body.RemoveBuff(Content.Buffs.SkullOfDoom);
                    behavior.enabled = false;
                }
                else
                {
                    MyLogger.LogMessage("Player {0}({1}) used {2}, applying damage DoT and movement speed buff.", body.GetUserName(), body.name, EquipmentName);
                    DealDamage(body);
                    body.AddBuff(Content.Buffs.SkullOfDoom);
                    behavior.enabled = true;
                    EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_SkullOfDoom_Use", body.gameObject);
                }
                return true;
            } else { return false; }

        }

        private static void DealDamage(CharacterBody body)
        {
            DamageInfo damageInfo = new DamageInfo();
            damageInfo.attacker = null; // if you put self as attacker then friendly fire damage reduction is applied
            damageInfo.crit = false;
            damageInfo.position = body.transform.position;
            damageInfo.damageColorIndex = DamageColorIndex.Item;
            damageInfo.damageType = DamageType.BypassArmor & DamageType.DoT;

            if (EnableFuelCellInteraction.Value)
            {
                damageInfo.damage = Math.Max(body.maxHealth * 0.01f,
                    body.maxHealth * (DamageOverTime.Value / 100) / (1 + (FuelCellDamageOverTime.Value * body.inventory.GetItemCount(RoR2Content.Items.EquipmentMagazine) / 100)));
            }
            else
            {
                damageInfo.damage = Math.Max(body.maxHealth * 0.01f, body.maxHealth * (DamageOverTime.Value / 100));
            }

            body.healthComponent.TakeDamage(damageInfo);
        }

        public void CreateBuffs()
        {
            var SkullOfDoomBuff = ScriptableObject.CreateInstance<BuffDef>();
            SkullOfDoomBuff.name = "Skull of Impending Doom";
            SkullOfDoomBuff.buffColor = Color.yellow;
            SkullOfDoomBuff.canStack = false;
            SkullOfDoomBuff.isDebuff = false;
            SkullOfDoomBuff.iconSprite = AssetBundle.LoadAsset<Sprite>("texSkullOfDoomBuffIcon.png");

            ContentAddition.AddBuffDef(SkullOfDoomBuff);

            Content.Buffs.SkullOfDoom = SkullOfDoomBuff;
        }

        private void CreateVisualEffects()
        {
            // have to use an empty object created in Unity, otherwise 
            // TemporaryVisualEffect class throws an error on start up
            var emptyObject = AssetBundle.LoadAsset<GameObject>("EmptyObject.prefab");

            var fireAsset = AssetBundle.LoadAsset<GameObject>("parFire.prefab");

            fireAsset.GetComponent<ParticleSystem>().GetComponent<Renderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matFirePillarParticle.mat").WaitForCompletion();

            var grounfFire = emptyObject.AddComponent<GroundFires>();
            grounfFire.fire = fireAsset;

            var tempEffectComponent = emptyObject.AddComponent<TemporaryVisualEffect>();
            tempEffectComponent.visualTransform = emptyObject.GetComponent<Transform>();

            var destroyOnTimerComponent = emptyObject.AddComponent<DestroyOnTimer>();
            destroyOnTimerComponent.duration = 0.1f;
            MonoBehaviour[] exitComponents = new MonoBehaviour[1];
            exitComponents[0] = destroyOnTimerComponent;

            tempEffectComponent.exitComponents = exitComponents;

            TempVisualEffectAPI.AddTemporaryVisualEffect(emptyObject.InstantiateClone("SkullOfDoomEffectL", false), (CharacterBody body) => { return body.HasBuff(Content.Buffs.SkullOfDoom); }, false, "FootL");
            TempVisualEffectAPI.AddTemporaryVisualEffect(emptyObject.InstantiateClone("SkullOfDoomEffectR", false), (CharacterBody body) => { return body.HasBuff(Content.Buffs.SkullOfDoom); }, false, "FootR");
        }

        protected override void CreateConfig(ConfigFile config)
        {
            SpeedBuff = config.Bind("Equipment: " + EquipmentName, "Speed Buff", 100f, "How much movement speed, in percentage, buff provides.");
            DamageOverTime = config.Bind("Equipment: " + EquipmentName, "Damage Over Time", 10f, "How much percentage damage from max health DoT deals.");
            DamageFrequency = config.Bind("Equipment: " + EquipmentName, "Damage Over Time Frequency", 3f, "How frequently, in seconds, damage is applied.");
            EnableFuelCellInteraction = config.Bind("Equipment: " + EquipmentName, "Fuel Cell Interaction", true, "Enables interaction with Fuel Cells.");
            FuelCellSpeedBuff = config.Bind("Equipment: " + EquipmentName, "Fuel Cell Speed Buff", 15f, "How much additional movement speed each Fuel Cell provides, linearly. EnableFuelCellInteraction should be enabled for it to work.");
            FuelCellDamageOverTime = config.Bind("Equipment: " + EquipmentName, "Fuel Cell Damage Over Time Reduction", 15f, "By how much each Fuel Cell reduces DoT damage, exponentially. EnableFuelCellInteraction should be enabled for it to work.");
            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(SpeedBuff, 1f, 200f, 1f);
                RiskOfOptionsCompat.CreateNewOption(DamageOverTime, 1f, 99f, 1f);
                RiskOfOptionsCompat.CreateNewOption(DamageFrequency, 0.1f, 10f, 0.1f);
                RiskOfOptionsCompat.CreateNewOption(EnableFuelCellInteraction);
                RiskOfOptionsCompat.CreateNewOption(FuelCellSpeedBuff, 1f, 50f, 1f);
                RiskOfOptionsCompat.CreateNewOption(FuelCellDamageOverTime, 1f, 50f, 1f);
                RiskOfOptionsCompat.AddDelegateOnModOptionsExit(OnModOptionsExit);
            }
        }
    }
}

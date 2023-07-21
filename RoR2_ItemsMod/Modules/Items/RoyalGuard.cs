using BepInEx.Configuration;
using EntityStates;
using ExtradimensionalItems.Modules.SkillStates;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using SimpleJSON;
using System.IO;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class RoyalGuard : ItemBase<RoyalGuard>
    {
        public enum ItemType
        {
            Lunar,
            Legendary
        }

        // these are used to determine windows for how many stacks of damage buff player gets on parry
        // from the base duration of parry stance 0.5 seconds:
        //   0.1 seconds for best parry or 0.5/5
        //   0.25 (or 0.15 from previous) for middle parry or 0.5/2
        //   the rest of the timer is for worst parry
        private const int BEST_PARRY_COEF = 5;
        private const int MIDDLE_PARRY_COEF = 2;

        public static ConfigEntry<int> MaxBuffStacks;
        public static ConfigEntry<float> DamageModifier;
        public static ConfigEntry<float> BaseDuration;
        public static ConfigEntry<float> PerStackDuration;
        public static ConfigEntry<float> DamageRadius;
        public static ConfigEntry<ItemType> ItemTier;

        public override string ItemName => "RoyalGuard";

        public override string ItemLangTokenName => "ROYAL_GUARD";

        public override ItemTier Tier => ItemTier.Value == ItemType.Lunar ? RoR2.ItemTier.Lunar : RoR2.ItemTier.Tier3;

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("royalguard");

        public override Sprite ItemIcon => ItemTier.Value == ItemType.Lunar ? AssetBundle.LoadAsset<Sprite>("texRoyalGuardItemIconLunar") : AssetBundle.LoadAsset<Sprite>("texRoyalGuardItemIconGood");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist };

        public override string BundleName => "royalguard";

        public override bool AIBlacklisted => true;

        private static GameObject RoyalGuardParryEffectInstance;
        public static GameObject RoyalGuardExplodeEffectInstance;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var ItemBodyModelPrefab = AssetBundle.LoadAsset<GameObject>("DisplayRoyalGuard");

            //var slice = ItemBodyModelPrefab.transform.Find("Slice1");
            var slice = ItemBodyModelPrefab.transform.GetChild(0).GetChild(0); // holy shit what the fuck am I doing

            var dynamicBone = slice.gameObject.AddComponent<DynamicBone>();

            dynamicBone.m_Root = slice;
            dynamicBone.m_Exclusions = new System.Collections.Generic.List<Transform>
            {
                slice
            };
            dynamicBone.m_UpdateMode = DynamicBone.UpdateMode.Normal;
            dynamicBone.m_Damping = 0.3f;
            dynamicBone.m_Elasticity = 0.1f;
            dynamicBone.m_Stiffness = 0.5f;
            dynamicBone.m_FreezeAxis = DynamicBone.FreezeAxis.None;

            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();

            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.02715F, 0.18238F, 0.15865F),
                    localAngles = new Vector3(2.73252F, 60.14296F, 14.28624F),
                    localScale = new Vector3(0.3925F, 0.3925F, 0.3925F)               }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.01113F, 0.13826F, 0.06382F),
                    localAngles = new Vector3(9.13171F, 51.33527F, 0.9676F),
                    localScale = new Vector3(0.53117F, 0.53117F, 0.53117F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.36527F, 2.75581F, -0.84396F),
                    localAngles = new Vector3(324.9764F, 234.7332F, 322.1666F),
                    localScale = new Vector3(4.38105F, 4.38105F, 4.38105F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.0172F, -0.07028F, 0.10839F),
                    localAngles = new Vector3(6.51912F, 74.05727F, 37.17924F),
                    localScale = new Vector3(0.61499F, 0.61499F, 0.61499F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00106F, 0.03093F, 0.10397F),
                    localAngles = new Vector3(11.59603F, 75.39549F, 39.31316F),
                    localScale = new Vector3(0.45F, 0.45F, 0.45F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.02296F, 0.03086F, 0.10394F),
                    localAngles = new Vector3(12.19386F, 10.67853F, 39.16569F),
                    localScale = new Vector3(0.45F, 0.45F, 0.45F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.02443F, 0.14028F, 0.12232F),
                    localAngles = new Vector3(16.40738F, 88.47655F, 26.14862F),
                    localScale = new Vector3(0.47598F, 0.47598F, 0.47598F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.03829F, -0.10831F, -0.02607F),
                    localAngles = new Vector3(325.4811F, 2.24852F, 276.3369F),
                    localScale = new Vector3(1.48928F, 1.48928F, 1.48928F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.01946F, 0.02703F, 0.05995F),
                    localAngles = new Vector3(5.61702F, 67.77467F, 2.19447F),
                    localScale = new Vector3(0.61055F, 0.61055F, 0.61055F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.83391F, 2.02289F, -0.39576F),
                    localAngles = new Vector3(35.81323F, 254.9808F, 268.5984F),
                    localScale = new Vector3(6.09834F, 6.09834F, 6.09834F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.0129F, 0.06471F, 0.08647F),
                    localAngles = new Vector3(20.18252F, 60.38128F, 34.15415F),
                    localScale = new Vector3(0.67517F, 0.67517F, 0.67517F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00791F, 0.0124F, 0.06362F),
                    localAngles = new Vector3(1.39284F, 85.91791F, 23.26068F),
                    localScale = new Vector3(0.4842F, 0.4842F, 0.4842F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.0922F, 0.47721F, 0.00613F),
                    localAngles = new Vector3(14.10536F, 197.0358F, 271.5122F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.01986F, 0.0315F, 0.03501F),
                    localAngles = new Vector3(14.7867F, 77.28765F, 0.6841F),
                    localScale = new Vector3(0.56718F, 0.56718F, 0.56718F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.05226F, 0.10878F, 0.05614F),
                    localAngles = new Vector3(342.865F, 86.90682F, 346.8902F),
                    localScale = new Vector3(0.60104F, 0.60104F, 0.60104F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00024F, 0.00058F, -0.00088F),
                    localAngles = new Vector3(14.90673F, 247.266F, 16.77556F),
                    localScale = new Vector3(0.00557F, 0.00557F, 0.00557F)
                }
            });
            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00128F, 0.00258F, 0.00066F),
                    localAngles = new Vector3(19.73685F, 0.00529F, 17.98487F),
                    localScale = new Vector3(0.0213F, 0.0213F, 0.0213F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.09655F, 0.00908F, 0.02423F),
                    localAngles = new Vector3(14.28483F, 328.6286F, 11.06765F),
                    localScale = new Vector3(0.63394F, 0.63394F, 0.63394F)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00402F, 0.13529F, 0.13018F),
                    localAngles = new Vector3(15.12866F, 61.26857F, 19.59844F),
                    localScale = new Vector3(0.84291F, 0.84291F, 0.84291F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "head",
                    localPos = new Vector3(0.08138F, 0.03718F, -0.02032F),
                    localAngles = new Vector3(30.98787F, 114.3583F, 13.91004F),
                    localScale = new Vector3(0.51658F, 0.51658F, 0.51658F)
                }
            });
            rules.Add("mdlSniper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.02635F, 0.1534F, -0.06885F),
                    localAngles = new Vector3(308.8667F, 65.8711F, 304.9602F),
                    localScale = new Vector3(0.64066F, 0.64066F, 0.64066F)
                }
            });
            return rules;

        }
        
        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            LoadSoundBank();
            CreateConfig(config);
            CreateSkills();
            CreateBuffs();
            LoadLanguageFile();
            CreateItem(ref Content.Items.RoyalGuard);
            CreateVisualEffects();
            Hooks();
        }

        protected override void LoadAssetBundle()
        {
            base.LoadAssetBundle();
        }

        protected override void LoadSoundBank()
        {
            base.LoadSoundBank();
            Utils.RegisterNetworkSound("EI_RoyalGuard_Block");
            Utils.RegisterNetworkSound("EI_RoyalGuard_JustBlock");
        }

        protected override void Hooks()
        {
            //On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            RoR2.CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            // we cannot use GlobalEventManager.onServerDamageDealt because by the time we get to our method
            // damage is already dealt, negating the entire point of blocking
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            //RoR2.GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (body.skillLocator)
            {
                body.ReplaceSkillIfItemPresent(body.skillLocator.utility, ItemDef.itemIndex, Content.Skills.Parry);
            }
        }

        public override string GetOverlayDescription(string value, JSONNode tokensNode)
        {
            return "";
        }

        protected override void OnModOptionsExit()
        {
            foreach (var overlay in overlayList)
            {
                overlay.Remove();
            }

            overlayList.Clear();

            string jsonText = File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExtradimensionalItemsPlugin.PInfo.Location), ExtradimensionalItemsPlugin.LanguageFolder, $"{BundleName}.json"));

            JSONNode languageNode = JSON.Parse(jsonText);
            if (languageNode == null)
            {
                return;
            }

            foreach (string languageKey in languageNode.Keys)
            {
                JSONNode tokensNode = languageNode[languageKey];

                string language = languageKey == "strings" ? "generic" : languageKey;

                overlayList.Add(
                    LanguageAPI.AddOverlay(
                        "ITEM_ROYAL_GUARD_DESCRIPTION",
                        string.Format(tokensNode["ITEM_ROYAL_GUARD_DESCRIPTION"].Value, BaseDuration.Value, PerStackDuration.Value, (DamageModifier.Value / 100).ToString("###%"), DamageRadius.Value, MaxBuffStacks.Value),
                        language));

                overlayList.Add(
                    LanguageAPI.AddOverlay(
                        "SKILL_ROYAL_GUARD_PARRY_DESC",
                        string.Format(tokensNode["SKILL_ROYAL_GUARD_PARRY_DESC"].Value, BaseDuration.Value, PerStackDuration.Value, MaxBuffStacks.Value),
                        language));

                overlayList.Add(
                    LanguageAPI.AddOverlay(
                        "SKILL_ROYAL_GUARD_RELEASE_DESC",
                        string.Format(tokensNode["SKILL_ROYAL_GUARD_RELEASE_DESC"].Value, (DamageModifier.Value / 100).ToString("###%"), DamageRadius.Value),
                        language));
            }
        }

        protected override void LoadDescription(string key, string value, string languageKey, JSONNode tokensNode)
        {
            if (key.Contains("DESCRIPTION"))
            {
                LanguageAPI.Add(key, string.Format(value, BaseDuration.Value, PerStackDuration.Value, (DamageModifier.Value / 100).ToString("###%"), DamageRadius.Value, MaxBuffStacks.Value), languageKey);
            }
            else if (key.Contains("PARRY_DESC"))
            {
                LanguageAPI.Add(key, string.Format(value, BaseDuration.Value, PerStackDuration.Value, MaxBuffStacks.Value), languageKey);
            }
            else if (key.Contains("RELEASE_DESC"))
            {
                LanguageAPI.Add(key, string.Format(value, (DamageModifier.Value / 100).ToString("###%"), DamageRadius.Value), languageKey);
            }
            else
            {
                LanguageAPI.Add(key, value, languageKey);
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var body = self.body;
            if (body.HasBuff(Content.Buffs.RoyalGuardParryState))
            {
                var timedBuff = body.GetTimedBuff(Content.Buffs.RoyalGuardParryState);
                var parryStateDuration = GetParryStateDuration(body);
                // TODO: there should be a better way to do this
                int numberOfBuffs;
                if ((parryStateDuration - timedBuff.timer) <= (parryStateDuration / BEST_PARRY_COEF))
                {
                    numberOfBuffs = 3;
                }
                else if ((parryStateDuration - timedBuff.timer) <= (parryStateDuration / MIDDLE_PARRY_COEF))
                {
                    numberOfBuffs = 2;
                }
                else
                {
                    numberOfBuffs = 1;
                }
                if (numberOfBuffs == 3)
                {
                    EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_RoyalGuard_JustBlock", body.gameObject);
                }
                else
                {
                    EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_RoyalGuard_Block", body.gameObject);
                }
                if (body.GetBuffCount(Content.Buffs.RoyalGuardDamage) + numberOfBuffs > MaxBuffStacks.Value)
                {
                    numberOfBuffs = Mathf.Max(0, MaxBuffStacks.Value - body.GetBuffCount(Content.Buffs.RoyalGuardDamage));
                }
                for (int i = 0; i < numberOfBuffs; i++)
                {
                    body.AddBuff(Content.Buffs.RoyalGuardDamage);
                }
                // end TODO
                MyLogger.LogMessage("Player {0}({1}) got damaged in {2} after entering parry state. Adding {3} damage buff(s), adding grace buff and removing parry state buff.", body.GetUserName(), body.name, (parryStateDuration - timedBuff.timer).ToString(), numberOfBuffs.ToString());
                body.AddTimedBuff(RoR2.RoR2Content.Buffs.HiddenInvincibility, 0.0167f);
                body.RemoveOldestTimedBuff(Content.Buffs.RoyalGuardParryState);
                damageInfo.rejected = true;
            }
            orig(self, damageInfo);
        }

        private void CreateSkills()
        {
            var RoyalGuardSkillParryDef = ScriptableObject.CreateInstance<SkillDef>();

            RoyalGuardSkillParryDef.activationState = new SerializableEntityStateType(typeof(Parry));
            RoyalGuardSkillParryDef.activationStateMachineName = "Body";
            RoyalGuardSkillParryDef.baseMaxStock = 1;
            RoyalGuardSkillParryDef.baseRechargeInterval = 1f;
            RoyalGuardSkillParryDef.beginSkillCooldownOnSkillEnd = true;
            RoyalGuardSkillParryDef.canceledFromSprinting = true;
            RoyalGuardSkillParryDef.cancelSprintingOnActivation = true;
            RoyalGuardSkillParryDef.fullRestockOnAssign = true;
            RoyalGuardSkillParryDef.interruptPriority = InterruptPriority.Skill;
            RoyalGuardSkillParryDef.isCombatSkill = true;
            RoyalGuardSkillParryDef.mustKeyPress = false;
            RoyalGuardSkillParryDef.rechargeStock = 1;
            RoyalGuardSkillParryDef.requiredStock = 1;
            RoyalGuardSkillParryDef.stockToConsume = 1;

            RoyalGuardSkillParryDef.icon = AssetBundle.LoadAsset<Sprite>("texRoyalGuardSkillGuard");
            RoyalGuardSkillParryDef.skillDescriptionToken = "SKILL_ROYAL_GUARD_PARRY_DESC";
            RoyalGuardSkillParryDef.skillName = "RoyalGuardParry";
            RoyalGuardSkillParryDef.skillNameToken = "SKILL_ROYAL_GUARD_PARRY_NAME";

            ContentAddition.AddSkillDef(RoyalGuardSkillParryDef);
            ContentAddition.AddEntityState<Parry>(out bool _);

            Content.Skills.Parry = RoyalGuardSkillParryDef;

            var RoyalGuardSkillExplodeDef = ScriptableObject.CreateInstance<SkillDef>();

            RoyalGuardSkillExplodeDef.activationState = new SerializableEntityStateType(typeof(Explode));
            RoyalGuardSkillExplodeDef.activationStateMachineName = "Body";
            RoyalGuardSkillExplodeDef.baseMaxStock = 1;
            RoyalGuardSkillExplodeDef.baseRechargeInterval = 1f;
            RoyalGuardSkillExplodeDef.beginSkillCooldownOnSkillEnd = true;
            RoyalGuardSkillExplodeDef.canceledFromSprinting = true;
            RoyalGuardSkillExplodeDef.cancelSprintingOnActivation = true;
            RoyalGuardSkillExplodeDef.fullRestockOnAssign = true;
            RoyalGuardSkillExplodeDef.interruptPriority = InterruptPriority.PrioritySkill;
            RoyalGuardSkillExplodeDef.isCombatSkill = true;
            RoyalGuardSkillExplodeDef.mustKeyPress = false;
            RoyalGuardSkillExplodeDef.rechargeStock = 1;
            RoyalGuardSkillExplodeDef.requiredStock = 1;
            RoyalGuardSkillExplodeDef.stockToConsume = 1;

            RoyalGuardSkillExplodeDef.icon = AssetBundle.LoadAsset<Sprite>("texRoyalGuardSkillRelease");
            RoyalGuardSkillExplodeDef.skillDescriptionToken = "SKILL_ROYAL_GUARD_RELEASE_DESC";
            RoyalGuardSkillExplodeDef.skillName = "RoyalGuardRelease";
            RoyalGuardSkillExplodeDef.skillNameToken = "SKILL_ROYAL_GUARD_RELEASE_NAME";

            ContentAddition.AddSkillDef(RoyalGuardSkillExplodeDef);
            ContentAddition.AddEntityState<Explode>(out bool _);

            Content.Skills.Explode = RoyalGuardSkillExplodeDef;
        }

        public static float GetParryStateDuration(CharacterBody body)
        {
            return BaseDuration.Value + (PerStackDuration.Value * (body.inventory.GetItemCount(Content.Items.RoyalGuard) - 1));
        }

        public void CreateBuffs()
        {
            var RoyalGuardParryStateBuff = ScriptableObject.CreateInstance<BuffDef>();
            RoyalGuardParryStateBuff.name = "Royal Guard Parry State";
            RoyalGuardParryStateBuff.buffColor = Color.red;
            RoyalGuardParryStateBuff.canStack = false;
            RoyalGuardParryStateBuff.isDebuff = false;
            RoyalGuardParryStateBuff.iconSprite = AssetBundle.LoadAsset<Sprite>("texRoyalGuardBuffIcon.png");

            ContentAddition.AddBuffDef(RoyalGuardParryStateBuff);

            Content.Buffs.RoyalGuardParryState = RoyalGuardParryStateBuff;

            var RoyalGuardDamageBuff = ScriptableObject.CreateInstance<BuffDef>();
            RoyalGuardDamageBuff.name = "Royal Guard Damage Buff";
            RoyalGuardDamageBuff.buffColor = Color.yellow;
            RoyalGuardDamageBuff.canStack = true;
            RoyalGuardDamageBuff.isDebuff = false;
            RoyalGuardDamageBuff.iconSprite = AssetBundle.LoadAsset<Sprite>("texRoyalGuardBuffIcon.png");

            ContentAddition.AddBuffDef(RoyalGuardDamageBuff);

            Content.Buffs.RoyalGuardDamage = RoyalGuardDamageBuff;

            if (BetterUICompat.enabled)
            {
                BetterUICompat.AddBuffInfo(RoyalGuardDamageBuff, "BUFF_ROYALGUARD_DAMAGE_NAME", "BUFF_ROYALGUARD_DAMAGE_DESCRIPTION");
                BetterUICompat.AddBuffInfo(RoyalGuardParryStateBuff, "BUFF_ROYALGUARD_PARRY_NAME", "BUFF_ROYALGUARD_PARRY_DESCRIPTION");
            }
        }

        private void CreateVisualEffects()
        {
            RoyalGuardParryEffectInstance = AssetBundle.LoadAsset<GameObject>("RoyalGuardEffect");
            var tempEffectComponent = RoyalGuardParryEffectInstance.AddComponent<TemporaryVisualEffect>();
            tempEffectComponent.visualTransform = RoyalGuardParryEffectInstance.GetComponent<Transform>();

            var destroyOnTimerComponent = RoyalGuardParryEffectInstance.AddComponent<DestroyOnTimer>();
            destroyOnTimerComponent.duration = 0.1f;
            MonoBehaviour[] exitComponents = new MonoBehaviour[1];
            exitComponents[0] = destroyOnTimerComponent;

            tempEffectComponent.exitComponents = exitComponents;

            TempVisualEffectAPI.AddTemporaryVisualEffect(RoyalGuardParryEffectInstance.InstantiateClone("RoyalGuardParryEffect", false), (CharacterBody body) => { return body.HasBuff(Content.Buffs.RoyalGuardParryState); }, true);

            RoyalGuardExplodeEffectInstance = AssetBundle.LoadAsset<GameObject>("RoyalGuard_ReleaseExplosion");

            var effectComponent = RoyalGuardExplodeEffectInstance.AddComponent<EffectComponent>();
            effectComponent.applyScale = true;
            effectComponent.soundName = "EI_RoyalGuard_Release";

            var vfxAttributes = RoyalGuardExplodeEffectInstance.AddComponent<VFXAttributes>();
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Medium;

            var destroyOnTimer = RoyalGuardExplodeEffectInstance.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 0.6f;

            R2API.ContentAddition.AddEffect(RoyalGuardExplodeEffectInstance);
        }

        public override void AddBetterUIStats(ItemDef item)
        {
            base.AddBetterUIStats(item);
            BetterUICompat.RegisterStat(item, "BETTERUICOMPAT_DESC_DAMAGE", DamageModifier.Value / 100, BetterUICompat.StackingFormula.Linear, BetterUICompat.StatFormatter.Percent, BetterUICompat.ItemTag.Damage);
            BetterUICompat.RegisterStat(item, "BETTERUICOMPAT_DESC_ROYALGUARD_PARRY_WINDOW", BaseDuration.Value, PerStackDuration.Value, BetterUICompat.StackingFormula.Linear, BetterUICompat.StatFormatter.Seconds);
        }

        public override void CreateConfig(ConfigFile config)
        {
            DamageModifier = config.Bind("Item: " + ItemName, "Damage Modifier", 1000f, "What base damage modifier (per stack) the item should use, in percentage.");
            MaxBuffStacks = config.Bind("Item: " + ItemName, "Maximum Buff Stacks", 8, "How many times the buff can stack.");
            BaseDuration = config.Bind("Item: " + ItemName, "Base Parry State Duration", 0.5f, "How long, in seconds, is base Parry skill duration.");
            PerStackDuration = config.Bind("Item: " + ItemName, "Additional Duration Per Stack", 0.1f, "How much, in seconds, each stack (after first one) of item adds to Parry skill duration.");
            DamageRadius = config.Bind("Item: " + ItemName, "Release Damage Radius", 15f, "What is the damage radius of Release skill, in meters.");
            ItemTier = config.Bind("Item: " + ItemName, "Item Tier", ItemType.Lunar, "Determines the type of the item. Requires game restart to take effect.");
            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(DamageModifier, 100f, 10000f, 10f);
                RiskOfOptionsCompat.CreateNewOption(MaxBuffStacks, 1, 20);
                RiskOfOptionsCompat.CreateNewOption(BaseDuration, 0.01f, 1f, 0.01f);
                RiskOfOptionsCompat.CreateNewOption(PerStackDuration, 0.01f, 1f, 0.01f);
                RiskOfOptionsCompat.CreateNewOption(DamageRadius, 1f, 50f, 1f);
                RiskOfOptionsCompat.CreateNewOption(ItemTier, true);
                RiskOfOptionsCompat.AddDelegateOnModOptionsExit(OnModOptionsExit);
            }
        }
    }
}

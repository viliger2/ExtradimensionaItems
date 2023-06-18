using BepInEx.Configuration;
using HarmonyLib;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Linq;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class VoidCooldownReduction : ItemBase<VoidCooldownReduction>
    {
        public static ConfigEntry<float> CooldownReduction;

        public override string ItemName => "CooldownReductionVoid";

        public override string ItemLangTokenName => "COOLDOWN_REDUCTION_VOID";

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override string BundleName => "cooldownreduction";

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("pillsbottle");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texCooldownReductionIcon");

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override ExpansionDef Expansion => ExpansionCatalog.expansionDefs.FirstOrDefault(def => def.nameToken == "DLC1_NAME");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // TODO: maybe someday but not today
            var ItemBodyModelPrefab = AssetBundle.LoadAsset<GameObject>("DisplayPillsBottle");

            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();

            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.07516F, -0.05492F, 0.13553F),
                    localAngles = new Vector3(2.26426F, 89.86844F, 174.589F),
                    localScale = new Vector3(0.16636F, 0.16636F, 0.16636F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.0528F, -0.06858F, 0.12351F),
                    localAngles = new Vector3(353.1088F, 79.621F, 206.8312F),
                    localScale = new Vector3(0.15433F, 0.15433F, 0.15433F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-1.77809F, 2.90239F, 2.14708F),
                    localAngles = new Vector3(357.69F, 258.8555F, 0.58918F),
                    localScale = new Vector3(1.60872F, 1.60872F, 1.60872F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.16073F, 0.09931F, 0.15727F),
                    localAngles = new Vector3(2.57822F, 133.2514F, 188.1384F),
                    localScale = new Vector3(0.24084F, 0.24084F, 0.24084F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.16923F, 0.00545F, -0.03211F),
                    localAngles = new Vector3(343.071F, 197.5598F, 188.7807F),
                    localScale = new Vector3(0.19561F, 0.19561F, 0.19561F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.06109F, 0.09777F, 0.08159F),
                    localAngles = new Vector3(2.20926F, 100.1557F, 185.1497F),
                    localScale = new Vector3(0.22943F, 0.22943F, 0.22943F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PlatformBase",
                    localPos = new Vector3(0.48713F, 0.32312F, 0.23833F),
                    localAngles = new Vector3(359.9178F, 193.3508F, 2.948F),
                    localScale = new Vector3(0.33434F, 0.33434F, 0.33434F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechBase",
                    localPos = new Vector3(-0.25907F, -0.1483F, -0.08383F),
                    localAngles = new Vector3(358.5586F, 163.9021F, 1.01637F),
                    localScale = new Vector3(0.22491F, 0.22491F, 0.22491F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-1.34077F, -3.03465F, 6.31578F),
                    localAngles = new Vector3(6.2976F, 297.0038F, 21.09479F),
                    localScale = new Vector3(2.25337F, 2.25337F, 2.25337F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(-0.1288F, 0.11963F, 0.15499F),
                    localAngles = new Vector3(349.8305F, 271.7673F, 350.0329F),
                    localScale = new Vector3(0.18674F, 0.18674F, 0.18674F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0.12719F, 0.02023F, -0.11143F),
                    localAngles = new Vector3(5.86676F, 359.7406F, 355.5642F),
                    localScale = new Vector3(0.15262F, 0.15262F, 0.15262F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.38342F, -0.2962F, -0.24457F),
                    localAngles = new Vector3(304.962F, 154.6794F, 293.6269F),
                    localScale = new Vector3(0.43861F, 0.43861F, 0.43861F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0.09116F, -0.01619F, 0.09544F),
                    localAngles = new Vector3(346.0011F, 356.7881F, 2.01989F),
                    localScale = new Vector3(0.18515F, 0.18515F, 0.18515F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(-0.16357F, -0.05884F, -0.03179F),
                    localAngles = new Vector3(9.30161F, 80.09175F, 340.7841F),
                    localScale = new Vector3(0.17164F, 0.17164F, 0.17164F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.00186F, 0.00078F, 0.00006F),
                    localAngles = new Vector3(25.9635F, 10.36156F, 10.50625F),
                    localScale = new Vector3(0.00187F, 0.00187F, 0.00187F)
                }
            });
            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.00009F, -0.00037F, 0.0079F),
                    localAngles = new Vector3(3.47628F, 92.63393F, 168.4496F),
                    localScale = new Vector3(0.00696F, 0.00696F, 0.00696F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.16468F, 0.08046F, 0.20869F),
                    localAngles = new Vector3(0.85266F, 121.4579F, 171.9462F),
                    localScale = new Vector3(0.22788F, 0.22788F, 0.22788F)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.2588F, 0.18911F, 0.24445F),
                    localAngles = new Vector3(349.7556F, 0.85367F, 1.82916F),
                    localScale = new Vector3(0.34841F, 0.34841F, 0.34841F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "thigh.R",
                    localPos = new Vector3(0.09489F, 0.00461F, 0.09076F),
                    localAngles = new Vector3(351.1149F, 165.2264F, 177.6239F),
                    localScale = new Vector3(0.15408F, 0.15408F, 0.15408F)
                }
            });
            rules.Add("mdlSniper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.23019F, 0.14104F, -0.01109F),
                    localAngles = new Vector3(11.31845F, 150.9209F, 8.90904F),
                    localScale = new Vector3(0.22536F, 0.22536F, 0.22536F)
                }
            });

            return rules;
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, (CooldownReduction.Value / 100).ToString("###%"));
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            LoadAssetBundle();
            CreateItem(ref Content.Items.VoidCooldownReduction);
            Hooks();
        }

        protected override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
        }

        private void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new ItemDef.Pair()
            {
                itemDef1 = RoR2Content.Items.Syringe,
                itemDef2 = Content.Items.VoidCooldownReduction
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }

        public override void AddBetterUIStats(ItemDef item)
        {
            base.AddBetterUIStats(item);
            BetterUICompat.RegisterStat(item, "BETTERUICOPMAT_COOLDOWN_REDUCTUION", CooldownReduction.Value, BetterUICompat.StackingFormula.ProbablyExponential, BetterUICompat.StatFormatter.Percent, BetterUICompat.ItemTag.CooldownReduction);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.cooldownMultAdd += (float)(1 / Math.Pow(2, (CooldownReduction.Value * GetCount(sender) / 100))) - 1;
        }

        public override void CreateConfig(ConfigFile config)
        {
            CooldownReduction = config.Bind("Item: " + ItemName, "Cooldown Reduction", 10f, "How much cooldown reduction, per stack, in percentage, you get. Stacks hyperbolically, like Haste in WoW or Ability Haste in LoL, as in 100% will reduce cooldown by half, 200% by 3/4, etc.");
            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(CooldownReduction, 1f, 25f, 1f);
            }
        }
    }
}

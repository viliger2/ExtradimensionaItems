using BepInEx.Configuration;
using R2API;
using RoR2;
using SimpleJSON;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class Atma : ItemBase<Atma>
    {
        public static ConfigEntry<float> HealthPerLevel;
        public static ConfigEntry<float> PerStackScaling;

        public override string ItemName => "AtmasImpaler";

        public override string ItemLangTokenName => "ATMAS_IMPALER";

        public override ItemTier Tier => ItemTier.Tier2;

        public override string BundleName => "atma";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("atma");

        // to fix small icon when printing set "Pixel Per Unit" for sprite to 25 in Unity
        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texAtmaIcon");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var ItemBodyModelPrefab = AssetBundle.LoadAsset<GameObject>("atma");
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
                    childName = "Chest",
                    localPos = new Vector3(0.05931F, 0.11769F, -0.21938F),
                    localAngles = new Vector3(23.27695F, 5.91633F, 22.06631F),
                    localScale = new Vector3(2.11112F, 2.11112F, 2.11112F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00121F, -0.01595F, -0.11217F),
                    localAngles = new Vector3(9.08415F, 326.9416F, 324.1578F),
                    localScale = new Vector3(2.18992F, 2.18992F, 2.18992F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.53324F, 2.89081F, 0.01006F),
                    localAngles = new Vector3(20.63849F, 243.5134F, 358.9025F),
                    localScale = new Vector3(18.88126F, 18.88126F, 18.88126F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.03588F, 0.0123F, -0.2605F),
                    localAngles = new Vector3(19.14014F, 357.4113F, 346.8241F),
                    localScale = new Vector3(2.76281F, 2.76281F, 2.76281F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.03411F, -0.30607F, -0.12951F),
                    localAngles = new Vector3(9.03134F, 359.823F, 356.4779F),
                    localScale = new Vector3(2.64689F, 2.64689F, 2.64689F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandR",
                    localPos = new Vector3(-0.25078F, 0.078F, 0.04389F),
                    localAngles = new Vector3(337.2337F, 332.8286F, 305.9801F),
                    localScale = new Vector3(3.25417F, 3.25417F, 3.25417F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.10445F, -0.35228F, -0.6879F),
                    localAngles = new Vector3(28.78763F, 36.88558F, 97.3734F),
                    localScale = new Vector3(3.71836F, 3.71836F, 3.71836F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechHandR",
                    localPos = new Vector3(0.0391F, -0.27704F, 0.07467F),
                    localAngles = new Vector3(18.69483F, 16.37429F, 359.6162F),
                    localScale = new Vector3(3F, 3F, 3F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-1.51356F, 3.06965F, 9.13635F),
                    localAngles = new Vector3(16.712F, 35.78552F, 237.5622F),
                    localScale = new Vector3(20F, 15F, 20F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.0603F, 0.06345F, -0.00892F),
                    localAngles = new Vector3(345.2615F, 324.2955F, 335.8669F),
                    localScale = new Vector3(2F, 1F, 2F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MainWeapon",
                    localPos = new Vector3(-0.01876F, 0.67848F, 0.01137F),
                    localAngles = new Vector3(16.80556F, 356.7202F, 359.0283F),
                    localScale = new Vector3(1.47453F, 1.47453F, 1.47453F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.33803F, -0.34522F, 0.16156F),
                    localAngles = new Vector3(286.6828F, 287.9903F, 141.7693F),
                    localScale = new Vector3(5F, 5F, 5F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Backpack",
                    localPos = new Vector3(0.23427F, -0.10423F, -0.05527F),
                    localAngles = new Vector3(19.80791F, 267.3803F, 0.90511F),
                    localScale = new Vector3(3F, 3F, 3F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LargeExhaust2R",
                    localPos = new Vector3(0.02325F, -0.07614F, -0.01583F),
                    localAngles = new Vector3(53.4817F, 5.6685F, 0.79375F),
                    localScale = new Vector3(3F, 1F, 3F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00079F, 0.00048F, -0.00194F),
                    localAngles = new Vector3(11.8281F, 9.81263F, 17.99667F),
                    localScale = new Vector3(0.02779F, 0.02779F, 0.02779F)
                }
            });
            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.01594F, -0.00074F, 0.00416F),
                    localAngles = new Vector3(352.4106F, 77.70187F, 339.1572F),
                    localScale = new Vector3(0.12158F, 0.12158F, 0.12158F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.31749F, -0.1789F, 0.11174F),
                    localAngles = new Vector3(20.46696F, 83.04866F, 334.5096F),
                    localScale = new Vector3(3.92409F, 3.92409F, 3.92409F)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.15985F, -0.69306F, -0.13191F),
                    localAngles = new Vector3(1.69802F, 8.71632F, 15.83549F),
                    localScale = new Vector3(4.30285F, 5.79825F, 4.30285F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.18752F, -0.52693F, 0.03871F),
                    localAngles = new Vector3(34.70107F, 248.6318F, 354.8853F),
                    localScale = new Vector3(3.94916F, 3.94916F, 3.94916F)
                }
            });
            rules.Add("mdlSniper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "GunBarrel",
                    localPos = new Vector3(0.00144F, 0.02943F, 0.3023F),
                    localAngles = new Vector3(70.40952F, 185.3266F, 184.8057F),
                    localScale = new Vector3(1.80279F, 1.80279F, 1.80279F)
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
            CreateConfig(config);
            LoadAssetBundle();
            LoadLanguageFile();
            CreateItem(ref Content.Items.Atma);
            Hooks();
        }

        protected override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var count = GetCount(sender);
            if (count > 0)
            {
                var damageLevelsToAdd = sender.maxHealth / (HealthPerLevel.Value * ((100f - Util.ConvertAmplificationPercentageIntoReductionPercentage(PerStackScaling.Value * (count - 1))) / 100f));
                if(damageLevelsToAdd < 1f)
                {
                    damageLevelsToAdd = 1f;
                }

                args.baseDamageAdd += damageLevelsToAdd * sender.levelDamage;
            }
        }
        public override string GetOverlayDescription(string value, JSONNode tokensNode)
        {
            return string.Format(
                     value,
                     HealthPerLevel.Value,
                     (PerStackScaling.Value / 100).ToString("0.#%"));
        }

        public override void AddBetterUIStats(ItemDef item) 
        {
            BetterUICompat.RegisterStat(item, "BETTERUICOMPAT_DESC_DAMAGE", HealthPerLevel.Value, PerStackScaling.Value / 100, BetterUICompat.StackingFormula.NegativeExponential, BetterUICompat.StatFormatter.DamageFromHealth);
        }

        protected override void ModifyBetterUIStats()
        {
            BetterUICompat.ModifyBetterUIStat(ItemDef, "BETTERUICOMPAT_DESC_DAMAGE", HealthPerLevel.Value, PerStackScaling.Value / 100);
        }

        public override void CreateConfig(ConfigFile config)
        {
            HealthPerLevel = config.Bind("Item: " + ItemName, "Health Per Level", 250.0f, "How much health item requires to grand additional level of damage.");
            PerStackScaling = config.Bind("Item: " + ItemName, "Per Stack Scaling", 25.0f, "By how much, in percent, health requirement lowers per stack.");

            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(HealthPerLevel, 10f, 500f, 1f);
                RiskOfOptionsCompat.CreateNewOption(PerStackScaling, 1f, 100f, 0.5f);
                RiskOfOptionsCompat.AddDelegateOnModOptionsExit(OnModOptionsExit);
            }
        }
    }
}

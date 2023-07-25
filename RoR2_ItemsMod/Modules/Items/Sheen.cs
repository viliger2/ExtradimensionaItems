using BepInEx.Configuration;
using ExtradimensionalItems.Modules.Items.ItemBehaviors;
using R2API;
using RoR2;
using SimpleJSON;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class Sheen : ItemBase<Sheen>
    {
        public static ConfigEntry<float> DamageModifier;
        public static ConfigEntry<float> DamageModifierPerStack;
        public static ConfigEntry<bool> CanStack;
        public static ConfigEntry<float> BuffDuration;
        //public static ConfigEntry<int> MaxBuffStacks;
        public static ConfigEntry<int> BuffStackPerItem;
        public static ConfigEntry<int> BuffStackPerItemPerStack;
        public static ConfigEntry<float> BuffApplicationCooldown;

        public override string ItemName => "Sheen";

        public override string ItemLangTokenName => "SHEEN";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("sheen");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texSheenIcon");

        public override string BundleName => "sheen";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var ItemBodyModelPrefab = AssetBundle.LoadAsset<GameObject>("sheen");

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
                    localPos = new Vector3(-0.03067F, 0.07146F, 0.05902F),
                    localAngles = new Vector3(1.90887F, 91.22519F, 210.6187F),
                    localScale = new Vector3(0.45594F, 0.45594F, 0.45594F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.00504F, 0.17843F, -0.02459F),
                    localAngles = new Vector3(4.74153F, 92.37143F, 192.8743F),
                    localScale = new Vector3(0.47188F, 0.47188F, 0.47188F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.41193F, 0.41255F, 0.53652F),
                    localAngles = new Vector3(1.35583F, 282.9102F, 269.4942F),
                    localScale = new Vector3(3.46359F, 3.46359F, 3.46359F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.47991F, 0.0994F, 0.02713F),
                    localAngles = new Vector3(29.73876F, 267.669F, 153.3432F),
                    localScale = new Vector3(0.50649F, 0.50649F, 0.50649F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.03771F, 0.11768F, 0.0704F),
                    localAngles = new Vector3(6.48352F, 85.16232F, 220.5408F),
                    localScale = new Vector3(0.45F, 0.45F, 0.45F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.01915F, 0.31327F, -0.03765F),
                    localAngles = new Vector3(357.8119F, 89.62235F, 199.579F),
                    localScale = new Vector3(0.5807F, 0.5807F, 0.5807F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.28753F, -0.13318F, -1.31828F),
                    localAngles = new Vector3(4.73324F, 192.5224F, 3.97189F),
                    localScale = new Vector3(1.00766F, 1.00766F, 1.00766F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.55103F, 0.13903F, 0.11334F),
                    localAngles = new Vector3(39.49573F, 268.0831F, 141.0754F),
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
                    localPos = new Vector3(3.7787F, 4.97462F, -3.70545F),
                    localAngles = new Vector3(339.402F, 199.1905F, 261.7234F),
                    localScale = new Vector3(7.46699F, 7.46699F, 7.46699F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(-0.08905F, -0.19667F, 0.02875F),
                    localAngles = new Vector3(6.90335F, 85.28687F, 21.10195F),
                    localScale = new Vector3(0.67517F, 0.67517F, 0.67517F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0.4446F, -0.11439F, 0.05154F),
                    localAngles = new Vector3(311.1034F, 68.81959F, 12.5257F),
                    localScale = new Vector3(0.32148F, 0.32148F, 0.32148F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.02348F, -0.08269F, -0.90291F),
                    localAngles = new Vector3(340.183F, 198.9777F, 312.7195F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Backpack",
                    localPos = new Vector3(-0.09621F, 0.09144F, -0.04846F),
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
                    localPos = new Vector3(0.39002F, -0.08753F, -0.02698F),
                    localAngles = new Vector3(338.6439F, 71.22115F, 46.23375F),
                    localScale = new Vector3(0.42001F, 0.42001F, 0.42001F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.00439F, 0.00001F, -0.00039F),
                    localAngles = new Vector3(348.0098F, 292.3209F, 304.2087F),
                    localScale = new Vector3(0.00495F, 0.00495F, 0.00495F)
                }
            });
            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.01358F, -0.00289F, 0.00178F),
                    localAngles = new Vector3(5.02896F, 261.8028F, 92.02483F),
                    localScale = new Vector3(0.01423F, 0.01423F, 0.01423F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.03622F, 0.16914F, -0.80482F),
                    localAngles = new Vector3(16.1576F, 155.3925F, 133.0468F),
                    localScale = new Vector3(0.80639F, 0.80639F, 0.80639F)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(0.26492F, 0.38209F, 0.50281F),
                    localAngles = new Vector3(341.4031F, 102.6975F, 256.9083F),
                    localScale = new Vector3(0.86107F, 0.86107F, 0.86107F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.07577F, -0.56217F, 0.01824F),
                    localAngles = new Vector3(68.50122F, 46.94481F, 50.33927F),
                    localScale = new Vector3(0.4098F, 0.4098F, 0.4098F)
                }
            });
            rules.Add("mdlSniper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.04431F, -0.0693F, -0.02998F),
                    localAngles = new Vector3(353.6227F, 274.0825F, 319.5515F),
                    localScale = new Vector3(0.46674F, 0.46674F, 0.46674F)
                }
            });
            return rules;
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            LoadAssetBundle();
            CreateBuffs();
            CreateItem(ref Content.Items.Sheen);
            LoadLanguageFile();
            Hooks();
            CreateVisualEffects();
        }

        protected override void LoadAssetBundle()
        {
            base.LoadAssetBundle();
        }

        protected override void Hooks()
        {
            base.Hooks();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (body)
            {
                body.AddItemBehavior<SheenBehavior>(GetCount(body));
            }
        }

        public override string GetOverlayDescription(string value, JSONNode tokensNode)
        {
            return string.Format(value, 
                (DamageModifier.Value / 100).ToString("###%"), 
                (DamageModifierPerStack.Value / 100).ToString("###%"),
                CanStack.Value ? BuffStackPerItem.Value : 1, 
                CanStack.Value ? BuffStackPerItemPerStack.Value : 0);
        }

        public void CreateBuffs()
        {
            var SheenBuff = ScriptableObject.CreateInstance<BuffDef>();
            SheenBuff.name = "Sheen Damage Bonus";
            SheenBuff.buffColor = Color.blue;
            SheenBuff.canStack = CanStack.Value;
            SheenBuff.isDebuff = false;
            SheenBuff.iconSprite = AssetBundle.LoadAsset<Sprite>("texSheenBuffIcon");

            ContentAddition.AddBuffDef(SheenBuff);

            Content.Buffs.Sheen = SheenBuff;

            if (BetterUICompat.enabled)
            {
                BetterUICompat.AddBuffInfo(SheenBuff, "BUFF_SHEEN_NAME", "BUFF_SHEEN_DESCRIPTION");
            }

        }

        private void CreateVisualEffects()
        {
            var SheenEffectInstance = AssetBundle.LoadAsset<GameObject>("SheenEffect");
            var tempEffectComponent = SheenEffectInstance.AddComponent<TemporaryVisualEffect>();
            tempEffectComponent.visualTransform = SheenEffectInstance.GetComponent<Transform>();

            var destroyOnTimerComponent = SheenEffectInstance.AddComponent<DestroyOnTimer>();
            destroyOnTimerComponent.duration = 0.1f;
            MonoBehaviour[] exitComponents = new MonoBehaviour[1];
            exitComponents[0] = destroyOnTimerComponent;

            tempEffectComponent.exitComponents = exitComponents;

            TempVisualEffectAPI.AddTemporaryVisualEffect(SheenEffectInstance.InstantiateClone("SheenEffectL", false), (CharacterBody body) => { return body.HasBuff(Content.Buffs.Sheen); }, true, "HandL");
            TempVisualEffectAPI.AddTemporaryVisualEffect(SheenEffectInstance.InstantiateClone("SheenEffectR", false), (CharacterBody body) => { return body.HasBuff(Content.Buffs.Sheen); }, true, "HandR");
        }

        public override void AddBetterUIStats(ItemDef item)
        {
            base.AddBetterUIStats(item);
            BetterUICompat.RegisterStat(item, "BETTERUICOMPAT_DESC_DAMAGE", DamageModifier.Value / 100, DamageModifierPerStack.Value / 100, BetterUICompat.StackingFormula.Linear, BetterUICompat.StatFormatter.Percent, BetterUICompat.ItemTag.Damage);
            BetterUICompat.RegisterStat(item, "BETTERUICOMPAT_DESC_STACKS", BuffStackPerItem.Value, BuffStackPerItemPerStack.Value, BetterUICompat.StackingFormula.Linear, BetterUICompat.StatFormatter.Charges);
        }

        public override void CreateConfig(ConfigFile config)
        {
            CanStack = config.Bind("Item: " + ItemName, "Can Buff Stack", true, "Determines whether the buff that indicates damage bonus can stack or not. Requires game restart to take effect.");
            DamageModifier = config.Bind("Item: " + ItemName, "Damage Modifier", 250f, "What damage modifier the item should use.");
            DamageModifierPerStack = config.Bind("Item: " + ItemName, "Damage Modifier Per Stack", 150f, "What damage modifier (per stack) the item should use.");
            BuffDuration = config.Bind("Item: " + ItemName, "Buff Duration", 10f, "How long the buff should remain active after using non-primary ability.");
            BuffStackPerItem = config.Bind("Item: " + ItemName, "Buff Stacks Per Item", 3, "How much stacks of a buff you get per item.");
            BuffStackPerItemPerStack = config.Bind("Item: " + ItemName, "Buff Stacks Per Item Per Stack", 2, "How much stacks of a buff you get per item per stack.");
            BuffApplicationCooldown = config.Bind("Item: " + ItemName, "Buff Application Cooldown", 1.5f, "Cooldown of buff application.");
            //MaxBuffStacks = config.Bind("Item: " + ItemName, "Maximum Buff Stacks", 8, "How many times the buff can stack.");
            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(CanStack, true);
                RiskOfOptionsCompat.CreateNewOption(DamageModifier, 100f, 500f, 10f);
                RiskOfOptionsCompat.CreateNewOption(DamageModifierPerStack, 100f, 500f, 10f);
                RiskOfOptionsCompat.CreateNewOption(BuffDuration, 1f, 30f, 1f);
                RiskOfOptionsCompat.CreateNewOption(BuffStackPerItem, 1, 10);
                RiskOfOptionsCompat.CreateNewOption(BuffStackPerItemPerStack, 1, 10);
                RiskOfOptionsCompat.CreateNewOption(BuffApplicationCooldown, 0.1f, 5f, 0.1f);
                RiskOfOptionsCompat.AddDelegateOnModOptionsExit(OnModOptionsExit);
            }
        }
    }
}

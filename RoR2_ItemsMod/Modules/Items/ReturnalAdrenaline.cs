using BepInEx.Configuration;
using ExtradimensionalItems.Modules.Items.ItemBehaviors;
using ExtradimensionalItems.Modules.UI;
using R2API;
using RoR2;
using SimpleJSON;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class ReturnalAdrenaline : ItemBase<ReturnalAdrenaline>
    {
        // TODO: maybe change from static values per kill to director credits?
        // using exp is not really an option, since you basically stop leveling first loop
        // and you would need to scale per level requirement with time
        // using credits would allow us to reward appropriate amount depending on enemy
        // so bettle and bettle guard won't reward the same number of points
        // although credits also scale with time, so it is not that much different from exp
        public static ConfigEntry<int> KillsPerLevel;
        public static ConfigEntry<int> NormalEnemyReward;
        public static ConfigEntry<int> EliteEnemyReward;
        public static ConfigEntry<int> BossEnemyReward;

        public static ConfigEntry<float> AttackSpeedBonus;
        public static ConfigEntry<float> MovementSpeedBonus;
        public static ConfigEntry<float> HealthBonus;
        public static ConfigEntry<float> ShieldBonus;
        public static ConfigEntry<float> CritBonus;

        public static ConfigEntry<float> CriticalDamage;

        public static ConfigEntry<float> HealthCheckFrequency;

        public static ConfigEntry<float> KillsPerLevelPerStack;
        public static ConfigEntry<float> AttackSpeedBonusPerStack;
        public static ConfigEntry<float> MovementSpeedBonusPerStack;
        public static ConfigEntry<float> HealthBonusPerStack;
        public static ConfigEntry<float> ShieldBonusPerStack;
        public static ConfigEntry<float> CritBonusPerStack;

        public static ConfigEntry<bool> MaxLevelProtection;

        public static ConfigEntry<bool> DisableHUD;

        public static ConfigEntry<bool> TranscendenceBehavior;

        public override string ItemName => "ReturnalAdrenaline";

        public override string ItemLangTokenName => "RETURNAL_ADRENALINE";

        public override ItemTier Tier => ItemTier.Tier3;

        public override string BundleName => "returnaladrenaline";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist };

        public override bool AIBlacklisted => true;

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("ReturnalAdrenalinePickUp");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texReturnalAdrenalineIcon");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // we don't actually need an animation controller, not sure why Aetherium adds it
            // but we need to make sure that the animated mesh is not at the top of hierarchy
            // otherwise the animation breaks item displays 
            var itemModel = AssetBundle.LoadAsset<GameObject>("ReturnalAdrenaline");

            itemModel.AddComponent<RoR2.ItemDisplay>();
            // to fix item fade enable "Dither" on hopoo shader in Unity
            itemModel.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(itemModel);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.50488F, 0.02964F, -0.72541F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.50488F, 0.02964F, -0.72541F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(4.04304F, -0.91748F, 7.12599F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.52667F, -0.46198F, -1.12223F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.37356F, -0.29609F, -0.61096F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                },
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.50488F, 0.02964F, -0.72541F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-1.16276F, 0.02964F, -2.2424F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.71667F, 0.71667F, 0.71667F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.50488F, -0.17957F, -0.72541F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-4.08327F, 3.17509F, 5.39152F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(2.54914F, 2.54914F, 2.54914F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.50488F, -0.28993F, -0.94577F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.28371F, -0.29292F, -0.65491F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Chest",
                    localPos = new Vector3(-0.20712F, -1.08186F, 1.06661F),
                    localAngles = new Vector3(0F, 0F, 130F),
                    localScale = new Vector3(0.73072F, 0.73072F, 0.73072F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.50488F, 0.02964F, -0.72541F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.50488F, 0.2595F, 0.71692F),
                    localAngles = new Vector3(290.1007F, 191.9825F, 350.031F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.00538F, 0.00139F, 0.01541F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.00536F, 0.00536F, 0.00536F)
                }
            });
            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(0.00776F, 0.03009F, -0.02382F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.0147F, 0.0147F, 0.0147F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.32838F, 0.14965F, 2.08372F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(-0.5719F, 1.36072F, -0.50222F),
                    localAngles = new Vector3(355.7884F, 0.12987F, 354.9423F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Base",
                    localPos = new Vector3(0.83275F, 0.34215F, 0.50625F),
                    localAngles = new Vector3(0F, 0F, 270F),
                    localScale = new Vector3(0.39888F, 0.39888F, 0.39888F)
                }
            });
            rules.Add("mdlSniper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = itemModel,
                    followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Pelvis",
                    localPos = new Vector3(-0.42742F, 1.02681F, -0.58784F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.44807F, 0.44807F, 0.44807F)
                }
            });
            return rules;
        }

        public override string GetOverlayDescription(string value, JSONNode tokensNode)
        {
            return string.Format(
                    value,
                    (AttackSpeedBonus.Value / 100).ToString("###%"),
                    (AttackSpeedBonusPerStack.Value / 100).ToString("###%"),
                    (MovementSpeedBonus.Value / 100).ToString("###%"),
                    (MovementSpeedBonusPerStack.Value / 100).ToString("###%"),
                    HealthBonus.Value.ToString(),
                    HealthBonusPerStack.Value.ToString(),
                    (ShieldBonus.Value / 100).ToString("###%"),
                    (ShieldBonusPerStack.Value / 100).ToString("###%"),
                    (CritBonus.Value / 100).ToString("###%"),
                    (CritBonusPerStack.Value / 100).ToString("###%"),
                    KillsPerLevel.Value,
                    (KillsPerLevelPerStack.Value / 100).ToString("###%"),
                    NormalEnemyReward.Value,
                    EliteEnemyReward.Value,
                    BossEnemyReward.Value,
                    (CriticalDamage.Value / 100).ToString("###%"),
                    HealthCheckFrequency.Value,
                    MaxLevelProtection.Value ? tokensNode["ITEM_RETURNAL_ADRENALINE_DESCRIPTION_SHIELD"].Value : ""
                    );
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            Hooks();
            LoadAssetBundle();
            SetLogbookCameraPosition();
            LoadSoundBank();
            LoadLanguageFile();
            CreateBuffs();
            CreateItem(ref Content.Items.ReturnalAdrenaline);
        }

        protected override void LoadSoundBank()
        {
            base.LoadSoundBank();
            Utils.RegisterNetworkSound("EI_Returnal_LevelUp");
            Utils.RegisterNetworkSound("EI_Returnal_Break");
            Utils.RegisterNetworkSound("EI_Returnal_LevelDown");
        }

        protected override void Hooks()
        {
            base.Hooks();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            if (!DisableHUD.Value)
            {
                On.RoR2.UI.HUD.Awake += HUD_Awake;
            }
            On.RoR2.CharacterMaster.Awake += CharacterMaster_Awake;
        }

        // attaching it here so networking works
        // basically you can't attach networked components during runtime, even with NetWeaver they won't network
        // so we have to do it during awake, which is also technically runtime but it works because some Unity stuff I don't know or understand
        // this results in every single master having this component, but since we disable it on creation it should be fiiiine
        private void CharacterMaster_Awake(On.RoR2.CharacterMaster.orig_Awake orig, CharacterMaster self)
        {
            if (self)
            {
                var component = self.gameObject.AddComponent<ReturnalAdrenalineItemBehavior>();
                component.master = self;
            }
            orig(self);
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            if (!DisableHUD.Value) // another check here in case user disables the UI via RiskOfOptions
            {
                ReturnalAdrenalineUI.CreateUI(self);
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (body)
            {
                int count = GetCount(body);
                if (count > 0)
                {
                    if (body.master.gameObject.TryGetComponent<ReturnalAdrenalineItemBehavior>(out var component))
                    {
                        if (component.itemCount != count)
                        {
                            component.enabled = true;
                            component.itemCount = count;
                            component.RecalculatePerLevelValue(count);

                            if (!ReturnalAdrenaline.DisableHUD.Value)
                            {
                                var adrenalineHUD = ReturnalAdrenalineUI.FindInstance(body.master);
                                if (adrenalineHUD)
                                {
                                    adrenalineHUD.Enable();
                                }
                            }

                            MyLogger.LogMessage("Player {0}({1}) picked up ReturnalAdrenaline, activating master component, current stack count {2}.", body.GetUserName(), body.name, count.ToString());
                        }

                        if (TranscendenceBehavior.Value)
                        {
                            int trCount = body.inventory.GetItemCount(RoR2.RoR2Content.Items.ShieldOnly);
                            if (trCount != component.transendanceCount)
                            {
                                if (component.transendanceCount == 0)
                                {
                                    MyLogger.LogMessage("Player {0}({1}) picked up ShieldOnly while having ReturnalAdrenaline, swapping health checks for shield checks.", body.GetUserName(), body.name);
                                }
                                else if (trCount == 0)
                                {
                                    MyLogger.LogMessage("Player {0}({1}) lost ShieldOnly while having ReturnalAdrenaline, swapping shield checks back to health checks.", body.GetUserName(), body.name);
                                }
                                component.transendanceCount = trCount;
                            }
                        }
                    }
                }
                else if (body.master.gameObject.TryGetComponent<ReturnalAdrenalineItemBehavior>(out var component) && component.enabled)
                {
                    component.enabled = false;

                    if (!ReturnalAdrenaline.DisableHUD.Value)
                    {
                        var adrenalineHUD = ReturnalAdrenalineUI.FindInstance(body.master);
                        if (adrenalineHUD)
                        {
                            adrenalineHUD.Disable();
                        }
                    }

                    MyLogger.LogMessage("Player {0}({1}) lost all stacks of ReturnalAdrenaline, deactivating master component.", body.GetUserName(), body.name);
                }
            }
        }

        private void CreateBuffs()
        {
            var ReturnalBuffProtection = ScriptableObject.CreateInstance<BuffDef>();
            ReturnalBuffProtection.name = "Adrenaline Protection";
            ReturnalBuffProtection.buffColor = Color.cyan;
            ReturnalBuffProtection.canStack = false;
            ReturnalBuffProtection.isDebuff = false;
            ReturnalBuffProtection.iconSprite = AssetBundle.LoadAsset<Sprite>("texReturnalAdrenalineBuffIcon");

            ContentAddition.AddBuffDef(ReturnalBuffProtection);

            Content.Buffs.ReturnalMaxLevelProtection = ReturnalBuffProtection;
        }

        public override void CreateConfig(ConfigFile config)
        {
            base.CreateConfig(config);

            KillsPerLevel = config.Bind("Item: " + ItemName, "Number of Kills Per Level", 15, "How many kills are needed per item's level.");
            NormalEnemyReward = config.Bind("Item: " + ItemName, "Normal Enemy Reward", 1, "How many points normal enemy rewards towards item's levels.");
            EliteEnemyReward = config.Bind("Item: " + ItemName, "Elite Enemy Reward", 3, "How many points elite enemy rewards towards item's levels.");
            BossEnemyReward = config.Bind("Item: " + ItemName, "Boss Enemy Reward", 5, "How many points boss enemy rewards towards item's levels.");

            AttackSpeedBonus = config.Bind("Item: " + ItemName, "Attack Speed Bonus", 45f, "How much attack speed item gives. By default it is equal to 3 Soldier's Syringes.");
            MovementSpeedBonus = config.Bind("Item: " + ItemName, "Movement Speed Bonus", 42f, "How much movement speed item gives. By default it is equal to 3 Paul's Goat Hoofs.");
            HealthBonus = config.Bind("Item: " + ItemName, "Health Bonus", 125f, "How much health item gives. By default it is equal to 5 Bison Steaks.");
            ShieldBonus = config.Bind("Item: " + ItemName, "Shield Bonus", 20f, "How much shield item gives. By default it is equal to 20% of max health.");
            CritBonus = config.Bind("Item: " + ItemName, "Crit Bonus", 20f, "How much crit item gives. By default it is equal to 2 Lens-Maker's Glasses.");

            CriticalDamage = config.Bind("Item: " + ItemName, "Critical Damage", 10f, "How much damage, in percentage of health, you need to take to lose item's levels.");

            HealthCheckFrequency = config.Bind("Item: " + ItemName, "Health Check Timer", 0.1f, "How frequently game check for lost HP. Higher values will result in multiple hits being lumped together for when lost health check occurs, lower velues will result in worse game performance but hits will be registered separately.");

            KillsPerLevelPerStack = config.Bind("Item: " + ItemName, "Reduction of Kills Per Level Per Stack", 10f, "How much, in percent, number of needed kills is being reduced by each stack. Stack hyperbolically.");
            AttackSpeedBonusPerStack = config.Bind("Item: " + ItemName, "Attack Speed Bonus Per Stack", 30f, "How much attack speed item gives per stack. By default it is equal to 2 Soldier's Syringes.");
            MovementSpeedBonusPerStack = config.Bind("Item: " + ItemName, "Movement Speed Bonus Per Stack", 28f, "How much movement speed item gives per stack. By default it is equal to 2 Paul's Goat Hoofs.");
            HealthBonusPerStack = config.Bind("Item: " + ItemName, "Health Bonus Per Stack", 75f, "How much health item gives per stack. By default it is equal to 3 Bison Steaks.");
            ShieldBonusPerStack = config.Bind("Item: " + ItemName, "Shield Bonus Per Stack", 12f, "How much shield item gives per stack. By default it is equal to 12% of max health.");
            CritBonusPerStack = config.Bind("Item: " + ItemName, "Crit Bonus Per Stack", 10f, "How much crit item gives per stack. By default it is equal to 1 Lens-Maker's Glasses.");

            MaxLevelProtection = config.Bind("Item: " + ItemName, "Max Level Protection", true, "Enables Max level protection. At level 5 you will get a buff that will save you a single time from losing item's levels.");

            DisableHUD = config.Bind("Item: " + ItemName, "Disable Adrenaline HUD", false, "Disables in-game Adrenaline HUD (level progress bar and level value text).");

            TranscendenceBehavior = config.Bind("Item: " + ItemName, "Transcendence Support", true, "Uses shield instead of HP for health checks when player has Transcendence.");
            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(KillsPerLevel);
                RiskOfOptionsCompat.CreateNewOption(NormalEnemyReward);
                RiskOfOptionsCompat.CreateNewOption(EliteEnemyReward);
                RiskOfOptionsCompat.CreateNewOption(BossEnemyReward);
                RiskOfOptionsCompat.CreateNewOption(AttackSpeedBonus);
                RiskOfOptionsCompat.CreateNewOption(MovementSpeedBonus);
                RiskOfOptionsCompat.CreateNewOption(HealthBonus);
                RiskOfOptionsCompat.CreateNewOption(ShieldBonus);
                RiskOfOptionsCompat.CreateNewOption(CritBonus);
                RiskOfOptionsCompat.CreateNewOption(CriticalDamage);
                RiskOfOptionsCompat.CreateNewOption(HealthCheckFrequency);
                RiskOfOptionsCompat.CreateNewOption(KillsPerLevelPerStack);
                RiskOfOptionsCompat.CreateNewOption(AttackSpeedBonusPerStack);
                RiskOfOptionsCompat.CreateNewOption(MovementSpeedBonusPerStack);
                RiskOfOptionsCompat.CreateNewOption(HealthBonusPerStack);
                RiskOfOptionsCompat.CreateNewOption(ShieldBonusPerStack);
                RiskOfOptionsCompat.CreateNewOption(CritBonusPerStack);
                RiskOfOptionsCompat.CreateNewOption(MaxLevelProtection);
                RiskOfOptionsCompat.CreateNewOption(DisableHUD);
                RiskOfOptionsCompat.CreateNewOption(TranscendenceBehavior);
                RiskOfOptionsCompat.AddDelegateOnModOptionsExit(OnModOptionsExit);
            }
        }
    }
}

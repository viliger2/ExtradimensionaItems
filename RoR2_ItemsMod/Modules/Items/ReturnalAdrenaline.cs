﻿using BepInEx.Configuration;
using ExtradimensionalItems.Modules.Items.ItemBehaviors;
using ExtradimensionalItems.Modules.UI;
using R2API;
using RoR2;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class ReturnalAdrenaline : ItemBase<ReturnalAdrenaline>
    {
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

        public override string ItemName => "ReturnalAdrenaline";

        public override string ItemLangTokenName => "RETURNAL_ADRENALINE";

        public override ItemTier Tier => ItemTier.Tier3;

        public override string BundleName => "returnaladrenaline";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override bool AIBlacklisted => true;

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("ReturnalAdrenaline");

        public override Sprite ItemIcon => null;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // we don't actually need an animation controller, not sure why aetherium adds it
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
                    childName = "Base",
                    localPos = new Vector3(-0.50488F, 0.02964F, -0.72541F),
                    localAngles = new Vector3(270F, 0.00001F, 0F),
                    localScale = new Vector3(0.4561F, 0.4561F, 0.4561F)
                }
            });
            return rules;

            //CharacterModel.GetIremDisplayObjects(Content.Items.ReturnalAdrenaline.itemIndex);
            //body.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().GetIremDisplayObjects(Content.Items.ReturnalAdrenaline.itemIndex);
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return pickupString;
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            Hooks();
            LoadAssetBundle();
            LoadSoundBank();
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
            ReturnalAdrenalineUI.CreateUI(self);
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (body)
            {
                if (GetCount(body) > 0)
                {
                    var component = body.master.gameObject.GetComponent<ReturnalAdrenalineItemBehavior>();
                    if (component)
                    {
                        component.enabled = true;
                        component.RecalculatePerLevelValue(GetCount(body));

                        if (!ReturnalAdrenaline.DisableHUD.Value)
                        {
                            var adrenalineHUD = ReturnalAdrenalineUI.FindInstance(body.master);
                            if (adrenalineHUD)
                            {
                                adrenalineHUD.Enable();
                            }
                        }

                        MyLogger.LogMessage("Player {0}({1}) picked up ReturnalAdrenaline, activating master component, current stack count {2}.", body.GetUserName(), body.name, GetCount(body).ToString());
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
            ReturnalBuffProtection.buffColor = Color.yellow;
            ReturnalBuffProtection.canStack = false;
            ReturnalBuffProtection.isDebuff = false;
            ReturnalBuffProtection.iconSprite = null; //TODO: fix

            ContentAddition.AddBuffDef(ReturnalBuffProtection);

            Content.Buffs.ReturnalMaxLevelProtection = ReturnalBuffProtection;

            //if (BetterUICompat.enabled)
            //{
            //    BetterUICompat.AddBuffInfo(ReturnalBuffProtection, "BUFF_DAMAGE_ON_COOLDOWNS_NAME", "BUFF_DAMAGE_ON_COOLDOWNS_DESCRIPTION");
            //}
        }

        public override void CreateConfig(ConfigFile config)
        {
            base.CreateConfig(config);

            KillsPerLevel = config.Bind("Item: " + ItemName, "Number of Kills Per Level", 15, "How many kills are needed per item's level.");
            NormalEnemyReward = config.Bind("Item: " + ItemName, "Normal Enemy Reward", 1, "How many points normal enemy rewards towards item's levels.");
            EliteEnemyReward = config.Bind("Item: " + ItemName, "Elite Enemy Reward", 3, "How many points elite enemy rewards towards item's levels.");
            BossEnemyReward = config.Bind("Item: " + ItemName, "Boss Enemy Reward", 5, "How many points boss enemy rewards towards item's levels.");

            AttackSpeedBonus = config.Bind("Item: " + ItemName, "Attack Speed Bonus", 75f, "How much attack speed item gives. By default it is equal to 5 Soldier's Syringes.");
            MovementSpeedBonus = config.Bind("Item: " + ItemName, "Movement Speed Bonus", 70f, "How much movement speed item gives. By default it is equal to 5 Paul's Goat Hoofs.");
            HealthBonus = config.Bind("Item: " + ItemName, "Health Bonus", 125f, "How much health item gives. By default it is equal to 5 Bison Steaks.");
            ShieldBonus = config.Bind("Item: " + ItemName, "Shield Bonus", 20f, "How much shield item gives. By default it is equal to 20% of max health, or one hit that would result in losing item's levels.");
            CritBonus = config.Bind("Item: " + ItemName, "Crit Bonus", 25f, "How much crit item gives.");

            CriticalDamage = config.Bind("Item: " + ItemName, "Critical Damage", 20f, "How much damage, in percentage of health, you need to take to lose item's levels.");

            HealthCheckFrequency = config.Bind("Item: " + ItemName, "Health Check Timer", 0.1f, "How frequently game check for lost HP. Higher values will result in multiple hits being lumped together for when lost health check occurs, lower velues will result in worse game performance but hits will be registered separately.");

            KillsPerLevelPerStack = config.Bind("Item: " + ItemName, "Number of Kills Per Level Reduction Per Stack", 10f, "How much, in percent, number of needed kills is being reduced by each stack. Stack hyperbolically.");
            AttackSpeedBonusPerStack = config.Bind("Item: " + ItemName, "Attack Speed Bonus Per Stack", 30f, "How much attack speed item gives per stack. By default it is equal to 2 Soldier's Syringes.");
            MovementSpeedBonusPerStack = config.Bind("Item: " + ItemName, "Movement Speed Bonus", 28f, "How much movement speed item gives per stack. By default it is equal to 2 Paul's Goat Hoofs.");
            HealthBonusPerStack = config.Bind("Item: " + ItemName, "Health Bonus", 50f, "How much health item gives per stack. By default it is equal to 2 Bison Steaks.");
            ShieldBonusPerStack = config.Bind("Item: " + ItemName, "Shield Bonus", 10f, "How much shield item gives per stack. By default it is equal to 10% of max health, or one hit that would result in losing item's levels.");
            CritBonusPerStack = config.Bind("Item: " + ItemName, "Crit Bonus", 10f, "How much crit item gives per stack.");

            MaxLevelProtection = config.Bind("Item: " + ItemName, "Max Level Protection", true, "Enables Max level protection. At level 5 you will get a buff that will save you a single time from losing item's levels.");

            DisableHUD = config.Bind("Item: " + ItemName, "Disable Adrenaline HUD", false, "Disables in-game Adrenaline HUD (level progress bar and level value text).");
        }
    }
}

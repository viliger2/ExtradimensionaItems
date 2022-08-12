using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Items
{
    public class Sheen : ItemBase<Sheen>
    {
        public static ConfigEntry<float> DamageModifier;
        public static ConfigEntry<bool> CanStack;
        public static ConfigEntry<float> BuffDuration;
        public static ConfigEntry<int> MaxBuffStacks;

        public override string ItemName => "Sheen";

        public override string ItemLangTokenName => "SHEEN";

        public override ItemTier Tier => ItemTier.Tier2;

        // I am not happy with this implementation but since there is no way to check which skill did the damage
        // we are just gonna put a flag on primary use and hope that the next non-periodic damage instance was 
        // a hit from the primary
        private static Dictionary<CharacterBody, bool> CharacterUsedPrimary = new Dictionary<CharacterBody, bool>();

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("SheenItem");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texSheenItemIcon");

        public override string BundleName => "sheen";

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // TODO: maybe someday but not today
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            LoadAssetBundle();
            CreateBuffs(AssetBundle, CanStack.Value);
            CreateItem(ref Content.Items.Sheen);
            Hooks();
        }

        protected override void Hooks()
        {
            base.Hooks();
            On.RoR2.Run.BeginStage += Run_BeginStage;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void Run_BeginStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            orig(self);
            MyLogger.LogMessage($"Clearing Dictionary<CharacterBody, bool> CharacterUsedPrimary for {ItemLangTokenName}.");
            CharacterUsedPrimary.Clear();
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            var attacker = damageInfo.attacker;
            var body = attacker.GetComponent<CharacterBody>();

            if (!damageInfo.rejected || damageInfo == null)
            {
                if (body.isPlayerControlled && body.HasBuff(Content.Buffs.Sheen) && (damageInfo.damageType & DamageType.DoT) != DamageType.DoT)
                {
                    if (CharacterUsedPrimary.TryGetValue(body, out bool bodyUsedPrimary))
                    {
                        if (bodyUsedPrimary)
                        {
                            var victimBody = victim.GetComponent<CharacterBody>();

                            DamageInfo damageInfo2 = new DamageInfo();
                            damageInfo2.damage = body.damage * GetCount(body) * DamageModifier.Value;
                            damageInfo2.attacker = attacker;
                            damageInfo2.crit = false;
                            damageInfo2.position = damageInfo.position;
                            damageInfo2.damageColorIndex = DamageColorIndex.Item;
                            damageInfo2.damageType = DamageType.Generic;

                            MyLogger.LogMessage(string.Format("Player {0}({1}) had buff {2}, dealing {3} damage to {4} and removing buff from the player.", body.GetUserName(), body.name, Content.Buffs.Sheen.name, damageInfo2.damage, victim.name));

                            victimBody.healthComponent.TakeDamage(damageInfo2);

                            body.RemoveTimedBuff(Content.Buffs.Sheen);
                            CharacterUsedPrimary[body] = false;
                        }
                    }
                }
            }
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);

            if (NetworkServer.active)
            {
                if (GetCount(self) > 0)
                {
                    var skillLocator = self.GetComponent<SkillLocator>();
                    if (skillLocator?.primary != skill && self.GetBuffCount(Content.Buffs.Sheen) < MaxBuffStacks.Value)
                    {
                        MyLogger.LogMessage(string.Format("Player {0}({1}) used non-primary skill, adding buff {2}.", self.GetUserName(), self.name, Content.Buffs.Sheen.name));
                        self.AddTimedBuff(Content.Buffs.Sheen, BuffDuration.Value);
                    }
                    else if (skillLocator?.primary == skill && self.HasBuff(Content.Buffs.Sheen))
                    {
                        CharacterUsedPrimary.AddOrReplace(self, true);
                    }
                }
            }
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, DamageModifier.Value.ToString("###%"), CanStack.Value ? MaxBuffStacks.Value : 1);
        }

        public void CreateBuffs(AssetBundle assetBundle, bool canStack)
        {
            var SheenBuff = ScriptableObject.CreateInstance<BuffDef>();
            SheenBuff.name = "Sheen Damage Bonus";
            SheenBuff.buffColor = Color.blue;
            SheenBuff.canStack = canStack;
            SheenBuff.isDebuff = false;
            SheenBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png"); // TODO: replace

            ContentAddition.AddBuffDef(SheenBuff);

            Content.Buffs.Sheen = SheenBuff;
        }

        public override void CreateConfig(ConfigFile config)
        {
            CanStack = config.Bind("Item: " + ItemName, "Can Buff Stack", true, "Determines whether the buff that indicates damage bonus can stack or not.");
            DamageModifier = config.Bind("Item: " + ItemName, "Damage Modifier", 2.5f, "What damage modifier (per stack) the item should use.");
            BuffDuration = config.Bind("Item: " + ItemName, "Buff Duration", 10f, "How long the buff should remain active after using non-primary ability.");
            MaxBuffStacks = config.Bind("Item: " + ItemName, "Maximum Buff Stacks", 8, "How many times the buff can stack.");
        }
    }
}

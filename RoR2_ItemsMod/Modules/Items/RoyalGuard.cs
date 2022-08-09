using BepInEx.Configuration;
using EntityStates;
using ExtradimensionalItems.Modules.SkillStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class RoyalGuard : ItemBase<RoyalGuard>
    {
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

        public override string ItemName => "RoyalGuard";

        public override string ItemLangTokenName => "ROYAL_GUARD";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("RoyalGuardItem");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texRoyalGuardItemIcon");

        public override string BundleName => "royalguard";

        public override bool AIBlacklisted => true;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // TODO: maybe someday but not today
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            CreateConfig(config);
            CreateSkill();
            CreateBuffs(AssetBundle);
            CreateItem(ref Content.Items.RoyalGuard);
            Hooks();
        }

        protected override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var body = self.body;
            if (body.HasBuff(Content.Buffs.RoyalGuardParryState))
            {
                var timedBuff = body.GetTimedBuff(Content.Buffs.RoyalGuardParryState);
                var parryStateDuration = GetParryStateDuration(body);
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
                if(body.GetBuffCount(Content.Buffs.RoyalGuardParryState) + numberOfBuffs > MaxBuffStacks.Value)
                {
                    numberOfBuffs = MaxBuffStacks.Value - body.GetBuffCount(Content.Buffs.RoyalGuardParryState);
                }
                for (int i = 0; i < numberOfBuffs; i++)
                {
                    body.AddBuff(Content.Buffs.RoyalGuardDamage);
                }
                MyLogger.LogMessage(string.Format("Player {0}({1}) got damaged in {2} after entering parry state. Adding {3} damage buff(s), adding grace buff and removing parry state buff.", body.GetUserName(), body.name, parryStateDuration - timedBuff.timer, numberOfBuffs));
                body.AddTimedBuff(Content.Buffs.RoyalGuardGrace, 0.0167f);
                body.RemoveTimedBuff(Content.Buffs.RoyalGuardParryState);
                damageInfo.rejected = true;
            } else if (body.HasBuff(Content.Buffs.RoyalGuardGrace))
            {
                MyLogger.LogMessage(string.Format("Player {0}({1}) got damaged while having grace buff, rejecting received damage.", body.GetUserName(), body.name));
                damageInfo.rejected = true;
            }
            orig(self, damageInfo);
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self.skillLocator)
            {
                self.ReplaceSkillIfItemPresent(self.skillLocator.utility, ItemDef.itemIndex, Content.Skills.Parry);
            }
        }

        private void CreateSkill()
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

            RoyalGuardSkillParryDef.icon = AssetBundle.LoadAsset<Sprite>("texRoyalGuardSkill");
            RoyalGuardSkillParryDef.skillDescriptionToken = "WACKY_WAHOO_PIZZA_MAN_DESCRIPTION";
            RoyalGuardSkillParryDef.skillName = "WACKY_WAHOO_PIZZA_MAN_NAME";
            RoyalGuardSkillParryDef.skillNameToken = "WACKY_WAHOO_PIZZA_MAN_NAME";

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

            RoyalGuardSkillExplodeDef.icon = AssetBundle.LoadAsset<Sprite>("texRoyalGuardSkill");
            RoyalGuardSkillExplodeDef.skillDescriptionToken = "WACKY_WAHOO_PIZZA_MAN_DESCRIPTION";
            RoyalGuardSkillExplodeDef.skillName = "WACKY_WAHOO_PIZZA_MAN_NAME";
            RoyalGuardSkillExplodeDef.skillNameToken = "WACKY_WAHOO_PIZZA_MAN_NAME";

            ContentAddition.AddSkillDef(RoyalGuardSkillExplodeDef);
            ContentAddition.AddEntityState<Explode>(out bool _);

            Content.Skills.Explode = RoyalGuardSkillExplodeDef;
        }

        public static float GetParryStateDuration(CharacterBody body)
        {
            return BaseDuration.Value + (PerStackDuration.Value * (body.inventory.GetItemCount(Content.Items.RoyalGuard) - 1));
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            //throw new System.NotImplementedException();
            return pickupString;
        }

        public static void CreateBuffs(AssetBundle assetBundle)
        {
            var RoyalGuardParryStateBuff = ScriptableObject.CreateInstance<BuffDef>();
            RoyalGuardParryStateBuff.name = "Royal Guard Parry State";
            RoyalGuardParryStateBuff.buffColor = Color.red;
            RoyalGuardParryStateBuff.canStack = false;
            RoyalGuardParryStateBuff.isDebuff = false;
            RoyalGuardParryStateBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png");

            ContentAddition.AddBuffDef(RoyalGuardParryStateBuff);

            Content.Buffs.RoyalGuardParryState = RoyalGuardParryStateBuff;

            var RoyalGuardDamageBuff = ScriptableObject.CreateInstance<BuffDef>();
            RoyalGuardDamageBuff.name = "Royal Guard Damage Buff";
            RoyalGuardDamageBuff.buffColor = Color.magenta;
            RoyalGuardDamageBuff.canStack = true;
            RoyalGuardDamageBuff.isDebuff = false;
            RoyalGuardDamageBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png");

            ContentAddition.AddBuffDef(RoyalGuardDamageBuff);

            Content.Buffs.RoyalGuardDamage = RoyalGuardDamageBuff;

            var RoyalGuardGraceBuff = ScriptableObject.CreateInstance<BuffDef>();
            RoyalGuardGraceBuff.name = "Royal Guard Grace State";
            RoyalGuardGraceBuff.buffColor = Color.green;
            RoyalGuardGraceBuff.canStack = false;
            RoyalGuardGraceBuff.isDebuff = false;
            RoyalGuardGraceBuff.isHidden = true;
            RoyalGuardGraceBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png");

            ContentAddition.AddBuffDef(RoyalGuardGraceBuff);

            Content.Buffs.RoyalGuardGrace = RoyalGuardGraceBuff;
        }

        public override void CreateConfig(ConfigFile config)
        {
            DamageModifier = config.Bind("Item: " + ItemName, "Damage Modifier", 10f, "What damage modifier (per stack) the item should use.");
            MaxBuffStacks = config.Bind("Item: " + ItemName, "Maximum Buff Stacks", 8, "How many times the buff can stack.");
            BaseDuration = config.Bind("Item: " + ItemName, "Base Parry State Duration", 0.5f, "How long is base Parry skill duration.");
            PerStackDuration = config.Bind("Item: " + ItemName, "Additional Duration Per Stack", 0.1f, "How much each start (after first one) of item adds to Parry skill duration.");
        }
    }
}

using BepInEx.Configuration;
using EntityStates;
using ExtradimensionalItems.Modules.SkillStates;
using HG;
using R2API;
using RoR2;
using RoR2.Skills;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        public static ConfigEntry<float> DamageRadius;

        public override string ItemName => "RoyalGuard";

        public override string ItemLangTokenName => "ROYAL_GUARD";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("royalguard");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texRoyalGuardItemIconGood");

        public override string BundleName => "royalguard";

        public override bool AIBlacklisted => true;

        private static GameObject RoyalGuardParryEffectInstance;
        public static GameObject RoyalGuardExplodeEffectInstance;

        // adding language checks because we can't check if token has been added to the OverlayDict for some reason
        private bool isDescAdded = false;
        private bool isParryDescAdded = false;
        private bool isReleaseDescAdded = false;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // TODO: maybe someday but not today
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            LoadSoundBank();
            CreateConfig(config);
            CreateSkills();
            CreateBuffs();
            CreateItem(ref Content.Items.RoyalGuard);
            CreateVisualEffects();
            Hooks();
        }

        protected override void LoadAssetBundle()
        {
            base.LoadAssetBundle();

            RoyalGuardParryEffectInstance = AssetBundle.LoadAsset<GameObject>("RoyalGuardEffect");
            var tempEffectComponent = RoyalGuardParryEffectInstance.AddComponent<TemporaryVisualEffect>();
            tempEffectComponent.visualTransform = RoyalGuardParryEffectInstance.GetComponent<Transform>();

            var destroyOnTimerComponent = RoyalGuardParryEffectInstance.AddComponent<DestroyOnTimer>();
            destroyOnTimerComponent.duration = 0.1f;
            MonoBehaviour[] exitComponents = new MonoBehaviour[1];
            exitComponents[0] = destroyOnTimerComponent;

            tempEffectComponent.exitComponents = exitComponents;

            RoyalGuardExplodeEffectInstance = AssetBundle.LoadAsset<GameObject>("RoyalGuard_ReleaseExplosion");

            var effectComponent = RoyalGuardExplodeEffectInstance.AddComponent<EffectComponent>();
            effectComponent.applyScale = true;
            effectComponent.soundName = "EI_RoyalGuard_Release";

            var vfxAttributes = RoyalGuardExplodeEffectInstance.AddComponent<VFXAttributes>();
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Medium;

            var destroyOnTimer = RoyalGuardExplodeEffectInstance.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 0.6f;
        }

        protected override void Hooks()
        {
            //On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            RoR2.CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            // we cannot use GlobalEventManager.onServerDamageDealt because by the time we get to our method
            // damage is already dealt, negating the entire point of blocking
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            //RoR2.GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            // implementing our own replacements instead of using base.Hooks()
            // since we also need to format skills
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if(body.skillLocator)
            {
                body.ReplaceSkillIfItemPresent(body.skillLocator.utility, ItemDef.itemIndex, Content.Skills.Parry);
            }
        }

        private string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
        {
            if (token.Equals($"ITEM_{ItemLangTokenName}_DESCRIPTION") && !isDescAdded)
            {
                LanguageAPI.AddOverlay(token, GetFormatedDiscription(orig(self, token)), self.name);
                isDescAdded = true;
            }
            else if (token.Equals($"SKILL_{ItemLangTokenName}_PARRY_DESC") && !isParryDescAdded)
            {
                LanguageAPI.AddOverlay(token, string.Format(orig(self, token), BaseDuration.Value, PerStackDuration.Value, MaxBuffStacks.Value), self.name);
                isParryDescAdded = true;
            }
            else if (token.Equals($"SKILL_{ItemLangTokenName}_RELEASE_DESC") && !isReleaseDescAdded)
            {
                LanguageAPI.AddOverlay(token, string.Format(orig(self, token), (DamageModifier.Value / 100).ToString("###%"), DamageRadius.Value), self.name);
                isReleaseDescAdded = true;
            }

            if (isDescAdded && isParryDescAdded && isReleaseDescAdded)
            {
                On.RoR2.Language.GetLocalizedStringByToken -= Language_GetLocalizedStringByToken;
            }

            return orig(self, token);
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
                if(numberOfBuffs == 3)
                {
                    Util.PlaySound("EI_RoyalGuard_JustBlock", body.gameObject);
                } else
                {
                    Util.PlaySound("EI_RoyalGuard_Block", body.gameObject);
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
                MyLogger.LogMessage(string.Format("Player {0}({1}) got damaged in {2} after entering parry state. Adding {3} damage buff(s), adding grace buff and removing parry state buff.", body.GetUserName(), body.name, parryStateDuration - timedBuff.timer, numberOfBuffs));
                body.AddTimedBuff(Content.Buffs.RoyalGuardGrace, 0.0167f);
                body.RemoveTimedBuff(Content.Buffs.RoyalGuardParryState);
                damageInfo.rejected = true;
            }
            else if (body.HasBuff(Content.Buffs.RoyalGuardGrace))
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

            RoyalGuardSkillParryDef.icon = AssetBundle.LoadAsset<Sprite>("texRoyalGuardSkill");
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

            RoyalGuardSkillExplodeDef.icon = AssetBundle.LoadAsset<Sprite>("texRoyalGuardSkill");
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

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, BaseDuration.Value, PerStackDuration.Value, (DamageModifier.Value / 100).ToString("###%"), DamageRadius.Value, MaxBuffStacks.Value);
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

            var RoyalGuardGraceBuff = ScriptableObject.CreateInstance<BuffDef>();
            RoyalGuardGraceBuff.name = "Royal Guard Grace State";
            RoyalGuardGraceBuff.buffColor = Color.green;
            RoyalGuardGraceBuff.canStack = false;
            RoyalGuardGraceBuff.isDebuff = false;
            RoyalGuardGraceBuff.isHidden = true;
            RoyalGuardGraceBuff.iconSprite = AssetBundle.LoadAsset<Sprite>("texRoyalGuardBuffIcon.png");

            ContentAddition.AddBuffDef(RoyalGuardGraceBuff);

            Content.Buffs.RoyalGuardGrace = RoyalGuardGraceBuff;
        }

        private void CreateVisualEffects()
        {
            TempVisualEffectAPI.AddTemporaryVisualEffect(RoyalGuardParryEffectInstance.InstantiateClone("RoyalGuardParryEffect", false), (CharacterBody body) => { return body.HasBuff(Content.Buffs.RoyalGuardParryState); }, false);

            R2API.ContentAddition.AddEffect(RoyalGuardExplodeEffectInstance);
        }

        public override void CreateConfig(ConfigFile config)
        {
            DamageModifier = config.Bind("Item: " + ItemName, "Damage Modifier", 1000f, "What base damage modifier (per stack) the item should use, in percentage.");
            MaxBuffStacks = config.Bind("Item: " + ItemName, "Maximum Buff Stacks", 8, "How many times the buff can stack.");
            BaseDuration = config.Bind("Item: " + ItemName, "Base Parry State Duration", 0.5f, "How long, in seconds, is base Parry skill duration.");
            PerStackDuration = config.Bind("Item: " + ItemName, "Additional Duration Per Stack", 0.1f, "How much, in seconds, each stack (after first one) of item adds to Parry skill duration.");
            DamageRadius = config.Bind("Item: " + ItemName, "Release Damage Radius", 15f, "What is the damage radius of Release skill, in meters.");
        }
    }
}

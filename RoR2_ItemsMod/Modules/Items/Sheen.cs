using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.CharacterBody;

namespace ExtradimensionalItems.Modules.Items
{
    public class Sheen : ItemBase<Sheen>
    {
        private class SheenBehavior : ItemBehavior
        {
            private GameObject RightHandEffectInstance;
            private GameObject LeftHandEffectInstance;

            public bool usedPrimary;

            public void DestroyEffects()
            {
                if (RightHandEffectInstance) { Destroy(RightHandEffectInstance); }
                if (LeftHandEffectInstance) { Destroy(LeftHandEffectInstance); }
            }

            public bool HasEffects()
            {
                return RightHandEffectInstance || LeftHandEffectInstance;
            }

            public void ShowEffects()
            {
                if(body.modelLocator.modelTransform.TryGetComponent(out ChildLocator childLocator))
                {
                    Transform leftHand = childLocator.FindChild("HandL");
                    Transform rightHand = childLocator.FindChild("HandR");
                    if (leftHand)
                    {
                        // some survivor bodies have lossy scale of 0.2, while majority have 1.0
                        // so we use this funky formula to upscale the effect for those bodies
                        // also void fiend doesn't have hands lol
                        LeftHandEffectInstance = Instantiate(SheenEffectInstance, leftHand.position, Quaternion.identity, leftHand);
                        LeftHandEffectInstance.transform.localScale = new Vector3(
                               Mathf.Max(0.5f / leftHand.lossyScale.x, 1.0f),
                               Mathf.Max(0.5f / leftHand.lossyScale.y, 1.0f),
                               Mathf.Max(0.5f / leftHand.lossyScale.z, 1.0f));
                    }
                    if (rightHand)
                    {
                        RightHandEffectInstance = Instantiate(SheenEffectInstance, rightHand.position, Quaternion.identity, rightHand);
                        LeftHandEffectInstance.transform.localScale = new Vector3(
                               Mathf.Max(0.5f / rightHand.lossyScale.x, 1.0f),
                               Mathf.Max(0.5f / rightHand.lossyScale.y, 1.0f),
                               Mathf.Max(0.5f / rightHand.lossyScale.z, 1.0f));
                    }
                }
            }
        }

        public static ConfigEntry<float> DamageModifier;
        public static ConfigEntry<bool> CanStack;
        public static ConfigEntry<float> BuffDuration;
        public static ConfigEntry<int> MaxBuffStacks;

        public override string ItemName => "Sheen";

        public override string ItemLangTokenName => "SHEEN";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("sheen");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texSheenIcon");

        private static GameObject SheenEffectInstance;

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
            CreateBuffs();
            CreateItem(ref Content.Items.Sheen);
            Hooks();
        }

        protected override void LoadAssetBundle()
        {
            base.LoadAssetBundle();
            SheenEffectInstance = AssetBundle.LoadAsset<GameObject>("SheenEffect");
        }

        protected override void Hooks()
        {
            base.Hooks();
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.CharacterBody.OnClientBuffsChanged += CharacterBody_OnClientBuffsChanged;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            self.AddItemBehavior<SheenBehavior>(GetCount(self));
        }

        private void CharacterBody_OnClientBuffsChanged(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody body)
        {
            orig(body);
            if(!body.TryGetComponent(out SheenBehavior sheenBehavior))
            {
                return;
            }
            var hasBuff = body.HasBuff(Content.Buffs.Sheen);
            if (!hasBuff && sheenBehavior.HasEffects())
            {
                sheenBehavior.DestroyEffects();
            }
            else if (hasBuff && !sheenBehavior.HasEffects())
            {
                sheenBehavior.ShowEffects();
            }
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (!damageInfo.rejected || damageInfo == null)
            {
                var attacker = damageInfo.attacker;
                if (attacker)
                {
                    var body = attacker.GetComponent<CharacterBody>();
                    if(body.TryGetComponent(out SheenBehavior sheenBehavior))
                    {
                        if(body.HasBuff(Content.Buffs.Sheen) && (damageInfo.damageType & DamageType.DoT) != DamageType.DoT && sheenBehavior.usedPrimary)
                        {
                            var victimBody = victim.GetComponent<CharacterBody>();

                            DamageInfo damageInfo2 = new DamageInfo();
                            damageInfo2.damage = body.damage * GetCount(body) * (DamageModifier.Value / 100);
                            damageInfo2.attacker = attacker;
                            damageInfo2.crit = false;
                            damageInfo2.position = damageInfo.position;
                            damageInfo2.damageColorIndex = DamageColorIndex.Item;
                            damageInfo2.damageType = DamageType.Generic;

                            MyLogger.LogMessage(string.Format("Body {0}({1}) had buff {2}, dealing {3} damage to {4} and removing buff from the body.", body.GetUserName(), body.name, Content.Buffs.Sheen.name, damageInfo2.damage, victim.name));

                            victimBody.healthComponent.TakeDamage(damageInfo2);

                            body.RemoveTimedBuff(Content.Buffs.Sheen);
                           sheenBehavior.usedPrimary = false;
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
                    else if (skillLocator?.primary == skill && self.HasBuff(Content.Buffs.Sheen) && self.TryGetComponent(out SheenBehavior component))
                    {
                        component.usedPrimary = true;
                    }
                }
            }
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, (DamageModifier.Value / 100).ToString("###%"), CanStack.Value ? MaxBuffStacks.Value : 1);
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
        }

        public override void CreateConfig(ConfigFile config)
        {
            CanStack = config.Bind("Item: " + ItemName, "Can Buff Stack", true, "Determines whether the buff that indicates damage bonus can stack or not.");
            DamageModifier = config.Bind("Item: " + ItemName, "Damage Modifier", 250f, "What damage modifier (per stack) the item should use.");
            BuffDuration = config.Bind("Item: " + ItemName, "Buff Duration", 10f, "How long the buff should remain active after using non-primary ability.");
            MaxBuffStacks = config.Bind("Item: " + ItemName, "Maximum Buff Stacks", 8, "How many times the buff can stack.");
        }
    }
}

using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class Sheen : ItemBase<Sheen>
    {
        private class SheenBehavior : CharacterBody.ItemBehavior
        {
            public bool usedPrimary;

            public void Awake()
            {
                this.enabled = false;
            }

            public void OnEnable()
            {
                if (body)
                {
                    body.onSkillActivatedServer += Body_onSkillActivatedServer;
                    GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
                }
            }

            private void OnDestroy()
            {
                if (body)
                {
                    body.onSkillActivatedServer -= Body_onSkillActivatedServer;
                    GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
                }
            }

            private void Body_onSkillActivatedServer(GenericSkill skill)
            {
                var self = body;
                if (body?.inventory?.GetItemCount(Content.Items.Sheen) > 0)
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

            private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
            {
                var damageInfo = damageReport.damageInfo;
                var attacker = damageInfo?.attacker;
                var body = attacker?.GetComponent<CharacterBody>();
                if (body && body == this.body)
                {
                    var victim = damageReport.victimBody;
                    if (!damageInfo.rejected || damageInfo == null)
                    {
                        if (body.HasBuff(Content.Buffs.Sheen) && (damageInfo.damageType & DamageType.DoT) != DamageType.DoT && this.usedPrimary)
                        {
                            var victimBody = victim.GetComponent<CharacterBody>();

                            DamageInfo damageInfo2 = new DamageInfo();
                            damageInfo2.damage = body.damage * body.inventory.GetItemCount(Content.Items.Sheen) * (DamageModifier.Value / 100);
                            damageInfo2.attacker = attacker;
                            damageInfo2.crit = false;
                            damageInfo2.position = damageInfo.position;
                            damageInfo2.damageColorIndex = DamageColorIndex.Item;
                            damageInfo2.damageType = DamageType.Generic;

                            MyLogger.LogMessage(string.Format("Body {0}({1}) had buff {2}, dealing {3} damage to {4} and removing buff from the body.", body.GetUserName(), body.name, Content.Buffs.Sheen.name, damageInfo2.damage, victim.name));

                            body.RemoveTimedBuff(Content.Buffs.Sheen);
                            this.usedPrimary = false;

                            victimBody.healthComponent.TakeDamage(damageInfo2);
                        }
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

        public override string BundleName => "sheen";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

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

        public override void CreateConfig(ConfigFile config)
        {
            CanStack = config.Bind("Item: " + ItemName, "Can Buff Stack", true, "Determines whether the buff that indicates damage bonus can stack or not.");
            DamageModifier = config.Bind("Item: " + ItemName, "Damage Modifier", 250f, "What damage modifier (per stack) the item should use.");
            BuffDuration = config.Bind("Item: " + ItemName, "Buff Duration", 10f, "How long the buff should remain active after using non-primary ability.");
            MaxBuffStacks = config.Bind("Item: " + ItemName, "Maximum Buff Stacks", 8, "How many times the buff can stack.");
        }
    }
}

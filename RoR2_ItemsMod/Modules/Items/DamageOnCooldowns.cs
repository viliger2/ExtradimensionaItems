using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class DamageOnCooldowns : ItemBase<DamageOnCooldowns>
    {
        public class DamageOnCooldownsBehavior : CharacterBody.ItemBehavior
        {
            public void FixedUpdate()
            {
                var newBuffCount = GetBuffCountFromSkill(body.skillLocator.primary)
                    + GetBuffCountFromSkill(body.skillLocator.secondary)
                    + GetBuffCountFromSkill(body.skillLocator.utility)
                    + GetBuffCountFromSkill(body.skillLocator.special)
                    + GetBuffCountFromInventory(body.equipmentSlot);

                var currentBuffCount = body.GetBuffCount(Content.Buffs.DamageOnCooldowns);
                while (newBuffCount > currentBuffCount)
                {
                    body.AddBuff(Content.Buffs.DamageOnCooldowns);
                    currentBuffCount++;
                }
                while(newBuffCount < currentBuffCount)
                {
                    body.RemoveBuff(Content.Buffs.DamageOnCooldowns);
                    newBuffCount++;
                }
            }

            private int GetBuffCountFromSkill(GenericSkill skill)
            {
                return skill.maxStock != skill.stock ? 1 : 0;
            }

            private int GetBuffCountFromInventory(EquipmentSlot es)
            {
                if (es.equipmentIndex != EquipmentIndex.None)
                {
                    return es.maxStock != es.stock ? 1 : 0;
                }
                return 0;
            }
        }

        public static ConfigEntry<float> DamageBonus;
        public static ConfigEntry<float> DamageBonusPerStack;

        public override string ItemName => "DamageOnCooldowns";

        public override string ItemLangTokenName => "DAMAGE_ON_COOLDOWNS";

        public override ItemTier Tier => ItemTier.Tier2;

        public override string BundleName => "damageoncooldowns";

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("damageoncooldowns");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texDamageOnCooldownIcon");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // TODO: maybe someday but not today
            return new ItemDisplayRuleDict();
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, (DamageBonus.Value / 100).ToString("###%"), (DamageBonusPerStack.Value / 100).ToString("###%"));
        }

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            CreateConfig(config);
            CreateBuffs();
            CreateItem(ref Content.Items.DamageOnCooldowns);
            Hooks();
        }

        private void CreateBuffs()
        {
            var DamageOnCooldownsBuff = ScriptableObject.CreateInstance<BuffDef>();
            DamageOnCooldownsBuff.name = "Damage On Cooldowns";
            DamageOnCooldownsBuff.buffColor = Color.grey;
            DamageOnCooldownsBuff.canStack = true;
            DamageOnCooldownsBuff.isDebuff = false;
            DamageOnCooldownsBuff.iconSprite = AssetBundle.LoadAsset<Sprite>("texDamageOnCooldownBuffIcon"); 

            ContentAddition.AddBuffDef(DamageOnCooldownsBuff);

            Content.Buffs.DamageOnCooldowns = DamageOnCooldownsBuff;
        }

        protected override void Hooks()
        {
            base.Hooks();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (body)
            {
                body.AddItemBehavior<DamageOnCooldownsBehavior>(GetCount(body));
                if (body.HasBuff(Content.Buffs.DamageOnCooldowns) && GetCount(body) == 0)
                {
                    while (body.HasBuff(Content.Buffs.DamageOnCooldowns))
                    {
                        body.RemoveBuff(Content.Buffs.DamageOnCooldowns);
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body.HasBuff(Content.Buffs.DamageOnCooldowns))
            {
                args.damageMultAdd += ((DamageBonus.Value / 100) + (DamageBonusPerStack.Value / 100) * (GetCount(body) - 1)) * body.GetBuffCount(Content.Buffs.DamageOnCooldowns);
            }
        }

        public override void CreateConfig(ConfigFile config)
        {
            DamageBonus = config.Bind("Item: " + ItemName, "Damage Bonus", 10f, "How much additional damage, in percentage, each ability and equipment on cooldown adds.");
            DamageBonusPerStack = config.Bind("Item: " + ItemName, "Damage Bonus Per Stack", 5f, "How much additional damage per item stack, in percentage, each each ability and equipment on cooldown adds.");
        }
    }
}

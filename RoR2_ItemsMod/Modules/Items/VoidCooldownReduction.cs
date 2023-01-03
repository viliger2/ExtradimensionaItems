using BepInEx.Configuration;
using HarmonyLib;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Linq;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class VoidCooldownReduction : ItemBase<VoidCooldownReduction>
    {
        public static ConfigEntry<float> CooldownReduction;

        public override string ItemName => "CooldownReductionVoid";

        public override string ItemLangTokenName => "COOLDOWN_REDUCTION_VOID";

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override string BundleName => "cooldownreduction";

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("pillsbottle");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texCooldownReductionIcon");

        public override ExpansionDef Expansion => ExpansionCatalog.expansionDefs.FirstOrDefault(def => def.nameToken == "DLC1_NAME");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // TODO: maybe someday but not today
            return new ItemDisplayRuleDict();
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, (CooldownReduction.Value / 100).ToString("###%"));
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            LoadAssetBundle();
            CreateItem(ref Content.Items.VoidCooldownReduction);
            Hooks();
        }

        protected override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;
        }

        private void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new ItemDef.Pair()
            {
                itemDef1 = RoR2Content.Items.Syringe,
                itemDef2 = Content.Items.VoidCooldownReduction
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.cooldownMultAdd += (float)(1 / Math.Pow(2, (CooldownReduction.Value * GetCount(sender) / 100))) - 1;
        }

        public override void CreateConfig(ConfigFile config)
        {
            CooldownReduction = config.Bind("Item: " + ItemName, "Cooldown Reduction", 10f, "How much cooldown reduction, per stack, in percentage, you get. Stacks hyperbolically, like Haste in WoW or Ability Haste in LoL, as in 100% will reduce cooldown by half, 200% by 3/4, etc.");
        }
    }
}

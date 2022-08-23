using BepInEx.Configuration;
using HarmonyLib;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class VoidCooldownReduction : ItemBase<VoidCooldownReduction>
    {
        public static ConfigEntry<float> CooldownReduction;

        public override string ItemName => "CooldownReduction";

        public override string ItemLangTokenName => "COOLDOWN_REDUCTION";

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override string BundleName => "cooldownreduction";

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override ExpansionDef Expansion => ExpansionCatalog.expansionDefs.FirstOrDefault(def => def.nameToken == "DLC1_NAME");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return pickupString;         
        }

        public override void Init(ConfigFile config)
        {
            //LoadAssetBundle();
            CreateItem(ref Content.Items.VoidCooldownReduction);
            CreateConfig(config);
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
            args.cooldownMultAdd += (1 / (1 + (CooldownReduction.Value * GetCount(sender) / 100))) - 1;
        }

        public override void CreateConfig(ConfigFile config)
        {
            CooldownReduction = config.Bind("Item: " + ItemName, "Cooldown Reduction", 15f, "How much cooldown reduction, per stack, in percentage, you get. Stacks exponentially.");
        }
    }
}

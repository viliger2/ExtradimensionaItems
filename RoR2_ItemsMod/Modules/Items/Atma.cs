using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class Atma : ItemBase<Atma>
    {
        public static ConfigEntry<float> PercentBonusDamage;
        public static ConfigEntry<float> PercentBonusDamagePerStack;

        public override string ItemName => "AtmasImpaler";

        public override string ItemLangTokenName => "ATMAS_IMPALER";

        public override ItemTier Tier => ItemTier.Tier2;

        public override string BundleName => "atma";

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("atma");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texAtmaIcon");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // TODO: maybe someday but not today
            return new ItemDisplayRuleDict();
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, (PercentBonusDamage.Value / 100).ToString("###%"), (PercentBonusDamagePerStack.Value / 100).ToString("###%"));
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            LoadAssetBundle();
            CreateItem(ref Content.Items.Atma);
            Hooks();
        }

        protected override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (GetCount(sender) > 0) {
                args.baseDamageAdd += sender.maxHealth * (PercentBonusDamage.Value / 100) + sender.maxHealth * (PercentBonusDamagePerStack.Value / 100 * (GetCount(sender) - 1));
            }
        }

        public override void CreateConfig(ConfigFile config)
        {
            PercentBonusDamage = config.Bind("Item: " + ItemName, "Percent Bonus Damage From Health", 0.5f, "How much bonus damage, in percentage, you get from health.");
            PercentBonusDamagePerStack = config.Bind("Item: " + ItemName, "Percent Bonus Damage From Health Per Item", 0.5f, "How much bonus damage, in percentage, per stack (above first), you get from health.");
        }
    }
}

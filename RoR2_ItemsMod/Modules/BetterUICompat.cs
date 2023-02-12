using RoR2;
using System;
using static BetterUI.ItemStats;

namespace ExtradimensionalItems.Modules
{
    public static class BetterUICompat
    {
        public struct StatFormatter
        {
            public static BetterUI.ItemStats.StatFormatter Charges => BetterUI.ItemStats.StatFormatter.Charges;
            public static BetterUI.ItemStats.StatFormatter Percent => BetterUI.ItemStats.StatFormatter.Percent;
            public static BetterUI.ItemStats.StatFormatter Seconds => BetterUI.ItemStats.StatFormatter.Seconds;
        }

        public struct ItemTags
        {
            public static BetterUI.ItemStats.ItemTag Damage => BetterUI.ItemStats.ItemTag.Damage;
            public static BetterUI.ItemStats.ItemTag CooldownReduction => BetterUI.ItemStats.ItemTag.SkillCooldown;
        }

        public struct StackingFormulas
        {
            public static StackingFormula LinearStacking => BetterUI.ItemStats.LinearStacking;
            public static StackingFormula ProbablyExponentialStacking => NotSureWhatExponentialStacking;
        }

        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.xoxfaby.BetterUI");
                }
                return (bool)_enabled;
            }
        }

        public static float NotSureWhatExponentialStacking(float value, float extraStackValue, int stacks)
        {
            return Math.Abs((float)(1 / Math.Pow(2, (value * stacks / 100))) - 1);
        }

        public static void AddBuffInfo(BuffDef buffDef, string nameToken = null, string descriptionToken = null)
        {
            BetterUI.Buffs.RegisterBuffInfo(buffDef, nameToken, descriptionToken);
        }

        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, StackingFormula stackingFormula = null, BetterUI.ItemStats.StatFormatter statFormater = null, BetterUI.ItemStats.ItemTag itemTag = null)
        {
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, stackingFormula, statFormater, itemTag);
        }

        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, float stackValue, StackingFormula stackingFormula = null, BetterUI.ItemStats.StatFormatter statFormater = null, BetterUI.ItemStats.ItemTag itemTag = null)
        {
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, stackValue, stackingFormula, statFormater, itemTag);
        }
    }
}
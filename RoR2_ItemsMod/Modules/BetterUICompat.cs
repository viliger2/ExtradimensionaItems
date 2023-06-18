using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ExtradimensionalItems.Modules
{
    public static class BetterUICompat
    {
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

        public enum StatFormatter
        {
            Charges,
            Percent,
            Seconds,
            DamageFromHealth
        }

        public enum ItemTag
        {
            Damage,
            CooldownReduction
        }

        public enum StackingFormula
        {
            Linear,
            NegativeExponential,
            ProbablyExponential
        }

        private static readonly Dictionary<StatFormatter, BetterUI.ItemStats.StatFormatter> statFormaters = new Dictionary<StatFormatter, BetterUI.ItemStats.StatFormatter>
        {
            {StatFormatter.Charges, BetterUI.ItemStats.StatFormatter.Charges},
            {StatFormatter.Percent, BetterUI.ItemStats.StatFormatter.Percent},
            {StatFormatter.Seconds, BetterUI.ItemStats.StatFormatter.Seconds },
            {StatFormatter.DamageFromHealth, new BetterUI.ItemStats.StatFormatter()
            {
                style = BetterUI.ItemStats.Styles.Damage,
                statFormatter = (sb, value, master) => { sb.Append((master.GetBody().maxHealth * value).ToString()); }
            } }
        };

        private static readonly Dictionary<ItemTag, BetterUI.ItemStats.ItemTag> itemTags = new Dictionary<ItemTag, BetterUI.ItemStats.ItemTag>
        {
            {ItemTag.Damage, BetterUI.ItemStats.ItemTag.Damage },
            {ItemTag.CooldownReduction, BetterUI.ItemStats.ItemTag.SkillCooldown }
        };

        private static readonly Dictionary<StackingFormula, BetterUI.ItemStats.StackingFormula> stackingFormulas = new Dictionary<StackingFormula, BetterUI.ItemStats.StackingFormula>
        {
            {StackingFormula.Linear, BetterUI.ItemStats.LinearStacking },
            {StackingFormula.NegativeExponential, BetterUI.ItemStats.NegativeExponentialStacking },
            {StackingFormula.ProbablyExponential, NotSureWhatExponentialStacking }
        };

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static float NotSureWhatExponentialStacking(float value, float extraStackValue, int stacks)
        {
            return Math.Abs((float)(1 / Math.Pow(2, (value * stacks / 100))) - 1);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddBuffInfo(BuffDef buffDef, string nameToken = null, string descriptionToken = null)
        {
            BetterUI.Buffs.RegisterBuffInfo(buffDef, nameToken, descriptionToken);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, StackingFormula stackingFormula, StatFormatter statFormatter)
        {
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, stackingFormulas[stackingFormula], statFormaters[statFormatter]);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, StackingFormula stackingFormula, StatFormatter statFormatter, ItemTag itemTag)
        {
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, stackingFormulas[stackingFormula], statFormaters[statFormatter], itemTags[itemTag]);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, float stackValue, StackingFormula stackingFormula, StatFormatter statFormatter)
        {
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, stackValue, stackingFormulas[stackingFormula], statFormaters[statFormatter]);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, float stackValue, StackingFormula stackingFormula, StatFormatter statFormatter, ItemTag itemTag)
        {
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, stackValue, stackingFormulas[stackingFormula], statFormaters[statFormatter], itemTags[itemTag]);
        }
    }
}
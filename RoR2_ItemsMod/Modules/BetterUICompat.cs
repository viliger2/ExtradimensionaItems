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

        private static BetterUI.ItemStats.StatFormatter GetStatFormatter(StatFormatter statFormatter)
        {
            switch (statFormatter)
            {
                case StatFormatter.Seconds:
                    return BetterUI.ItemStats.StatFormatter.Seconds;
                case StatFormatter.DamageFromHealth:
                    return new BetterUI.ItemStats.StatFormatter()
                    {
                        style = BetterUI.ItemStats.Styles.Damage,
                        statFormatter = (sb, value, master) => { sb.Append((master.GetBody().maxHealth * value).ToString()); }
                    };
                case StatFormatter.Percent:
                    return BetterUI.ItemStats.StatFormatter.Percent;
                case StatFormatter.Charges:
                default:
                    return BetterUI.ItemStats.StatFormatter.Charges;
            }
        }

        private static BetterUI.ItemStats.ItemTag GetItemTag(ItemTag itemTag)
        {
            switch (itemTag)
            {
                case ItemTag.CooldownReduction:
                    return BetterUI.ItemStats.ItemTag.SkillCooldown;
                case ItemTag.Damage:
                default:
                    return BetterUI.ItemStats.ItemTag.Damage;
            }
        }

        private static BetterUI.ItemStats.StackingFormula GetStackingFormula(StackingFormula stackingFormula)
        {
            switch (stackingFormula)
            {
                case StackingFormula.Linear:
                default:
                    return BetterUI.ItemStats.LinearStacking;
                case StackingFormula.NegativeExponential:
                    return BetterUI.ItemStats.NegativeExponentialStacking;
                case StackingFormula.ProbablyExponential:
                    return NotSureWhatExponentialStacking;
            }
        }

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
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, GetStackingFormula(stackingFormula), GetStatFormatter(statFormatter));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, StackingFormula stackingFormula, StatFormatter statFormatter, ItemTag itemTag)
        {
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, GetStackingFormula(stackingFormula), GetStatFormatter(statFormatter), GetItemTag(itemTag));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, float stackValue, StackingFormula stackingFormula, StatFormatter statFormatter)
        {
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, stackValue, GetStackingFormula(stackingFormula), GetStatFormatter(statFormatter));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, float stackValue, StackingFormula stackingFormula, StatFormatter statFormatter, ItemTag itemTag)
        {
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, stackValue, GetStackingFormula(stackingFormula), GetStatFormatter(statFormatter), GetItemTag(itemTag));
        }


    }
}
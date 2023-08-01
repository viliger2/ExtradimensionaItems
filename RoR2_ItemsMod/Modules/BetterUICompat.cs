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
            BetterUI.ItemStats.StackingFormula formula;
            switch (stackingFormula)
            {
                case StackingFormula.Linear:
                default:
                    formula = BetterUI.ItemStats.LinearStacking;
                    break;
                case StackingFormula.NegativeExponential:
                    formula = BetterUI.ItemStats.NegativeExponentialStacking;
                    break;
                case StackingFormula.ProbablyExponential:
                    formula = NotSureWhatExponentialStacking;
                    break;
            }

            BetterUI.ItemStats.StatFormatter formatter;
            switch (statFormatter)
            {
                case StatFormatter.Seconds:
                    formatter = BetterUI.ItemStats.StatFormatter.Seconds;
                    break;
                case StatFormatter.DamageFromHealth:
                    formatter = new BetterUI.ItemStats.StatFormatter()
                    {
                        style = BetterUI.ItemStats.Styles.Damage,
                        statFormatter = (sb, valuef, master) => { sb.Append((master.GetBody().maxHealth * valuef).ToString()); }
                    };
                    break;
                case StatFormatter.Percent:
                    formatter = BetterUI.ItemStats.StatFormatter.Percent;
                    break;
                case StatFormatter.Charges:
                default:
                    formatter = BetterUI.ItemStats.StatFormatter.Charges;
                    break;
            }

            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, formula, formatter);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, StackingFormula stackingFormula, StatFormatter statFormatter, ItemTag itemTag)
        {
            BetterUI.ItemStats.StackingFormula formula;
            switch (stackingFormula)
            {
                case StackingFormula.Linear:
                default:
                    formula = BetterUI.ItemStats.LinearStacking;
                    break;
                case StackingFormula.NegativeExponential:
                    formula = BetterUI.ItemStats.NegativeExponentialStacking;
                    break;
                case StackingFormula.ProbablyExponential:
                    formula = NotSureWhatExponentialStacking;
                    break;
            }

            BetterUI.ItemStats.StatFormatter formatter;
            switch (statFormatter)
            {
                case StatFormatter.Seconds:
                    formatter = BetterUI.ItemStats.StatFormatter.Seconds;
                    break;
                case StatFormatter.DamageFromHealth:
                    formatter = new BetterUI.ItemStats.StatFormatter()
                    {
                        style = BetterUI.ItemStats.Styles.Damage,
                        statFormatter = (sb, valuef, master) => { sb.Append((master.GetBody().maxHealth * valuef).ToString()); }
                    };
                    break;
                case StatFormatter.Percent:
                    formatter = BetterUI.ItemStats.StatFormatter.Percent;
                    break;
                case StatFormatter.Charges:
                default:
                    formatter = BetterUI.ItemStats.StatFormatter.Charges;
                    break;
            }
            BetterUI.ItemStats.ItemTag tag;
            switch (itemTag)
            {
                case ItemTag.CooldownReduction:
                    tag = BetterUI.ItemStats.ItemTag.SkillCooldown;
                    break;
                case ItemTag.Damage:
                default:
                    tag = BetterUI.ItemStats.ItemTag.Damage;
                    break;
            }

            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, formula, formatter, tag);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterStat(ItemDef itemDef, string nameToken, float value, float stackValue, StackingFormula stackingFormula, StatFormatter statFormatter)
        {
            BetterUI.ItemStats.StackingFormula formula;
            switch (stackingFormula)
            {
                case StackingFormula.Linear:
                default:
                    formula = BetterUI.ItemStats.LinearStacking;
                    break;
                case StackingFormula.NegativeExponential:
                    formula = BetterUI.ItemStats.NegativeExponentialStacking;
                    break;
                case StackingFormula.ProbablyExponential:
                    formula = NotSureWhatExponentialStacking;
                    break;
            }

            BetterUI.ItemStats.StatFormatter formatter;
            switch (statFormatter)
            {
                case StatFormatter.Seconds:
                    formatter = BetterUI.ItemStats.StatFormatter.Seconds;
                    break;
                case StatFormatter.DamageFromHealth:
                    formatter = new BetterUI.ItemStats.StatFormatter()
                    {
                        style = BetterUI.ItemStats.Styles.Damage,
                        statFormatter = (sb, valuef, master) => {
                            sb.Append((master.GetBody().levelDamage * (master.GetBody().maxHealth / valuef < 1 ? 1 : master.GetBody().maxHealth / valuef)).ToString()); 
                        }
                    };
                    break;
                case StatFormatter.Percent:
                    formatter = BetterUI.ItemStats.StatFormatter.Percent;
                    break;
                case StatFormatter.Charges:
                default:
                    formatter = BetterUI.ItemStats.StatFormatter.Charges;
                    break;
            }
            BetterUI.ItemStats.RegisterStat(itemDef, nameToken, value, stackValue, formula, formatter);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void ModifyBetterUIStat(ItemDef itemDef, string nameToken, float value, float stackValue)
        {
            var itemStats = BetterUI.ItemStats.GetItemStats(itemDef);
            foreach(var itemStatsItem in itemStats)
            {
                if (itemStatsItem.nameToken.Equals(nameToken))
                {
                    itemStatsItem.value = value;
                    itemStatsItem.stackValue = stackValue;
                }
            }
        }
    }
}
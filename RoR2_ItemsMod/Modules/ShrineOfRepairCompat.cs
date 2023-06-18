using RoR2;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ExtradimensionalItems.Modules
{
    public static class ShrineOfRepairCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Viliger.ShrineOfRepair");
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddListenerToFillDictionary()
        {
            ShrineOfRepair.Modules.ModExtension.AddListener(AddFuelCellDepletedToRepairList);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddFuelCellDepletedToRepairList(ref List<ShrineOfRepair.Modules.ModExtension.RepairableItems> list)
        {
            list.Add(new ShrineOfRepair.Modules.ModExtension.RepairableItems
            {
                brokenItem = Content.Items.FuelCellDepleted.itemIndex,
                repairedItem = RoR2Content.Items.EquipmentMagazine.itemIndex
            });
        }
    }
}

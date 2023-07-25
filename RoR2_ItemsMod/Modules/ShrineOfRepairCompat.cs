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
            ShrineOfRepair.Modules.ModExtension.AddItemsListener(AddFuelCellDepletedToRepairList);
            //ShrineOfRepair.Modules.ModExtension.AddEquipmentListener(AddWhateverDebugCrap);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddFuelCellDepletedToRepairList()
        {
            ShrineOfRepair.Modules.ModExtension.AddItemsToList(Content.Items.FuelCellDepleted.itemIndex, RoR2Content.Items.EquipmentMagazine.itemIndex, "ExtradimensionalItems");
        }

        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        //public static void AddWhateverDebugCrap()
        //{
        //    ShrineOfRepair.Modules.ModExtension.AddEquipmentToList(RoR2Content.Equipment.Recycle.equipmentIndex, RoR2Content.Equipment.AffixHaunted.equipmentIndex, "ExtradimensionalItems");
        //}
    }
}

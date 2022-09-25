using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class FuelCellDepleted : ItemBase<FuelCellDepleted>
    {
        public override string ItemName => "FuelCellDepleted";

        public override string ItemLangTokenName => "FUEL_CELL_DEPLETED";

        public override ItemTier Tier => ItemTier.NoTier;

        public override string BundleName => "fuelcelldepleted";

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texFuelCellDepletedIcon");

        public override bool AIBlacklisted => true;

        public override bool CanRemove => false;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // TODO: maybe someday but not today
            return new ItemDisplayRuleDict();
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return pickupString;
        }

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            CreateItem(ref Content.Items.FuelCellDepleted);
            if (ShrineOfRepairCompat.enabled)
            {
                ShrineOfRepairCompat.AddListenerToFillDictionary(AddFuelCellDepletedToRepairList);
            }
        }

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

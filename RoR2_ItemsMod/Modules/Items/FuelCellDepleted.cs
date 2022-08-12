using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{
    public class FuelCellDepleted : ItemBase<FuelCellDepleted>
    {
        public override string ItemName => "FuelCellDepleted";

        public override string ItemLangTokenName => "FUEL_CELL_DEPLETED";

        public override ItemTier Tier => ItemTier.Tier2;

        public override string BundleName => "fuelcelldepleted";

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("FuelCellDepleted");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texFuelCellDepletedIcon");

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
            LoadAssetBundle();
            CreateItem(ref Content.Items.FuelCellDepleted);
            if (ShrineOfRepairCompat.enabled)
            {
                ShrineOfRepairCompat.AddListenerToFillDictionary(AddFuelCellDepletedToRepairList);
            }
        }

        public static void AddFuelCellDepletedToRepairList(ref Dictionary<ItemIndex, ItemIndex> dic)
        {
            if (!dic.ContainsKey(Content.Items.FuelCellDepleted.itemIndex))
            {
                dic.Add(Content.Items.FuelCellDepleted.itemIndex, RoR2Content.Items.EquipmentMagazine.itemIndex);
            }
        }

    }
}

using BepInEx.Configuration;
using R2API;
using RoR2;
using SimpleJSON;
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
            return new ItemDisplayRuleDict();
        }

        public override string GetOverlayDescription(string value, JSONNode tokensNode)
        {
            return value;
            //throw new System.NotImplementedException();
        }

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            LoadLanguageFile();
            CreateItem(ref Content.Items.FuelCellDepleted);
            if (ShrineOfRepairCompat.enabled)
            {
                ShrineOfRepairCompat.AddListenerToFillDictionary();
            }
        }
    }
}

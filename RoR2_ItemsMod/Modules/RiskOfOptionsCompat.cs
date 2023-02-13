using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace ExtradimensionalItems.Modules
{
    public static class RiskOfOptionsCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                }
                return (bool)_enabled;
            }
        }

        public static void SetDescription()
        {
            ModSettingsManager.SetModDescription("Items from different worlds.", "com.Viliger.ExtradimensionalItems", "ExtradimensionalItems");
        }

        public static void SetIcon()
        {
            var bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExtradimensionalItemsPlugin.PInfo.Location), ExtradimensionalItemsPlugin.BundleFolder, "config"));

            Sprite icon = bundle.LoadAsset<Sprite>("ModIcon.png");
            ModSettingsManager.SetModIcon(icon, "com.Viliger.ExtradimensionalItems", "ExtradimensionalItems");
        }

        public static void CreateNewOption(ConfigEntry<float> entry)
        {
            ModSettingsManager.AddOption(new StepSliderOption(entry, new StepSliderConfig()));
        }

        public static void CreateNewOption(ConfigEntry<float> entry, float min, float max, float increment = 1f)
        {
            ModSettingsManager.AddOption(new StepSliderOption(entry, new StepSliderConfig() { min = min, max = max, increment = increment }));
        }

        public static void CreateNewOption(ConfigEntry<bool> entry)
        {
            ModSettingsManager.AddOption(new CheckBoxOption(entry));
        }

        public static void CreateNewOption(ConfigEntry<int> entry, int min = 0, int max = 200)
        {
            ModSettingsManager.AddOption(new IntSliderOption(entry, new IntSliderConfig() { min = min, max = max }));
        }

    }
}
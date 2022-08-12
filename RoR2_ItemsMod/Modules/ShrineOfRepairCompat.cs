using RoR2;
using System.Collections.Generic;
using UnityEngine.Events;

namespace ExtradimensionalItems.Modules
{
    public static class ShrineOfRepairCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if(_enabled == null )
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Viliger.ShrineOfRepair");       
                }
                return (bool)_enabled;
            }
        }

        public static void AddListenerToFillDictionary(ShrineOfRepair.Modules.ModExtension.DictionaryFillDelegate callback)
        {
            ShrineOfRepair.Modules.ModExtension.AddListener(callback);
        }
    }
}

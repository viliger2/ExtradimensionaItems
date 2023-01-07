using BepInEx;
using BepInEx.Configuration;
using ExtradimensionalItems.Modules.Equipment;
using ExtradimensionalItems.Modules.Items;
using R2API.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ExtradimensionalItems.Modules.Equipment.Chronoshift;

namespace ExtradimensionalItems.Modules
{
    [BepInPlugin("com.Viliger.ExtradimensionalItems", "ExtradimensionalItems", "1.0")]
    [BepInDependency(R2API.ItemAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(R2API.TempVisualEffectAPI.PluginGUID)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency("com.Viliger.ShrineOfRepair", BepInDependency.DependencyFlags.SoftDependency)]
    public class ExtradimensionalItemsPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> ExtensiveLogging;

        public static BepInEx.PluginInfo PInfo;

        public const string BundleFolder = "Assets";
        public const string SoundBanksFolder = "Soundbanks";

        // thanks KomradeSpectre
        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"Stubbed Hopoo Games/Deferred/Standard", "shaders/deferred/hgstandard"},
            {"Hopoo Games/FX/Cloud Remap", "shaders/fx/hgcloudremap" },
            {"Stubbed Hopoo Games/FX/Cloud Remap", "shaders/fx/hgcloudremap" }
            //{"fake ror/hopoo games/fx/hgcloud intersection remap", "shaders/fx/hgintersectioncloudremap" },
            //{"fake ror/hopoo games/fx/hgcloud remap", "shaders/fx/hgcloudremap" },
            //{"fake ror/hopoo games/fx/hgdistortion", "shaders/fx/hgdistortion" },
            //{"fake ror/hopoo games/deferred/hgsnow topped", "shaders/deferred/hgsnowtopped" },
            //{"fake ror/hopoo games/fx/hgsolid parallax", "shaders/fx/hgsolidparallax" }
        };

        private void Awake()
        {
            ExtensiveLogging = Config.Bind("Logging", "Enable extensive logging?", true, "Enables extensive logging, logs every major event related to new content.");

            MyLogger.Init(Logger);
            //MyLogger = Logger;
            PInfo = Info;

#if DEBUG == true
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
#endif

            new ExtradimensionalItemsLanguages().Init(PInfo);

            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));
            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                if (ValidateItem(item))
                {
                    item.Init(Config);
                    MyLogger.LogInfo($"Item: {item.ItemLangTokenName} loaded.");
                }
            }

            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));
            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                if (ValidateEquipment(equipment))
                {
                    equipment.Init(Config);
                    MyLogger.LogInfo($"Equipment: {equipment.EquipmentLangTokenName} loaded.");
                }
            }

            #region ChronoshiftMessages
            NetworkingAPI.RegisterMessageType<ChronoshiftStartMovingOnClient>();
            NetworkingAPI.RegisterMessageType<ChronoshiftRestoreStateOnServer>();
            #endregion

            #region DamageOnCooldownsMessages
            NetworkingAPI.RegisterMessageType<DamageOnCooldowns.DamageOnCooldownsSendNumberBuffs>();
            #endregion

        }

        public bool ValidateEquipment(EquipmentBase equipment)
        {
            var enabled = Config.Bind("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value;

            return enabled;

        }

        public bool ValidateItem(ItemBase item)
        {
            var enabled = Config.Bind("Item: " + item.ItemName, "Enable item?", true, "Should this item appear in runs?").Value;

            return enabled;
        }
    }

}

using BepInEx.Configuration;
using ExtradimensionalItems.Modules.Interactables;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Equipment
{
    public class RespawnFlag : EquipmentBase<RespawnFlag>
    {
        // TODO: implement spending of batteries on death instead of destroying the flag (as option) 

        public static ConfigEntry<bool> EnableFuelCellInteraction;

        public override string EquipmentName => "RespawnFlag";

        public override string EquipmentLangTokenName => "RESPAWN_FLAG";

        public override string BundleName => "respawnflag";

        public override GameObject EquipmentModel => AssetBundle.LoadAsset<GameObject>("FlagItem");

        public override Sprite EquipmentIcon => AssetBundle.LoadAsset<Sprite>("texFlagItemIcon");

        public override float Cooldown => 0.1f;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // TODO: maybe someday but not today
            return new ItemDisplayRuleDict();
        }

        private static GameObject flagInteractablePrefab;

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            LoadInteractable();
            CreateConfig(config);
            CreateEquipment(ref Content.Equipment.RespawnFlag);
            Hooks();
        }

        private void LoadInteractable()
        {
            var flagInteractablePrefab2 = RespawnFlagInteractable.GetInteractable(AssetBundle.LoadAsset<GameObject>("FlagInteractable"), EquipmentLangTokenName);
            flagInteractablePrefab = PrefabAPI.InstantiateClone(flagInteractablePrefab2, "RespawnFlagInteractable"); // always use PrefabAPI, it will network it
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return pickupString;
        }

        protected override void Hooks()
        {
            base.Hooks();
            On.RoR2.CharacterBody.OnEquipmentGained += CharacterBody_OnEquipmentGained;
        }

        private void CharacterBody_OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);

            if (equipmentDef != Content.Equipment.RespawnFlag)
            {
                for(uint i = 0; i< self.inventory.GetEquipmentSlotCount(); i++)
                {
                    var equipmentState = self.inventory.GetEquipment(i);
                    if(equipmentState.equipmentIndex == EquipmentIndex.None)
                    {
                        return;
                    }
                }
                var result = DestroyExistingFlag(self, out Vector3 position);
                if (result)
                {
                    MyLogger.LogMessage(string.Format("Player {0}({1}) has existing {2} and picked up new equipment, destroying it and spawning equipment at its place.", self.GetUserName(), self.name, EquipmentLangTokenName));
                    PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(Content.Equipment.RespawnFlag.equipmentIndex);
                    PickupDropletController.CreatePickupDroplet(pickupIndex, position, Vector3.up * 5);
                }
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;

            if (!body || !body.teamComponent) return false;

            MyLogger.LogMessage(string.Format("Player {0}({1}) used equipment {2}.", body.GetUserName(), body.name, EquipmentLangTokenName));

            var result = DestroyExistingFlag(body, out _);
            if (result)
            {
                MyLogger.LogMessage(string.Format("Player {0}({1}) has existing {2}, destroying it.", body.GetUserName(), body.name, EquipmentLangTokenName));
            }

            GameObject gameObject = Object.Instantiate(flagInteractablePrefab, body.transform.position, Quaternion.identity);
            RespawnFlagInteractable.RespawnFlagInteractableManager flagManager = gameObject.GetComponent<RespawnFlagInteractable.RespawnFlagInteractableManager>();
            flagManager.owner = body;

            NetworkServer.Spawn(gameObject);

            // thanks ThinkInvisible
            body.inventory.SetEquipment(new EquipmentState(EquipmentIndex.None, Run.FixedTimeStamp.now + Cooldown, 0), (uint)slot.characterBody.inventory.activeEquipmentSlot);

            return true;
        }

        private bool DestroyExistingFlag(CharacterBody body, out Vector3 position)
        {
            position = new Vector3();

            var objects = GameObject.FindGameObjectsWithTag("Respawn");

            GameObject existingGameObject = objects.ToList().Find(x =>
            {
                return x.GetComponent<RespawnFlagInteractable.RespawnFlagInteractableManager>()?.owner == body;
            });

            if (existingGameObject)
            {
                position = existingGameObject.transform.position;
                Object.Destroy(existingGameObject);
                return true;
            }

            return false;
        }

        protected override void CreateConfig(ConfigFile config)
        {
            EnableFuelCellInteraction = config.Bind("Equipment: " + EquipmentName, "Enable Fuel Cell Interaction", true, "Enables Fuel Cell interaction, using Fuel Cells to revive instead of destroying checkpoint.");
        }
    }

}

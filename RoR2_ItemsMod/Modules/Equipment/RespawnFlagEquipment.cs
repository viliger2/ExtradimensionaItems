using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using ExtradimensionalItems.Modules.Interactables;
using static ExtradimensionalItems.Modules.ExtradimensionalItemsPlugin;

namespace ExtradimensionalItems.Modules.Equipment
{
    internal class RespawnFlagEquipment : EquipmentBase<RespawnFlagEquipment>
    {
        public override string EquipmentName => "Checkpoint";

        public override string EquipmentLangTokenName => "RESPAWN_FLAG";

        public override string BundleName => "respawnflag";

        public override GameObject EquipmentModel => AssetBundle.LoadAsset<GameObject>("FlagItem.prefab");

        public override Sprite EquipmentIcon => AssetBundle.LoadAsset<Sprite>("FlagItemIcon.png");

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
            CreateEquipment();
        }

        private void LoadInteractable()
        {
            var flagInteractablePrefab2 = RespawnFlagInteractable.GetInteractable(AssetBundle.LoadAsset<GameObject>("FlagInteractable"), EquipmentLangTokenName);
            flagInteractablePrefab = PrefabAPI.InstantiateClone(flagInteractablePrefab2, "RespawnFlagInteractable"); // always use PrefabAPI, it will network it
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.teamComponent) return false;

            GameObject gameObject = UnityEngine.Object.Instantiate(flagInteractablePrefab, slot.characterBody.transform.position, Quaternion.identity);
            var flagManager = gameObject.GetComponent<RespawnFlagInteractable.RespawnFlagInteractableManager>();
            flagManager.owner = slot.characterBody;
            flagManager.flagEquipmentIndex = EquipmentDef.equipmentIndex;

            NetworkServer.Spawn(gameObject);

            // thanks ThinkInvisible
            slot.characterBody.inventory.SetEquipment(new EquipmentState(EquipmentIndex.None, Run.FixedTimeStamp.now + Cooldown, 0), (uint)slot.characterBody.inventory.activeEquipmentSlot);

            return true;
        }
    }

}

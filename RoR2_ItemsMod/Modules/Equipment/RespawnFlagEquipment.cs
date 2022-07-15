﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
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

        public static List<GameObject> ListOfExistingFlags = new List<GameObject>();

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            LoadInteractable();
            CreateEquipment();
            Hooks();
        }

        private void LoadInteractable()
        {
            var flagInteractablePrefab2 = RespawnFlagInteractable.GetInteractable(AssetBundle.LoadAsset<GameObject>("FlagInteractable"), EquipmentLangTokenName);
            flagInteractablePrefab = PrefabAPI.InstantiateClone(flagInteractablePrefab2, "RespawnFlagInteractable"); // always use PrefabAPI, it will network it
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;

            if (!body || !body.teamComponent) return false;

            MyLogger.LogMessage(string.Format("Player {0}({1}) used equipment {2}.", body.GetUserName(), body.name, EquipmentLangTokenName));

            GameObject gameObject = Object.Instantiate(flagInteractablePrefab, body.transform.position, Quaternion.identity);
            RespawnFlagInteractable.RespawnFlagInteractableManager flagManager = gameObject.GetComponent<RespawnFlagInteractable.RespawnFlagInteractableManager>();
            flagManager.owner = body;
            flagManager.flagEquipmentIndex = EquipmentDef.equipmentIndex;

            GameObject existingGameObject = ListOfExistingFlags.Find(x => {
                                        var flagManager2 = x.GetComponent<RespawnFlagInteractable.RespawnFlagInteractableManager>();
                                        return flagManager2.owner == body;
                                        });
            if (existingGameObject)
            {
                MyLogger.LogMessage(string.Format("Player {0}({1}) has existing {2}, destroying it and removing it the list.", body.GetUserName(), body.name, EquipmentLangTokenName));
                ListOfExistingFlags.Remove(existingGameObject);
                Object.Destroy(existingGameObject);
            }

            NetworkServer.Spawn(gameObject);
            ListOfExistingFlags.Add(gameObject);

            // thanks ThinkInvisible
            body.inventory.SetEquipment(new EquipmentState(EquipmentIndex.None, Run.FixedTimeStamp.now + Cooldown, 0), (uint)slot.characterBody.inventory.activeEquipmentSlot);

            return true;
        }

        protected override void Hooks()
        {
            On.RoR2.Run.BeginStage += Run_BeginStage;
        }

        private void Run_BeginStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            orig(self);
            MyLogger.LogMessage($"Clearing list of existing INTERACTABLE_{EquipmentLangTokenName}.");
            ListOfExistingFlags.Clear();
        }
    }

}

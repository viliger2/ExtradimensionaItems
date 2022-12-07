using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static ExtradimensionalItems.Modules.Equipment.RespawnFlag;

namespace ExtradimensionalItems.Modules.Interactables
{
    internal class RespawnFlagInteractable
    {
        public static GameObject GetInteractable(GameObject interactableModel, string langToken)
        {
            interactableModel.AddComponent<NetworkIdentity>();

            var genericInteractController = interactableModel.AddComponent<GenericInteraction>();
            genericInteractController.contextToken = $"INTERACTABLE_{langToken}_CONTEXT";
            genericInteractController.shouldShowOnScanner = false;

            var genericNameDisplay = interactableModel.AddComponent<GenericDisplayNameProvider>();
            genericNameDisplay.displayToken = $"INTERACTABLE_{langToken}_NAME";

            var modelLocator = interactableModel.AddComponent<ModelLocator>();
            modelLocator.modelTransform = interactableModel.transform.Find("mdlRespawnFlagInteractable");
            modelLocator.modelBaseTransform = interactableModel.transform.Find("Base");
            modelLocator.dontDetatchFromParent = false;
            modelLocator.autoUpdateModelTransform = true;

            var entityLocator = interactableModel.GetComponentInChildren<MeshCollider>().gameObject.AddComponent<EntityLocator>();
            entityLocator.entity = interactableModel;

            var respawnFlagManager = interactableModel.AddComponent<RespawnFlagInteractableManager>();
            respawnFlagManager.genericInteraction = genericInteractController;
            respawnFlagManager.langToken = langToken;

            var highlightController = interactableModel.AddComponent<Highlight>();
            highlightController.targetRenderer = interactableModel.GetComponentsInChildren<SkinnedMeshRenderer>().Where(x => x.gameObject.name.Contains("mdlRespawnFlagInteractable")).First();
            highlightController.strength = 1;
            highlightController.highlightColor = Highlight.HighlightColor.interactive;

            interactableModel.tag = "Respawn";

            return interactableModel;
        }

        public class RespawnFlagInteractableManager : NetworkBehaviour
        {
            public GenericInteraction genericInteraction;
            public CharacterBody owner;
            public string langToken;

            public void Start()
            {
                genericInteraction.onActivation.AddListener(OnActivation);
                // You are not supposed to use hooks when onBodyDeath is itself a UnityEvent
                // however due to how both Dios are made, using event will result in two respawns
                // consuming both Dio and the flag, to alliviate this issue we use hook
                // Can probably be fixed with Reflection
                //owner.master.onBodyDeath.AddListener(OnBodyDeath);
                On.RoR2.CharacterMaster.OnBodyDeath += OnBodyDeath;

            }

            //public void OnBodyDeath()
            //{
            //    if (NetworkServer.active)
            //    {
            //        if (gameObject && owner.isPlayerControlled && owner.inventory.GetItemCount(RoR2Content.Items.ExtraLife) == 0 && owner.inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoid) == 0)
            //        {
            //            var newbody = owner.master.Respawn(gameObject.transform.position, owner.master.transform.rotation);

            //            GameObject spawnEffect = Resources.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
            //            EffectManager.SpawnEffect(spawnEffect, new EffectData
            //            {
            //                origin = gameObject.transform.position,
            //                rotation = owner.master.gameObject.transform.rotation
            //            }, true);

            //            Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
            //            {
            //                subjectAsCharacterBody = newbody,
            //                baseToken = $"INTERACTABLE_{langToken}_RESPAWN"
            //            });

            //            if (Equipment.RespawnFlag.EnableFuelCellInteraction.Value && newbody.inventory.GetItemCount(RoR2Content.Items.EquipmentMagazine) > 0)
            //            {
            //                MyLogger.LogMessage(string.Format("Player {0}({1}) has died, has {2} up and has {3} in their inventory, respawning them and replacing {3} with {4}.", newbody.GetUserName(), newbody.name, $"INTERACTABLE_{langToken}", RoR2Content.Items.EquipmentMagazine.name, Content.Items.FuelCellDepleted.name));
            //                newbody.inventory.RemoveItem(RoR2Content.Items.EquipmentMagazine);
            //                newbody.inventory.GiveItem(Content.Items.FuelCellDepleted);
            //                CharacterMasterNotificationQueue.SendTransformNotification(newbody.master, RoR2Content.Items.EquipmentMagazine.itemIndex, Content.Items.FuelCellDepleted.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            //                owner = newbody;
            //            }
            //            else
            //            {
            //                MyLogger.LogMessage(string.Format("Player {0}({1}) has died and has {2} up, respawning them and destroying interactable.", newbody.GetUserName(), newbody.name, $"INTERACTABLE_{langToken}"));
            //                RespawnFlagBehavior behavior = newbody.GetComponent<RespawnFlagBehavior>();
            //                if (behavior)
            //                {
            //                    Object.Destroy(behavior);
            //                }
            //                Destroy(owner);
            //                Destroy(gameObject);
            //            }
            //        }
            //    }
            //}

            public void OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
            {
                bool overrideVanilla = false;
                if (NetworkServer.active)
                {
                    if (gameObject && owner.isPlayerControlled && owner.inventory.GetItemCount(RoR2Content.Items.ExtraLife) == 0 && owner.inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoid) == 0)
                    {
                        overrideVanilla = true;
                        Invoke("RespawnOnCheckpoint", 2f);
                        Invoke("PlayExtraLifeSFX", 1f);
                    }
                }
                if (!overrideVanilla)
                {
                    orig(self, body);
                }
            }

            public void RespawnOnCheckpoint()
            {
                CharacterMaster master = owner.master;

                if (EnableFuelCellInteraction.Value && owner.inventory.GetItemCount(RoR2Content.Items.EquipmentMagazine) > 0)
                {
                    MyLogger.LogMessage(string.Format("Player {0}({1}) has died, has {2} up and has {3} in their inventory, respawning them and replacing {3} with {4}.", owner.GetUserName(), owner.name, $"INTERACTABLE_{langToken}", RoR2Content.Items.EquipmentMagazine.name, Content.Items.FuelCellDepleted.name));
                    owner.inventory.RemoveItem(RoR2Content.Items.EquipmentMagazine);
                    owner.inventory.GiveItem(Content.Items.FuelCellDepleted);
                    CharacterMasterNotificationQueue.SendTransformNotification(master, RoR2Content.Items.EquipmentMagazine.itemIndex, Content.Items.FuelCellDepleted.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                } else
                {
                    MyLogger.LogMessage(string.Format("Player {0}({1}) has died and has {2} up, respawning them and destroying interactable.", owner.GetUserName(), owner.name, $"INTERACTABLE_{langToken}"));
                    RespawnFlagBehavior behavior = owner.GetComponent<RespawnFlagBehavior>();
                    if (behavior)
                    {
                        Destroy(behavior);
                    }
                    Destroy(gameObject);
                }

                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = owner,
                    baseToken = $"INTERACTABLE_{langToken}_RESPAWN"
                });

                master.Respawn(gameObject.transform.position, gameObject.transform.rotation);
                master.GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 3f);

                GameObject respawnEffect = Resources.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
                if (respawnEffect)
                {
                    EffectManager.SpawnEffect(respawnEffect, new EffectData
                    {
                        origin = gameObject.transform.position,
                        rotation = gameObject.transform.rotation
                    }, true);
                }

                owner = master.GetBody();
            }

            public void PlayExtraLifeSFX()
            {
                owner.master.PlayExtraLifeSFX();
            }

            public void OnDestroy()
            {
                On.RoR2.CharacterMaster.OnBodyDeath -= OnBodyDeath;
                //owner.master.onBodyDeath.RemoveListener(OnBodyDeath);
                genericInteraction.onActivation.RemoveListener(OnActivation);
            }

            [Server]
            public void OnActivation(Interactor interactor)
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogWarning("[Server] function 'ExtradimensionalItems.Modules.Interactables.RespawnFlagInteractable::OnActivation(RoR2.Interactor)' called on client.");
                    return;
                }

                var body = interactor.GetComponent<CharacterBody>();
                // we are checking for owner here instead of On.RoR2.GenericInteraction.RoR2_IInteractable_GetInteractability
                // because it doesn't work, most likely something wrong with game's code since MMMHOOK is generated on launch
                // TODO: write IL hook that might or might not work
                if (body && body == owner)
                {
                    MyLogger.LogMessage(string.Format("Player {0}({1}) used their {2}, spawning equipment and destroying interactable.", body.GetUserName(), body.name, $"INTERACTABLE_{langToken}"));

                    PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(Content.Equipment.RespawnFlag.equipmentIndex);
                    PickupDropletController.CreatePickupDroplet(pickupIndex, transform.position, Vector3.up * 5 + transform.forward * 3);

                    Destroy(gameObject);
                }
            }
        }
    }
}

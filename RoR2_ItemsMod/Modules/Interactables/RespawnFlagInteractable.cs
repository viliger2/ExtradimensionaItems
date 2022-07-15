using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using RoR2;
using static ExtradimensionalItems.Modules.ExtradimensionalItemsPlugin;

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
            modelLocator.modelTransform = interactableModel.transform.Find("mdlFlagInteractable");
            modelLocator.modelBaseTransform = interactableModel.transform.Find("mdlFlagInteractable");
            modelLocator.dontDetatchFromParent = true;
            modelLocator.autoUpdateModelTransform = true;

            var entityLocator = interactableModel.GetComponentInChildren<MeshCollider>().gameObject.AddComponent<EntityLocator>();
            entityLocator.entity = interactableModel;

            var respawnFlagManager = interactableModel.AddComponent<RespawnFlagInteractableManager>();
            respawnFlagManager.genericInteraction = genericInteractController;
            respawnFlagManager.langToken = langToken;

            var highlightController = interactableModel.AddComponent<Highlight>();
            highlightController.targetRenderer = interactableModel.GetComponentsInChildren<MeshRenderer>().Where(x => x.gameObject.name.Contains("mdlFlagInteractable")).First();
            highlightController.strength = 1;
            highlightController.highlightColor = Highlight.HighlightColor.interactive;

            return interactableModel;
        }

        public class RespawnFlagInteractableManager : NetworkBehaviour
        {
            public GenericInteraction genericInteraction;
            public CharacterBody owner;
            public string langToken;
            public EquipmentIndex flagEquipmentIndex;

            public void Start()
            {
                genericInteraction.onActivation.AddListener(OnActivation);
                On.RoR2.CharacterMaster.OnBodyDeath += OnBodyDeath;
            }

            public void OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
            {
                orig(self, body);
                if (NetworkServer.active)
                {
                    if (this.gameObject && body.isPlayerControlled && body == owner)
                    {
                        body.master.Respawn(this.gameObject.transform.position, body.master.transform.rotation);

                        GameObject spawnEffect = Resources.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
                        EffectManager.SpawnEffect(spawnEffect, new EffectData
                        {
                            origin = this.gameObject.transform.position,
                            rotation = body.master.gameObject.transform.rotation
                        }, true);

                        Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                        {
                            subjectAsCharacterBody = body,
                            baseToken = $"INTERACTABLE_{langToken}_INTERACT"
                        });

                        UnityEngine.Object.Destroy(this.gameObject);
                    }
                }
            }

            public void OnDestroy()
            {
                On.RoR2.CharacterMaster.OnBodyDeath -= OnBodyDeath;
                genericInteraction.onActivation.RemoveListener(OnActivation);
            }

            [Server]
            public void OnActivation(Interactor interactor)
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogWarning("[Server] function 'ExtradimensionalItems.Modules.Interactables.RespawnFlagInteractable::OnActivation(RoR2.Interactor)' called on cliend.");
                    return;
                }

                var charBody = interactor.GetComponent<CharacterBody>();
                if (charBody && charBody == owner)
                {
                    var pickupIndex = PickupCatalog.FindPickupIndex(flagEquipmentIndex);
                    PickupDropletController.CreatePickupDroplet(pickupIndex, transform.position, Vector3.up * 5 + transform.forward * 3);
                    UnityEngine.Object.Destroy(this.gameObject);
                }
            }
        }
    }
}

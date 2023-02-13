using RoR2;
using static ExtradimensionalItems.Modules.Equipment.RespawnFlag;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.Audio;
using UnityEngine.Bindings;

namespace ExtradimensionalItems.Modules.Interactables
{
    // use NetworkWeaver after build to patch dll so it actually works
    public class RespawnFlagInteractableManager : NetworkBehaviour, IInteractable, IDisplayNameProvider
    {
        private CharacterBody _owner;

        [SyncVar]
        private uint ownerNetId;

        public CharacterBody owner
        {
            get
            {
                if (_owner)
                {
                    return _owner;
                }
                else
                {
                    var gameObject = Util.FindNetworkObject(new NetworkInstanceId(ownerNetId));
                    if (gameObject)
                    {
                        return gameObject.GetComponent<CharacterBody>();
                    }
                    return null;
                }
            }
            set
            {
                _owner = value;
                ownerNetId = value.networkIdentity.netId.Value;
            }
        }

        public string langToken;

        public void Start()
        {
            EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_Checkpoint_Use", gameObject);
            // TODO: You are not supposed to use hooks when onBodyDeath is itself a UnityEvent
            // however due to how both Dios are made, using event will result in two respawns
            // consuming both Dio and the flag, to alliviate this issue we use hook
            // Can probably be fixed with Reflection
            //owner.master.onBodyDeath.AddListener(OnBodyDeath);
            On.RoR2.CharacterMaster.OnBodyDeath += OnBodyDeath;
        }

        public void OnDestroy()
        {
            On.RoR2.CharacterMaster.OnBodyDeath -= OnBodyDeath;
        }

        public void OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            if (NetworkServer.active)
            {
                if (owner && gameObject && owner.isPlayerControlled && body == owner)
                {
                    Invoke("RespawnOnCheckpoint", 2f);
                    Invoke("PlayExtraLifeSFX", 1f);
                }
                else
                {
                    orig(self, body);
                }
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
            }
            else
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

        public string GetContextString([NotNull] Interactor activator)
        {
            return Language.GetString($"INTERACTABLE_{langToken}_CONTEXT");
        }
        public string GetDisplayName()
        {
            return Language.GetString($"INTERACTABLE_{langToken}_NAME");
        }

        public Interactability GetInteractability([NotNull] Interactor activator)
        {
            var body = activator.GetComponent<CharacterBody>();

            if (body == owner)
            {
                return Interactability.Available;
            }

            return Interactability.Disabled;
        }

        public void OnInteractionBegin([NotNull] Interactor activator)
        {
            if (!NetworkServer.active)
            {
                MyLogger.LogWarning("[Server] function 'ExtradimensionalItems.Modules.Interactables.RespawnFlagInteractable::OnActivation(RoR2.Interactor)' called on client.");
                return;
            }

            var body = activator.GetComponent<CharacterBody>();

            MyLogger.LogMessage(string.Format("Player {0}({1}) used their {2}, spawning equipment and destroying interactable.", body.GetUserName(), body.name, $"INTERACTABLE_{langToken}"));
            PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(Content.Equipment.RespawnFlag.equipmentIndex);
            PickupDropletController.CreatePickupDroplet(pickupIndex, transform.position, Vector3.up * 5 + transform.forward * 3);

            Destroy(gameObject);
        }

        public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator)
        {
            return false;
        }

        public bool ShouldShowOnScanner()
        {
            return false;
        }

    }

}

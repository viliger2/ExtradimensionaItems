using BepInEx.Configuration;
using ExtradimensionalItems.Modules.Interactables;
using R2API;
using RoR2;
using ShrineOfRepair.Modules;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Equipment
{
    public class RespawnFlag : EquipmentBase<RespawnFlag>
    {
        public class RespawnFlagBehavior : CharacterBody.ItemBehavior
        {
            public GameObject flag;
        }

        public static ConfigEntry<bool> EnableFuelCellInteraction;

        public override string EquipmentName => "RespawnFlag";

        public override string EquipmentLangTokenName => "RESPAWN_FLAG";

        public override string BundleName => "respawnflag";

        public override GameObject EquipmentModel => AssetBundle.LoadAsset<GameObject>("RespawnFlagItem");

        public override Sprite EquipmentIcon => AssetBundle.LoadAsset<Sprite>("texRespawnFlagIcon");

        public override float Cooldown => 0.1f;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var ItemBodyModelPrefab = AssetBundle.LoadAsset<GameObject>("RespawnFlagItem_bones");

            //var slice = ItemBodyModelPrefab.transform.Find("Slice1");
            var clothBone = ItemBodyModelPrefab.transform.GetChild(0).GetChild(0).GetChild(0); // holy shit what the fuck am I doing

            var dynamicBone = clothBone.gameObject.AddComponent<DynamicBone>();

            dynamicBone.m_Root = clothBone;
            dynamicBone.m_Exclusions = new System.Collections.Generic.List<Transform>
            {
                clothBone
            };
            dynamicBone.m_UpdateMode = DynamicBone.UpdateMode.Normal;
            dynamicBone.m_FreezeAxis = DynamicBone.FreezeAxis.None;

            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();

            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00143F, 0.38005F, -0.01142F),
                    localAngles = new Vector3(355.4379F, 79.65914F, 349.5461F),
                    localScale = new Vector3(0.24767F, 0.24767F, 0.24767F)             }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.01185F, 0.28904F, -0.09079F),
                    localAngles = new Vector3(358.3515F, 86.88728F, 339.5181F),
                    localScale = new Vector3(0.18663F, 0.18663F, 0.18663F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(1.9871F, 2.50696F, 2.36408F),
                    localAngles = new Vector3(0.00013F, 49.68109F, 0.00004F),
                    localScale = new Vector3(1.20251F, 1.20251F, 1.20251F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadR",
                    localPos = new Vector3(-0.23701F, 0.27058F, -0.04455F),
                    localAngles = new Vector3(38.89613F, 179.1935F, 271.3203F),
                    localScale = new Vector3(0.24534F, 0.24534F, 0.24534F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.01606F, 0.25679F, -0.18706F),
                    localAngles = new Vector3(358.619F, 101.2193F, 7.99748F),
                    localScale = new Vector3(0.27445F, 0.27445F, 0.27445F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.05745F, -0.0375F, -0.11991F),
                    localAngles = new Vector3(328.8714F, 309.1855F, 114.4362F),
                    localScale = new Vector3(0.18412F, 0.18412F, 0.18412F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0.10425F, 1.75524F, 0F),
                    localAngles = new Vector3(-0.00001F, 53.90197F, -0.00001F),
                    localScale = new Vector3(0.714F, 0.714F, 0.714F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechUpperArmL",
                    localPos = new Vector3(0.01918F, -0.02052F, -0.0571F),
                    localAngles = new Vector3(6.65857F, 75.58593F, 207.9738F),
                    localScale = new Vector3(0.2174F, 0.2174F, 0.2174F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.05182F, 0.5866F, 1.52736F),
                    localAngles = new Vector3(27.80823F, 99.30228F, 110.4551F),
                    localScale = new Vector3(2.1302F, 2.1302F, 2.1302F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ClavicleL",
                    localPos = new Vector3(0.04779F, 0.15514F, -0.19104F),
                    localAngles = new Vector3(302.566F, 232.1999F, 106.8433F),
                    localScale = new Vector3(0.1575F, 0.1575F, 0.1575F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hat",
                    localPos = new Vector3(-0.0062F, 0.08837F, -0.06124F),
                    localAngles = new Vector3(7.65024F, 268.0452F, 23.97468F),
                    localScale = new Vector3(0.13693F, 0.13693F, 0.13693F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.62503F, -0.77494F, -0.06419F),
                    localAngles = new Vector3(20.29629F, 32.98375F, 148.0506F),
                    localScale = new Vector3(0.55407F, 0.55407F, 0.55407F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.01986F, 0.0315F, 0.03501F),
                    localAngles = new Vector3(14.7867F, 77.28765F, 0.6841F),
                    localScale = new Vector3(0.56718F, 0.56718F, 0.56718F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShoulderR",
                    localPos = new Vector3(0.12226F, 0.33691F, 0.13032F),
                    localAngles = new Vector3(290.5543F, 268.2901F, 324.7278F),
                    localScale = new Vector3(0.23348F, 0.23348F, 0.23348F)
                }
            });

            return rules;
        }

        private static GameObject flagInteractablePrefab;

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            LoadSoundBank();
            LoadInteractable();
            CreateConfig(config);
            CreateEquipment(ref Content.Equipment.RespawnFlag);
            Hooks();
        }

        protected override void LoadSoundBank()
        {
            base.LoadSoundBank();
            Utils.RegisterNetworkSound("EI_Checkpoint_Use");
        }

        private void LoadInteractable()
        {
            var flagInteractablePrefab2 = GetInteractable(AssetBundle.LoadAsset<GameObject>("RespawnFlagInteractable"), EquipmentLangTokenName);
            flagInteractablePrefab = PrefabAPI.InstantiateClone(flagInteractablePrefab2, "RespawnFlagInteractable"); // always use PrefabAPI, it will network it
        }

        private GameObject GetInteractable(GameObject interactableModel, string langToken)
        {
            interactableModel.AddComponent<NetworkIdentity>();

            var mesh = interactableModel.transform.Find("mdlRespawnFlagInteractable").gameObject;

            var interactableController = interactableModel.AddComponent<RespawnFlagInteractableManager>();
            interactableController.langToken = langToken;

            var modelLocator = interactableModel.AddComponent<ModelLocator>();
            modelLocator.modelTransform = mesh.transform;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.autoUpdateModelTransform = true;

            var entityLocator = mesh.AddComponent<EntityLocator>();
            entityLocator.entity = interactableModel;

            var highlightController = interactableModel.AddComponent<Highlight>();
            highlightController.targetRenderer = interactableModel.GetComponentsInChildren<SkinnedMeshRenderer>().Where(x => x.gameObject.name.Contains("mdlRespawnFlagInteractable")).First();
            highlightController.strength = 1;
            highlightController.highlightColor = Highlight.HighlightColor.interactive;

            return interactableModel;
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, EnableFuelCellInteraction.Value ? Language.GetString("EQUIPMENT_RESPAWN_FLAG_FUEL_CELL") : "");
        }

        protected override void Hooks()
        {
            base.Hooks();
            RoR2.CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (EquipmentCatalog.GetEquipmentDef(body.inventory.currentEquipmentIndex) == Content.Equipment.RespawnFlag)
            {
                body.AddItemBehavior<RespawnFlagBehavior>(1);
            }
            else if(body.TryGetComponent<RespawnFlagBehavior>(out var behavior))
            {
                // check for MUL-T, checking if the other slot is empty when
                // picking up equipment that is not RespawnFlag so we don't despawn
                // existing interactable when other slot is empty
                for (uint i = 0; i < body.inventory.GetEquipmentSlotCount(); i++)
                {
                    var equipmentState = body.inventory.GetEquipment(i);
                    if (equipmentState.equipmentIndex == EquipmentIndex.None)
                    {
                        return;
                    }
                }

                var result = DestroyExistingFlag(behavior, out Vector3 position);
                if (result)
                {
                    MyLogger.LogMessage(string.Format("Player {0}({1}) has existing {2} and picked up new equipment, destroying it and spawning equipment at its place.", body.GetUserName(), body.name, EquipmentLangTokenName));
                    PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(Content.Equipment.RespawnFlag.equipmentIndex);
                    PickupDropletController.CreatePickupDroplet(pickupIndex, position, Vector3.up * 5);
                    Object.Destroy(behavior);
                }
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!NetworkServer.active)
            {
                MyLogger.LogWarning("[Server] function Modules.Equipment.RespawnFlag::ActivateEquipment(RoR2.EquipmentSlot) called on client.");
                return false;
            }

            CharacterBody body = slot.characterBody;

            MyLogger.LogMessage(string.Format("Player {0}({1}) used equipment {2}.", body.GetUserName(), body.name, EquipmentLangTokenName));

            var behavior = body.AddItemBehavior<RespawnFlagBehavior>(1);

            var result = DestroyExistingFlag(behavior, out _);
            if (result)
            {
                MyLogger.LogMessage(string.Format("Player {0}({1}) has existing {2}, destroying it.", body.GetUserName(), body.name, EquipmentLangTokenName));
            }

            GameObject gameObject = Object.Instantiate(flagInteractablePrefab, body.transform.position, Quaternion.identity);
            RespawnFlagInteractableManager flagManager = gameObject.GetComponent<RespawnFlagInteractableManager>();
            flagManager.owner = body;

            NetworkServer.Spawn(gameObject);

            behavior.flag = gameObject;

            // thanks ThinkInvisible
            body.inventory.SetEquipment(new EquipmentState(EquipmentIndex.None, Run.FixedTimeStamp.now + Cooldown, 0), (uint)slot.characterBody.inventory.activeEquipmentSlot);

            return true;
        }

        private bool DestroyExistingFlag(RespawnFlagBehavior behavior, out Vector3 position)
        {
            position = new Vector3();

            if (behavior && behavior.flag)
            {
                position = behavior.flag.transform.position;
                Object.Destroy(behavior.flag);
                return true;
            }

            return false;
        }

        protected override void CreateConfig(ConfigFile config)
        {
            EnableFuelCellInteraction = config.Bind("Equipment: " + EquipmentName, "Enable Fuel Cell Interaction", true, "Enables Fuel Cell interaction, using Fuel Cells to revive instead of destroying checkpoint.");
            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(EnableFuelCellInteraction);
            }
        }
    }

}

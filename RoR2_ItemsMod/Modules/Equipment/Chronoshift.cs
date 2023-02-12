using BepInEx.Configuration;
using HG;
using KinematicCharacterController;
using R2API;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Audio;
using ShrineOfRepair.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Equipment
{
    public class Chronoshift : EquipmentBase<Chronoshift>
    {
        // TODO: maybe rewrite this entire shitshow to use EntityStateMachine and NetworkStateMachine
        private class ChronoshiftBehavior : CharacterBody.ItemBehavior
        {
            public enum ChronoshiftState
            {
                Saving,
                Moving,
                Restoring
            }

            private class CharacterState
            {
                public class Cooldowns
                {
                    public int stock;
                    public float rechargeStopwatch;
                }

                public Vector3 position;
                public uint money;
                public float health;
                public float shield;
                public float barrier;
                public bool outOfDanger;
                public List<CharacterBody.TimedBuff> buffs = new List<CharacterBody.TimedBuff>();
                public Cooldowns[] skillCooldowns;
                //public Inventory inventory = new Inventory();
                public int[] itemStacks = ItemCatalog.RequestItemStackArray();
                public List<ItemIndex> itemAcquisitionOrder = new List<ItemIndex>();
                public bool opalState;
                public float timer;
            }

            private List<CharacterState> states = new List<CharacterState>();

            private float stopwatch;
            private float timer = Frequency.Value;
            private float teleportTimer = 0.01f;
            private int currentState = -1;
            public ChronoshiftState currentEquipmentState;

            private int listCapacity = (int)(RewindTime.Value / Frequency.Value);

            private float speed = 0f;

            private TrailRenderer trailRenderer;

            private KinematicCharacterMotor characterMotor;
            private NetworkInstanceId netId;

            public void Awake()
            {
                currentEquipmentState = ChronoshiftState.Saving;
                this.enabled = false;
            }

            public void OnEnable()
            {
                if (body)
                {
                    trailRenderer = body.gameObject.AddComponent<TrailRenderer>();
                    trailRenderer.minVertexDistance = 2f;
                    trailRenderer.time = RewindTime.Value;
                    trailRenderer.endColor = new Color(1, 1, 1, 0);
                    trailRenderer.enabled = body.equipmentSlot.stock > 0;

                    characterMotor = body.GetComponent<KinematicCharacterMotor>();
                    netId = body.GetComponent<NetworkIdentity>().netId;
                }
            }

            public void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;

                if (body)
                {
                    if (currentEquipmentState == ChronoshiftState.Saving && stopwatch > timer && body.equipmentSlot.stock > 0)
                    {
                        if (!trailRenderer.enabled)
                        {
                            trailRenderer.enabled = true;
                        }
                        stopwatch -= timer;

                        CharacterState state = SnapshotCurrentCharacterState();
                        if (state != null)
                        {
                            states.Insert(0, state);
                            while (states.Count > listCapacity)
                            {
                                states.Remove(states.Last());
                            }
                        }
                    }
                    else if (currentEquipmentState == ChronoshiftState.Moving && body.hasAuthority)
                    {
                        CharacterState state = states[currentState];

                        Vector3 position = characterMotor.Rigidbody.position;
                        Vector3 target = state.position;
                        if (speed == 0f) speed = Vector3.Distance(position, target) / teleportTimer;
                        characterMotor.SetPosition(Vector3.MoveTowards(position, target, speed * Time.fixedDeltaTime));
                        if (Vector3.Distance(characterMotor.Rigidbody.position, target) < 0.001f)
                        {
                            currentState++;
                            if (currentState >= states.Count)
                            {
                                currentEquipmentState = ChronoshiftState.Restoring;
                                trailRenderer.Clear();
                                trailRenderer.enabled = false;
                                if (NetworkServer.active)
                                {
                                    MyLogger.LogMessage(string.Format("Player {0}({1}) finished moving back in time, restoring state.", body.GetUserName(), body.name));
                                    RestoreSkills();
                                    //RestoreHealth();
                                    RestoreState();
                                }
                                else
                                {
                                    MyLogger.LogMessage(string.Format("Player {0}({1}) finished moving back in time, sending message to server to restore state.", body.GetUserName(), body.name));
                                    RestoreSkills();
                                    //RestoreHealth();
                                    new ChronoshiftRestoreStateOnServer(netId).Send(R2API.Networking.NetworkDestination.Server);
                                    ClearStatesAndStartSaving();
                                }
                                return;
                            }
                            speed = Vector3.Distance(characterMotor.Rigidbody.position, states[currentState].position) / teleportTimer;
                        }
                    }
                }
            }

            public void OnDestroy()
            {
                ClearStatesAndStartSaving();
                UnityEngine.Object.Destroy(trailRenderer);
            }

            private CharacterState SnapshotCurrentCharacterState()
            {
                if (body)
                {
                    CharacterState state = new CharacterState();

                    state.position = body.gameObject.transform.position;
                    state.money = body.master.money;
                    state.health = body.healthComponent.health;
                    state.barrier = body.healthComponent.barrier;
                    state.shield = body.healthComponent.shield;

                    state.outOfDanger = body.outOfDanger;

                    state.skillCooldowns = new CharacterState.Cooldowns[body.skillLocator.allSkills.Length];

                    // skills
                    foreach (GenericSkill skill in body.skillLocator.allSkills)
                    {
                        var cooldown = new CharacterState.Cooldowns
                        {
                            stock = skill.stock,
                            rechargeStopwatch = skill.rechargeStopwatch
                        };

                        state.skillCooldowns[Array.IndexOf(body.skillLocator.allSkills, skill)] = cooldown;
                    };

                    // inventory
                    ArrayUtils.CloneTo(body.inventory.itemStacks, ref state.itemStacks);
                    Util.CopyList(body.inventory.itemAcquisitionOrder, state.itemAcquisitionOrder);

                    // buffs
                    state.buffs.Clear();
                    for (BuffIndex buffIndex = 0; buffIndex < (BuffIndex)BuffCatalog.buffCount; buffIndex++)
                    {
                        if (buffIndex == DLC1Content.Buffs.VoidSurvivorCorruptMode.buffIndex) continue;
                        var count = body.GetBuffCount(buffIndex);
                        if (buffIndex > 0)
                        {
                            var timedBuffs = body.timedBuffs.FindAll(x => x.buffIndex == buffIndex);
                            if (timedBuffs.Count() > 0)
                            {
                                foreach (var buff in timedBuffs)
                                {
                                    state.buffs.Add(new CharacterBody.TimedBuff { timer = buff.timer, buffIndex = buff.buffIndex });
                                }
                            }
                            else
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    state.buffs.Add(new CharacterBody.TimedBuff { timer = -1, buffIndex = buffIndex });
                                }
                            }
                        }
                    }

                    // setting opal state so it wouldn't break
                    if (body.TryGetComponent<OutOfCombatArmorBehavior>(out var opalComponent))
                    {
                        state.opalState = opalComponent.providingBuff;
                    }

                    state.timer = Run.instance.time;
                    return state;
                }
                else
                {
                    return null;
                }
            }

            private CharacterState GetRewindState()
            {
                return states.Count > 0 ? states.Last() : null;
            }

            private void RestoreSkills()
            {
                CharacterState state = GetRewindState();

                // skills
                foreach (GenericSkill skill in body.skillLocator.allSkills)
                {
                    int index = Array.IndexOf(body.skillLocator.allSkills, skill);
                    // additional check for when current cooldown is zero and
                    // restore cooldown is zero so we won't get free skill charge
                    if (skill.rechargeStopwatch != 0 || state.skillCooldowns[index].rechargeStopwatch != 0 || skill.stock != state.skillCooldowns[index].stock)
                    {
                        skill.rechargeStopwatch = skill.finalRechargeInterval - state.skillCooldowns[index].rechargeStopwatch;
                        skill.stock = state.skillCooldowns[index].stock;

                        // checking if we have max stocks and recharge is done
                        // if so, setting recharge timer as 0 so we won't get a free skill charge
                        if (skill.rechargeStopwatch == skill.finalRechargeInterval && skill.stock == skill.maxStock)
                        {
                            skill.rechargeStopwatch = 0;
                        }
                    }
                }
            }

            private void RestoreHealth()
            {
                CharacterState state = GetRewindState();

                body.healthComponent.health = state.health;
                body.healthComponent.barrier = state.barrier;
                body.healthComponent.shield = state.shield;
            }

            public void RestoreState()
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogWarning("ChronoshiftBehavior.RestoreState is called on client.");
                }

                CharacterState state = GetRewindState();

                body.master.money = state.money;

                //if (body.healthComponent.health != state.health)
                //{
                //    if (body.healthComponent.health > state.health)
                //    {
                //        body.healthComponent.TakeDamage(new DamageInfo
                //        {
                //            attacker = null,
                //            crit = false,
                //            position = body.transform.position,
                //            damageColorIndex = DamageColorIndex.Default,
                //            damageType = DamageType.BypassArmor,
                //            damage = body.healthComponent.health - state.health
                //        });
                //    }
                //    else
                //    {
                //        body.healthComponent.Heal(state.health - body.healthComponent.health, default);
                //    }
                //}
                //body.healthComponent.RechargeShield(body.healthComponent.shield - state.shield);
                //body.healthComponent.AddBarrier(body.healthComponent.barrier - state.barrier);

                //body.healthComponent.Networkhealth = state.health;
                if (Mathf.Abs(body.healthComponent.barrier - state.barrier) > 1f)
                {
                    body.healthComponent.Networkbarrier = state.barrier;
                }
                if(Mathf.Abs(body.healthComponent.shield - state.shield) > 1f)
                {
                    body.healthComponent.Networkshield = state.shield;
                }
                if(Mathf.Abs(body.healthComponent.health - state.health) > 1f)
                {
                    body.healthComponent.Networkhealth = state.health;
                }

                // items
                Inventory inv = new Inventory();
                ArrayUtils.CloneTo(state.itemStacks, ref inv.itemStacks);
                Util.CopyList(state.itemAcquisitionOrder, inv.itemAcquisitionOrder);

                body.inventory.CopyItemsFrom(inv);

                // buffs
                Util.CleanseBody(body, true, true, true, true, false, false);

                //additional loop to clear all permament buffs
                for (BuffIndex buffIndex = 0; buffIndex < (BuffIndex)BuffCatalog.buffCount; buffIndex++)
                {
                    if (buffIndex == DLC1Content.Buffs.VoidSurvivorCorruptMode.buffIndex) continue; // excluding Void Fiend since otherwise he completely breaks
                    if (body.HasBuff(buffIndex))
                    {
                        body.RemoveBuff(buffIndex);
                    }
                }

                foreach (CharacterBody.TimedBuff buff in state.buffs)
                {
                    if (buff.timer == -1) body.AddBuff(buff.buffIndex); else body.AddTimedBuff(buff.buffIndex, buff.timer);
                }

                // it doesn't fix opal but at least it stops it from breaking
                if (body.TryGetComponent<OutOfCombatArmorBehavior>(out var opalComponent))
                {
                    opalComponent.providingBuff = state.opalState;
                };

                // states, resetting them
                // while we can try and get them from EntityStateMachine
                // the reality is that it is not worth it and it might break more stuff than it fixes
                EntityStateMachine.FindByCustomName(body.gameObject, "Body")?.SetNextStateToMain();
                EntityStateMachine.FindByCustomName(body.gameObject, "Weapon")?.SetNextStateToMain();

                EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_Chronoshift_End", body.gameObject);

                MyLogger.LogMessage(string.Format("Player {0}({1}) finished restoring state, starting proccess of saving states.", body.GetUserName(), body.name));

                ClearStatesAndStartSaving();
            }

            public void ClearStatesAndStartSaving()
            {
                currentEquipmentState = ChronoshiftState.Saving;
                currentState = -1;
                speed = 0f;
                states.Clear();
            }

            public void StartMoving()
            {
                currentEquipmentState = ChronoshiftState.Moving;
                currentState = 0;
            }

            public void SetTrailRendererMaterial(Material material)
            {
                if (trailRenderer)
                {
                    trailRenderer.material = material;
                }
            }
        }

        public class ChronoshiftStartMovingOnClient : INetMessage
        {
            private NetworkInstanceId netId;

            public ChronoshiftStartMovingOnClient() { }
            public ChronoshiftStartMovingOnClient(NetworkInstanceId netId)
            {
                this.netId = netId;
            }

            public void Deserialize(NetworkReader reader)
            {
                netId = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    MyLogger.LogMessage("Recieved ChronoshiftStartMovingOnClient message on server, doing nothing...");
                    return;
                }

                GameObject gameObject = Util.FindNetworkObject(netId);
                if (gameObject)
                {
                    if (gameObject.GetComponent<CharacterBody>().hasAuthority)
                    {
                        if (gameObject.TryGetComponent(out ChronoshiftBehavior component))
                        {
                            component.StartMoving();
                        }
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(netId);
            }
        }

        public class ChronoshiftRestoreStateOnServer : INetMessage
        {
            private NetworkInstanceId netId;

            public ChronoshiftRestoreStateOnServer() { }
            public ChronoshiftRestoreStateOnServer(NetworkInstanceId netid)
            {
                netId = netid;
            }

            public void Deserialize(NetworkReader reader)
            {
                netId = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogMessage("Recieved ChronoshiftRestoreStateOnServer message on client, doing nothing...");
                    return;
                }

                GameObject gameObject = Util.FindNetworkObject(netId);
                if (gameObject)
                {
                    if (gameObject.TryGetComponent(out ChronoshiftBehavior component))
                    {
                        component.RestoreState();
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(netId);
            }
        }

        public static ConfigEntry<float> RewindTime;
        public static ConfigEntry<float> Frequency;
        public static ConfigEntry<float> CooldownConfig;

        public override string EquipmentName => "Chronoshift";

        public override string EquipmentLangTokenName => "CHRONOSHIFT";

        public override GameObject EquipmentModel => AssetBundle.LoadAsset<GameObject>("Chronoshift");

        public override Sprite EquipmentIcon => AssetBundle.LoadAsset<Sprite>("texChronoshiftIcon");

        public override string BundleName => "chronoshift";

        public override float Cooldown => CooldownConfig.Value;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var ItemBodyModelPrefab = AssetBundle.LoadAsset<GameObject>("Chronoshift");
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();

            // to fix item fade enable "Dither" on hopoo shader in Unity
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.01868F, 0.15171F, -0.10387F),
                    localAngles = new Vector3(45.49194F, 183.5922F, 345.4405F),
                    localScale = new Vector3(0.51963F, 0.51963F, 0.51963F)               }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.01835F, 0.08114F, 0.00864F),
                    localAngles = new Vector3(31.32549F, 197.5746F, 36.63119F),
                    localScale = new Vector3(0.48011F, 0.48011F, 0.48011F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.16778F, 0.77172F, -1.07452F),
                    localAngles = new Vector3(33.70054F, 189.291F, 358.7404F),
                    localScale = new Vector3(3.03086F, 3.03086F, 3.03086F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00006F, 0.14264F, -0.20751F),
                    localAngles = new Vector3(31.79489F, 185.1696F, 355.3399F),
                    localScale = new Vector3(0.48752F, 0.48752F, 0.48752F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.01074F, 0.0313F, -0.2331F),
                    localAngles = new Vector3(23.68356F, 193.0841F, 358.9043F),
                    localScale = new Vector3(0.29518F, 0.29518F, 0.29518F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.01603F, 0.15346F, -0.14982F),
                    localAngles = new Vector3(311.9305F, 180.8366F, 179.298F),
                    localScale = new Vector3(0.53588F, 0.53588F, 0.53588F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PlatformBase",
                    localPos = new Vector3(-0.00033F, 0.04071F, -0.25942F),
                    localAngles = new Vector3(31.31434F, 190.4047F, 0.61905F),
                    localScale = new Vector3(1.48928F, 1.48928F, 1.48928F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.0082F, -0.03117F, -0.26995F),
                    localAngles = new Vector3(28.29659F, 186.0077F, 352.9836F),
                    localScale = new Vector3(0.50544F, 0.50544F, 0.50544F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.24748F, 0.28157F, 2.99287F),
                    localAngles = new Vector3(45.40444F, 27.58954F, 32.31604F),
                    localScale = new Vector3(5.1585F, 5.1585F, 5.1585F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00684F, 0.16017F, -0.07591F),
                    localAngles = new Vector3(332.7996F, 187.619F, 166.1224F),
                    localScale = new Vector3(0.67517F, 0.67517F, 0.67517F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.0091F, 0.18334F, -0.06412F),
                    localAngles = new Vector3(8.04993F, 202.6387F, 37.24955F),
                    localScale = new Vector3(0.4842F, 0.4842F, 0.4842F)
                }
            });
            //rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Chest",
            //        localPos = new Vector3(0F, 0.00339F, -0.00108F),
            //        localAngles = new Vector3(317.3549F, 221.5528F, 123.0072F),
            //        localScale = new Vector3(0.00557F, 0.00557F, 0.00557F)
            //    }
            //});
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.06566F, -0.22289F, -0.10183F),
                    localAngles = new Vector3(42.46209F, 59.69307F, 213.2425F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Backpack",
                    localPos = new Vector3(-0.02928F, -0.24107F, 0.00183F),
                    localAngles = new Vector3(350.0237F, 150.1604F, 225.3768F),
                    localScale = new Vector3(0.56718F, 0.56718F, 0.56718F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.02187F, 0.06523F, -0.0598F),
                    localAngles = new Vector3(350.3206F, 192.9503F, 149.9679F),
                    localScale = new Vector3(0.60224F, 0.60224F, 0.60224F)
                }
            });
            // EXAMPLE
            //rules.Add("body", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Chest",
            //        localPos = new Vector3(0, 0, 0),
            //        localAngles = new Vector3(0, 0, 0),
            //        localScale = new Vector3(1, 1, 1)
            //    }
            //});
            // END EXAMPLE
            return rules;
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, RewindTime.Value, Frequency.Value);
        }

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            SetLogbookCameraPosition();
            LoadSoundBank();
            CreateConfig(config);
            CreateEquipment(ref Content.Equipment.Chronoshift);
            Hooks();
        }

        protected override void LoadSoundBank()
        {
            base.LoadSoundBank();
            Utils.RegisterNetworkSound("EI_Chronoshift_Start");
            Utils.RegisterNetworkSound("EI_Chronoshift_End");
        }

        private void SetLogbookCameraPosition()
        {
            var modelParameters = EquipmentModel.AddComponent<ModelPanelParameters>();

            modelParameters.focusPointTransform = EquipmentModel.transform.Find("FocusPoint");
            modelParameters.cameraPositionTransform = EquipmentModel.transform.Find("CameraPosition");
            modelParameters.modelRotation = new Quaternion(21.43f, -10.32f, 0f, 1f);

            modelParameters.minDistance = 1;
            modelParameters.maxDistance = 3;
        }

        protected override void Hooks()
        {
            base.Hooks();
            On.RoR2.GenericPickupController.BodyHasPickupPermission += GenericPickupController_BodyHasPickupPermission;
            RoR2.CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (body)
            {
                var behavior = body.AddItemBehavior<ChronoshiftBehavior>(EquipmentCatalog.GetEquipmentDef(body.inventory.currentEquipmentIndex) == Content.Equipment.Chronoshift ? 1 : 0);
                if (behavior)
                {
                    behavior.SetTrailRendererMaterial(AssetBundle.LoadAsset<Material>("matChronoshiftTrail"));
                }
            }
        }

        private bool GenericPickupController_BodyHasPickupPermission(On.RoR2.GenericPickupController.orig_BodyHasPickupPermission orig, CharacterBody body)
        {
            if (body.TryGetComponent<ChronoshiftBehavior>(out var chronoshiftBehavior))
            {
                if(chronoshiftBehavior.currentEquipmentState == ChronoshiftBehavior.ChronoshiftState.Moving)
                {
                    return false;
                }
            }
            return orig(body);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!NetworkServer.active)
            {
                MyLogger.LogWarning("[Server] function Modules.Equipment.Chronoshift::ActivateEquipment(RoR2.EquipmentSlot) called on client.");
                return false;
            }

            var body = slot.characterBody;

            if (body.gameObject.TryGetComponent<ChronoshiftBehavior>(out var component))
            {
                MyLogger.LogMessage(string.Format("Player {0}({1}) used equipment {2}, moving back in time...", body.GetUserName(), body.name, EquipmentLangTokenName));

                if (!body.TryGetComponent<NetworkIdentity>(out var identity))
                {
                    MyLogger.LogWarning(string.Format("Body {0} did not have NetworkIdentity.", body));
                    return false;
                }

                component.StartMoving();

                EntitySoundManager.EmitSoundServer((AkEventIdArg)"EI_Chronoshift_Start", body.gameObject);

                if (body != PlayerCharacterMasterController.instances[0].master.GetBody())
                {
                    new ChronoshiftStartMovingOnClient(identity.netId).Send(R2API.Networking.NetworkDestination.Clients);
                }
            }

            return true;
        }

        protected override void CreateConfig(ConfigFile config)
        {
            RewindTime = config.Bind("Equipment: " + EquipmentName, "Rewind time", 10f, "How much, in seconds, back in time equipment takes you.");
            Frequency = config.Bind("Equipment: " + EquipmentName, "Frequency", 0.25f, "How frequently, in seconds, your state in snapshotted. Smaller values will result in higher memory consumption.");
            CooldownConfig = config.Bind("Equipment: " + EquipmentName, "Cooldown", 120f, "What is the cooldown of equipment.");

            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(RewindTime, 1f, 20f);
                RiskOfOptionsCompat.CreateNewOption(Frequency, 0.01f, 1f, 0.01f);
                RiskOfOptionsCompat.CreateNewOption(CooldownConfig, 1f, 200f, 1f);
            }
        }
    }
}

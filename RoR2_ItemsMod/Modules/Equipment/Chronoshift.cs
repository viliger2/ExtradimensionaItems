using BepInEx.Configuration;
using HG;
using KinematicCharacterController;
using R2API;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Equipment
{
    public class Chronoshift : EquipmentBase<Chronoshift>
    {
        // TODO: maybe rewrite this entire shitshow to use EntityStateMachine and NetworkStateMachine
        private class ChronoshiftBehavior : CharacterBody.ItemBehavior
        {
            private enum ChronoshiftState
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
            private ChronoshiftState currentEquipmentState;

            private int listCapacity = (int)(RewindTime.Value / Frequency.Value);

            private float speed = 0f;

            private TrailRenderer trailRenderer;

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

                        var motor = body.GetComponent<KinematicCharacterMotor>();
                        Vector3 position = motor.Rigidbody.position;
                        Vector3 target = state.position;
                        if (speed == 0f) speed = Vector3.Distance(position, target) / teleportTimer;
                        motor.SetPosition(Vector3.MoveTowards(position, target, speed * Time.fixedDeltaTime));
                        if (Vector3.Distance(motor.Rigidbody.position, target) < 0.001f)
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
                                    RestoreState();
                                }
                                else
                                {
                                    MyLogger.LogMessage(string.Format("Player {0}({1}) finished moving back in time, sending message to server to restore state.", body.GetUserName(), body.name));
                                    RestoreSkills();
                                    new ChronoshiftRestoreStateOnServer(body.GetComponent<NetworkIdentity>().netId).Send(R2API.Networking.NetworkDestination.Server);
                                    ClearStatesAndStartSaving();
                                }
                                return;
                            }
                            speed = Vector3.Distance(motor.Rigidbody.position, states[currentState].position) / teleportTimer;
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
                } else {
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

            public void RestoreState()
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogWarning("ChronoshiftBehavior.RestoreState is called on client.");
                }

                CharacterState state = GetRewindState();

                body.master.money = state.money;
                body.healthComponent.health = state.health;
                body.healthComponent.barrier = state.barrier;
                body.healthComponent.shield = state.shield;
                body.outOfDanger = state.outOfDanger;

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
                if(gameObject)
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
            // TODO: unfuck item fade, it doesn't work now and I have no idea why
            // probably has something to do with shaders
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00088F, 0.10924F, -0.06687F),
                    localAngles = new Vector3(59.57273F, 151.3646F, 327.5803F),
                    localScale = new Vector3(0.67373F, 0.67373F, 0.67373F)                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.0288F, 0.15945F, -0.01174F),
                    localAngles = new Vector3(60.44611F, 149.7126F, 14.5503F),
                    localScale = new Vector3(0.53117F, 0.53117F, 0.53117F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.05995F, 0.80019F, -1.16F),
                    localAngles = new Vector3(52.71721F, 186.379F, 9.95464F),
                    localScale = new Vector3(3.51943F, 3.51943F, 3.51943F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00007F, 0.12585F, -0.18822F),
                    localAngles = new Vector3(49.57471F, 172.3253F, 1.44653F),
                    localScale = new Vector3(0.61499F, 0.61499F, 0.61499F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00138F, 0.03181F, -0.23878F),
                    localAngles = new Vector3(34.9938F, 172.7735F, 1.38432F),
                    localScale = new Vector3(0.33224F, 0.33224F, 0.33224F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00466F, 0.15284F, -0.16268F),
                    localAngles = new Vector3(298.223F, 185.8875F, 178.754F),
                    localScale = new Vector3(0.47598F, 0.47598F, 0.47598F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PlatformBase",
                    localPos = new Vector3(-0.00033F, 0.04076F, -0.23915F),
                    localAngles = new Vector3(43.94766F, 181.4132F, 0.98089F),
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
                    localPos = new Vector3(0.017F, -0.05289F, -0.2494F),
                    localAngles = new Vector3(40.70776F, 178.8556F, 353.7026F),
                    localScale = new Vector3(0.61055F, 0.61055F, 0.61055F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.25128F, 0.5368F, 2.74673F),
                    localAngles = new Vector3(59.82613F, 68.69579F, 76.82799F),
                    localScale = new Vector3(6.09834F, 6.09834F, 6.09834F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.01424F, 0.16052F, -0.08735F),
                    localAngles = new Vector3(305.6797F, 174.4509F, 187.9807F),
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
                    localPos = new Vector3(0.01333F, 0.17094F, -0.07179F),
                    localAngles = new Vector3(35.16169F, 213.0192F, 44.44061F),
                    localScale = new Vector3(0.4842F, 0.4842F, 0.4842F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.00339F, -0.00108F),
                    localAngles = new Vector3(317.3549F, 221.5528F, 123.0072F),
                    localScale = new Vector3(0.00557F, 0.00557F, 0.00557F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.06566F, -0.22289F, -0.10183F),
                    localAngles = new Vector3(23.48283F, 50.56009F, 233.6516F),
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
                    localAngles = new Vector3(345.5057F, 132.0578F, 251.016F),
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
                    localPos = new Vector3(-0.00399F, 0.05458F, -0.06697F),
                    localAngles = new Vector3(350.4411F, 203.1579F, 158.2204F),
                    localScale = new Vector3(0.60104F, 0.60104F, 0.60104F)
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

        protected override void Hooks()
        {
            base.Hooks();
            // we cannot use CharacterBody.onBodyInventoryChangedGlobal because by the time we get to our method
            // previous equipment is already overwritten by new equipment so we can't detect changes
            On.RoR2.CharacterBody.OnEquipmentGained += CharacterBody_OnEquipmentGained;
            On.RoR2.CharacterBody.OnEquipmentLost += CharacterBody_OnEquipmentLost;
        }

        private void CharacterBody_OnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == Content.Equipment.Chronoshift)
            {
                self.AddItemBehavior<ChronoshiftBehavior>(0);
            }
            orig(self, equipmentDef);
        }

        private void CharacterBody_OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == Content.Equipment.Chronoshift)
            {
                var behaviour = self.AddItemBehavior<ChronoshiftBehavior>(1);
                behaviour.SetTrailRendererMaterial(AssetBundle.LoadAsset<Material>("matChronoshiftTrail"));
            }
            orig(self, equipmentDef);
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

                var identity = body.GetComponent<NetworkIdentity>();
                if (!identity)
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
        }
    }
}

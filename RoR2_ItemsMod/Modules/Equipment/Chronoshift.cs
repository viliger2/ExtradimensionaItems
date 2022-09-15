using BepInEx.Configuration;
using HG;
using KinematicCharacterController;
using R2API;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.CharacterBody;

namespace ExtradimensionalItems.Modules.Equipment
{
    public class Chronoshift : EquipmentBase<Chronoshift>
    {
        // TODO: maybe rewrite this entire shitshow to use EntityStateMachine and NetworkStateMachine
        private class ChronoshiftBehavior : ItemBehavior
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

            public void Awake()
            {
                currentEquipmentState = ChronoshiftState.Saving;
            }

            public void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;

                if (body)
                {
                    if(currentEquipmentState == ChronoshiftState.Saving && stopwatch > timer)
                    {
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
                            if(currentState >= states.Count)
                            {
                                currentEquipmentState = ChronoshiftState.Restoring;
                                if (NetworkServer.active)
                                {
                                    MyLogger.LogMessage(string.Format("Player {0}({1}) finished moving back in time, restoring state.", body.GetUserName(), body.name));
                                    RestoreSkills();
                                    RestoreState();
                                } else
                                {
                                    MyLogger.LogMessage(string.Format("Player {0}({1}) finished moving back in time, sending message to server to restore state.", body.GetUserName(), body.name));
                                    RestoreSkills();
                                    new ChronoshiftRestoreStateOnServer(body.netId).Send(R2API.Networking.NetworkDestination.Server);
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
                return states.Last();
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
                    if (buffIndex == DLC1Content.Buffs.VoidSurvivorCorruptMode.buffIndex) continue;
                    if (body.HasBuff(buffIndex))
                    {
                        body.RemoveBuff(buffIndex);
                    }
                }

                foreach (TimedBuff buff in state.buffs)
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

                GameObject gameObject = Utils.FindNetworkPlayer(netId);
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

                GameObject gameObject = Utils.FindNetworkPlayer(netId);
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

        public override string EquipmentName => "Chronoshift";

        public override string EquipmentLangTokenName => "CHRONOSHIFT";

        public override GameObject EquipmentModel => null;

        public override Sprite EquipmentIcon => null;

        public override string BundleName => "chronoshift";

        public override float Cooldown => 120f;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return string.Format(pickupString, RewindTime.Value, Frequency.Value);
        }

        public override void Init(ConfigFile config)
        {
            //LoadAssetBundle();
            CreateConfig(config);
            CreateEquipment(ref Content.Equipment.Chronoshift);
            Hooks();
        }

        protected override void Hooks()
        {
            base.Hooks();
            On.RoR2.CharacterBody.OnEquipmentGained += CharacterBody_OnEquipmentGained;
            On.RoR2.CharacterBody.OnEquipmentLost += CharacterBody_OnEquipmentLost;
        }

        private void CharacterBody_OnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == Content.Equipment.Chronoshift)
            {
                self.AddItemBehavior<ChronoshiftBehavior>(1);
            }
            orig(self, equipmentDef);
        }

        private void CharacterBody_OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == Content.Equipment.Chronoshift)
            {
                self.AddItemBehavior<ChronoshiftBehavior>(1);
            }
            orig(self, equipmentDef);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
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
        }
    }
}

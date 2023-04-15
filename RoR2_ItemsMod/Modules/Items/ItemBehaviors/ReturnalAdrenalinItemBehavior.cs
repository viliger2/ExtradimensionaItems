using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Items.ItemBehaviors
{
    // attach it to master for it to last through stages
    public class ReturnalAdrenalinItemBehavior : NetworkBehaviour
    {
        public CharacterMaster master;

        public int stack;

        //[SyncVar]
        //private int _adrenalineLevel = 0;

        [SyncVar]
        public int adrenalineLevel;
        //{
        //    set
        //    {
        //        _adrenalineLevel = value;
        //        //ReturnalAdrenalineUI.UpdateUI(value);
        //    }
        //    get
        //    {
        //        return _adrenalineLevel;
        //    }
        //}

        public static int adrenalinePerLevel = 10;

        public int normalKillReward = 1;
        public int eliteKillReward = 4;
        public int championKillReward = 5;

        private float previousHp;

        private float checkTimer = 0.1f;

        private float stopwatch;

        public void Awake()
        {
            enabled = false;
        }

        public void OnEnable()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            if (master && master.GetBody())
            {
                previousHp = master.GetBody().healthComponent.health;
            }
        }

        public void OnDisable()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            adrenalineLevel = 0;
        }

        public void OnDestroy()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (enabled && adrenalineLevel < adrenalinePerLevel * 5)
            {
                CharacterBody attackerBody = damageReport.attackerBody;
                if (attackerBody && attackerBody == master.GetBody())
                {
                    if (damageReport.victimIsElite)
                    {
                        adrenalineLevel += eliteKillReward;
                    }
                    else if (damageReport.victimIsChampion)
                    {
                        adrenalineLevel += championKillReward;
                    }
                    else
                    {
                        adrenalineLevel += normalKillReward;
                    }
                    if (adrenalineLevel > adrenalinePerLevel * 5)
                    {
                        adrenalineLevel = adrenalinePerLevel * 5;
                    }
                    MyLogger.LogMessage("new stack number {0}", adrenalineLevel.ToString());
                }
            }
        }

        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch < checkTimer)
            {
                return;
            }

            var body = master.GetBody();

            stopwatch -= checkTimer;

            if (body)
            {
                if ((previousHp - body.healthComponent.health) > body.healthComponent.fullHealth * 0.2)
                {
                    adrenalineLevel = 0;
                    MyLogger.LogMessage("lost all stacks");
                }

                previousHp = body.healthComponent.health;
            }
        }

    }

}

using BepInEx.Configuration;
using ExtradimensionalItems.Modules.UI;
using IL.RoR2.UI;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace ExtradimensionalItems.Modules.Items
{
    public class ReturnalAdrenalin : ItemBase<ReturnalAdrenalin>
    {
        // attach it to master for it to last through stages
        public class ReturnalAdrenalinItemBehavior : MonoBehaviour
        {
            public CharacterMaster master;

            public int stack;

            private int _adrenalineLevel = 0;

            public int adrenalineLevel
            {
                set
                {
                    _adrenalineLevel = value;
                    ReturnalAdrenalineUI.UpdateUI(value);
                }
                get
                {
                    return _adrenalineLevel;
                }
            }

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
                if (master.GetBody())
                {
                    previousHp = master.GetBody().healthComponent.health;
                    RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
                    ReturnalAdrenalineUI.UpdateUI(adrenalineLevel);
                }
            }

            public void OnDestroy()
            {
                if (master.GetBody())
                {
                    RoR2.GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
                }
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
                        if(adrenalineLevel > adrenalinePerLevel * 5)
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

        public override string ItemName => "ReturnalAdrenalin";

        public override string ItemLangTokenName => "RETURNAL_ADRENALIN";

        public override ItemTier Tier => ItemTier.Tier3;

        public override string BundleName => "returnaladrenalin";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override bool AIBlacklisted => true;

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override string GetFormatedDiscription(string pickupString)
        {
            return pickupString;
        }

        public override void Init(ConfigFile config)
        {
            Hooks();
            CreateItem(ref Content.Items.ReturnalAdrenalin);
        }

        protected override void Hooks()
        {
            base.Hooks();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            ReturnalAdrenalineUI.CreateUI(self);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (GetCount(body) > 0 && body.master.TryGetComponent(out ReturnalAdrenalinItemBehavior component))
            {
                args.attackSpeedMultAdd += 0.15f * 5 * ((component.adrenalineLevel >= (ReturnalAdrenalinItemBehavior.adrenalinePerLevel * 1)) ? 1 : 0);
                args.moveSpeedMultAdd += 0.14f * 5 * ((component.adrenalineLevel >= (ReturnalAdrenalinItemBehavior.adrenalinePerLevel * 2)) ? 1 : 0);
                args.baseHealthAdd += 25f * 5 * ((component.adrenalineLevel >= (ReturnalAdrenalinItemBehavior.adrenalinePerLevel * 3)) ? 1 : 0);
                args.baseShieldAdd += body.maxHealth * 0.25f * ((component.adrenalineLevel >= (ReturnalAdrenalinItemBehavior.adrenalinePerLevel * 4)) ? 1 : 0);
                args.critAdd += 25f * ((component.adrenalineLevel >= (ReturnalAdrenalinItemBehavior.adrenalinePerLevel * 5)) ? 1 : 0);
                //args.baseDamageAdd += sender.maxHealth * (PercentBonusDamage.Value / 100) + sender.maxHealth * (PercentBonusDamagePerStack.Value / 100 * (GetCount(sender) - 1));
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (body)
            {
                if (GetCount(body) > 0)
                {
                    ReturnalAdrenalineUI.Enable();

                    var component = body.master.gameObject.GetComponent<ReturnalAdrenalinItemBehavior>();
                    if (!component)
                    {
                        component = body.master.gameObject.AddComponent<ReturnalAdrenalinItemBehavior>();
                        component.master = body.master;
                        component.enabled = true;
                    }
                    component.stack = GetCount(body);
                }
                else if (body.master.gameObject.TryGetComponent<ReturnalAdrenalinItemBehavior>(out var component))
                {
                    UnityEngine.Object.Destroy(component);

                    ReturnalAdrenalineUI.Disable();
                }
            }
        }
    }
}

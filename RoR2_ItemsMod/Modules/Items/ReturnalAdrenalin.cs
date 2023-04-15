using BepInEx.Configuration;
using ExtradimensionalItems.Modules.Items.ItemBehaviors;
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
            On.RoR2.CharacterMaster.Awake += CharacterMaster_Awake;
        }

        private void CharacterMaster_Awake(On.RoR2.CharacterMaster.orig_Awake orig, CharacterMaster self)
        {
            if (self)
            {
                var component = self.gameObject.AddComponent<ReturnalAdrenalinItemBehavior>();
                component.master = self;
            }
            orig(self);
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
                    //ReturnalAdrenalineUI.Enable();

                    var component = body.master.gameObject.GetComponent<ReturnalAdrenalinItemBehavior>();
                    if (component)
                    {
                        component.enabled = true;
                        component.stack = GetCount(body);
                        if (ReturnalAdrenalineUI.instance)
                        {
                            ReturnalAdrenalineUI.instance.Enable();
                        }
                    }
                }
                else if (body.master.gameObject.TryGetComponent<ReturnalAdrenalinItemBehavior>(out var component))
                {
                    component.enabled = false;
                    //UnityEngine.Object.Destroy(component);

                    if (ReturnalAdrenalineUI.instance && ReturnalAdrenalineUI.instance.hud.targetMaster == body.master)
                    {
                        ReturnalAdrenalineUI.instance.Disable();
                    }
                }
            }
        }
    }
}

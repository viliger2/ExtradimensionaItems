using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.CharacterBody;

namespace ExtradimensionalItems.Modules.Equipment
{
    public class SkullOfDoom : EquipmentBase<SkullOfDoom>
    {
        public class SkullOfDoomBehavior : ItemBehavior
        {
            private float stopwatch;

            public float damageTimer = DamageFrequency.Value;

            public void FixedUpdate()
            {
                if (!NetworkServer.active)
                {
                    return;
                }

                stopwatch += Time.fixedDeltaTime;
                if(stopwatch > damageTimer && body.HasBuff(Content.Buffs.SkullOfDoom))
                {
                    stopwatch -= damageTimer;
                    DealDamage(body);
                }
            }
        }

        public static ConfigEntry<float> SpeedBuff;
        public static ConfigEntry<float> DamageOverTime;
        public static ConfigEntry<float> DamageFrequency;
        public static ConfigEntry<bool> EnableFuelCellInteraction;
        public static ConfigEntry<float> FuelCellSpeedBuff;
        public static ConfigEntry<float> FuelCellDamageOverTime;

        public override string EquipmentName => "SkullOfDoom";

        public override string EquipmentLangTokenName => "SKULL_OF_DOOM";

        public override GameObject EquipmentModel => AssetBundle.LoadAsset<GameObject>("SkullOfDoomItem");

        public override Sprite EquipmentIcon => AssetBundle.LoadAsset<Sprite>("texSkullOfDoomIcon");

        public override string BundleName => "skullofdoom";

        public override float Cooldown => 1f;

        public override bool IsLunar => true;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            CreateConfig(config);
            CreateBuffs(AssetBundle);
            CreateEquipment(ref Content.Equipment.SkullOfDoom);
            Hooks();
        }

        protected override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnEquipmentGained += CharacterBody_OnEquipmentGained; ;
        }

        private void CharacterBody_OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody body, EquipmentDef equipmentDef)
        {
            orig(body, equipmentDef);
            if(equipmentDef != Content.Equipment.SkullOfDoom)
            {
                if (body.HasBuff(Content.Buffs.SkullOfDoom))
                {
                    MyLogger.LogMessage(string.Format("Player {0}({1}) picked up another equipment while having {2} buff, removing it.", body.GetUserName(), body.name, Content.Buffs.SkullOfDoom.name));
                    body.RemoveBuff(Content.Buffs.SkullOfDoom);
                    body.AddItemBehavior<SkullOfDoomBehavior>(0);
                }
            }

        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body.inventory)
            {
                if(body.inventory.currentEquipmentIndex == Content.Equipment.SkullOfDoom.equipmentIndex)
                {
                    if (body.HasBuff(Content.Buffs.SkullOfDoom))
                    {
                        args.moveSpeedMultAdd += SpeedBuff.Value + (FuelCellSpeedBuff.Value * body.inventory.GetItemCount(RoR2Content.Items.EquipmentMagazine));
                    }
                }
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            var body = slot.characterBody;

            if (!body || !body.teamComponent) return false;

            if (body.HasBuff(Content.Buffs.SkullOfDoom)){
                MyLogger.LogMessage(string.Format("Player {0}({1}) used {2}, removing damage DoT and movement speed buff.", body.GetUserName(), body.name, EquipmentName));
                body.RemoveBuff(Content.Buffs.SkullOfDoom);
                body.AddItemBehavior<SkullOfDoomBehavior>(0);
            }
            else
            {
                MyLogger.LogMessage(string.Format("Player {0}({1}) used {2}, applying damage DoT and movement speed buff.", body.GetUserName(), body.name, EquipmentName));
                DealDamage(body);
                body.AddBuff(Content.Buffs.SkullOfDoom);
                body.AddItemBehavior<SkullOfDoomBehavior>(1);
            }
            return true;
        }

        private static void DealDamage(CharacterBody body)
        {
            DamageInfo damageInfo = new DamageInfo();
            damageInfo.damage = Math.Max(1f, (body.maxHealth * DamageOverTime.Value) * Mathf.Pow(1 - FuelCellDamageOverTime.Value, body.inventory.GetItemCount(RoR2Content.Items.EquipmentMagazine)));
            damageInfo.attacker = null; // if you put self as attacker then friendly fire damage reduction is applied
            damageInfo.crit = false;
            damageInfo.position = body.transform.position;
            damageInfo.damageColorIndex = DamageColorIndex.Item;
            damageInfo.damageType = DamageType.BypassArmor;

            body.healthComponent.TakeDamage(damageInfo);
        }

        public void CreateBuffs(AssetBundle assetBundle)
        {
            var SkullOfDoomBuff = ScriptableObject.CreateInstance<BuffDef>();
            SkullOfDoomBuff.name = "Skull of Impending Doom";
            SkullOfDoomBuff.buffColor = Color.yellow;
            SkullOfDoomBuff.canStack = false;
            SkullOfDoomBuff.isDebuff = false;
            SkullOfDoomBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png"); // TODO: replace

            ContentAddition.AddBuffDef(SkullOfDoomBuff);

            Content.Buffs.SkullOfDoom = SkullOfDoomBuff;
        }

        protected override void CreateConfig(ConfigFile config)
        {
            SpeedBuff                 = config.Bind("Equipment: " + EquipmentName, "Speed Buff",                           1f,    "How much speed buff provides.");
            DamageOverTime            = config.Bind("Equipment: " + EquipmentName, "Damage Over Time",                     0.1f,  "How much percentage damage from max health DoT deals.");
            DamageFrequency           = config.Bind("Equipment: " + EquipmentName, "Damage Over Time Frequency",           3f,    "How frequently, in seconds, damage is applied.");
            EnableFuelCellInteraction = config.Bind("Equipment: " + EquipmentName, "Fuel Cell Interaction",                true,  "Enables interaction with Fuel Cells.");
            FuelCellSpeedBuff         = config.Bind("Equipment: " + EquipmentName, "Fuel Cell Speed Buff",                 0.15f, "How much additional speed each Fuel Cell provides, linearly.");
            FuelCellDamageOverTime    = config.Bind("Equipment: " + EquipmentName, "Fuel Cell Damage Over Time Reduction", 0.15f, "By how much each Fuel Cell reduces DoT damage, exponentially.");
        }

    }
}

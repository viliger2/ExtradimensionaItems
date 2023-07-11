using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Items
{
    public class DamageOnPing : ItemBase<DamageOnPing>
    {
        public override string ItemName => "DamageOnPing";

        public override string ItemLangTokenName => "DAMAGE_ON_PING";

        public override ItemTier Tier => ItemTier.Tier1;

        public override string BundleName => "damageonping";

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("DamageOnPing"); // TODO

        public override Sprite ItemIcon => null; // TODO

        public override bool AIBlacklisted => true;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            LoadAssetBundle();
            LoadLanguageFile();
            CreateBuffs();
            CreateItem(ref Content.Items.DamageOnPing);
            Hooks();
        }

        private void CreateBuffs()
        {
            var DamageBuff = ScriptableObject.CreateInstance<BuffDef>();
            DamageBuff.name = "Ping Damage Bonus";
            DamageBuff.buffColor = Color.yellow;
            DamageBuff.canStack = true;
            DamageBuff.isDebuff = true;
            DamageBuff.isHidden = false; // TODO: replace with true when done
            DamageBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Achievements/texBandit2StackSuperBleedIcon.png").WaitForCompletion(); // TODO

            ContentAddition.AddBuffDef(DamageBuff);

            Content.Buffs.DamageOnPing = DamageBuff;

            //if (BetterUICompat.enabled)
            //{
            //    BetterUICompat.AddBuffInfo(DamageBuff, "BUFF_SHEEN_NAME", "BUFF_SHEEN_DESCRIPTION");
            //}
        }

        protected override void Hooks()
        {
            base.Hooks();
            //On.RoR2.UI.PingIndicator.RebuildPing += PingIndicator_RebuildPing;
            On.RoR2.PingerController.RebuildPing += PingerController_RebuildPing;
            On.RoR2.UI.PingIndicator.OnDisable += PingIndicator_OnDisable;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if(self.body && self.body.HasBuff(Content.Buffs.DamageOnPing))
            {
                damageInfo.damage += damageInfo.damage * (0.05f * self.body.GetBuffCount(Content.Buffs.DamageOnPing));
                //damageInfo.damageColorIndex = DamageColorIndex.DeathMark;
            }
            orig(self, damageInfo);
        }

        private void PingerController_RebuildPing(On.RoR2.PingerController.orig_RebuildPing orig, PingerController self, PingerController.PingInfo pingInfo)
        {
            MyLogger.LogMessage("rebuild ping");
            RemoveBuffsIfPresent(self.pingIndicator);
            orig(self, pingInfo);
            AdddBuffsToPingedTarget(self.pingIndicator);
        }


        private void PingIndicator_OnDisable(On.RoR2.UI.PingIndicator.orig_OnDisable orig, RoR2.UI.PingIndicator self)
        {
            MyLogger.LogMessage("ping disable");
            RemoveBuffsIfPresent(self);
            orig(self);
        }

        //private void PingIndicator_RebuildPing(On.RoR2.UI.PingIndicator.orig_RebuildPing orig, RoR2.UI.PingIndicator self)
        //{
        //    RemoveBuffsIfPresent(self);
        //    orig(self);
        //    MyLogger.LogMessage("pinging something");
        //    if(self.pingType == RoR2.UI.PingIndicator.PingType.Enemy && self.pingTarget && self.pingOwner)
        //    {
        //        var targetBody = self.pingTarget.GetComponent<CharacterBody>();
        //        var ownerController = self.pingOwner.GetComponent<PlayerCharacterMasterController>();
        //        if(targetBody && ownerController)
        //        {
        //            var ownerBody = ownerController.body;
        //            if (ownerBody)
        //            {
        //                var count = GetCount(ownerBody);
        //                for (int i = 0; i < count; i++)
        //                {
        //                    targetBody.AddTimedBuff(Content.Buffs.DamageOnPing, 30); // by default is equal to ping duration
        //                }
        //            }
        //        }
        //    }
        //}

        private void RemoveBuffsIfPresent(RoR2.UI.PingIndicator pingIndicator)
        {
            if (pingIndicator && NetworkServer.active)
            {
                if (pingIndicator.pingType == RoR2.UI.PingIndicator.PingType.Enemy && pingIndicator.pingTarget && pingIndicator.pingOwner)
                {
                    var targetBody = pingIndicator.pingTarget.GetComponent<CharacterBody>();
                    if (targetBody)
                    {
                        var count = targetBody.GetBuffCount(Content.Buffs.DamageOnPing);
                        for (int i = 0; i < count; i++)
                        {
                            targetBody.RemoveOldestTimedBuff(Content.Buffs.DamageOnPing);
                        }
                    }
                }
            }
        }

        private void AdddBuffsToPingedTarget(RoR2.UI.PingIndicator pingIndicator)
        {
            if (pingIndicator && NetworkServer.active)
            {
                if (pingIndicator.pingType == RoR2.UI.PingIndicator.PingType.Enemy && pingIndicator.pingTarget && pingIndicator.pingOwner)
                {
                    var targetBody = pingIndicator.pingTarget.GetComponent<CharacterBody>();
                    var ownerController = pingIndicator.pingOwner.GetComponent<PlayerCharacterMasterController>();
                    if (targetBody && ownerController)
                    {
                        var ownerBody = ownerController.master?.GetBody();
                        if (ownerBody)
                        {
                            var count = GetCount(ownerBody);
                            for (int i = 0; i < count; i++)
                            {
                                targetBody.AddTimedBuff(Content.Buffs.DamageOnPing, 30); // by default is equal to ping duration
                            }
                        }
                    }
                }
            }
        }
    }
}

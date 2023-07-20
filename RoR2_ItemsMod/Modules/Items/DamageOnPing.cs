using BepInEx.Configuration;
using R2API;
using RoR2;
using SimpleJSON;
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

        // basically, game does some funky scaling so all item displays are roughly the same size
        // by using model's bounding box and comparing to "ideal model" which is just a cube of Vector3.one
        // changing size in blender doesn't matter that much (or it matters sometimes, because it did before this item)
        // as much as your bouncing box being as close to square as possible AND scale in unity
        // for some reason, for this item at least, lowering scale in unity makes the item larger
        // I guess it make sense since the code upscales the model, but I feel like it is just an error in the code
        // rather than intentional feature, since it doesn't make sense
        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("DamageOnPing");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texWitchHunterTools");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override string GetOverlayDescription(string value, JSONNode tokensNode)
        {
            return value;
            //throw new System.NotImplementedException();
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
            DamageBuff.buffColor = Color.blue;
            DamageBuff.canStack = true;
            DamageBuff.isDebuff = true;
            DamageBuff.isHidden = false; // TODO: replace with true when done
            DamageBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/CritOnUse/texBuffFullCritIcon.tif").WaitForCompletion();

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

        private void RemoveBuffsIfPresent(RoR2.UI.PingIndicator pingIndicator)
        {
            if (pingIndicator && NetworkServer.active)
            {
                if (pingIndicator.pingType == RoR2.UI.PingIndicator.PingType.Enemy && pingIndicator.pingTarget && pingIndicator.pingOwner)
                {
                    var ownerController = pingIndicator.pingOwner.GetComponent<PlayerCharacterMasterController>();
                    var targetBody = pingIndicator.pingTarget.GetComponent<CharacterBody>();
                    if (targetBody && ownerController)
                    {
                        var count = GetCount(ownerController.master?.GetBody());
                        var countBuffs = targetBody.GetBuffCount(Content.Buffs.DamageOnPing);
                        if(countBuffs < count)
                        {
                            count = countBuffs;
                        }
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

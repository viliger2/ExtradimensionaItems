using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static RoR2.HurtBox;

namespace ExtradimensionalItems.Modules.Items
{
    public class DamageOnPing : ItemBase<DamageOnPing>
    {
        public static ConfigEntry<float> DamageIncrease;
        public static ConfigEntry<bool> IsDebuff;
        public static ConfigEntry<bool> IsHidden;
        public static ConfigEntry<float> DebuffDuration;
        public static ConfigEntry<bool> DisablePingEnemyChatMessages;

        public override string ItemName => "DamageOnPing";

        public override string ItemLangTokenName => "DAMAGE_ON_PING";

        public override ItemTier Tier => ItemTier.Tier1;

        public override string BundleName => "damageonping";

        // basically, game does some funky scaling so all item displays are roughly the same size
        // by using model's bounding box and comparing to "ideal model" which is just a cube of Vector3.one;
        // changing size in blender doesn't matter that much (or it matters sometimes, because it did before this item)
        // as much as your bouncing box being as close to square as possible AND scale in unity;
        // for some reason, for this item at least, lowering scale in unity makes the item larger;
        // I feel like it is just an error in the code rather than intentional feature, since it doesn't make sense
        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("DamageOnPing");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texWitchHunterTools");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override string GetOverlayDescription(string value, JSONNode tokensNode)
        {
            string text = (DamageIncrease.Value / 100).ToString("###%");
            return string.Format(value, text, text);
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
            DamageBuff.isDebuff = IsDebuff.Value; // otherwise you have permanent, easy to apply debuff for death mark
            DamageBuff.isHidden = IsHidden.Value; 
            DamageBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/CritOnUse/texBuffFullCritIcon.tif").WaitForCompletion();

            ContentAddition.AddBuffDef(DamageBuff);

            Content.Buffs.DamageOnPing = DamageBuff;

            if (BetterUICompat.enabled)
            {
                BetterUICompat.AddBuffInfo(DamageBuff, "BUFF_DAMAGE_ON_PING_NAME", "BUFF_DAMAGE_ON_PING_DESCRIPTION");
            }
        }

        protected override void Hooks()
        {
            base.Hooks();
            //On.RoR2.UI.PingIndicator.RebuildPing += PingIndicator_RebuildPing;
            On.RoR2.PingerController.RebuildPing += PingerController_RebuildPing;
            On.RoR2.UI.PingIndicator.OnDisable += PingIndicator_OnDisable;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            if (DisablePingEnemyChatMessages.Value)
            {
                IL.RoR2.UI.PingIndicator.RebuildPing += IL_RebuildPing_RemoveEnemyPingMessage;
            }
        }

        private void IL_RebuildPing_RemoveEnemyPingMessage(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            Mono.Cecil.Cil.Instruction breakOffset;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdstr("PLAYER_PING_ENEMY"),
                x => x.MatchCallOrCallvirt<RoR2.Language>("GetString"),
                x => x.MatchLdloc(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchCallOrCallvirt<System.String>("Format")
                ))
            {
                c.Index += 7;
                breakOffset = c.Previous;
                c.Index -= 7;

                ILCursor breaksCursor = c.Clone();

                if (breaksCursor.TryGotoPrev(MoveType.After,
                    x => x.MatchLdloc(out _),
                    x => x.MatchCallOrCallvirt<RoR2.ModelLocator>("get_modelTransform"),
                    x => x.MatchStloc(out _),
                    x => x.MatchLdloc(out _),
                    x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit")))
                {
                    breaksCursor.Remove();
                    breaksCursor.Emit(OpCodes.Brfalse_S, breakOffset);
                }
                else
                {
                    MyLogger.LogWarning("Couldn't find \"Transform modelTransform = modelLocator.modelTransform\" in PingIndicator.RebuildPing");
                    return;
                }

                if (breaksCursor.TryGotoNext(MoveType.After,
                    x => x.MatchLdloc(out _),
                    x => x.MatchCallOrCallvirt<UnityEngine.Component>("GetComponent"),
                    x => x.MatchStloc(out _),
                    x => x.MatchLdloc(out _),
                    x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit")))
                {
                    breaksCursor.Remove();
                    breaksCursor.Emit(OpCodes.Brfalse_S, breakOffset);
                }
                else
                {
                    MyLogger.LogWarning("Couldn't find \"CharacterModel component2 = modelTransform.GetComponent<CharacterModel>()\" in PingIndicator.RebuildPing");
                    return;
                }

                c.RemoveRange(6);
            }
            else
            {
                MyLogger.LogWarning("Couldn't find PLAYER_PING_ENEMY in PingIndicator.RebuildPing");
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if(self.body && self.body.HasBuff(Content.Buffs.DamageOnPing))
            {
                damageInfo.damage += damageInfo.damage * ((DamageIncrease.Value / 100) * self.body.GetBuffCount(Content.Buffs.DamageOnPing));
            }
            orig(self, damageInfo);
        }

        private void PingerController_RebuildPing(On.RoR2.PingerController.orig_RebuildPing orig, PingerController self, PingerController.PingInfo pingInfo)
        {
            RemoveBuffsIfPresent(self.pingIndicator);
            orig(self, pingInfo);
            AddBuffsToPingedTarget(self.pingIndicator);
        }


        private void PingIndicator_OnDisable(On.RoR2.UI.PingIndicator.orig_OnDisable orig, RoR2.UI.PingIndicator self)
        {
            RemoveBuffsIfPresent(self);
            orig(self);
        }

        private void RemoveBuffsIfPresent(RoR2.UI.PingIndicator pingIndicator)
        {
            if(NetworkServer.active && pingIndicator && pingIndicator.pingType == RoR2.UI.PingIndicator.PingType.Enemy && pingIndicator.pingTarget && pingIndicator.pingOwner)
            {
                var ownerBody = pingIndicator.pingOwner.GetComponent<PlayerCharacterMasterController>()?.master?.GetBody() ?? null;
                var count = GetCount(ownerBody);
                if (count > 0)
                {
                    var targetBody = pingIndicator.pingTarget.GetComponent<CharacterBody>() ?? null;
                    if (targetBody)
                    {
                        var countBuffs = targetBody.GetBuffCount(Content.Buffs.DamageOnPing);
                        if(countBuffs < count) { count = countBuffs; }
                        for (int i = 0; i < count; i++)
                        {
                            targetBody.RemoveOldestTimedBuff(Content.Buffs.DamageOnPing);
                        }
                        MyLogger.LogMessage("Removed {0} instances of DamageOnPing buff from {1}, applied by {2}({3}).", count.ToString(), targetBody.name, ownerBody.GetUserName(), ownerBody.name);
                    }
                }

            }
        }

        private void AddBuffsToPingedTarget(RoR2.UI.PingIndicator pingIndicator)
        {
            if (NetworkServer.active && pingIndicator && pingIndicator.pingType == RoR2.UI.PingIndicator.PingType.Enemy && pingIndicator.pingTarget && pingIndicator.pingOwner)
            {
                var ownerBody = pingIndicator.pingOwner?.GetComponent<PlayerCharacterMasterController>()?.master?.GetBody() ?? null;
                var count = GetCount(ownerBody);
                if(count > 0)
                {
                    var targetBody = pingIndicator.pingTarget?.GetComponent<CharacterBody>() ?? null;
                    if(targetBody)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            targetBody.AddTimedBuff(Content.Buffs.DamageOnPing, DebuffDuration.Value); // by default is equal to ping duration
                        }
                        MyLogger.LogMessage("Added {0} instances of DamageOnPing buff to {1}, applied by {2}({3}).", count.ToString(), targetBody.name, ownerBody.GetUserName(), ownerBody.name);
                    }
                }
            }
        }

        public override void CreateConfig(ConfigFile config)
        {
            DamageIncrease = config.Bind("Item: " + ItemName, "Damage Increase Per Buff Stack", 5f, "By how much each stack of item increases damage to pinged enemy.");
            IsHidden = config.Bind("Item: " + ItemName, "Is Damage Debuff Hidden", true, "Item works by applying invisible debuff. This setting determines if it shows above enemy healthbar or now.");
            IsDebuff = config.Bind("Item: " + ItemName, "Does Damage Debuff Count to Death Mark", false, "Determines value of isDebuff in BuffDef. It is mainly used for Death Mark, whether buff counts towards it or not. Requires game restart to take effect.");
            DebuffDuration = config.Bind("Item: " + ItemName, "Debuff Duration", 30f, "How long debuff lasts. By default it is equal to the duration of ping. Has no effect on whether debuff is removed or not when ping is removed from an enemy. Requires game restart to take effect.");
            DisablePingEnemyChatMessages = config.Bind("Item: " + ItemName, "Disable Enemy Ping Chat Messages", false, "Disables chat messages on pinging enemy. Setting is client-side. Requires game restart to take effect.");
            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(DamageIncrease, 1f, 50f, 0.5f);
                RiskOfOptionsCompat.CreateNewOption(DebuffDuration, 1f, 60f, 1f);
                RiskOfOptionsCompat.CreateNewOption(DisablePingEnemyChatMessages, true);
                RiskOfOptionsCompat.CreateNewOption(IsHidden, true);
                RiskOfOptionsCompat.CreateNewOption(IsDebuff, true);
                RiskOfOptionsCompat.AddDelegateOnModOptionsExit(OnModOptionsExit);
            }
        }
    }
}

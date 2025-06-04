using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

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
            // TODO: item displays for CHEF, Teslalads and VideogameMod2
            // maybe HAN-D once I get off my ass and unlock him

            var modelGun = AssetBundle.LoadAsset<GameObject>("DamageOnPingGun");
            modelGun.AddComponent<RoR2.ItemDisplay>();
            modelGun.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(modelGun);

            var modelRapier = AssetBundle.LoadAsset<GameObject>("DamageOnPingRapier");
            modelRapier.AddComponent<RoR2.ItemDisplay>();
            modelRapier.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(modelRapier);

            var modelHat = AssetBundle.LoadAsset<GameObject>("DamageOnPingHat");
            modelHat.AddComponent<RoR2.ItemDisplay>();
            modelHat.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(modelHat);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelGun, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Chest",
                    localPos = new Vector3(0.18377F, -0.10061F, 0.02524F),
                    localAngles = new Vector3(0.20077F, 269.9417F, 37.69195F),
                    localScale = new Vector3(0.72029F, 0.72029F, 0.72029F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelGun, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Pelvis",
                    localPos = new Vector3(0.13904F, -0.07351F, -0.01589F),
                    localAngles = new Vector3(-0.00006F, 270F, 213.1598F),
                    localScale = new Vector3(0.72259F, 0.72259F, 0.72259F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelGun, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "HandR",
                    localPos = new Vector3(0.31382F, 1.74157F, 0.60946F),
                    localAngles = new Vector3(346.4059F, 260.1173F, 213.3281F),
                    localScale = new Vector3(11.3512F, 11.3512F, 11.3512F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelGun, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "HandR",
                    localPos = new Vector3(0.0571F, 0.1966F, -0.03461F),
                    localAngles = new Vector3(0.1862F, 351.38F, 196.787F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelHat, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(0.00279F, 0.16615F, -0.03092F),
                    localAngles = new Vector3(9.03134F, 359.823F, 356.4779F),
                    localScale = new Vector3(1.2F, 1.2F, 1.2F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelRapier, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "HandR",
                    localPos = new Vector3(0.26595F, 0.20585F, 0.05858F),
                    localAngles = new Vector3(332.4011F, 179.8374F, 79.77181F),
                    localScale = new Vector3(1F, 1F, 1.26649F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelHat, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Eye",
                    localPos = new Vector3(0.00409F, 0.74916F, -0.17407F),
                    localAngles = new Vector3(280.5755F, 180F, 180F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelHat, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.00117F, 0.22252F, -0.00043F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.14343F, 1.14343F, 1.14343F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelHat, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.03373F, 0.81664F, 1.54496F),
                    localAngles = new Vector3(86.68117F, 224.971F, 222.2006F),
                    localScale = new Vector3(9F, 9F, 9F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelRapier, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "HandR",
                    localPos = new Vector3(0.03741F, 0.16865F, 0.27323F),
                    localAngles = new Vector3(318.6001F, 97.67763F, 90.44086F),
                    localScale = new Vector3(1F, 1F, 1.3F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelGun, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "SideWeapon",
                    localPos = new Vector3(0.02772F, -0.17812F, -0.06708F),
                    localAngles = new Vector3(2.39753F, 93.10816F, 88.09953F),
                    localScale = new Vector3(0.787F, 0.787F, 0.787F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelHat, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.25373F, -0.15981F, 0.01191F),
                    localAngles = new Vector3(283.2346F, 250.2164F, 187.7384F),
                    localScale = new Vector3(2.24814F, 2F, 2.30177F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelGun, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Pelvis",
                    localPos = new Vector3(0.17497F, 0.16396F, 0.03976F),
                    localAngles = new Vector3(-0.00003F, 270F, 212.9068F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelHat, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.02234F, 0.11475F, -0.05931F),
                    localAngles = new Vector3(312.8158F, 336.3667F, 28.12838F),
                    localScale = new Vector3(1.5F, 1.5F, 1.5F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelRapier, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Pelvis",
                    localPos = new Vector3(0.0018F, 0.00082F, -0.0004F),
                    localAngles = new Vector3(16.55219F, 269.3384F, 97.96724F),
                    localScale = new Vector3(0.01F, 0.01F, 0.01F)
                }
            });
            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelHat, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.00002F, 0.00659F, 0.00022F),
                    localAngles = new Vector3(355.565F, 266.5702F, 3.54357F),
                    localScale = new Vector3(0.0566F, 0.05263F, 0.05272F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelHat, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(0.0349F, 0.17922F, 0.0079F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(2.02453F, 1.94496F, 2.25229F)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelRapier, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "HandL",
                    localPos = new Vector3(-0.10191F, 0.22259F, 0.392F),
                    localAngles = new Vector3(49.80999F, 261.9417F, 274.5118F),
                    localScale = new Vector3(1.48088F, 1.48088F, 1.98821F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelGun, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Chest",
                    localPos = new Vector3(0.18035F, -0.31125F, -0.16028F),
                    localAngles = new Vector3(7.6152F, 343.379F, 53.77204F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlSniper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = modelGun, followerPrefabAddress = new UnityEngine.AddressableAssets.AssetReferenceGameObject(""),
                    childName = "Pelvis",
                    localPos = new Vector3(0.23522F, 0.03285F, 0.0116F),
                    localAngles = new Vector3(348.0713F, 277.8353F, 38.48006F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });

            return rules;
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
            SetLogbookCameraPosition();
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

                ILCursor break1Cursor = c.Clone();

                if (break1Cursor.TryGotoPrev(MoveType.After,
                    x => x.MatchLdloc(out _),
                    x => x.MatchCallOrCallvirt<RoR2.ModelLocator>("get_modelTransform"),
                    x => x.MatchStloc(out _),
                    x => x.MatchLdloc(out _),
                    x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit")))
                {
                    ILCursor break2Cursor = break1Cursor.Clone();

                    if (break2Cursor.TryGotoNext(MoveType.After,
                        x => x.MatchLdloc(out _),
                        x => x.MatchCallOrCallvirt<UnityEngine.Component>("GetComponent"),
                        x => x.MatchStloc(out _),
                        x => x.MatchLdloc(out _),
                        x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit")))
                    {
                        break2Cursor.Remove();
                        break2Cursor.Emit(OpCodes.Brfalse_S, breakOffset);

                        break1Cursor.Remove();
                        break1Cursor.Emit(OpCodes.Brfalse_S, breakOffset);

                        c.RemoveRange(6);
                    }
                    else
                    {
                        MyLogger.LogWarning("Couldn't find \"CharacterModel component2 = modelTransform.GetComponent<CharacterModel>()\" in PingIndicator.RebuildPing");
                        return;
                    }
                }
                else
                {
                    MyLogger.LogWarning("Couldn't find \"Transform modelTransform = modelLocator.modelTransform\" in PingIndicator.RebuildPing");
                    return;
                }
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
            IsHidden = config.Bind("Item: " + ItemName, "Is Damage Debuff Hidden", true, "Item works by applying invisible debuff. This setting determines if it shows above enemy healthbar or not.");
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

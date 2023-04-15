using BepInEx.Configuration;
using ExtradimensionalItems.Modules.Items.ItemBehaviors;
using R2API;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.Items
{
    public class DamageOnCooldowns : ItemBase<DamageOnCooldowns>
    {

        public class DamageOnCooldownsSendNumberBuffs : INetMessage
        {
            private NetworkInstanceId netId;
            private int numberOfBuffsFromClient;

            public DamageOnCooldownsSendNumberBuffs() { }
            public DamageOnCooldownsSendNumberBuffs(NetworkInstanceId netId, int buffsCount)
            {
                this.netId = netId;
                this.numberOfBuffsFromClient = buffsCount;
            }

            public void Deserialize(NetworkReader reader)
            {
                netId = reader.ReadNetworkId();
                numberOfBuffsFromClient = reader.ReadInt32();
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                {
                    MyLogger.LogInfo("Recieved ChronoshiftRestoreStateOnServer message on client, doing nothing...");
                    return;
                }

                GameObject gameObject = Util.FindNetworkObject(netId);
                if (gameObject)
                {
                    if (gameObject.TryGetComponent(out CharacterBody body))
                    {
                        ApplyBuffs(body, numberOfBuffsFromClient);
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(netId);
                writer.Write(numberOfBuffsFromClient);
            }
        }

        public static ConfigEntry<float> DamageBonus;
        public static ConfigEntry<float> DamageBonusPerStack;

        public override string ItemName => "DamageOnCooldowns";

        public override string ItemLangTokenName => "DAMAGE_ON_COOLDOWNS";

        public override ItemTier Tier => ItemTier.Tier2;

        public override string BundleName => "damageoncooldowns";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override GameObject ItemModel => AssetBundle.LoadAsset<GameObject>("damageoncooldowns");

        public override Sprite ItemIcon => AssetBundle.LoadAsset<Sprite>("texDamageOnCooldownIcon");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var ItemBodyModelPrefab = AssetBundle.LoadAsset<GameObject>("damageoncooldowns");
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();

            // to fix item fade enable "Dither" on hopoo shader in Unity
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = Utils.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.09552F, 0.11592F, 0.06752F),
                    localAngles = new Vector3(27.08141F, 304.4822F, 189.2228F),
                    localScale = new Vector3(0.20751F, 0.20751F, 0.20751F)              }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.11649F, 0.18389F, 0.06237F),
                    localAngles = new Vector3(28.06488F, 266.8881F, 196.0231F),
                    localScale = new Vector3(0.19636F, 0.19636F, 0.19636F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.12048F, 0.90659F, 0.87745F),
                    localAngles = new Vector3(23.94655F, 178.5852F, 175.6136F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.18828F, 0.13572F, 0.00151F),
                    localAngles = new Vector3(30.69436F, 274.8616F, 176.1863F),
                    localScale = new Vector3(0.22972F, 0.22972F, 0.22972F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.11235F, 0.22891F, 0.05888F),
                    localAngles = new Vector3(25.38397F, 272.8091F, 189.4688F),
                    localScale = new Vector3(0.17773F, 0.17773F, 0.17773F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.10393F, 0.20613F, 0.06577F),
                    localAngles = new Vector3(29.68564F, 300.2674F, 182.2623F),
                    localScale = new Vector3(0.23426F, 0.23426F, 0.23426F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighBackR",
                    localPos = new Vector3(0.32198F, 0.39853F, 0.15919F),
                    localAngles = new Vector3(333.9132F, 289.5818F, 57.39796F),
                    localScale = new Vector3(0.34275F, 0.34275F, 0.34275F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.12364F, 0.15695F, 0.09189F),
                    localAngles = new Vector3(37.29633F, 301.3473F, 186.7303F),
                    localScale = new Vector3(0.22691F, 0.22691F, 0.22691F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-1.14331F, 1.28121F, -0.09511F),
                    localAngles = new Vector3(29.22408F, 243.6448F, 166.6056F),
                    localScale = new Vector3(2.34388F, 2.34388F, 2.34388F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.12231F, 0.10491F, 0.03471F),
                    localAngles = new Vector3(20.49907F, 97.37933F, 172.5078F),
                    localScale = new Vector3(0.24559F, 0.24559F, 0.24559F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.07267F, 0.24929F, 0.03758F),
                    localAngles = new Vector3(28.44042F, 288.6184F, 185.2202F),
                    localScale = new Vector3(0.19305F, 0.19305F, 0.19305F)
                }
            });
            rules.Add("mdlHeretic", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.22414F, 0.16346F, 0.20641F),
                    localAngles = new Vector3(343.4409F, 25.48389F, 270.7061F),
                    localScale = new Vector3(0.42563F, 0.42563F, 0.42563F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.10685F, 0.08854F, 0.03331F),
                    localAngles = new Vector3(34.29095F, 98.02614F, 174.7414F),
                    localScale = new Vector3(0.19173F, 0.19173F, 0.19173F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.04302F, 0.27724F, -0.11008F),
                    localAngles = new Vector3(39.20871F, 5.40946F, 183.1687F),
                    localScale = new Vector3(0.21219F, 0.21219F, 0.21219F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LegR",
                    localPos = new Vector3(0.00047F, 0.00182F, 0.0011F),
                    localAngles = new Vector3(36.96081F, 203.9341F, 184.2606F),
                    localScale = new Vector3(0.00194F, 0.00194F, 0.00194F)
                }
            });
            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LegR",
                    localPos = new Vector3(-0.00262F, 0.00736F, -0.00406F),
                    localAngles = new Vector3(38.16177F, 26.56483F, 169.9774F),
                    localScale = new Vector3(0.00825F, 0.00825F, 0.00825F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.05615F, 0.31438F, 0.13046F),
                    localAngles = new Vector3(32.51384F, 163.4914F, 191.6256F),
                    localScale = new Vector3(0.23618F, 0.23618F, 0.23618F)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.15149F, 0.49419F, -0.18609F),
                    localAngles = new Vector3(38.69322F, 308.8141F, 179.926F),
                    localScale = new Vector3(0.31299F, 0.31299F, 0.31299F)
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
            return string.Format(pickupString, (DamageBonus.Value / 100).ToString("###%"), (DamageBonusPerStack.Value / 100).ToString("###%"));
        }

        public override void Init(ConfigFile config)
        {
            LoadAssetBundle();
            CreateConfig(config);
            CreateBuffs();
            CreateItem(ref Content.Items.DamageOnCooldowns);
            Hooks();
        }

        private void CreateBuffs()
        {
            var DamageOnCooldownsBuff = ScriptableObject.CreateInstance<BuffDef>();
            DamageOnCooldownsBuff.name = "Damage On Cooldowns";
            DamageOnCooldownsBuff.buffColor = Color.grey;
            DamageOnCooldownsBuff.canStack = true;
            DamageOnCooldownsBuff.isDebuff = false;
            DamageOnCooldownsBuff.iconSprite = AssetBundle.LoadAsset<Sprite>("texDamageOnCooldownBuffIcon");

            ContentAddition.AddBuffDef(DamageOnCooldownsBuff);

            Content.Buffs.DamageOnCooldowns = DamageOnCooldownsBuff;

            if (BetterUICompat.enabled)
            {
                BetterUICompat.AddBuffInfo(DamageOnCooldownsBuff, "BUFF_DAMAGE_ON_COOLDOWNS_NAME", "BUFF_DAMAGE_ON_COOLDOWNS_DESCRIPTION");
            }
        }

        protected override void Hooks()
        {
            base.Hooks();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var attacker = damageInfo?.attacker ?? null;
            if (attacker && attacker.TryGetComponent<CharacterBody>(out var body))
            {
                damageInfo.damage += damageInfo.damage * (((DamageBonus.Value / 100) + (DamageBonusPerStack.Value / 100) * (GetCount(body) - 1)) * body.GetBuffCount(Content.Buffs.DamageOnCooldowns));
            }
            orig(self, damageInfo);
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            if (body)
            {
                if (body.hasAuthority)
                {
                    body.AddItemBehavior<DamageOnCooldownsBehavior>(GetCount(body));
                }
                if (NetworkServer.active)
                {
                    if (body.HasBuff(Content.Buffs.DamageOnCooldowns) && GetCount(body) == 0)
                    {
                        while (body.HasBuff(Content.Buffs.DamageOnCooldowns))
                        {
                            body.RemoveBuff(Content.Buffs.DamageOnCooldowns);
                        }
                    }
                }
            }
        }

        public static void ApplyBuffs(CharacterBody body, int count)
        {
            var currentBuffCount = body.GetBuffCount(Content.Buffs.DamageOnCooldowns);
            while (count > currentBuffCount)
            {
                body.AddBuff(Content.Buffs.DamageOnCooldowns);
                currentBuffCount++;
            }
            while (count < currentBuffCount)
            {
                body.RemoveBuff(Content.Buffs.DamageOnCooldowns);
                count++;
            }
        }

        public override void AddBetterUIStats(ItemDef item)
        {
            base.AddBetterUIStats(item);
            BetterUICompat.RegisterStat(item, "BETTERUICOMPAT_DESC_DAMAGE", DamageBonus.Value / 100, DamageBonusPerStack.Value / 100, BetterUICompat.StackingFormulas.LinearStacking, BetterUICompat.StatFormatter.Percent, BetterUICompat.ItemTags.Damage);
        }

        public override void CreateConfig(ConfigFile config)
        {
            DamageBonus = config.Bind("Item: " + ItemName, "Damage Bonus", 10f, "How much additional damage, in percentage, each ability and equipment on cooldown adds.");
            DamageBonusPerStack = config.Bind("Item: " + ItemName, "Damage Bonus Per Stack", 5f, "How much additional damage per item stack, in percentage, each each ability and equipment on cooldown adds.");
            if (RiskOfOptionsCompat.enabled)
            {
                RiskOfOptionsCompat.CreateNewOption(DamageBonus, 1f, 30f, 1f);
                RiskOfOptionsCompat.CreateNewOption(DamageBonusPerStack, 1f, 30f, 1f);
            }
        }
    }
}

using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static RoR2.HurtBox;
using static ExtradimensionalItems.Modules.Items.Sheen;

namespace ExtradimensionalItems.Modules.Items.ItemBehaviors
{
    // TODO: rewrite so the buff is independent from ItemBehavior, so we can use it outside of the intended item
    // not sure if it is worth it since it would create additional overhead
    public class SheenBehavior : CharacterBody.ItemBehavior, IOnDamageDealtServerReceiver
    {
        private bool usedPrimary;

        public void Awake()
        {
            this.enabled = false;
        }

        public void OnEnable()
        {
            if (body)
            {
                body.onSkillActivatedServer += Body_onSkillActivatedServer;
            }
        }

        private void OnDestroy()
        {
            if (body)
            {
                body.onSkillActivatedServer -= Body_onSkillActivatedServer;
            }
        }

        private void Body_onSkillActivatedServer(GenericSkill skill)
        {
            int itemCount = body?.inventory?.GetItemCount(Content.Items.Sheen) ?? 0;
            if (itemCount > 0)
            {
                if (body.skillLocator.primary != skill && body.GetBuffCount(Content.Buffs.Sheen) < BuffStackPerItem.Value * itemCount)
                {
                    MyLogger.LogMessage("Player {0}({1}) used non-primary skill, adding buff {2}.", body.GetUserName(), body.name, Content.Buffs.Sheen.name);
                    body.AddTimedBuff(Content.Buffs.Sheen, BuffDuration.Value);
                }
                else if (body.skillLocator.primary == skill && body.HasBuff(Content.Buffs.Sheen))
                {
                    this.usedPrimary = true;
                }
            }
        }

        public void OnDamageDealtServer(DamageReport damageReport)
        {
            if (this.usedPrimary && enabled)
            {
                var damageInfo = damageReport.damageInfo;
                var attacker = damageReport?.attackerBody ?? null;
                var victim = damageReport?.victimBody ?? null;
                if (attacker && victim)
                {
                    if (attacker == this.body && attacker.HasBuff(Content.Buffs.Sheen))
                    {
                        if (!damageInfo.rejected && (damageInfo.damageType & DamageType.DoT) != DamageType.DoT)
                        {
                            DamageInfo damageInfo2 = new DamageInfo();
                            damageInfo2.damage = body.damage * body.inventory.GetItemCount(Content.Items.Sheen) * (Sheen.DamageModifier.Value / 100);
                            damageInfo2.attacker = damageReport.attacker;
                            damageInfo2.crit = false;
                            damageInfo2.position = damageInfo.position;
                            damageInfo2.damageColorIndex = DamageColorIndex.Item;
                            damageInfo2.damageType = DamageType.Generic;

                            MyLogger.LogMessage("Body {0}({1}) had buff {2}, dealing {3} damage to {4} and removing buff from the body.", body.GetUserName(), body.name, Content.Buffs.Sheen.name, damageInfo2.damage.ToString(), victim.name);

                            body.RemoveTimedBuff(Content.Buffs.Sheen);
                            this.usedPrimary = false;

                            damageReport.victim.TakeDamage(damageInfo2);
                        }
                    }
                }
            }
        }
    }

}

using RoR2;
using static ExtradimensionalItems.Modules.Items.Sheen;
using UnityEngine.Networking;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items.ItemBehaviors
{
    // TODO: rewrite so the buff is independent from ItemBehavior, so we can use it outside of the intended item
    // not sure if it is worth it since it would create additional overhead
    public class SheenBehavior : CharacterBody.ItemBehavior, IOnDamageDealtServerReceiver
    {
        private bool usedPrimary;

        private float stopwatch;

        private float buffTimer = Sheen.BuffApplicationCooldown.Value;

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

        public void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            stopwatch += Time.fixedDeltaTime;
            if(stopwatch > buffTimer)
            {
                stopwatch = buffTimer + 1f;
            }
        }

        private void Body_onSkillActivatedServer(GenericSkill skill)
        {
            var primarySkill = body?.skillLocator?.primary ?? null;
            if (stack > 0 && primarySkill)
            {
                if (primarySkill != skill && stopwatch > buffTimer)
                {
                    if (body.GetBuffCount(Content.Buffs.Sheen) >= BuffStackPerItem.Value * stack)
                    {
                        body.RemoveOldestTimedBuff(Content.Buffs.Sheen);
                    }
                    MyLogger.LogMessage("Player {0}({1}) used non-primary skill, adding buff {2}.", body.GetUserName(), body.name, Content.Buffs.Sheen.name);
                    body.AddTimedBuff(Content.Buffs.Sheen, BuffDuration.Value);
                    stopwatch = 0f;
                }
                else if (primarySkill == skill && body.HasBuff(Content.Buffs.Sheen))
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
                            damageInfo2.damage = body.damage * (Sheen.DamageModifier.Value / 100) + body.damage * (stack - 1) * (Sheen.DamageModifierPerStack.Value / 100);
                            damageInfo2.attacker = damageReport.attacker;
                            damageInfo2.crit = false;
                            damageInfo2.position = damageInfo.position;
                            damageInfo2.damageColorIndex = DamageColorIndex.Item;
                            damageInfo2.damageType = DamageType.Generic;
                            damageInfo2.procCoefficient = 0f;

                            MyLogger.LogMessage("Body {0}({1}) had buff {2}, dealing {3} damage to {4} and removing buff from the body.", body.GetUserName(), body.name, Content.Buffs.Sheen.name, damageInfo2.damage.ToString(), victim.name);

                            body.RemoveOldestTimedBuff(Content.Buffs.Sheen);
                            this.usedPrimary = false;

                            damageReport.victim.TakeDamage(damageInfo2);
                        }
                    }
                }
            }
        }
    }

}

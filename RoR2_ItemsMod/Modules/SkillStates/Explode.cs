﻿using EntityStates;
using RoR2;

namespace ExtradimensionalItems.Modules.SkillStates
{
    public class Explode : BaseSkillState
    {

        public override void OnEnter()
        {
            base.OnEnter();
            ExplodeNow();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!characterBody.HasBuff(Content.Buffs.RoyalGuardDamage))
            {
                outer.SetNextStateToMain();
                MyLogger.LogMessage("Player {0}({1}) has left parry state because they don't have damage buff after using Release.", characterBody.GetUserName(), characterBody.name);
            }
        }

        private void ExplodeNow()
        {
            if (characterBody.HasBuff(Content.Buffs.RoyalGuardDamage))
            {
                BlastAttack blastAttack = new BlastAttack
                {
                    radius = Items.RoyalGuard.DamageRadius.Value,
                    procCoefficient = 2f,
                    position = base.characterBody.corePosition,
                    attacker = base.characterBody.gameObject,
                    crit = RollCrit(),
                    baseDamage = base.characterBody.baseDamage * characterBody.GetBuffCount(Content.Buffs.RoyalGuardDamage) * (Items.RoyalGuard.DamageModifier.Value / 100),
                    falloffModel = BlastAttack.FalloffModel.SweetSpot,
                    baseForce = 1f,
                    teamIndex = TeamComponent.GetObjectTeam(base.characterBody.gameObject),
                    damageType = DamageType.AOE,
                    attackerFiltering = AttackerFiltering.NeverHitSelf
                };
                blastAttack.Fire();

                characterBody.SetBuffCount(Content.Buffs.RoyalGuardDamage.buffIndex, 0);
                if (characterBody.HasBuff(Content.Buffs.RoyalGuardParryState))
                {
                    characterBody.RemoveOldestTimedBuff(Content.Buffs.RoyalGuardParryState);
                }

                EffectData effectData = new EffectData();
                effectData.origin = base.characterBody.footPosition;
                effectData.scale = 10;

                EffectManager.SpawnEffect(Items.RoyalGuard.RoyalGuardExplodeEffectInstance, effectData, true);

                MyLogger.LogMessage("Player {0}({1}) used Release, dealing to everyone around {2} damage.", characterBody.GetUserName(), characterBody.name, blastAttack.baseDamage.ToString());
            }
        }
    }
}

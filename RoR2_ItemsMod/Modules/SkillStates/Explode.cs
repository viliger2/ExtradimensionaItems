using EntityStates;
using ExtradimensionalItems.Modules.Buffs;
using RoR2;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.SkillStates
{
    public class Explode : BaseSkillState
    {

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                ExplodeNow();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!characterBody.HasBuff(RoyalGuardBuffs.RoyalGuardDamageBuff))
            {
                outer.SetNextStateToMain();
                MyLogger.LogMessage(string.Format("Player {0}({1}) has left parry state because they don't have parry state buff after using Release.", characterBody.GetUserName(), characterBody.name));
            }
        }

        private void ExplodeNow()
        {
            if (characterBody.HasBuff(RoyalGuardBuffs.RoyalGuardDamageBuff))
            {
                BlastAttack blastAttack = new BlastAttack
                {
                    radius = 8f,
                    procCoefficient = 1f,
                    position = base.characterBody.corePosition,
                    attacker = base.characterBody.gameObject,
                    crit = RollCrit(),
                    baseDamage = base.characterBody.baseDamage * characterBody.GetBuffCount(RoyalGuardBuffs.RoyalGuardDamageBuff) * Items.RoyalGuardItem.DamageModifier.Value,
                    falloffModel = BlastAttack.FalloffModel.SweetSpot,
                    baseForce = 1f,
                    teamIndex = TeamComponent.GetObjectTeam(base.characterBody.gameObject),
                    damageType = DamageType.AOE,
                    attackerFiltering = AttackerFiltering.NeverHitSelf
                };
                blastAttack.Fire();

                characterBody.SetBuffCount(RoyalGuardBuffs.RoyalGuardDamageBuff.buffIndex, 0);
                if (characterBody.HasBuff(RoyalGuardBuffs.RoyalGuardParryStateBuff))
                {
                    characterBody.RemoveTimedBuff(RoyalGuardBuffs.RoyalGuardParryStateBuff);
                }

                MyLogger.LogMessage(string.Format("Player {0}({1}) used Release, dealing to everyone around {2} damage.", characterBody.GetUserName(), characterBody.name, blastAttack.baseDamage));
            }
        }
    }
}

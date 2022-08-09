using EntityStates;
using RoR2;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules.SkillStates
{
    public class Parry : BaseSkillState
    {
        private float duration;
        private bool buffAdded = false; // flag for network play, since it takes at least 3 ticks for buff to register on client

        public override void OnEnter()
        {
            base.OnEnter();

            duration = Items.RoyalGuardItem.GetParryStateDuration(characterBody);
            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(Content.Buffs.RoyalGuardParryStateBuff, duration);
                MyLogger.LogMessage(string.Format("Player {0}({1}) entered parry state for {2} seconds.", base.characterBody.GetUserName(), base.characterBody.name, duration));

            }
            if (isAuthority)
            {
                if (characterBody.skillLocator && characterBody.HasBuff(Content.Buffs.RoyalGuardDamageBuff))
                {
                    characterBody.skillLocator.primary.SetSkillOverride(characterBody, Content.Skills.Explode, GenericSkill.SkillOverridePriority.Replacement);
                }
            }
        }

        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                if (characterBody.HasBuff(Content.Buffs.RoyalGuardParryStateBuff))
                {
                    characterBody.RemoveTimedBuff(Content.Buffs.RoyalGuardParryStateBuff);
                }
            }
            if (isAuthority)
            {
                if (characterBody.skillLocator)
                {
                    characterBody.skillLocator.primary.UnsetSkillOverride(characterBody, Content.Skills.Explode, GenericSkill.SkillOverridePriority.Replacement);
                }
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (characterBody.HasBuff(Content.Buffs.RoyalGuardParryStateBuff))
            {
                buffAdded = true;
            }
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                MyLogger.LogMessage(string.Format("Player {0}({1}) has left parry state due to timeout without being damaged.", characterBody.GetUserName(), characterBody.name));
            }
            if (buffAdded && !characterBody.HasBuff(Content.Buffs.RoyalGuardParryStateBuff) && isAuthority)
            {
                outer.SetNextStateToMain();
                MyLogger.LogMessage(string.Format("Player {0}({1}) has left parry state because they don't have parry state buff after being damaged.", characterBody.GetUserName(), characterBody.name));
            }
        }

        public override void Update()
        {
            base.Update();
            if (isAuthority && inputBank && characterBody.HasBuff(Content.Buffs.RoyalGuardDamageBuff))
            {
                if (inputBank.skill1.justPressed)
                {
                    skillLocator.primary.ExecuteIfReady();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}

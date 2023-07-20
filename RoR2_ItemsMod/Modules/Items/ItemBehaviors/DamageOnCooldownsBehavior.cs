using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static ExtradimensionalItems.Modules.Items.DamageOnCooldowns;
using UnityEngine.Networking;
using R2API.Networking.Interfaces;

namespace ExtradimensionalItems.Modules.Items.ItemBehaviors
{
    public class DamageOnCooldownsBehavior : CharacterBody.ItemBehavior
    {
        private int prevNumberOfBuffs;
        private NetworkInstanceId netId;
        public void OnEnable()
        {
            if (body)
            {
                this.netId = body.GetComponent<NetworkIdentity>().netId;
            }
        }

        public void FixedUpdate()
        {
            if (body.hasAuthority)
            {
                var newBuffCount = GetBuffCountFromSkill(body.skillLocator.primary)
                    + GetBuffCountFromSkill(body.skillLocator.secondary)
                    + GetBuffCountFromSkill(body.skillLocator.utility)
                    + GetBuffCountFromSkill(body.skillLocator.special)
                    + GetBuffCountFromInventory(body.equipmentSlot);

                if (prevNumberOfBuffs != newBuffCount)
                {
                    if (!NetworkServer.active)
                    {
                        MyLogger.LogMessage("Number of buffs for DamageOnCooldown changed for Player {0}({1}) to {2}, sending message to server.", body.GetUserName(), body.name, newBuffCount.ToString());
                        new DamageOnCooldownsSendNumberBuffs(netId, newBuffCount).Send(R2API.Networking.NetworkDestination.Server);
                    }
                    else
                    {
                        MyLogger.LogMessage("Number of buffs for DamageOnCooldown changed for Player {0}({1}) to {2}, we are on server, applying buffs.", body.GetUserName(), body.name, newBuffCount.ToString());
                        ApplyBuffs(body, newBuffCount);
                    }
                }

                prevNumberOfBuffs = newBuffCount;
            }
        }

        private int GetBuffCountFromSkill(GenericSkill skill)
        {
            return skill && skill.maxStock != skill.stock ? 1 : 0;
        }

        private int GetBuffCountFromInventory(EquipmentSlot es)
        {
            return es && es.equipmentIndex != EquipmentIndex.None && es.maxStock != es.stock ? 1 : 0;
        }
    }

}

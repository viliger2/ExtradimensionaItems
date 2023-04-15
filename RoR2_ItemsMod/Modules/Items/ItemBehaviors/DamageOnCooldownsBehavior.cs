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
                        new DamageOnCooldownsSendNumberBuffs(netId, newBuffCount).Send(R2API.Networking.NetworkDestination.Server);
                    }
                    else
                    {
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

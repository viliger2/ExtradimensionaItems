using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using static ExtradimensionalItems.Modules.ExtradimensionalItemsPlugin;
using static RoR2.CharacterBody;

namespace ExtradimensionalItems.Modules
{
    public static class ExtensionMethods
    {
        public static void AddOrReplace(this Dictionary<CharacterBody, bool> dictionary, CharacterBody key, bool value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static void RemoveTimedBuff(this CharacterBody body, BuffDef buff)
        {
            if (!NetworkServer.active)
            {
                MyLogger.LogWarning("[Server] extension function 'System.Void RoR2.CharacterBody::RemoveTimedBuff(RoR2.BuffDef)' called on cliend");
            } else
            {
                body.RemoveTimedBuff(buff.buffIndex);
            }
        }

        public static void RemoveTimedBuff(this CharacterBody body, BuffIndex buff)
        {
            if (!NetworkServer.active)
            {
                MyLogger.LogWarning("[Server] extension function 'System.Void RoR2.CharacterBody::RemoveTimedBuff(RoR2.BuffIndex)' called on cliend");
                return;
            }

            // finding TimedBuff with lowest duration
            TimedBuff lowest = null;

            foreach (TimedBuff timedBuff in body.timedBuffs)
            {
                if(timedBuff.buffIndex == buff) 
                {
                    if(lowest == null || lowest?.timer > timedBuff.timer)
                    {
                        lowest = timedBuff;
                    }
                }
            }

            if (lowest != null)
            {
                body.timedBuffs.Remove(lowest);
                body.RemoveBuff(lowest.buffIndex);
            }
        }

    }
}

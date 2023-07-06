using RoR2;
using System.Collections.Generic;
using UnityEngine.Networking;
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

        //public static void RemoveTimedBuff(this CharacterBody body, BuffDef buff)
        //{
        //    if (!NetworkServer.active)
        //    {
        //        MyLogger.LogWarning("[Server] extension function 'System.Void RoR2.CharacterBody::RemoveTimedBuff(RoR2.BuffDef)' called on client");
        //    }
        //    else
        //    {
        //        body.RemoveTimedBuff(buff.buffIndex);
        //    }
        //}

        //public static void RemoveTimedBuff(this CharacterBody body, BuffIndex buff)
        //{
        //    if (!NetworkServer.active)
        //    {
        //        MyLogger.LogWarning("[Server] extension function 'System.Void RoR2.CharacterBody::RemoveTimedBuff(RoR2.BuffIndex)' called on client");
        //        return;
        //    }

        //    TimedBuff lowest = body.GetTimedBuff(buff, true);

        //    if (lowest != null)
        //    {
        //        body.timedBuffs.Remove(lowest);
        //        body.RemoveBuff(lowest.buffIndex);
        //    }
        //}

        public static TimedBuff GetTimedBuff(this CharacterBody body, BuffDef buff, bool getLowest = false)
        {
            if (!NetworkServer.active)
            {
                MyLogger.LogWarning("[Server] extension function 'System.Void RoR2.CharacterBody::GetTimedBuff(RoR2.BuffDef, bool)' called on client");
                return null;
            }
            else
            {
                return body.GetTimedBuff(buff.buffIndex, getLowest);
            }
        }

        public static TimedBuff GetTimedBuff(this CharacterBody body, BuffIndex buff, bool getLowest = false)
        {
            if (!NetworkServer.active)
            {
                MyLogger.LogWarning("[Server] extension function 'System.Void RoR2.CharacterBody::GetTimedBuff(RoR2.BuffIndex, bool)' called on client");
                return null;
            }

            TimedBuff lowest = null;

            foreach (TimedBuff timedBuff in body.timedBuffs)
            {
                if (timedBuff.buffIndex == buff)
                {
                    if (getLowest)
                    {
                        if (lowest == null || lowest?.timer > timedBuff.timer)
                        {
                            lowest = timedBuff;
                        }
                    }
                    else
                    {
                        return timedBuff;
                    }
                }
            }

            return lowest;
        }
    }
}

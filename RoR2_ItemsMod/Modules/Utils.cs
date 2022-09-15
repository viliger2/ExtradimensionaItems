using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ExtradimensionalItems.Modules
{
    public static class Utils
    {
        public static GameObject FindNetworkPlayer(NetworkInstanceId netId)
        {
            GameObject bodyObject = Util.FindNetworkObject(netId);
            if (!bodyObject)
            {
                if (bodyObject == PlayerCharacterMasterController.instances[0].master.GetBody().gameObject)
                {
                    return bodyObject;
                }
            }
            else
            {
                MyLogger.LogMessage(string.Format("ChronoshiftStartMovingOnClient: Util.FindNetworkObject found nothing for id {0}, checking all existing players.", netId));
                foreach (PlayerCharacterMasterController playerCharacterMaster in PlayerCharacterMasterController.instances)
                {
                    var body = playerCharacterMaster.master.GetBody();
                    if (body.gameObject.TryGetComponent<NetworkIdentity>(out var networkIdentity))
                    {
                        if (networkIdentity.netId == netId)
                        {
                            return body.gameObject;
                        }
                    }
                }
            }

            return null;
        }
    }
}

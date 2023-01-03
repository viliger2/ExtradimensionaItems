﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
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
                MyLogger.LogMessage(string.Format("FindNetworkPlayer: Util.FindNetworkObject found nothing for id {0}, checking all existing players.", netId));
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

        // thanks KomradeSpectre
        public static void ShaderConversion(AssetBundle assets)
        {
            var assets2 = assets.LoadAllAssets<Material>();

            var materialAssets = assets.LoadAllAssets<Material>().Where(material => material.shader.name.Contains("Hopoo Games"));

            foreach (Material material in materialAssets)
            {
                var replacementShader = LegacyResourcesAPI.Load<Shader>(ExtradimensionalItemsPlugin.ShaderLookup[material.shader.name]); // TODO this might not be correct
                if (replacementShader)
                {
                    material.shader = replacementShader;
                }
            }
        }

        // thanks KomradeSpectre
        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj)
        {
            List<Renderer> AllRenderers = new List<Renderer>();

            var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length > 0) { AllRenderers.AddRange(meshRenderers); }

            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers.Length > 0) { AllRenderers.AddRange(skinnedMeshRenderers); }

            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[AllRenderers.Count];

            for (int i = 0; i < AllRenderers.Count; i++)
            {
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = AllRenderers[i] is SkinnedMeshRenderer ? AllRenderers[i].sharedMaterial : AllRenderers[i].material,
                    renderer = AllRenderers[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }
    }
}

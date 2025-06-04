using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]

namespace ExtradimensionalItems.Modules
{
    public static class Utils
    {
        public static bool RegisterNetworkSound(string eventName)
        {
            RoR2.NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<RoR2.NetworkSoundEventDef>();
            (networkSoundEventDef as ScriptableObject).name = eventName;
            networkSoundEventDef.eventName = eventName;

            return R2API.ContentAddition.AddNetworkSoundEventDef(networkSoundEventDef);
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

        //thanks Harb for "give_item" template
        [ConCommand(commandName = "give_item_ai", flags = ConVarFlags.ExecuteOnServer, helpText = "Gives the specified item to AI team. Only works with Artifact of Evolution enabled or if players are in Void Fields. Requires 2 arguments: {item} [count:1]")]
        private static void CCGiveItemAI(ConCommandArgs args)
        {
            // using Debug.Log() so it shows in the console
            if (args.Count == 0)
            {
                Debug.Log("No parameters specified. Requires 2 arguments: {item} [count:1]");
                return;
            }

            int iCount = 1;
            if (args.Count >= 2 && args[1] != "")
            {
                iCount = int.TryParse(args[1], out iCount) ? iCount : 1;
            }

            var item = GetItemFromPartial(args[0]);
            if (item != ItemIndex.None)
            {
                if (RoR2.RunArtifactManager.instance.IsArtifactEnabled(RoR2.RoR2Content.Artifacts.monsterTeamGainsItemsArtifactDef) && RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.monsterTeamInventory)
                {
                    RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.monsterTeamInventory.GiveItem(item, iCount);
                    Debug.Log(string.Format("Gave {0} {1} to AIs", iCount, item));
                } else if (RoR2.SceneCatalog.GetSceneDefForCurrentScene() == RoR2.SceneCatalog.GetSceneDefFromSceneName("arena") && ArenaMissionController.instance && RoR2.ArenaMissionController.instance.inventory)
                {
                    RoR2.ArenaMissionController.instance.inventory.GiveItem(item, iCount);
                    Debug.Log(string.Format("Gave {0} {1} to AIs", iCount, item));
                }
                else
                {
                    Debug.Log("Only works when Artifact of Evolution is enabled or if players are in Void Fields.");
                }
            }
            else
            {
                Debug.Log(string.Format("Item {0} is not found.", args[0]));
            }
        }

        [ConCommand(commandName = "ei_give_all_items", flags = ConVarFlags.ExecuteOnServer, helpText = "Gives all Extradimensional Items. Can have additional argument that specifies the ammount of stacks for each item.")]
        private static void CCGiveAllEDItems(ConCommandArgs args)
        {
            int stackCount = 1;
            if(args.Count >= 1)
            {
                stackCount = int.TryParse(args[0], out stackCount) ? stackCount : 1;
            }

            var target = args.senderMaster;
            
            if(target == null)
            {
                Debug.Log("Couldn't find target to add items to.");
            }

            var inventory = target.inventory;

            if(inventory == null)
            {
                Debug.Log("Target has no inventory.");
            }

            inventory.GiveItem(Content.Items.Atma, stackCount);
            inventory.GiveItem(Content.Items.DamageOnPing, stackCount);
            inventory.GiveItem(Content.Items.DamageOnCooldowns, stackCount);
            inventory.GiveItem(Content.Items.RoyalGuard, stackCount);
            inventory.GiveItem(Content.Items.FuelCellDepleted, stackCount);
            inventory.GiveItem(Content.Items.ReturnalAdrenaline, stackCount);
            inventory.GiveItem(Content.Items.Sheen, stackCount);
            inventory.GiveItem(Content.Items.VoidCooldownReduction, stackCount);

            Debug.Log(string.Format("Gave {0} of all Extradimensional items.", stackCount));

        }


        private static ItemIndex GetItemFromPartial(string name)
        {
            string langInvar;

            if (Enum.TryParse(name, true, out ItemIndex foundItem) && ItemCatalog.IsIndexValid(foundItem))
            {
                return foundItem;
            }

            

            foreach (var item in ItemCatalog.allItemDefs)
            {
                langInvar = GetLangInvar(item.nameToken.ToUpper());
                if (item.name.ToUpper().Contains(name.ToUpper()) || langInvar.ToUpper().Contains(name.ToUpper()) || langInvar.ToUpper().Contains(RemoveSpacesAndAlike(name.ToUpper())))
                {
                    return item.itemIndex;
                }
            }
            return ItemIndex.None;
        }

        private static string GetLangInvar(string baseToken)
        {
            return RemoveSpacesAndAlike(Language.GetString(baseToken));
        }

        private static string RemoveSpacesAndAlike(string input)
        {
            return Regex.Replace(input, @"[ '-]", string.Empty);
        }
    }
}

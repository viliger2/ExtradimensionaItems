using BepInEx.Configuration;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Items
{

    // The directly below is entirely from TILER2 API (by ThinkInvis) specifically the Item module. Utilized to implement instancing for classes.
    // TILER2 API can be found at the following places:
    // https://github.com/ThinkInvis/RoR2-TILER2
    // https://thunderstore.io/package/ThinkInvis/TILER2/

    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        //This, which you will see on all the -base classes, will allow both you and other modders to enter through any class with this to access internal fields/properties/etc as if they were a member inheriting this -Base too from this class.
        public static T instance { get; private set; }

        public ItemBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; set; } = new ItemTag[] { };
        public abstract string BundleName { get; }
        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }
        public ItemDef ItemDef;
        public AssetBundle AssetBundle;
        public virtual bool CanRemove { get; } = true;
        public virtual bool AIBlacklisted { get; } = false;
        public virtual ExpansionDef Expansion { get; } = null;

        protected List<LanguageAPI.LanguageOverlay> overlayList = new List<LanguageAPI.LanguageOverlay>();

        /// <summary>
        /// This method structures your code execution of this class. An example implementation inside of it would be:
        /// <para>CreateConfig(config);</para>
        /// <para>CreateLang();</para>
        /// <para>CreateItem();</para>
        /// <para>Hooks();</para>
        /// <para>This ensures that these execute in this order, one after another, and is useful for having things available to be used in later methods.</para>
        /// <para>P.S. CreateItemDisplayRules(); does not have to be called in this, as it already gets called in CreateItem();</para>
        /// </summary>
        /// <param name="config">The config file that will be passed into this from the main class.</param>
        public abstract void Init(ConfigFile config);

        public virtual void CreateConfig(ConfigFile config) { }

        protected virtual void LoadAssetBundle()
        {
            AssetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExtradimensionalItemsPlugin.PInfo.Location), ExtradimensionalItemsPlugin.BundleFolder, BundleName));
            Utils.ShaderConversion(AssetBundle);
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateItem(ref ItemDef staticItemDef)
        {
            if (AIBlacklisted)
            {
                ItemTags = new List<ItemTag>(ItemTags) { ItemTag.AIBlacklist }.ToArray();
            }

            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = ItemName;
            ItemDef.nameToken = "ITEM_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_" + ItemLangTokenName + "_LORE";
            ItemDef.pickupModelPrefab = ItemModel;
            ItemDef.pickupIconSprite = ItemIcon;
            ItemDef.hidden = false;
            ItemDef.canRemove = CanRemove;
            ItemDef.tier = Tier;
            ItemDef.deprecatedTier = Tier;
            ItemDef.requiredExpansion = Expansion;

            if (ItemTags.Length > 0) { ItemDef.tags = ItemTags; }

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));

            staticItemDef = ItemDef;
        }

        protected virtual void Hooks() { }

        protected virtual void LoadSoundBank()
        {
            SoundAPI.SoundBanks.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExtradimensionalItemsPlugin.PInfo.Location), ExtradimensionalItemsPlugin.SoundBanksFolder, string.Concat(BundleName, ".bnk")));
        }

        protected void LoadLanguageFile()
        {
            string jsonText = File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExtradimensionalItemsPlugin.PInfo.Location), ExtradimensionalItemsPlugin.LanguageFolder, $"{BundleName}.json"));

            JSONNode languageNode = JSON.Parse(jsonText);
            if (languageNode == null)
            {
                return;
            }

            foreach(string languageKey in languageNode.Keys)
            {
                JSONNode tokensNode = languageNode[languageKey];
                foreach (string key in tokensNode.Keys)
                {
                    LoadDescription(key, tokensNode[key], languageKey == "strings" ? "generic" : languageKey, tokensNode);
                }
            }
        }
        protected virtual void SetLogbookCameraPosition()
        {
            var modelParameters = ItemModel.AddComponent<ModelPanelParameters>();

            modelParameters.focusPointTransform = ItemModel.transform.Find("FocusPoint");
            modelParameters.cameraPositionTransform = ItemModel.transform.Find("CameraPosition");
            modelParameters.modelRotation = new Quaternion(0f, 0f, 0f, 1f);

            modelParameters.minDistance = 1;
            modelParameters.maxDistance = 3;
        }

        protected virtual void LoadDescription(string key, string value, string languageKey, JSONNode tokensNode)
        {
            if (key.Contains("DESCRIPTION"))
            {
                LanguageAPI.Add(key, GetOverlayDescription(tokensNode[key], tokensNode), languageKey);
            }
            else
            {
                LanguageAPI.Add(key, tokensNode[key], languageKey);
            }
        }

        protected virtual void OnModOptionsExit()
        {
            foreach (var overlay in overlayList)
            {
                overlay.Remove();
            }

            overlayList.Clear();

            string jsonText = File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExtradimensionalItemsPlugin.PInfo.Location), ExtradimensionalItemsPlugin.LanguageFolder, $"{BundleName}.json"));

            JSONNode languageNode = JSON.Parse(jsonText);
            if (languageNode == null)
            {
                return;
            }

            foreach (string languageKey in languageNode.Keys)
            {
                JSONNode tokensNode = languageNode[languageKey];
                overlayList.Add(
                    LanguageAPI.AddOverlay(
                        "ITEM_" + ItemLangTokenName + "_DESCRIPTION",
                        GetOverlayDescription(tokensNode["ITEM_" + ItemLangTokenName + "_DESCRIPTION"].Value, tokensNode),
                        languageKey == "strings" ? "generic" : languageKey)); 
            }
        }

        public abstract string GetOverlayDescription(string value, JSONNode tokensNode);

        //Based on ThinkInvis' methods
        public int GetCount(CharacterBody body)
        {
            return body?.inventory?.GetItemCount(ItemDef) ?? 0;
        }

        public int GetCount(CharacterMaster master)
        {
            return master?.inventory?.GetItemCount(ItemDef) ?? 0;
        }
    }
}

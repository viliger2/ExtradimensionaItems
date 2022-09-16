using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
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

        public const string BundleFolder = "Assets";

        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }

        public ItemDef ItemDef;

        public AssetBundle AssetBundle;

        public virtual bool CanRemove { get; } = true;

        public virtual bool AIBlacklisted { get; } = false;

        public virtual ExpansionDef Expansion { get; } = null;

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

        protected void LoadAssetBundle()
        {
            AssetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExtradimensionalItemsPlugin.PInfo.Location), BundleFolder, BundleName));
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

        protected virtual void Hooks()
        {
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        }

        private string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
        {
            if (token.Equals($"ITEM_{ItemLangTokenName}_DESCRIPTION"))
            {
                LanguageAPI.AddOverlay(token, GetFormatedDiscription(orig(self, token)), self.name);
                On.RoR2.Language.GetLocalizedStringByToken -= Language_GetLocalizedStringByToken;
            }
            return orig(self, token);
        }


        public abstract string GetFormatedDiscription(string pickupString);

        //Based on ThinkInvis' methods
        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(ItemDef);
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(ItemDef);
        }

        public int GetCountSpecific(CharacterBody body, ItemDef itemDef)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(itemDef);
        }
    }
}

﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Equipment
{
    public abstract class EquipmentBase<T> : EquipmentBase where T : EquipmentBase<T>
    {
        public static T instance { get; private set; }

        public EquipmentBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting EquipmentBoilerplate/Equipment was instantiated twice");
            instance = this as T;
        }
    }
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }

        public abstract string EquipmentLangTokenName { get; }

        public abstract GameObject EquipmentModel { get; }
        
        public abstract Sprite EquipmentIcon { get; }

        public abstract string BundleName { get; }

        public const string BundleFolder = "Assets";

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;

        public virtual bool CanDrop { get; } = true;

        public virtual float Cooldown { get; } = 60f;

        public virtual bool EnigmaCompatible { get; } = true;

        public virtual bool IsBoss { get; } = false;

        public virtual bool IsLunar { get; } = false;

        public EquipmentDef EquipmentDef;

        public AssetBundle AssetBundle;

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        /// <summary>
        /// This method structures your code execution of this class. An example implementation inside of it would be:
        /// <para>CreateConfig(config);</para>
        /// <para>CreateLang();</para>
        /// <para>CreateEquipment();</para>
        /// <para>Hooks();</para>
        /// <para>This ensures that these execute in this order, one after another, and is useful for having things available to be used in later methods.</para>
        /// <para>P.S. CreateItemDisplayRules(); does not have to be called in this, as it already gets called in CreateEquipment();</para>
        /// </summary>
        /// <param name="config">The config file that will be passed into this from the main class.</param>
        public abstract void Init(ConfigFile config);

        protected void LoadAssetBundle()
        {
            AssetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExtradimensionalItemsPlugin.PInfo.Location), BundleFolder, BundleName));
        }

        protected virtual void CreateConfig(ConfigFile config){}

        protected void CreateEquipment(ref EquipmentDef staticEquipmentDef)
        {
            EquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            EquipmentDef.name = EquipmentName;
            EquipmentDef.nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME";
            EquipmentDef.pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP";
            EquipmentDef.descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION";
            EquipmentDef.loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE";
            EquipmentDef.pickupModelPrefab = EquipmentModel;
            EquipmentDef.pickupIconSprite = EquipmentIcon;
            EquipmentDef.appearsInSinglePlayer = AppearsInSinglePlayer;
            EquipmentDef.appearsInMultiPlayer = AppearsInMultiPlayer;
            EquipmentDef.canDrop = CanDrop;
            EquipmentDef.cooldown = Cooldown;
            EquipmentDef.enigmaCompatible = EnigmaCompatible;
            EquipmentDef.isBoss = IsBoss;
            EquipmentDef.isLunar = IsLunar;

            ItemAPI.Add(new CustomEquipment(EquipmentDef, CreateItemDisplayRules()));
            staticEquipmentDef = EquipmentDef;

            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }

        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EquipmentDef)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        protected abstract bool ActivateEquipment(EquipmentSlot slot);

        protected virtual void Hooks() {
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        }

        private string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
        {
            if (token.Equals($"EQUIPMENT_{EquipmentLangTokenName}_DESCRIPTION"))
            {
                return GetFormatedDiscription(orig(self, token));
            }
            return orig(self, token);
        }

        public abstract string GetFormatedDiscription(string pickupString);

        //#region Targeting Setup
        ////Targeting Support
        //public virtual bool UseTargeting { get; } = false;
        //public GameObject TargetingIndicatorPrefabBase = null;
        //public enum TargetingType
        //{
        //    Enemies,
        //    Friendlies,
        //}
        //public virtual TargetingType TargetingTypeEnum { get; } = TargetingType.Enemies;

        ////Based on MysticItem's targeting code.
        //protected void UpdateTargeting(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
        //{
        //    orig(self);

        //    if (self.equipmentIndex == EquipmentDef.equipmentIndex)
        //    {
        //        var targetingComponent = self.GetComponent<TargetingControllerComponent>();
        //        if (!targetingComponent)
        //        {
        //            targetingComponent = self.gameObject.AddComponent<TargetingControllerComponent>();
        //            targetingComponent.VisualizerPrefab = TargetingIndicatorPrefabBase;
        //        }

        //        if (self.stock > 0)
        //        {
        //            switch (TargetingTypeEnum)
        //            {
        //                case (TargetingType.Enemies):
        //                    targetingComponent.ConfigureTargetFinderForEnemies(self);
        //                    break;
        //                case (TargetingType.Friendlies):
        //                    targetingComponent.ConfigureTargetFinderForFriendlies(self);
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            targetingComponent.Invalidate();
        //            targetingComponent.Indicator.active = false;
        //        }
        //    }
        //}

        //public class TargetingControllerComponent : MonoBehaviour
        //{
        //    public GameObject TargetObject;
        //    public GameObject VisualizerPrefab;
        //    public Indicator Indicator;
        //    public BullseyeSearch TargetFinder;
        //    public Action<BullseyeSearch> AdditionalBullseyeFunctionality = (search) => { };

        //    public void Awake()
        //    {
        //        Indicator = new Indicator(gameObject, null);
        //    }

        //    public void OnDestroy()
        //    {
        //        Invalidate();
        //    }

        //    public void Invalidate()
        //    {
        //        TargetObject = null;
        //        Indicator.targetTransform = null;
        //    }

        //    public void ConfigureTargetFinderBase(EquipmentSlot self)
        //    {
        //        if (TargetFinder == null) TargetFinder = new BullseyeSearch();
        //        TargetFinder.teamMaskFilter = TeamMask.allButNeutral;
        //        TargetFinder.teamMaskFilter.RemoveTeam(self.characterBody.teamComponent.teamIndex);
        //        TargetFinder.sortMode = BullseyeSearch.SortMode.Angle;
        //        TargetFinder.filterByLoS = true;
        //        float num;
        //        Ray ray = CameraRigController.ModifyAimRayIfApplicable(self.GetAimRay(), self.gameObject, out num);
        //        TargetFinder.searchOrigin = ray.origin;
        //        TargetFinder.searchDirection = ray.direction;
        //        TargetFinder.maxAngleFilter = 10f;
        //        TargetFinder.viewer = self.characterBody;
        //    }

        //    public void ConfigureTargetFinderForEnemies(EquipmentSlot self)
        //    {
        //        ConfigureTargetFinderBase(self);
        //        TargetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(self.characterBody.teamComponent.teamIndex);
        //        TargetFinder.RefreshCandidates();
        //        TargetFinder.FilterOutGameObject(self.gameObject);
        //        AdditionalBullseyeFunctionality(TargetFinder);
        //        PlaceTargetingIndicator(TargetFinder.GetResults());
        //    }

        //    public void ConfigureTargetFinderForFriendlies(EquipmentSlot self)
        //    {
        //        ConfigureTargetFinderBase(self);
        //        TargetFinder.teamMaskFilter = TeamMask.none;
        //        TargetFinder.teamMaskFilter.AddTeam(self.characterBody.teamComponent.teamIndex);
        //        TargetFinder.RefreshCandidates();
        //        TargetFinder.FilterOutGameObject(self.gameObject);
        //        AdditionalBullseyeFunctionality(TargetFinder);
        //        PlaceTargetingIndicator(TargetFinder.GetResults());

        //    }

        //    public void PlaceTargetingIndicator(IEnumerable<HurtBox> TargetFinderResults)
        //    {
        //        HurtBox hurtbox = TargetFinderResults.Any() ? TargetFinderResults.First() : null;

        //        if (hurtbox)
        //        {
        //            TargetObject = hurtbox.healthComponent.gameObject;
        //            Indicator.visualizerPrefab = VisualizerPrefab;
        //            Indicator.targetTransform = hurtbox.transform;
        //        }
        //        else
        //        {
        //            Invalidate();
        //        }
        //        Indicator.active = hurtbox;
        //    }
        //}

        //#endregion Targeting Setup
    }
}

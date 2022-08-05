using R2API;
using RoR2;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Buffs
{
    public static class RoyalGuardBuffs
    {
        public static BuffDef RoyalGuardParryStateBuff;

        public static BuffDef RoyalGuardDamageBuff;

        public static BuffDef RoyalGuardGraceBuff;

        public static void CreateBuffs(AssetBundle assetBundle)
        {
            RoyalGuardParryStateBuff = ScriptableObject.CreateInstance<BuffDef>();
            RoyalGuardParryStateBuff.name = "Royal Guard Parry State";
            RoyalGuardParryStateBuff.buffColor = Color.red;
            RoyalGuardParryStateBuff.canStack = false;
            RoyalGuardParryStateBuff.isDebuff = false;
            RoyalGuardParryStateBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png");

            ContentAddition.AddBuffDef(RoyalGuardParryStateBuff);

            RoyalGuardDamageBuff = ScriptableObject.CreateInstance<BuffDef>();
            RoyalGuardDamageBuff.name = "Royal Guard Damage Buff";
            RoyalGuardDamageBuff.buffColor = Color.magenta;
            RoyalGuardDamageBuff.canStack = true;
            RoyalGuardDamageBuff.isDebuff = false;
            RoyalGuardDamageBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png");

            ContentAddition.AddBuffDef(RoyalGuardDamageBuff);

            RoyalGuardGraceBuff = ScriptableObject.CreateInstance<BuffDef>();
            RoyalGuardGraceBuff.name = "Royal Guard Grace State";
            RoyalGuardGraceBuff.buffColor = Color.green;
            RoyalGuardGraceBuff.canStack = false;
            RoyalGuardGraceBuff.isDebuff = false;
            RoyalGuardGraceBuff.isHidden = true;
            RoyalGuardGraceBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png");

            ContentAddition.AddBuffDef(RoyalGuardGraceBuff);
        }



    }
}

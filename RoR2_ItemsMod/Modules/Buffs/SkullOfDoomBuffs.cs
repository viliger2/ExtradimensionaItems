using R2API;
using RoR2;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Buffs
{
    public static class SkullOfDoomBuffs
    {
        public static BuffDef SkullOfDoomBuff;

        public static void CreateBuffs(AssetBundle assetBundle)
        {
            SkullOfDoomBuff = ScriptableObject.CreateInstance<BuffDef>();
            SkullOfDoomBuff.name = "Skull of Impending Doom";
            SkullOfDoomBuff.buffColor = Color.yellow;
            SkullOfDoomBuff.canStack = false;
            SkullOfDoomBuff.isDebuff = false;
            SkullOfDoomBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png"); // TODO: replace

            ContentAddition.AddBuffDef(SkullOfDoomBuff);
        }



    }
}

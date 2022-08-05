using R2API;
using RoR2;
using UnityEngine;

namespace ExtradimensionalItems.Modules.Buffs
{
    public static class SheenBuffs
    {
        public static BuffDef SheenBuff;

        public static void CreateBuffs(AssetBundle assetBundle, bool canStack)
        {
            SheenBuff = ScriptableObject.CreateInstance<BuffDef>();
            SheenBuff.name = "Sheen Damage Bonus";
            SheenBuff.buffColor = Color.blue;
            SheenBuff.canStack = canStack;
            SheenBuff.isDebuff = false;
            SheenBuff.iconSprite = assetBundle.LoadAsset<Sprite>("FlagItemIcon.png"); // TODO: replace

            ContentAddition.AddBuffDef(SheenBuff);
        }

    }
}

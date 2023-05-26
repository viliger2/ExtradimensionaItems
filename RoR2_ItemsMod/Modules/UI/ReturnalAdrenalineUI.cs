using ExtradimensionalItems.Modules.Items.ItemBehaviors;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ExtradimensionalItems.Modules.UI
{
    // this is terrible, I am sorry
    public class ReturnalAdrenalineUI : MonoBehaviour
    {
        public static List<ReturnalAdrenalineUI> instancesList = new List<ReturnalAdrenalineUI>();

        //public static ReturnalAdrenalineUI instance;

        private HGTextMeshProUGUI textMesh;

        private Image levelBar;

        public RoR2.UI.HUD hud { get; private set; }

        private void Update()
        {
            //MyLogger.LogMessage("Hud belongs to: " + hud.targetMaster.netId);
            if (hud && hud.targetMaster)
            {
                var itemBehavior = hud.targetMaster.GetComponent<ReturnalAdrenalineItemBehavior>();
                if (itemBehavior && hud.targetMaster.inventory.GetItemCount(Content.Items.ReturnalAdrenaline) > 0)
                {
                    UpdateUI(itemBehavior.adrenalineLevel, itemBehavior.adrenalinePerLevel);
                }
            }
        }

        private void OnDestroy()
        {
            if (instancesList != null)
            {
                instancesList.Remove(this);
            }
        }

        public void UpdateUI(int adrenalineLevel, float adrenalinePerLevel)
        {
            if (textMesh)
            {
                textMesh.SetText(string.Format("Lv. {0}", (int)(adrenalineLevel / adrenalinePerLevel)));
            }
            if (levelBar)
            {
                if (adrenalineLevel >= adrenalinePerLevel * 5)
                {
                    levelBar.fillAmount = 1f;
                }
                else
                {
                    levelBar.fillAmount = Mathf.Clamp((float)adrenalineLevel % adrenalinePerLevel / adrenalinePerLevel, 0f, 1f);
                }

            }
        }

        public static void CreateUI(RoR2.UI.HUD HUD)
        {
            var AdrenalineHUD = new GameObject("AdrenalineHUD");

            var instance = AdrenalineHUD.AddComponent<ReturnalAdrenalineUI>();

            RectTransform rectTransform = AdrenalineHUD.AddComponent<RectTransform>();

            AdrenalineHUD.transform.SetParent(HUD.healthBar.transform);

            Image image = AdrenalineHUD.AddComponent<Image>();
            Image copyImage = HUD.itemInventoryDisplay.gameObject.GetComponent<Image>();

            image.sprite = copyImage.sprite;
            image.color = copyImage.color;
            image.type = Image.Type.Sliced;

            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.localScale = Vector3.one;
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(421, 15);
            rectTransform.anchoredPosition = new Vector2(-1, -5);
            rectTransform.eulerAngles = new Vector3(0, -6, 0);

            GameObject AdrenalineLevelText = new GameObject("AdrenalineLevelText");
            RectTransform rectTransform1 = AdrenalineLevelText.AddComponent<RectTransform>();

            var textMesh = AdrenalineLevelText.AddComponent<HGTextMeshProUGUI>();

            AdrenalineLevelText.transform.SetParent(AdrenalineHUD.transform);

            rectTransform1.localPosition = Vector3.zero;
            rectTransform1.anchorMin = Vector2.zero;
            rectTransform1.anchorMax = Vector2.one;
            rectTransform1.localScale = Vector3.one;
            rectTransform1.sizeDelta = new Vector2(0, 0);
            rectTransform1.anchoredPosition = new Vector2(380, 0);
            rectTransform1.eulerAngles = new Vector3(0, -6, 0);

            textMesh.enableAutoSizing = true;
            textMesh.fontSizeMax = 256;
            textMesh.faceColor = Color.white;
            textMesh.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            textMesh.text = "Testing";

            GameObject AdrenalineLevelBar = new GameObject("AdrenalineLevelBar");
            RectTransform rectTransform2 = AdrenalineLevelBar.AddComponent<RectTransform>();

            AdrenalineLevelBar.transform.SetParent(AdrenalineHUD.transform);

            // I don't know how I did it but it is now attached to the top left corner of the parent
            rectTransform2.anchorMin = new Vector2(0, 1);
            rectTransform2.anchorMax = new Vector2(0, 1);
            rectTransform2.pivot = new Vector2(0, 1);
            rectTransform2.eulerAngles = new Vector3(0, -6, 0);
            rectTransform2.localScale = Vector3.one;
            rectTransform2.anchoredPosition = new Vector2(12, 2);
            rectTransform2.sizeDelta = new Vector2(360, 8);

            var levelBar = AdrenalineLevelBar.AddComponent<Image>();

            // you need to have a sprite if you want "Fill" to work
            levelBar.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUINonsegmentedHealthbar.png").WaitForCompletion();
            levelBar.color = Color.yellow;
            levelBar.type = Image.Type.Filled;
            levelBar.fillMethod = Image.FillMethod.Horizontal;
            levelBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            levelBar.fillAmount = 0.5f;

            instance.hud = HUD;
            instance.textMesh = textMesh;
            instance.levelBar = levelBar;

            AdrenalineHUD.gameObject.SetActive(false);

            instancesList.Add(instance);
        }

        public static ReturnalAdrenalineUI FindInstance(CharacterMaster master)
        {
            foreach (ReturnalAdrenalineUI instance in instancesList)
            {
                if (instance.hud.targetMaster == master) return instance;
            }
            return null;
        }

        public void Enable()
        {
            if (gameObject)
            {
                gameObject.SetActive(true);
            }
        }

        public void Disable()
        {
            if (gameObject)
            {
                gameObject.SetActive(false);
            }
        }

    }
}

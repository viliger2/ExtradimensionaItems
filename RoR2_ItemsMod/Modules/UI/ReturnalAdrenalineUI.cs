using ExtradimensionalItems.Modules.Items;
using ExtradimensionalItems.Modules.Items.ItemBehaviors;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static ExtradimensionalItems.Modules.Items.ReturnalAdrenalin;

namespace ExtradimensionalItems.Modules.UI
{
    // this is terrible, I am sorry
    public class ReturnalAdrenalineUI : MonoBehaviour
    {
        public static ReturnalAdrenalineUI instance;

        //private static ReturnalAdrenalineUI instance;

        //private static GameObject AdrenalineHUD;

        private HGTextMeshProUGUI textMesh;

        private Image levelBar;

        public RoR2.UI.HUD hud;

        private ReturnalAdrenalinItemBehavior itemBehavior;

        private void Update()
        {
            if (hud && hud.targetMaster)
            {
                if (!itemBehavior)
                {
                    itemBehavior = hud.targetMaster.GetComponent<ReturnalAdrenalinItemBehavior>();
                }
                if (itemBehavior && hud.targetMaster.inventory.GetItemCount(Content.Items.ReturnalAdrenalin) > 0)
                {
                    UpdateUI(itemBehavior.adrenalineLevel);
                } else
                {
                    Disable();
                }
            }
        }

        public void UpdateUI(int adrenalineLevel)
        {
            if (textMesh)
            {
                textMesh.SetText(string.Format("Lv. {0}", adrenalineLevel / ReturnalAdrenalinItemBehavior.adrenalinePerLevel));
            }
            if (levelBar)
            {
                if (adrenalineLevel >= ReturnalAdrenalinItemBehavior.adrenalinePerLevel * 5)
                {
                    levelBar.fillAmount = 1f;
                } else 
                {
                    levelBar.fillAmount = Mathf.Clamp((float)adrenalineLevel % ReturnalAdrenalinItemBehavior.adrenalinePerLevel / ReturnalAdrenalinItemBehavior.adrenalinePerLevel, 0f, 1f);
                }

            }
        }

        public static ReturnalAdrenalineUI CreateUI(RoR2.UI.HUD HUD)
        {
            if (!instance)
            {
                //if (!instance)
                //{
                var AdrenalineHUD = new GameObject("AdrenalineHUD");

                instance = AdrenalineHUD.AddComponent<ReturnalAdrenalineUI>();

                RectTransform rectTransform = AdrenalineHUD.AddComponent<RectTransform>();

                //AdrenalineHUD.transform.SetParent(HUD.mainContainer.transform);
                AdrenalineHUD.transform.SetParent(HUD.healthBar.transform);
                //AdrenalineHUD.transform.SetAsFirstSibling();

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

                //AdrenalineHUD.transform.SetParent(HUD.mainContainer.transform);

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

                //var body = LocalUserManager.GetFirstLocalUser().cachedMasterController.master.GetBody();
                //AdrenalineHUD.gameObject.SetActive(body ? body.inventory.GetItemCount(Content.Items.ReturnalAdrenalin) > 0 : false);

                //var component = LocalUserManager.GetFirstLocalUser().cachedMasterController.master.GetComponent<ReturnalAdrenalinItemBehavior>();

                instance.hud = HUD;
                instance.textMesh = textMesh;
                instance.levelBar = levelBar;

                //instance.UpdateUI(component ? component.adrenalineLevel : 0);
                AdrenalineHUD.gameObject.SetActive(true);
            }

            return instance;
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

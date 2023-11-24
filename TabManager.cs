using BepInEx;
using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.EventSystems;
using UnityEngine.UI;
// using ReflectionUtility;

namespace FamilyTreeMod
{
    class TabManager : MonoBehaviour
    {

        public static TabManager instance;
        public static PowersTab familyTreeTab;
        public static GameObject familyTreeButtons;

        public static void init()
        {
            UI.createTab("Button Tab_FamilyTreeMod", "Tab_FamilyTreeMod", "FamilyTreeMod", "This tab is for family features", -200);

            familyTreeTab = getPowersTab("FamilyTreeMod");
            familyTreeButtons = new GameObject("DejButtons");
            Image familyTreeButtonsImg = familyTreeButtons.AddComponent<Image>();
            familyTreeButtonsImg.color = new Color(1f, 1f, 1f, 0f);
            familyTreeButtons.transform.SetParent(familyTreeTab.transform);
            familyTreeButtons.GetComponent<RectTransform>().localPosition = new Vector3(550, -15, 0);
            familyTreeButtons.GetComponent<RectTransform>().sizeDelta = new Vector3(950, 100, 0);
            GridLayoutGroup layoutGroup = familyTreeButtons.AddComponent<GridLayoutGroup>();
            layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            layoutGroup.cellSize = new Vector2(30, 30);
            layoutGroup.spacing = new Vector2(5, 0);

            loadButtons();

            GameObject instanceObj = new GameObject();
            instance = instanceObj.AddComponent<TabManager>();
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(3f);
            startButtons();
            yield return null;
        }

        public void startButtons()
        {
            StartCoroutine("buttonsCoroutine");
        }

        public void stopButtons()
        {
            StopCoroutine("buttonsCoroutine");
        }

        IEnumerator buttonsCoroutine()
        {
            while (true)
            {
                while (!Plugin.settings.families.ContainsKey(SaveManager.currentSavePath))
                {
                    yield return new WaitForSeconds(1f);
                }
                foreach(KeyValuePair<string, PowerButton> kv in UI.CustomButtons)
                {
                    if (kv.Key.Contains("FamilyTreeButton"))
                    {
                        Destroy(kv.Value.gameObject);
                    }
                }
                foreach(FamilyInfo info in Plugin.settings.families[SaveManager.currentSavePath])
                {
                    UI.CreateButton($"FamilyTreeButton{info.familyIndex}",
                        AssetLoader.cached_assets_list["FamilyTreeUI/icon.png"][0],
                        $"Family Tree {info.familyIndex}", 
                        $"Show Family {info.familyName}", 
                        new Vector2(0, 0),
                        ButtonType.Click,
                        familyTreeButtons.transform, 
                        () => FamilyTreeWindow.openWindow(info.familyIndex)
                    );
                    yield return new WaitForSeconds(0.5f);
                    while (!Plugin.settings.families.ContainsKey(SaveManager.currentSavePath))
                    {
                        yield return new WaitForSeconds(1.2f);
                    }
                }

                yield return new WaitForSeconds(5f);
            }
        }

        private static void loadButtons()
        {
            UI.CreateButton("inspectMember_Dej",
                AssetLoader.cached_assets_list["FamilyTreeUI/iconWorldInfo.png"][0],
                "Inspect Family Member", 
                "Show Member's Info", 
                new Vector2(0, 0),
                ButtonType.GodPower,
                familyTreeButtons.transform, 
                null
            );

            UI.CreateButton("createFamily_Dej",
                AssetLoader.cached_assets_list["FamilyTreeUI/familyOverview_icon.png"][0],
                "Create A New Family", 
                "Select A Unit With This Power To Create A Family", 
                new Vector2(0, 0),
                ButtonType.GodPower,
                familyTreeButtons.transform, 
                null
            );

            // UI.CreateButton("FamilyTreeButton",
            //     AssetLoader.cached_assets_list["FamilyTreeUI/icon.png"][0],
            //     "Family Tree", 
            //     "Show Families", 
            //     new Vector2(0, 0),
            //     ButtonType.Click,
            //     familyTreeButtons.transform, 
            //     () => FamilyUnitTreeWindow.openWindow(0)
            // );

            // UI.CreateButton("refreshTab_Dej",
            //     AssetLoader.cached_assets_list["FamilyTreeUI/icon.png"][0],
            //     "Refresh Tab", 
            //     "If family tree buttons are not being added in tab press this to refresh", 
            //     new Vector2(0, 0),
            //     ButtonType.Click,
            //     familyTreeButtons.transform, 
            //     delegate{
            //         instance.stopButtons();
            //         instance.startButtons();
            //         }
            // );
            return;
        }

        private static PowersTab getPowersTab(string id)
		{
			GameObject gameObject = Utils.FindInActiveObjectByName("Tab_" + id);
			return gameObject.GetComponent<PowersTab>();
		}
    }
}
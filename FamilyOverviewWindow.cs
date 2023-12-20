using BepInEx;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FamilyTreeMod
{
    public class FamilyOverviewWindow : MonoBehaviour
    {
        public static int currentFamilyIndex = 0;
        private static GameObject contents;
        private static Vector3 originalSize = new Vector3(0, 207);
        public static void init()
        {
            contents = WindowManager.windowContents["familyOverviewWindow"];

            UI.createBGWindowButton(
                GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyOverviewWindow/Background"), 
                0, 
                AssetLoader.cached_assets_list["iconDownvote.png"][0],//string iconName, 
                "familyTreeButton",//string buttonName, 
                "View All Family Members",//string buttonTitle, 
                "See Every Member Of The Family Either Dead Or Alive",//string buttonDesc, 
                () => FamilyTreeWindow.openWindow(currentFamilyIndex)
            );
        }

        public static void openWindow(int pFamilyIndex)
        {
            currentFamilyIndex = pFamilyIndex;
            foreach(Transform child in contents.transform)
            {
                Destroy(child.gameObject);
            }
            contents.GetComponent<RectTransform>().sizeDelta = originalSize;

            loadUI();
            UI.ShowWindow("familyOverviewWindow");
        }

        private static void loadUI()
        {
            FamilyInfo info = Plugin.settings.families[SaveManager.currentSavePath][currentFamilyIndex];
            Text mainText = UI.addText(
                $"Family Name: {info.familyName}\nFamily Index: {info.familyIndex}\nTotal Members: {info.memberIndex}\nAlive Members: {info.memberIndex-info.deadMembers.Count}",
                contents,
                20,
                new Vector3(100, -30, 0),
                new Vector2(100, 100)
            );
            mainText.alignment = TextAnchor.UpperLeft;
        }
    }
}
using BepInEx;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FamilyTreeMod
{
    public class FamilyTreeWindow : MonoBehaviour
    {
        private static int currentFamilyIndex = 0;
        private static GameObject contents;
        private static Vector3 originalSize = new Vector3(100, 207);
        private static int currentUIIndex = 0;

        public static void init()
        {
            contents = WindowManager.windowContents["familyTreeWindow"];
            contents.GetComponent<RectTransform>().sizeDelta = originalSize;

            GameObject scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyTreeWindow/Background/Scroll View");
            scrollView.GetComponent<ScrollRect>().horizontal = true;

            GameObject viewPort = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyTreeWindow/Background/Scroll View/Viewport");
            viewPort.GetComponent<RectTransform>().localPosition = new Vector3(-100f, 107.6f, 0f);
            viewPort.GetComponent<RectTransform>().sizeDelta = new Vector2(-60f, 0);

            GridLayoutGroup layoutGroup = contents.AddComponent<GridLayoutGroup>();
            layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            layoutGroup.cellSize = new Vector2(40, 40);
            layoutGroup.spacing = new Vector2(10, 10);
            layoutGroup.padding = new RectOffset(30, 10, 10, 0);

            UI.createBGWindowButton(
                GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyTreeWindow/Background"), 
                0, 
                AssetLoader.cached_assets_list["iconDownvote.png"][0],//string iconName, 
                "downButton2",//string buttonName, 
                "Increase Scroll Size",//string buttonTitle, 
                "Press This To Increase Scroll Space",//string buttonDesc, 
                increaseScrollSize
            );
            UI.createBGWindowButton(
                GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyTreeWindow/Background"), 
                -100, 
                AssetLoader.cached_assets_list["iconDownvote.png"][0],//string iconName, 
                "familyOverviewButton",//string buttonName, 
                "View Family Overview",//string buttonTitle, 
                "Go To Family Overview Window",//string buttonDesc, 
                () => FamilyOverviewWindow.openWindow(currentFamilyIndex)
            );
        }

        public static void openWindow(int familyIndex)
        {
            currentFamilyIndex = familyIndex;
            currentUIIndex = 0;

            foreach(Transform child in contents.transform)
            {
                Destroy(child.gameObject);
            }
            contents.GetComponent<RectTransform>().sizeDelta = originalSize;

            FamilyInfo info = Plugin.settings.families[SaveManager.currentSavePath][currentFamilyIndex];
            
            for(int i = 1; i < /*info.memberIndex*/26; i++)
            {
                if (info.memberIndex >= i)
                {
                    loadMemberUI(i);
                    currentUIIndex = i;
                }
            }
            UI.ShowWindow("familyTreeWindow");
        }

        private static void loadMemberUI(int memberIndex)
        {
            DeadFamilyMember dead = null;
            Actor actor = Utils.findActorByMemberIndex(memberIndex, ref dead, currentFamilyIndex);
            UI.createActorUI(
                actor,
                contents,
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1),
                dead
            );
        }

        private static void increaseScrollSize()
        {
            FamilyInfo info = Plugin.settings.families[SaveManager.currentSavePath][currentFamilyIndex];
            int count = currentUIIndex+11;
            for(int i = currentUIIndex+1; i < count; i++)
            {
                if (info.memberIndex >= i)
                {
                    loadMemberUI(i);
                    currentUIIndex = i;
                }
            }
            contents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 100);
            foreach(Transform child in contents.transform)
            {
                child.localPosition += new Vector3(0, 50, 0);
            }
        }
    }
}
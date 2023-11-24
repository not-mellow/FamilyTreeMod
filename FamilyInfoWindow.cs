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
    public class FamilyInfoWindow : MonoBehaviour
    {
        private static int currentFamilyIndex = 0;
        private static GameObject contents;
        private static Vector3 originalSize = new Vector3(0, 207);

        public static void init()
        {
            contents = WindowManager.windowContents["familyInfoWindow"];
            contents.GetComponent<RectTransform>().sizeDelta = originalSize;

            UI.createBGWindowButton(
                GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyInfoWindow/Background"), 
                0, 
                AssetLoader.cached_assets_list["FamilyTreeUI/iconDownvote.png"][0],//string iconName, 
                "downButton2",//string buttonName, 
                "Increase Scroll Size",//string buttonTitle, 
                "Press This To Increase Scroll Space",//string buttonDesc, 
                increaseScrollSize
            );
        }

        public static void openWindow(int familyIndex)
        {
        }

        private static void increaseScrollSize()
        {
            contents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 100);
            foreach(Transform child in contents.transform)
            {
                child.localPosition += new Vector3(0, 50, 0);
            }
        }
    }
}
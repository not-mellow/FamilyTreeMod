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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FamilyTreeMod
{
    class WindowManager : MonoBehaviour
    {
        public static Dictionary<string, GameObject> windowContents = new Dictionary<string, GameObject>();
        public static Dictionary<string, ScrollWindow> createdWindows = new Dictionary<string, ScrollWindow>();

        public static void init()
        {
            newWindow("familyTreeWindow", "Family Tree");
            FamilyTreeWindow.init();
            newWindow("familyUnitTreeWindow", "Family Unit Tree");
            FamilyUnitTreeWindow.init();
            newWindow("familyMemberWindow", "Family Member");
            FamilyMemberWindow.init();
            newWindow("familyInfoWindow", "Family Info");
            FamilyInfoWindow.init();
            newWindow("familyOverviewWindow", "Family Overview");
            FamilyOverviewWindow.init();
        }

        private static void newWindow(string id, string title)
        {
            ScrollWindow window;
            GameObject content;
            window = UI.CreateNewWindow(id, title);
            createdWindows.Add(id, window);

            GameObject scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            scrollView.gameObject.SetActive(true);

            content = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
            if (content != null)
            {
                windowContents.Add(id, content);
            }
        }

    }
}
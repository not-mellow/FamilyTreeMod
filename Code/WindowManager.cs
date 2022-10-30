using System;
using System.Collections;
using System.Collections.Generic;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using ReflectionUtility;

namespace FamilyTreeMod
{
    class WindowManager
    {
        public static Dictionary<string, GameObject> windowContents = new Dictionary<string, GameObject>();
        public static Dictionary<string, ScrollWindow> createdWindows = new Dictionary<string, ScrollWindow>();

        public static void init()
        {
            newWindow("familyWindow", "Family Window");
            newWindow("familyOverview", "Family Overview");
            newWindow("relationsWindow", "Relations Window");
            newWindow("modSettingsWindow", "Mod Settings Window");
            newWindow("searchWindow", "Search Unit");
            newWindow("statsWindow", "Stats Limiter Window");
            newWindow("addStatsWindow", "Add Stats Window");
            newWindow("cultureWindow", "Culture Window");
            // newWindow("filterWindow", "Filters");
        }

        private static void newWindow(string id, string title)
        {
            ScrollWindow window;
            GameObject content;
            window = Windows.CreateNewWindow(id, title);
            createdWindows.Add(id, window);

            var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
            scrollView.gameObject.SetActive(true);

            content = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");
            if (content != null)
            {
                windowContents.Add(id, content);
            }
        }

        public static void updateScrollRect(GameObject content, int count, int size)
        {
            var scrollRect = content.GetComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(0, count*size);
        }
    }
}
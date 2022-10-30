using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.UI;
using ReflectionUtility;

namespace FamilyTreeMod
{
    class SearchWindow : MonoBehaviour
    {
        public static GameObject scrollView;
        public static List<ToggleIcon> toggles = new List<ToggleIcon>();
        public static Dictionary<string, bool> filterToggles = new Dictionary<string, bool>();

        public static void init()
        {
            initWindow();
            initButtons();
        }

        public static void initWindow()
        {
            scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/searchWindow/Background/Scroll View");
            GameObject scrollGradient = GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/searchWindow/Background/Scrollgradient");
            scrollGradient.gameObject.SetActive(true);

            RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
            scrollViewRect.sizeDelta = new Vector2((float)200, (float)215.21);
            scrollViewRect.localPosition = new Vector3(0, 0, 0);
        }

        public static void initButtons()
        {
            Button dButton = createBGButton(scrollView, "dmgFilter", 50, "Damage", "Damage", "Search Units By Damage");
            dButton.onClick.AddListener(() => searchUnits("DMG"));
            Button kButton = createBGButton(scrollView, "killsFilter", 20, "DeathMark", "Kills", "Search Units By Kills");
            kButton.onClick.AddListener(() => searchUnits("Kills"));
            Button lButton = createBGButton(scrollView, "lvlFilter", -10, "Levels", "Levels", "Search Units By Levels");
            lButton.onClick.AddListener(() => searchUnits("LVL"));
            Button aButton = createBGButton(scrollView, "ageFilter", -40, "OldAge", "Age", "Search Units By Age");
            aButton.onClick.AddListener(() => searchUnits("AGE"));

        }

        public static void openWindow()
        {
            Windows.ShowWindow("searchWindow");
            ScrollWindow._isWindowActive = false;
            Config.paused = false;
        }

        public static void searchUnits(string sortName)
        {
            foreach(Transform child in WindowManager.windowContents["searchWindow"].transform)
            {
                Destroy(child.gameObject);
            }
            List<Actor> filteredList = sortUnits(sortName);
            RectTransform contentRect = WindowManager.windowContents["searchWindow"].GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0, 207 + (filteredList.Count * 50));
            for(int i = 0; i < filteredList.Count; i++)
            {
                Actor actor = filteredList[i];
                addNewWindowElement(i, actor);
            }

        }

        public static List<Actor> sortUnits(string sortName)
        {
            List<Actor> unitList = MapBox.instance.units.getSimpleList();
            List<Actor> copiedList = unitList.ToList();
            List<Actor> filteredList = new List<Actor>();
            int listCount = unitList.Count;
            if (unitList.Count > 100)
            {
                listCount = 100;
            }
            for(int i = 0; i < listCount; i++)
            {
                Actor actor1 = null;
                int num = 0;
                foreach(Actor actor2 in copiedList)
                {
                    int num2;
                    switch(sortName)
                    {
                        case "Kills":
                            num2 = actor2.data.kills;
                            break;
                        case "DMG":
                            num2 = actor2.curStats.damage;
                            break;
                        case "LVL":
                            num2 = actor2.data.level;
                            break;
                        case "AGE":
                            num2 = actor2.data.age;
                            break;
                        default:
                            return unitList;
                    }
                    if(actor1 != null && num2 == num)
                    {
                        int attribute1 = actor1.data.kills + actor1.curStats.damage + actor1.data.level;
                        int attribute2 = actor2.data.kills + actor2.curStats.damage + actor2.data.level;
                        if (attribute2 > attribute1)
                        {
                            actor1 = actor2;
                        }
                    }
                    else if (actor1 == null || num2 > num)
                    {
                        num = num2;
                        actor1 = actor2;
                    }
                }
                filteredList.Add(actor1);
                copiedList.Remove(actor1);
            }
            return filteredList;
        }

        public static void addNewWindowElement(int i, Actor actor)
        {
            GameObject bgElement = new GameObject("WindowElement");
            bgElement.transform.SetParent(WindowManager.windowContents["searchWindow"].transform);
            Image bgImage = bgElement.AddComponent<Image>();
            bgImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.backgroundKingdomElement.png");
            RectTransform bgRect = bgElement.GetComponent<RectTransform>();
            bgRect.localPosition = new Vector3(90, -40 + (i*-50), 0);
            bgRect.sizeDelta = new Vector2(650, 120);
            CanvasGroup bgCanvas = bgElement.AddComponent<CanvasGroup>();
            bgCanvas.blocksRaycasts = false;

            GameObject avatar = NewBGs.createAvatar(actor, bgElement, 30,  new Vector3(-100, -50, 0));

            int nameSize = 30;
            if (actor.getName().Length > 6)
            {
                nameSize = 15;
            }
            GameObject nameText = NewBGs.addText($"{actor.getName()}", bgElement, 30, new Vector3(-100, 40, 0)).gameObject;

            int posY = 5;
            GameObject numText = NewBGs.addText($"{(i+1).ToString()}.", bgElement, 50, new Vector3(-200, 5, 0)).gameObject;
            GameObject dmgText = NewBGs.addText($"DMG: {Toolbox.formatNumber(actor.curStats.damage)}", bgElement, 25, new Vector3(70, 20, 0)).gameObject;
            GameObject killsText = NewBGs.addText($"KILLS: {Toolbox.formatNumber(actor.data.kills)}", bgElement, 25, new Vector3(180, 20, 0)).gameObject;
            GameObject lvlText = NewBGs.addText($"LVL: {actor.data.level.ToString()}", bgElement, 25, new Vector3(70, -25, 0)).gameObject;
            GameObject ageText = NewBGs.addText($"AGE: {actor.data.age.ToString()}", bgElement, 25, new Vector3(180, -25, 0)).gameObject;

            GameObject inspectButton = new GameObject("inspectButton");
            inspectButton.transform.SetParent(WindowManager.windowContents["searchWindow"].transform);
            Image inspectImage = inspectButton.AddComponent<Image>();
            inspectImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.Icons.iconSpectate.png");
            RectTransform inspectRect = inspectButton.GetComponent<RectTransform>();
            inspectRect.sizeDelta = new Vector2(80, 80);
            inspectRect.localPosition = new Vector3(180, -40 + (i*-50) );
            Button inspectButtonComp = inspectButton.AddComponent<Button>();
            inspectButtonComp.onClick.AddListener(() => avatarOnClick(actor));
        }

        private static void avatarOnClick(Actor actor)
        {
            Config.selectedUnit = actor;
            Windows.ShowWindow("inspect_unit");
        }

        public static Button createBGButton(GameObject parent, string name, int posY, string iconName, string buttonName, string buttonDesc)
        {
            PowerButton button = PowerButtons.CreateButton(
                buttonName,
                Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.Icons.icon{iconName}.png"),
                buttonName,
                buttonDesc,
                new Vector2(118, posY),
                ButtonType.Click,
                parent.transform,
                null
            );

            Image buttonBG = button.gameObject.GetComponent<Image>();
            buttonBG.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.backgroundTabButton.png");
            Button buttonButton = button.gameObject.GetComponent<Button>();

            GameObject toggleHolder = new GameObject("ToggleIcon");
            toggleHolder.transform.SetParent(button.gameObject.transform);
            Image toggleImage = toggleHolder.AddComponent<Image>();
            ToggleIcon toggleIcon = toggleHolder.AddComponent<ToggleIcon>();
            toggleIcon.spriteON = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.buttonToggleIndicator0.png");
            toggleIcon.spriteOFF = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.buttonToggleIndicator1.png");
            toggleIcon.updateIcon(false);

            RectTransform toggleRect = toggleHolder.GetComponent<RectTransform>();
            toggleRect.localPosition = new Vector3(0, 12, 0);
            toggleRect.sizeDelta = new Vector2(8, 7);

            toggles.Add(toggleIcon);
            // filterToggles.Add(name, false);
            buttonButton.onClick.AddListener(() => checkToggledIcon(toggleIcon, name));

            return buttonButton;
        }

        public static void checkToggledIcon(ToggleIcon toggleIcon, string name)
        {
            foreach(ToggleIcon toggle in toggles)
            {
                toggle.updateIcon(false);
            }
            toggleIcon.updateIcon(true);
        }
    }
}
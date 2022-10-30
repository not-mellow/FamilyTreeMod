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
            Button fButton = createBGButton(scrollView, "dmgFilter", 50, "Damage");
            fButton.onClick.AddListener(() => searchUnits("DMG"));
            Button rButton = createBGButton(scrollView, "killsFilter", 10, "DeathMark");
            rButton.onClick.AddListener(() => searchUnits("Kills"));
            Button searchButton = createBGButton(scrollView, "lvlFilter", -30, "Levels");
            searchButton.onClick.AddListener(() => searchUnits("LVL"));

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
            GameObject avatar = NewActions.addUnitAvatar(actor, WindowManager.windowContents["searchWindow"]);
            RectTransform avatarRect = avatar.GetComponent<RectTransform>();
            avatarRect.localPosition = new Vector3(40, -40 + (i*-50), 0);
            GameObject avatarImage = avatar.transform.GetChild(0).gameObject;
            RectTransform avatarImageRect = avatarImage.GetComponent<RectTransform>();
            avatarImageRect.localPosition = new Vector3(0,0,0);
            avatarImageRect.sizeDelta = new Vector2(10, 10);
            GameObject avatarItem = null;
            if (avatar.transform.GetChildCount() == 2)
            {
                avatarItem = avatar.transform.GetChild(1).gameObject;
                RectTransform avatarItemRect = avatarItem.GetComponent<RectTransform>();
                avatarItemRect.localPosition = new Vector3(-4, 5, 0);
                avatarItemRect.sizeDelta = new Vector2(3, 6);
            }
            int posY = 5;
            GameObject numText = NewActions.addText($"{(i+1).ToString()}.", avatar, 5).gameObject;
            RectTransform numRect = numText.GetComponent<RectTransform>();
            numRect.localPosition = new Vector3(-10, posY, 0);
            GameObject nameText = NewActions.addText($"{actor.getName()}", avatar, 3).gameObject;
            RectTransform nameRect = nameText.GetComponent<RectTransform>();
            nameRect.localPosition = new Vector3(0, 10, 0);
            GameObject dmgText = NewActions.addText($"DMG: {Toolbox.formatNumber(actor.curStats.damage)}", avatar, 3).gameObject;
            RectTransform dmgRect = dmgText.GetComponent<RectTransform>();
            dmgRect.localPosition = new Vector3(25, posY, 0);
            GameObject killsText = NewActions.addText($"Kills: {Toolbox.formatNumber(actor.data.kills)}", avatar, 3).gameObject;
            RectTransform killsRect = killsText.GetComponent<RectTransform>();
            killsRect.localPosition = new Vector3(40, posY, 0);
            GameObject lvlText = NewActions.addText($"LVL: {actor.data.level.ToString()}", avatar, 3).gameObject;
            RectTransform lvlRect = lvlText.GetComponent<RectTransform>();
            lvlRect.localPosition = new Vector3(55, posY, 0);
        }

        public static Button createBGButton(GameObject parent, string name, int posY, string iconName)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent.transform);
            RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.localPosition = new Vector3(118, posY, 0);
            Button buttonButton = buttonGO.AddComponent<Button>();
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.backgroundTabButton.png");

            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(buttonGO.transform);
            Image iconImage = iconGO.AddComponent<Image>();
            iconImage.sprite = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.Icons.icon{iconName}.png");
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.localPosition = new Vector3(0, 0, 0);

            GameObject toggleHolder = new GameObject("ToggleIcon");
            toggleHolder.transform.SetParent(buttonGO.transform);
            Image toggleImage = toggleHolder.AddComponent<Image>();
            ToggleIcon toggleIcon = toggleHolder.AddComponent<ToggleIcon>();
            toggleIcon.spriteON = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.buttonToggleIndicator0.png");
            toggleIcon.spriteOFF = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.buttonToggleIndicator1.png");
            toggleIcon.updateIcon(false);

            RectTransform toggleRect = toggleHolder.GetComponent<RectTransform>();
            toggleRect.localPosition = new Vector3(0, 40, 0);
            toggleRect.sizeDelta = new Vector2(25, 25);

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
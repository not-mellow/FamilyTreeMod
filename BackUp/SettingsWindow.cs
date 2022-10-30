using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;

namespace FamilyTreeMod
{
    class SettingsWindow : MonoBehaviour
    {
        private static Dictionary<int, ToggleIcon> toggles = new Dictionary<int, ToggleIcon>();
        public static Dictionary<string, bool> toggleBools = new Dictionary<string, bool>();

        public static void init()
        {
            createOption(
                "NameOption",
                "debug_icon",
                "Family Name",
                "The Family Founder's Name Will Be Inherited By All Family Members",
                new Vector2(0, 0),
                1
            );

            createOption(
                "ParentOption",
                "debug_icon",
                "2 Parents(WILL PROBABLY BREAK THE MOD)",
                "Children Will Be Able To Have 2 Parents With A Family",
                new Vector2(1, 0),
                2
            );

            createOption(
                "InheritanceOption",
                "debug_icon",
                "Inheritance",
                "Families Will Not Inherit Positions From Their Family Heads",
                new Vector2(2, 0),
                3
            );

            createOption(
                "RelationsOption",
                "debug_icon",
                "Relations(WORK IN PROGRESS)",
                "Relations Will Not Affect Rebellions or Diplomacy Between Other Nations",
                new Vector2(3, 0),
                4
            );
        }

        public static void createOption(string objName, string spriteName, string title, string desc, Vector2 pos, int index)
        {
            PowerButton newButton = PowerButtons.CreateButton(
                objName,
                Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.Icons.{spriteName}.png"),
                title,
                desc,
                createPos((int)pos.x, (int)pos.y),
                ButtonType.Click,
                WindowManager.windowContents["modSettingsWindow"].transform,
                () => toggleOption(index, objName)
            );

            GameObject toggleHolder = new GameObject("ToggleIcon");
            toggleHolder.transform.SetParent(newButton.gameObject.transform);
            Image toggleImage = toggleHolder.AddComponent<Image>();
            ToggleIcon toggleIcon = toggleHolder.AddComponent<ToggleIcon>();
            toggleIcon.spriteON = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.buttonToggleIndicator0.png");
            toggleIcon.spriteOFF = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.buttonToggleIndicator1.png");
            toggleIcon.updateIcon(false);

            RectTransform toggleRect = toggleHolder.GetComponent<RectTransform>();
            toggleRect.localPosition = new Vector3(0, 15, 0);
            toggleRect.sizeDelta = new Vector2(10, 10);

            toggles.Add(index, toggleIcon);
            toggleBools.Add(objName, false);
        }

        private static void toggleOption(int index, string objName)
        {
            ToggleIcon toggleIcon = toggles[index];
            if (toggleIcon == null)
            {
                return;
            }
            toggleBools[objName] = !toggleBools[objName];
            toggleIcon.updateIcon(toggleBools[objName]);
        }

        private static Vector2 createPos(int rowIndex, int colIndex)
        {
            float startX = 60;
            float startY = -50;
            Vector2 size = new Vector2(40, 25);
            float posX = rowIndex * size.x;
            float posY = colIndex * -size.y;

            var result = new Vector2(startX + posX, startY + posY);
            return result;
        }
    }
}
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
        public static Dictionary<string, string> inputOptions = new Dictionary<string, string>();

        public static void init()
        {
            createOption(
                "NameOption",
                "debug_icon",
                "Family Name",
                "Instead Of Inheriting Founder's Name, Children Will Inherit Their Parent's First Name",
                new Vector2(0, 0),
                1
            );

            createOption(
                "ParentOption",
                "debug_icon",
                "Only Incest?",
                "If Turned On, Units Won't Be Able To Have Children With Units From Other Families. ~looks at cousins, seductively~ I guess they have no choice",
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

            createInputOption(
                "ChildrenOption",
                "debug_icon",
                "Max Children",
                "Modify The Max Amount Of Children\nA Parent Can Produce",
                new Vector2(0, 1),
                5
            );

            createInputOption(
                "InheritTraits", 
                "debug_icon", 
                "Trait Inheritance", 
                "Modify the chance (1-100)\n of children inheriting their parent's traits", 
                new Vector2(0, 3), 
                6
            );
        }

        public static void createInputOption(string objName, string spriteName, string title, string desc, Vector2 pos, int index)
        {
            GameObject statHolder = new GameObject("OptionHolder");
            statHolder.transform.SetParent(WindowManager.windowContents["modSettingsWindow"].transform);
            Image statImage = statHolder.AddComponent<Image>();
            statImage.sprite = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.windowInnerSliced.png");
            RectTransform statHolderRect = statHolder.GetComponent<RectTransform>();
            Vector2 newPos = createPos((int)pos.x, (int)pos.y);
            statHolderRect.localPosition = new Vector3(130, newPos.y-25, 0);
            statHolderRect.sizeDelta = new Vector2(350, 150);

            Text statText = NewBGs.addText(title, statHolder, 20, new Vector3(0, 20, 0));
            RectTransform statTextRect = statText.gameObject.GetComponent<RectTransform>();
            statTextRect.sizeDelta = new Vector2(statTextRect.sizeDelta.x, 80);

            Text descText = NewBGs.addText(desc, statHolder, 20, new Vector3(0, -60, 0));
            RectTransform descTextRect = descText.gameObject.GetComponent<RectTransform>();
            descTextRect.sizeDelta = new Vector2(descTextRect.sizeDelta.x, 80);

            GameObject inputRef = NCMS.Utils.GameObjects.FindEvenInactive("NameInputElement");

            GameObject inputField = Instantiate(inputRef, statHolder.transform);
            NameInput nameInputComp = inputField.GetComponent<NameInput>();
            nameInputComp.setText("-1");
            RectTransform inputRect = inputField.GetComponent<RectTransform>();
            inputRect.localPosition = new Vector3(0,10,0);
            inputRect.sizeDelta += new Vector2(120, 40);

            GameObject inputChild = inputField.transform.Find("InputField").gameObject;
            RectTransform inputChildRect = inputChild.GetComponent<RectTransform>();
            inputChildRect.sizeDelta *= 2;
            Text inputChildText = inputChild.GetComponent<Text>();
            inputChildText.resizeTextMaxSize = 20;
            InputField inputFieldComp = inputChild.GetComponent<InputField>();
            inputFieldComp.characterValidation = InputField.CharacterValidation.Integer;
            nameInputComp.inputField.onValueChanged.AddListener(delegate{
                changeInput(objName, inputFieldComp);
            });

            inputOptions.Add(objName, "-1");
        }

        private static void changeInput(string inputName, InputField inputField)
        {
            int value = -1;
            if (!int.TryParse(inputField.text, out value))
            {
                return;
            }
            inputOptions[inputName] = inputField.text;
            Debug.Log(inputField.text);
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
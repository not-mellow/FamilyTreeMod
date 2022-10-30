using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using NCMS;
using NCMS.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using ReflectionUtility;
using HarmonyLib;
using System.Threading.Tasks;

namespace FamilyTreeMod
{
    class CultureTechWindow : MonoBehaviour
    {
        private static Dictionary<int, ToggleIcon> techToggleDict = new Dictionary<int, ToggleIcon>();
        private static Dictionary<string, bool> techToggleBoolDict = new Dictionary<string, bool>();
        private static Vector2 originalSize;

        public static void init()
        {
            RectTransform contentRect = WindowManager.windowContents["cultureWindow"].GetComponent<RectTransform>();
            originalSize = contentRect.sizeDelta;
        }
        public static void openCultureWindow()
        {
            initUICulture();
            Windows.ShowWindow("cultureWindow");
        }

        private static void initUICulture()
        {
            foreach (Transform child in WindowManager.windowContents["cultureWindow"].transform)
            {
                Destroy(child.gameObject);
            }
            RectTransform contentRect = WindowManager.windowContents["cultureWindow"].GetComponent<RectTransform>();
            contentRect.sizeDelta = originalSize;
            List<Culture> cultureList = CultureManager.instance.list;
            contentRect.sizeDelta += new Vector2(0, cultureList.Count*70);
            int y = 0;
            foreach(Culture culture in cultureList)
            {
                GameObject bgHolder = NewBGs.createRedButton(WindowManager.windowContents["cultureWindow"], new Vector2(150, 60), createPos(0, y));
                RectTransform bgRect = bgHolder.GetComponent<RectTransform>();
                Button bgButton = bgHolder.AddComponent<Button>();
                bgButton.onClick.AddListener(() => initUITech(culture));
                NewBGs.addText(culture.name, bgHolder, 20, new Vector3(0, 0, 0));
                y++;
            }
            return;
        }

        private static void initUITech(Culture culture)
        {
            foreach (Transform child in WindowManager.windowContents["cultureWindow"].transform)
            {
                Destroy(child.gameObject);
            }
            RectTransform contentRect = WindowManager.windowContents["cultureWindow"].GetComponent<RectTransform>();
            contentRect.sizeDelta = originalSize;
            techToggleDict.Clear();
            techToggleBoolDict.Clear();
            Dictionary<string, CultureTechAsset> techDict = AssetManager.culture_tech.dict;
            int x = -2;
            int y = 0;

            for(int i = 0; i < techDict.Count; i++)
            {
                if(x > 2)
                {
                    x = -2;
                    y++;
                    contentRect.sizeDelta += new Vector2(0, 65);
                }
                KeyValuePair<string, CultureTechAsset> kv = techDict.ElementAt(i);
                bool isOn = false;
                if(culture.list_tech_ids.Contains(kv.Key))
                {
                    isOn = true;
                }
                createTechButton(kv.Key, kv.Value.path_icon, kv.Key, kv.Key, new Vector2(x, y), i, culture, isOn);
                x++;
            }

            GameObject textGo = NewBGs.addText(culture.name, WindowManager.windowContents["cultureWindow"], 15, new Vector3(0, 0, 0)).gameObject;
            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.localPosition = new Vector3(130, -20, 0);
        }

        private static void createTechButton(string objName, string spritePath, string title, string desc, Vector2 pos, int index, Culture culture, bool isOn)
        {
            Dictionary<string, string> dict = LocalizedTextManager.instance.localizedText;
            dict.Remove(objName);
            dict.Remove(objName + " Description");
            PowerButtons.CustomButtons.Remove(objName);
            PowerButton newButton = PowerButtons.CreateButton(
                objName,
                Resources.Load<Sprite>($"ui/Icons/{spritePath}"),
                title,
                desc,
                createPos((int)pos.x, (int)pos.y),
                ButtonType.Click,
                WindowManager.windowContents["cultureWindow"].transform,
                () => toggleOption(index, objName, culture)
            );

            GameObject toggleHolder = new GameObject("ToggleIcon");
            toggleHolder.transform.SetParent(newButton.gameObject.transform);
            Image toggleImage = toggleHolder.AddComponent<Image>();
            ToggleIcon toggleIcon = toggleHolder.AddComponent<ToggleIcon>();
            toggleIcon.spriteON = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.buttonToggleIndicator0.png");
            toggleIcon.spriteOFF = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.buttonToggleIndicator1.png");
            toggleIcon.updateIcon(isOn);

            RectTransform toggleRect = toggleHolder.GetComponent<RectTransform>();
            toggleRect.localPosition = new Vector3(0, 15, 0);
            toggleRect.sizeDelta = new Vector2(10, 10);
            techToggleDict.Add(index, toggleIcon);
            techToggleBoolDict.Add(objName, isOn);

        }

        private static void toggleOption(int index, string objName, Culture culture)
        {
            ToggleIcon toggleIcon = techToggleDict[index];
            if (toggleIcon == null)
            {
                return;
            }
            techToggleBoolDict[objName] = !techToggleBoolDict[objName];
            if(techToggleBoolDict[objName])
            {
                culture.addFinishedTech(objName);
            }
            else
            {
                culture.list_tech_ids.Remove(objName);
                culture.setDirty();
            }
            toggleIcon.updateIcon(techToggleBoolDict[objName]);
        }

        private static Vector2 createPos(int rowIndex, int colIndex)
        {
            float startX = 130;
            float startY = -50;
            Vector2 size = new Vector2(40, 30);
            float posX = rowIndex * size.x;
            float posY = colIndex * -size.y;

            var result = new Vector2(startX + posX, startY + posY);
            return result;
        }
    }
}
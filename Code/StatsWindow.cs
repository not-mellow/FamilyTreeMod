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
    class StatsWindow : MonoBehaviour
    {
        public static Harmony harmony = new Harmony("dej.mymod.wb.statrestrictmod");
        public static BaseStats restrictionStats = new BaseStats();

        public static void init()
        {
            harmony.Patch(AccessTools.Method(typeof(BaseStats), "normalize"), 
            postfix: new HarmonyMethod(AccessTools.Method(typeof(StatsWindow), "normalize_Postfix")));

            restrictionStats.clear();
            createNewStats();
            loadStats();
            initUIStats();
        }

        private static void initUIStats()
        {
            RectTransform contentRect = WindowManager.windowContents["statsWindow"].GetComponent<RectTransform>();
            FieldInfo[] props = restrictionStats.GetType().GetFields();
            contentRect.sizeDelta += new Vector2(0, ((int)props.Count() * 40));
            for(int i = 0; i < props.Count(); i++)
            {
                createInput(WindowManager.windowContents["statsWindow"], createPos(0, i), props[i].Name, props[i].GetValue(restrictionStats).ToString());
            }
        }

        private static void createInput(GameObject parent, Vector3 pos, string statName, string value)
        {
            GameObject statHolder = new GameObject("StatHolder");
            statHolder.transform.SetParent(parent.transform);
            Image statImage = statHolder.AddComponent<Image>();
            statImage.sprite = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.UI.windowInnerSliced.png");

            Text statText = NewBGs.addText(statName, statHolder, 20, new Vector3(0, -20, 0));
            statText.alignment = TextAnchor.UpperCenter;
            RectTransform statTextRect = statText.gameObject.GetComponent<RectTransform>();
            statTextRect.sizeDelta = new Vector2(statTextRect.sizeDelta.x, 80);

            GameObject inputRef = NCMS.Utils.GameObjects.FindEvenInactive("NameInputElement");

            GameObject inputField = Instantiate(inputRef, statHolder.transform);
            NameInput nameInputComp = inputField.GetComponent<NameInput>();
            nameInputComp.setText(value);
            RectTransform inputRect = inputField.GetComponent<RectTransform>();
            inputRect.localPosition = new Vector3(0,0,0);
            inputRect.sizeDelta += new Vector2(120, 40);

            GameObject inputChild = inputField.transform.Find("InputField").gameObject;
            RectTransform inputChildRect = inputChild.GetComponent<RectTransform>();
            inputChildRect.sizeDelta *= 2;
            Text inputChildText = inputChild.GetComponent<Text>();
            inputChildText.resizeTextMaxSize = 20;
            InputField inputFieldComp = inputChild.GetComponent<InputField>();
            inputFieldComp.characterValidation = InputField.CharacterValidation.Integer;
            nameInputComp.inputField.onValueChanged.AddListener(delegate{
                changeStats(statName, inputFieldComp);
            });

            RectTransform statHolderRect = statHolder.GetComponent<RectTransform>();
            statHolderRect.localPosition = pos;
            statHolderRect.sizeDelta = new Vector2(300, 100);

        }

        private static void loadStats()
        {
            if (!File.Exists($"{Core.NCMSModsPath}/StatRestrictions.json"))
            {
                File.Delete($"{Core.NCMSModsPath}/StatRestrictions.json");
                string json = JsonConvert.SerializeObject(restrictionStats, Formatting.Indented);
                File.WriteAllText($"{Core.NCMSModsPath}/StatRestrictions.json", json);
            }

            string data = File.ReadAllText($"{Core.NCMSModsPath}/StatRestrictions.json");
            BaseStats loadedData = JsonConvert.DeserializeObject<BaseStats>(data);
            restrictionStats = loadedData;
        }

        private static void saveStats()
        {
            string json = JsonConvert.SerializeObject(restrictionStats, Formatting.Indented);
            File.WriteAllText($"{Core.NCMSModsPath}/StatRestrictions.json", json);
        }

        private static void changeStats(string statName, InputField input)
        {
            if (string.IsNullOrEmpty(input.text))
            {
                return;
            }
            foreach (FieldInfo prop in restrictionStats.GetType().GetFields())
            {
                if (prop.Name == statName)
                {
                    prop.SetValue(restrictionStats, int.Parse(input.text));
                    Debug.Log(statName);
                }
            }
            saveStats();
        }

        private static void createNewStats()
        {
            foreach (FieldInfo prop in restrictionStats.GetType().GetFields())
            {
                prop.SetValue(restrictionStats, -1);
            }
        }

        private static Vector2 createPos(int rowIndex, int colIndex)
        {
            float startX = 130;
            float startY = -50;
            Vector2 size = new Vector2(40, 40);
            float posX = rowIndex * size.x;
            float posY = colIndex * -size.y;

            var result = new Vector2(startX + posX, startY + posY);
            return result;
        }

        public static void normalize_Postfix(BaseStats __instance)
        {
            foreach (FieldInfo prop in restrictionStats.GetType().GetFields())
            {
                if(prop.FieldType == typeof(int))
                {
                    int value = (int)prop.GetValue(restrictionStats);
                    if(value < 0)
                    {
                        continue;
                    }
                    FieldInfo prop2 = __instance.GetType().GetField(prop.Name);
                    int value2 = (int)prop2.GetValue(__instance);
                    prop2.SetValue(__instance, Mathf.Clamp(value2, 0, value));
                }
                else if (prop.FieldType == typeof(float))
                {
                    float value = (float)prop.GetValue(restrictionStats);
                    if(value < 0f /*|| prop.Name == "s_crit_chance" || prop.Name == "mod_attackSpeed"*/)
                    {
                        continue;
                    }
                    FieldInfo prop2 = __instance.GetType().GetField(prop.Name);
                    float value2 = (float)prop2.GetValue(__instance);
                    prop2.SetValue(__instance, Mathf.Clamp(value2, 0f, value));
                }
            }
        }
    }
}
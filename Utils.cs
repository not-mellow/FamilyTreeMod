using BepInEx;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
// using NCMS;
// using NCMS.Utils;
using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.EventSystems;
using UnityEngine.UI;
// using ReflectionUtility;

namespace FamilyTreeMod
{
    public class Utils
    {
        public static void AddOrSet(string key, string value)
        {
            if (LocalizedTextManager.instance.localizedText.ContainsKey(key))
            {
                LocalizedTextManager.instance.localizedText[key] = value;
            }
            else
            {
                LocalizedTextManager.instance.localizedText.Add(key, value);
            }
        }

        public static GameObject FindInActiveObjectByName(string name)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i].gameObject;
                    }
                }
            }
            return null;
        }

        public static Actor findActorByMemberIndex(int index, ref DeadFamilyMember dead, int currentFamilyIndex)
        {
            Dictionary<string, DeadFamilyMember> deadDict = Plugin.settings.families[SaveManager.currentSavePath][currentFamilyIndex].deadMembers;
            if (deadDict.ContainsKey(index.ToString()))
            {
                dead = deadDict[index.ToString()];
                return null;
            }
            foreach (KeyValuePair<string, Race> kvp in AssetManager.raceLibrary.dict)
            {
                foreach(Actor actor in kvp.Value.units.getSimpleList())
                {
                    int actorFamilyIndex = -1;
                    actor.data.get("familyIndex", out actorFamilyIndex, -1);
                    if (actorFamilyIndex < 0 || actorFamilyIndex != currentFamilyIndex)
                    {
                        continue;
                    }
                    int actorMemberIndex = -1;
                    actor.data.get("memberIndex", out actorMemberIndex, -1);
                    if (actorMemberIndex != index)
                    {
                        continue;
                    }
                    return actor;
                }
                
            }
            return null;
        }
    }
}
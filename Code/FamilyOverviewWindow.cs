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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace FamilyTreeMod
{
    class FamilyOverviewWindow : MonoBehaviour
    {
        public static Dictionary<string, Family> families = new Dictionary<string, Family>();
        public static Dictionary<ActorStatus, UnbornActor> unbornActorList = new Dictionary<ActorStatus, UnbornActor>();
        public static Dictionary<string, deadActor> deadActorList = new Dictionary<string, deadActor>();
        public static Dictionary<Kingdom, Family> kingdomFamilies = new Dictionary<Kingdom, Family>();
        public static Dictionary<string, ActorHead> headInfoList = new Dictionary<string, ActorHead>();
        public static int curID = 0;

        public static void openWindow()
        {
            foreach(Transform child in WindowManager.windowContents["familyOverview"].transform)
            {
                Destroy(child.gameObject);
            }

            if (families.Count > 30)
            {
                int height = families.Count - 30;
                WindowManager.windowContents["familyOverview"].GetComponent<RectTransform>().sizeDelta += new Vector2(0, height * 30);
            }
            int posX = 0;
            int posY = 0;
            foreach(KeyValuePair<string, Family> kv in families)
            {
                if (posX > 4)
                {
                    posX = 0;
                    posY++;
                }
                Actor head = NewActions.getActorByIndex(kv.Value.HEADID, kv.Value.index);
                GameObject familyHolder = new GameObject("familyHolder");
                familyHolder.transform.SetParent(WindowManager.windowContents["familyOverview"].transform);
                RectTransform familyRect = familyHolder.AddComponent<RectTransform>();
                familyRect.localPosition = new Vector3(50 + (posX*40), -30 + (posY*-40), 0);

                GameObject banner = null;
                if (head != null)
                {
                    banner = NewBGs.createBanner(familyHolder, head.kingdom, new Vector2(130, 130));
                }
                else
                {
                    banner = NewBGs.createBanner(familyHolder, null, new Vector2(130, 130));
                }
                Button bannerButton = banner.GetComponent<Button>();
                bannerButton.onClick.AddListener(() => RelationsWindow.openWindow(kv.Value));
                NewBGs.createAvatar(head, familyHolder, 20, new Vector3(20, -30, 0));
                NewBGs.addText(kv.Value.founderName, familyHolder, 20, new Vector3(0, -50, 0));

                GameObject deleteUI = new GameObject("deleteButton");
                deleteUI.transform.SetParent(familyHolder.transform);
                Image deleteImg = deleteUI.AddComponent<Image>();
                deleteImg.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.delete_ui.png");
                deleteImg.rectTransform.sizeDelta = new Vector2(40, 40);
                deleteImg.rectTransform.localPosition = new Vector3(30, 35, 0);
                deleteUI.AddComponent<Button>().onClick.AddListener(() => deleteFamily(kv.Key, kv.Value, familyHolder));
                posX++;
            }
            Windows.ShowWindow("familyOverview");
        }

        private static void deleteFamily(string familyIndex, Family family, GameObject holder)
        {
            families.Remove(familyIndex);
            // Removes Kingdoms Related To Family
            if (kingdomFamilies.ContainsValue(family))
            {
                List<Kingdom> kingdomToRemove = new List<Kingdom>();
                foreach(KeyValuePair<Kingdom, Family> kv in kingdomFamilies)
                {
                    if (kv.Value == family)
                    {
                        kingdomToRemove.Add(kv.Key);
                    }
                }

                foreach (Kingdom kingdom in kingdomToRemove)
                {
                    kingdomFamilies.Remove(kingdom);
                }
            }

            // Removes Family Actor Component From Actors In Family
            if (family.actors.Count > 0)
            {
                foreach(Actor actor in family.actors)
                {
                    if (actor == null || !actor.data.alive)
                    {
                        continue;
                    }
                    FamilyActor actorFamily = actor.gameObject.GetComponent<FamilyActor>();
                    Destroy(actorFamily);
                }
            }

            // Removes Heads Or Dead Unit Stats From Family Tree
            List<string> headsToRemove = new List<string>();
            foreach(KeyValuePair<string, ActorHead> kv in headInfoList)
            {
                if (family.prevHeads.Contains(kv.Key))
                {
                    headsToRemove.Add(kv.Key);
                }
                else if (kv.Value.familyIndex.ToString() == familyIndex)
                {
                    headsToRemove.Add(kv.Key);
                }
            }

            foreach (string head in headsToRemove)
            {
                headInfoList.Remove(head);
            }

            // Removes Dead Units From Family Tree
            List<string> deadToRemove = new List<string>();
            foreach(KeyValuePair<string, deadActor> kv in deadActorList)
            {
                if (kv.Value.familyIndex.ToString() == familyIndex)
                {
                    deadToRemove.Add(kv.Key);
                }
            }

            foreach(string dead in deadToRemove)
            {
                deadActorList.Remove(dead);
            }

            // Removes Unborn Actors From Family Tree
            List<ActorStatus> unbornToRemove = new List<ActorStatus>();
            foreach(KeyValuePair<ActorStatus, UnbornActor> kv in unbornActorList)
            {
                if (kv.Value.familyIndex.ToString() == familyIndex)
                {
                    unbornToRemove.Add(kv.Key);
                }
            }

            foreach(ActorStatus status in unbornToRemove)
            {
                unbornActorList.Remove(status);
            }

            Destroy(holder);
        }

        public static int nextID()
        {
            int newID = curID;
            curID++;
            return newID;
        }
        
        public static deadActor getDeadActor(string deadID)
        {
            if (deadID == null || !deadActorList.ContainsKey(deadID))
            {
                return null;
            }
            return deadActorList[deadID];
        }

        public static ActorHead getHeadActor(string headID)
        {
            if (headID == null || !headInfoList.ContainsKey(headID))
            {
                return null;
            }
            return headInfoList[headID];
        }

        public static string getTitle(UnitProfession profession)
        {
            switch (profession)
            {
                case UnitProfession.Unit:
                    return "Peasant";
                case UnitProfession.Warrior:
                    return "Knight";
                case UnitProfession.Leader:
                    return "Noble";
                case UnitProfession.King:
                    return "Royalty";
                default:
                    return "Peasant";
            }
        }

        public static Family getFromFamilies(ref int index)
        {
            string key = index.ToString();
            if (families.ContainsKey(key))
            {
                return families[key];
            }
            index = -1;
            return null;
        }
    }
}
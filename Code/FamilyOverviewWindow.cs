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
                posX++;
            }
            Windows.ShowWindow("familyOverview");
        }

        public static int nextID()
        {
            int newID = curID;
            curID++;
            return newID;
        }
        
    }
    
    class ActorHead
    {
        
    }
}
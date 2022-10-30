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
        public static HashSet<ActorParent> families = new HashSet<ActorParent>();
        public static HashSet<Actor> familyActors = new HashSet<Actor>();
        public static Dictionary<ActorParent, ActorHead> familyHeads = new Dictionary<ActorParent, ActorHead>();
        public static Dictionary<string, ActorParent> originalFamilies = new Dictionary<string, ActorParent>();
        public static int maxFamilies = 15;
        public static int numOfFamilies = 0;
        public static Text familiesText = null;
        public static NewActions newActionsInstance;


        public static void init()
        {
            var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyOverview/Background/Scroll View");
            var scrollGradient = GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/familyOverview/Background/");
            var viewPort = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyOverview/Background/Scroll View");
            scrollGradient.gameObject.SetActive(true);

            RectTransform viewPortRect = viewPort.GetComponent<RectTransform>();
            viewPortRect.sizeDelta = new Vector2((float)200, (float)215.21);
            // 0 -215.21 258.8 215.21
            viewPortRect.localPosition = new Vector3(0, 0, 0);

            ScrollRect scrollRect = scrollView.gameObject.GetComponent<ScrollRect>();
            scrollRect.horizontal = true;

            var contentRect = WindowManager.windowContents["familyOverview"].GetComponent<RectTransform>();
            contentRect.sizeDelta += new Vector2(500, 100);

            Text textComp = NewActions.addText(
                numOfFamilies.ToString() + "/" + maxFamilies.ToString(),
                scrollView,
                20
            );
            familiesText = textComp;
            RectTransform textRect = textComp.gameObject.GetComponent<RectTransform>();
            textRect.localPosition = new Vector3(70, 100, 0);

            GameObject newInstance = new GameObject("newActionsInstance");
            NewActions newActions = newInstance.AddComponent<NewActions>();
            newActionsInstance = newActions;
        }

        public static void addParent(Actor actor)
        {
            if (familyActors.Contains(actor) || numOfFamilies >= maxFamilies)
            {
                return;
            }
    
            ActorParent newParent = new ActorParent(actor.getName(), actor, new List<Actor>(), new List<ActorStatus>(), null, null);
            families.Add(newParent);
            familyActors.Add(actor);
            ActorHead newHead = new ActorHead(newParent, MapBox.instance.mapStats.year, 0, 1);
            familyHeads.Add(newParent, newHead);

            numOfFamilies++;
            familiesText.text = numOfFamilies.ToString() + "/" + maxFamilies.ToString();
        }

        public static void loadParent(Actor actor, List<Actor> children, SavedHead savedHead, SavedParent savedOriginal)
        {
            if (familyActors.Contains(actor))
            {
                return;
            }
            ActorParent newParent = new ActorParent(actor.getName(), actor, children, new List<ActorStatus>(), null, null);
            if (!originalFamilies.ContainsKey(savedOriginal.familyName) && savedHead != null)
            {
                originalFamilies.Add(savedOriginal.familyName, newParent);
            }
            newParent.originalFamily = originalFamilies[savedOriginal.familyName];
            families.Add(newParent);
            familyActors.Add(actor);

            if (savedHead == null)
            {
                return;
            }
            ActorHead newHead = new ActorHead(newParent, savedHead.foundingYear, savedHead.currentGeneration, savedHead.numOfMembers);
            familyHeads.Add(newParent, newHead);

            if(actor.isProfession(UnitProfession.King))
            {
                if(RelationsWindow.royalFamilies.ContainsKey(actor.kingdom))
                {
                    RelationsWindow.royalFamilies.Remove(actor.kingdom);
                }
                RelationsWindow.royalFamilies.Add(actor.kingdom, actor.getName());
                Debug.Log("family king added");
            }
            if(actor.isProfession(UnitProfession.Leader))
            {
                if(RelationsWindow.nobleFamilies.ContainsKey(actor.city))
                {
                    RelationsWindow.nobleFamilies.Remove(actor.city);
                }
                RelationsWindow.nobleFamilies.Add(actor.city, actor.getName());
                Debug.Log("family city leader added");
            }

            numOfFamilies++;
            familiesText.text = numOfFamilies.ToString() + "/" + maxFamilies.ToString();
        }

        public static void loadBanners()
        {
            foreach(Transform child in WindowManager.windowContents["familyOverview"].transform)
            {
                Destroy(child.gameObject);
            }
            int posX = 0;
            int posY = 0;
            foreach(KeyValuePair<ActorParent, ActorHead> kv in familyHeads)
            {
                ActorParent actorParent = kv.Key;
                ActorHead actorHead = kv.Value;
                GameObject parent = addBannerSlot(
                    WindowManager.windowContents["familyOverview"],
                    actorParent.parentActor.kingdom,
                    actorParent.originalFamily.familyName,
                    new Vector3((100*posX)+100, (-60*posY)-40, 0),
                    actorParent,
                    actorHead
                );
                posX += 1;
                if (posX > 5)
                {
                    posY += 1;
                    posX = 0;
                }
            }
        }

        public static GameObject addBannerSlot(GameObject canvasParent, Kingdom kingdom, string familyName, Vector3 pos, ActorParent actorParent, ActorHead actorHead)
        {
            GameObject bannerSlot = NewActions.addCanvasRenderer(canvasParent, pos);
            NewActions.addText(familyName, bannerSlot, 21);
            NewActions.addBanner(bannerSlot, kingdom, actorParent);
            GameObject crownIcon = NewActions.createCrownIcon(bannerSlot, actorHead.getTitle());
            RectTransform crownRect = crownIcon.GetComponent<RectTransform>();
            crownRect.localPosition = new Vector3(0, 80, 0);
            newActionsInstance.addDeleteButton(bannerSlot, actorParent);

            return bannerSlot;
        }

        public static IEnumerator deleteFamily(GameObject banner, ActorParent actorParent)
        {
            if (banner != null)
            {
                Destroy(banner);
                numOfFamilies--;
                familiesText.text = numOfFamilies.ToString() + "/" + maxFamilies.ToString();
            }
            families.Remove(actorParent);
            familyHeads.Remove(actorParent);
            familyActors.Remove(actorParent.parentActor);
            List<ActorParent> familiesCopy = (from parent in families
            select parent).ToList();
            
            foreach(ActorParent family in familiesCopy)
            {
                if(family.originalFamily == actorParent.originalFamily)
                {
                    deleteFamily(null, family);
                    Debug.Log(family.familyName + " Family Deleted");
                }
                yield return new WaitForSeconds(.2f);
            }
            yield break;
        }

        public static void replaceHead(ActorParent actorParent)
        {
            Actor successor = null;
            foreach(Actor child in actorParent.children)
            {
                if (child != null && child.data.alive)
                {
                    successor = child;
                    break;
                }
            }
            if (successor != null)
            {
                foreach(ActorParent family in families)
                {
                    if (family.parentActor.data.alive && family.parentActor == successor && !familyHeads.ContainsKey(family))
                    {
                        ActorHead newHead = new ActorHead(family, familyHeads[actorParent].foundingYear, familyHeads[actorParent].currentGeneration, familyHeads[actorParent].numOfMembers);
                        familyHeads.Remove(actorParent);
                        familyHeads.Add(family, newHead);
                        break;
                    }
                }
                return;
            }
            Debug.Log($"There Is No Available Successor!");
            ActorParent origins = actorParent.originalFamily;
            bool hasSuccessor = false;
            foreach(ActorParent family in families)
            {
                if (family.parentActor != null && family.parentActor.data.alive && family.originalFamily == origins && !familyHeads.ContainsKey(family))
                {
                    ActorHead newHead = new ActorHead(family, familyHeads[actorParent].foundingYear, familyHeads[actorParent].currentGeneration, familyHeads[actorParent].numOfMembers);
                    familyHeads.Remove(actorParent);
                    familyHeads.Add(family, newHead);
                    hasSuccessor = true;
                    break;
                }
            }
            if (hasSuccessor)
            {
                return;
            }
            if(RelationsWindow.royalFamilies.ContainsValue(origins.familyName))
            {
                Kingdom kingdomRemoval = null;
                foreach(KeyValuePair<Kingdom, string> kv in RelationsWindow.royalFamilies)
                {
                    if(kv.Value == origins.familyName)
                    {
                        kingdomRemoval = kv.Key;
                        break;
                    }
                }
                RelationsWindow.royalFamilies.Remove(kingdomRemoval);
                Debug.Log("A ROYAL FAMILY HAS DIED OUT");
            }
            if (RelationsWindow.nobleFamilies.ContainsValue(origins.familyName))
            {
                City cityRemoval = null;
                foreach(KeyValuePair<City, string> kv in RelationsWindow.nobleFamilies)
                {
                    if(kv.Value == origins.familyName)
                    {
                        cityRemoval = kv.Key;
                        break;
                    }
                }
                RelationsWindow.nobleFamilies.Remove(cityRemoval);
                Debug.Log("A NOBLE FAMILY HAS DIED OUT");
            }
        }
    }

    [Serializable]
    public class ActorParent
    {
        public List<Actor> children;
        public string familyName;
        public Actor parentActor;
        public List<ActorStatus> childrenData;
        public ActorParent parentFamily;
        public ActorParent originalFamily;

        public ActorParent(string name, Actor actor, List<Actor> actorList, List<ActorStatus> actorStatusList, ActorParent family, ActorParent foundingFamily)
        {
            if (name.Contains(" "))
            {
                var split = name.Split(' ');
                name = split[0];
            }
            familyName = name;
            parentActor = actor;
            children = actorList;
            childrenData = actorStatusList;
            if (family == null)
            {
                parentFamily = this;
            }else
            {
                parentFamily = family;
            }

            if (foundingFamily == null)
            {
                originalFamily = this;
            }else
            {
                originalFamily = foundingFamily;
            }
        }
    }
    
    class ActorHead
    {
        public ActorParent headFamily;
        public int foundingYear;
        public int currentGeneration;
        public int numOfMembers;

        public ActorHead(ActorParent actorParent, int year, int generation, int members)
        {
            headFamily = actorParent;
            foundingYear = year;
            currentGeneration = ++generation;
            numOfMembers = members;
        }

        public string getTitle()
        {
            switch(headFamily.parentActor.data.profession)
            {
                case UnitProfession.King:
                    return "Royalty";
                    break;
                case UnitProfession.Leader:
                    return "Noble";
                    break;
                case UnitProfession.Warrior:
                    return "Knight";
                    break;
                default:
                    return "Peasant";
                    break;
            }
        }

        public int getAge()
        {
            int currentYear = MapBox.instance.mapStats.year;
            return currentYear - foundingYear;
        }

        public Kingdom getKingdom()
        {
            return headFamily.parentActor.kingdom;
        }

        public City getCity()
        {
            return headFamily.parentActor.city;
        }

        public void changeMemberInfo(int value)
        {
            numOfMembers += value;
            if (numOfMembers < 0)
            {
                numOfMembers = 0;
            }
        }
    }
}
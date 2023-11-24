using BepInEx;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FamilyTreeMod
{
    public class FamilyUnitTreeWindow : MonoBehaviour
    {
        private static int currentFamilyIndex = 0;
        private static GameObject contents;
        private static Vector3 originalSize = new Vector3(0, 207);

        public static void init()
        {
            contents = WindowManager.windowContents["familyUnitTreeWindow"];

            UI.createBGWindowButton(
                GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyUnitTreeWindow/Background"), 
                0, 
                AssetLoader.cached_assets_list["FamilyTreeUI/iconDownvote.png"][0],//string iconName, 
                "downButton",//string buttonName, 
                "Increase Scroll Size",//string buttonTitle, 
                "Press This To Increase Scroll Space",//string buttonDesc, 
                increaseScrollSize
            );
            return;
        }

        public static void openWindow(int familyIndex, int actorIndex = 1)
        {
            currentFamilyIndex = familyIndex;
            
            foreach(Transform child in contents.transform)
            {
                Destroy(child.gameObject);
            }
            contents.GetComponent<RectTransform>().sizeDelta = originalSize;

            if (!Plugin.settings.families.ContainsKey(SaveManager.currentSavePath))
            {
                WorldTip.instance.show("ERROR: No Save Path", false, "top", 3f);
                return;
            }
            else if ((Plugin.settings.families[SaveManager.currentSavePath].Count -1) < currentFamilyIndex)
            {
                WorldTip.instance.show("ERROR: No Current Family Available", false, "top", 3f);
                return;
            }

            loadActorsFromIndex(actorIndex);
            UI.ShowWindow("familyUnitTreeWindow");
        }

        private static void loadActorsFromIndex(int actorIndex)
        {
            DeadFamilyMember dead = null;
            Actor actor = Utils.findActorByMemberIndex(actorIndex, ref dead, currentFamilyIndex);
            UI.createActorUI(
                actor,
                contents,
                new Vector3(70, -40, 0),
                new Vector3(1, 1, 1),
                dead
            );

            int actorParentIndex = -1;
            int actorParentIndex2 = -1;
            string actorChildrenIndex = "";
            int actorSpouseIndex = -1;
            if (actor == null && dead != null)
            {
                actorChildrenIndex = dead.childrenIndex;
                actorParentIndex = dead.parentIndex;
                actorParentIndex2 = dead.parentIndex2;
                actorSpouseIndex = dead.spouseIndex;
            }
            else
            {
                actor.data.get("childrenIndex", out actorChildrenIndex, "");
                actor.data.get("parentIndex", out actorParentIndex, -1);
                actor.data.get("parentIndex2", out actorParentIndex2, -1);
                actor.data.get("spouseIndex", out actorSpouseIndex, -1);
            }

            if (actorSpouseIndex != -1)
            {
                UI.addText(
                    "Spouse", 
                    contents, 
                    15, 
                    new Vector3(140, -30, 0), 
                    default(Vector2)
                );
                DeadFamilyMember deadSpouse = null;
                Actor spouse = Utils.findActorByMemberIndex(actorSpouseIndex, ref deadSpouse, currentFamilyIndex);
                UI.createActorUI(
                    spouse,
                    contents,
                    new Vector3(140, -40, 0),
                    new Vector3(1, 1, 1),
                    deadSpouse
                );
            }

            createParentsUI(actorParentIndex, actorParentIndex2);

            if (!string.IsNullOrEmpty(actorChildrenIndex))
            {
                createChildrenGrid(actorChildrenIndex);
            }
        }

        private static void createChildrenGrid(string actorChildrenIndex)
        {
            UI.addText(
                "Children", 
                contents, 
                15, 
                new Vector3(70, -100, 0), 
                default(Vector2)
            );
            GameObject childrenGrid = new GameObject("childrenGrid");
            childrenGrid.transform.SetParent(contents.transform);
            childrenGrid.AddComponent<Image>().color = new Color(1, 1, 1, 0);
            childrenGrid.transform.localPosition = new Vector3(130, -265, 0);
            childrenGrid.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 400);
            childrenGrid.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            GridLayoutGroup cLayoutGroup = childrenGrid.AddComponent<GridLayoutGroup>();
            cLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            cLayoutGroup.cellSize = new Vector2(60, 60);
            cLayoutGroup.spacing = new Vector2(10, 0);
            cLayoutGroup.padding = new RectOffset(20, 20, 20, 20);
            foreach(string childIndex in actorChildrenIndex.Split(','))
            {
                DeadFamilyMember deadChild = null;
                Actor child = Utils.findActorByMemberIndex(int.Parse(childIndex), ref deadChild, currentFamilyIndex);
                UI.createActorUI(
                    child,
                    childrenGrid,
                    new Vector3(0, 0, 0),
                    new Vector3(1, 1, 1),
                    deadChild
                );
            }
        }

        private static void createParentsUI(int actorParentIndex, int actorParentIndex2)
        {
            UI.addText(
                "Parents", 
                contents, 
                15, 
                new Vector3(70, -30, 0), 
                default(Vector2)
            );
            if (actorParentIndex != -1)
            {
                DeadFamilyMember deadParent = null;
                Actor parent1 = Utils.findActorByMemberIndex(actorParentIndex, ref deadParent, currentFamilyIndex);
                UI.createActorUI(
                    parent1,
                    contents,
                    new Vector3(55, -110, 0),
                    new Vector3(0.7f, 0.7f, 0.7f),
                    deadParent
                ); 
            }
            if (actorParentIndex2 != -1)
            {
                DeadFamilyMember deadParent2 = null;
                Actor parent2 = Utils.findActorByMemberIndex(actorParentIndex2, ref deadParent2, currentFamilyIndex);
                UI.createActorUI(
                    parent2,
                    contents,
                    new Vector3(100, -110, 0),
                    new Vector3(0.7f, 0.7f, 0.7f),
                    deadParent2
                ); 
            }
        }

        private static void increaseScrollSize()
        {
            contents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 100);
            foreach(Transform child in contents.transform)
            {
                child.localPosition += new Vector3(0, 50, 0);
            }
        }
    }
}
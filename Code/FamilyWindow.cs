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
    class FamilyWindow : MonoBehaviour
    {
        private static Family currentFamily;
        private static Actor currentHead;
        private static RectTransform contentRect;
        private static Vector2 originalContentSize;
        private static Vector3 currentZoom = new Vector3(0.33f, 0.33f, 0);
        public static int curID = 0;

        public static void init()
        {
            GameObject scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyWindow/Background/Scroll View");
            GameObject scrollGradient = GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/familyWindow/Background/Scrollgradient");
            scrollGradient.gameObject.SetActive(true);

            RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
            scrollViewRect.sizeDelta = new Vector2((float)200, (float)215.21);
            scrollViewRect.localPosition = new Vector3(0, 0, 0);

            ScrollRect scrollRect = scrollView.gameObject.GetComponent<ScrollRect>();
            scrollRect.horizontal = true;

            contentRect = WindowManager.windowContents["familyWindow"].GetComponent<RectTransform>();
            originalContentSize = contentRect.sizeDelta;

            initSideButtons(scrollView);
        }

        private static void initSideButtons(GameObject parent)
        {
            GameObject inButton = NewBGs.createSideButton(parent, 50, new Vector2(80, 80), "iconUpvote", "Zoom In", "Press To Zoom In The Tree");
            inButton.GetComponent<Button>().onClick.AddListener(() => zoomObject(WindowManager.windowContents["familyWindow"], "structureHolder", 0.06f));
            GameObject outButton = NewBGs.createSideButton(parent, 20, new Vector2(80, 80), "iconDownvote", "Zoom Out", "Press To Zoom Out In The Tree");
            outButton.GetComponent<Button>().onClick.AddListener(() => zoomObject(WindowManager.windowContents["familyWindow"], "structureHolder", -0.06f));
        }

        private static void zoomObject(GameObject parent, string objectName, float zoomValue)
        {
            Transform obj = parent.transform.Find(objectName);
            if (obj == null)
            {
                return;
            }
            Vector3 newZoom = obj.localScale + new Vector3(zoomValue, zoomValue, 0);
            newZoom = new Vector3(Mathf.Clamp(newZoom.x, 0.15f, 0.8f), Mathf.Clamp(newZoom.y, 0.15f, 0.8f), 0);
            obj.localScale = newZoom;
            currentZoom = newZoom;
            contentRect.localPosition = new Vector3(contentRect.sizeDelta.x/-2, contentRect.localPosition.x, contentRect.localPosition.y);
        }

        public static void openFounderWindow(Family family)
        {
            foreach(Transform child in WindowManager.windowContents["familyWindow"].transform)
            {
                Destroy(child.gameObject);
            }
            contentRect.sizeDelta = originalContentSize;
            currentFamily = family;

            GameObject structure = createFamilyStructure(WindowManager.windowContents["familyWindow"], null, family.founderID, new Vector3(130, -80, 0), 4);
            if (contentRect.sizeDelta.x > 200)
            {
                structure.transform.localPosition = new Vector3((contentRect.sizeDelta.x/2)+100, -80, 0);
                contentRect.localPosition = new Vector3(contentRect.sizeDelta.x/-2, contentRect.localPosition.x, contentRect.localPosition.y);
            }
            structure.transform.localScale = currentZoom;
            Windows.ShowWindow("familyWindow");
        }

        public static void openHeadWindow(Family family)
        {
            Actor head = NewActions.getActorByIndex(family.HEADID, family.index);

            foreach(Transform child in WindowManager.windowContents["familyWindow"].transform)
            {
                Destroy(child.gameObject);
            }
            contentRect.sizeDelta = originalContentSize;
            currentFamily = family;
            currentHead = head;
            string IDsetup = "";
            if (family.prevHeads.Count > 0 && head == null)
            {
                IDsetup = family.prevHeads[family.prevHeads.Count - 1];
            }
            else
            {
                IDsetup = head.data.actorID;
            }
            GameObject structure = createFamilyStructure(WindowManager.windowContents["familyWindow"], head, IDsetup, new Vector3(130, -80, 0), 4);
            if (contentRect.sizeDelta.x > 200)
            {
                structure.transform.localPosition = new Vector3((contentRect.sizeDelta.x/2)+100, -80, 0);
                contentRect.localPosition = new Vector3(contentRect.sizeDelta.x/-2, contentRect.localPosition.x, contentRect.localPosition.y);
            }
            Windows.ShowWindow("familyWindow");
        }

        private static GameObject createFamilyStructure(GameObject parent, Actor actorSetup, string actorIDSetup, Vector3 pos, int increaseX)
        {
            Actor actor = null;
            if (actorSetup != null)
            {
                actor = actorSetup;
            }
            else if (actorIDSetup != null && !actorIDSetup.Contains("dead"))
            {
                actor = NewActions.getActorByIndex(actorIDSetup, currentFamily.index);
            }

            GameObject structure = actorStructure(parent, actor, actorIDSetup);
            if (actorIDSetup.Contains("dead"))
            {
                deadActor dead = FamilyOverviewWindow.getDeadActor(actorIDSetup);
                createChildrenStructure(structure, dead.childrenID, increaseX);
            }
            else
            {
                createChildrenStructure(structure, FamilyActor.getFamily(actor).childrenID, increaseX);
            }
            structure.transform.localScale = new Vector3(1, 1, 0);
            structure.transform.localPosition = pos;
            return structure;
        }

        private static GameObject actorStructure(GameObject parent, Actor actor, string actorID)
        {
            GameObject structure = new GameObject("structureHolder");
            structure.transform.SetParent(parent.transform);

            GameObject tempObj = null;
            deadOrAliveActorBG(structure, actorID, new Vector3(-80, 0, 0), ref tempObj);

            string loverID = null;
            if (actor != null)
            {
                loverID = FamilyActor.getFamily(actor).loverID;
            }
            else if (actorID.Contains("dead"))
            {
                loverID = FamilyOverviewWindow.getDeadActor(actorID).loverID;
            }

            if (loverID == null)
            {
                return structure;
            }
            Vector3 loverPos = new Vector3(80, 0, 0);
            deadOrAliveActorBG(structure, loverID, loverPos, ref tempObj);

            return structure;
        }

        private static List<string> deadOrAliveActorBG(GameObject parent, string actorID, Vector3 pos, ref GameObject actorBG)
        {
            List<string> newChildrenID = null;
            if (!string.IsNullOrEmpty(actorID) && !actorID.Contains("dead"))
            {
                Actor actor = MapBox.instance.getActorByID(actorID);
                actorBG = NewBGs.createAvatarBG(parent, new Vector2(100, 100), pos);
                NewBGs.createAvatar(actor, actorBG, 30, new Vector3(0, -30, 0));
                GameObject name = actorBG.transform.GetChild(0).gameObject;
                addSizedText(actor.getName(), name, 20, new Vector3(0, 0, 0));
                newChildrenID = FamilyActor.getFamily(actor).childrenID;
            }
            else if (actorID.Contains("dead"))
            {
                deadActor dead = FamilyOverviewWindow.getDeadActor(actorID);
                actorBG = NewBGs.createAvatarBG(parent, new Vector2(100, 100), pos);
                NewBGs.createAvatar(null, actorBG, 30, new Vector3(0, -30, 0), actorID);
                GameObject name = actorBG.transform.GetChild(0).gameObject;
                addSizedText(dead.name, name, 20, new Vector3(0, 0, 0));
                newChildrenID = dead.childrenID;
            }
            return newChildrenID;
        }

        private static void createChildrenStructure(GameObject parent, List<string> childrenID, int increaseX)
        {
            if (childrenID.Count > increaseX && contentRect.sizeDelta.x <= 2000)
            {
                int increaseSize = 1 + (childrenID.Count/2);
                contentRect.sizeDelta += new Vector2(increaseSize*70, 0);
            }
            int posX = childrenID.Count/2;
            foreach (string childID in childrenID)
            {
                List<string> newChildrenID = new List<string>();
                GameObject childBG = null;

                newChildrenID = deadOrAliveActorBG(parent, childID, new Vector3(posX*-120, -130, 0), ref childBG);
                // if (childID.Contains("dead"))
                // {
                //     deadActor childDead = FamilyOverviewWindow.deadActorList[childID];
                //     childBG = NewBGs.createAvatarBG(parent, new Vector2(100, 100), new Vector3(posX*-120, -130, 0));
                //     NewBGs.createAvatar(null, childBG, 30, new Vector3(0, -30, 0), childID);
                //     GameObject name = childBG.transform.GetChild(0).gameObject;
                //     addSizedText(childDead.name/*childID*/, name, 20, new Vector3(0, 0, 0));
                //     newChildrenID = childDead.childrenID;
                // }
                // else
                // {
                //     Actor child = NewActions.getActorByIndex(childID, currentFamily.index);
                //     childBG = NewBGs.createAvatarBG(parent, new Vector2(100, 100), new Vector3(posX*-120, -130, 0));
                //     NewBGs.createAvatar(child, childBG, 30, new Vector3(0, -30, 0));
                //     if (child != null)
                //     {
                //         GameObject name = childBG.transform.GetChild(0).gameObject;
                //         addSizedText(child.getName()/*childID*/, name, 20, new Vector3(0, 0, 0));
                //         newChildrenID = FamilyActor.getFamily(child).childrenID;
                //     }
                // }

                if (newChildrenID.Count > 0)
                {
                    GameObject childrenObj = new GameObject("childrenButton");
                    childrenObj.transform.SetParent(childBG.transform);
                    Image childrenImage = childrenObj.AddComponent<Image>();
                    childrenImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.tab_buttons_selected.png");
                    Button childrenButton = childrenObj.AddComponent<Button>();

                    RectTransform childrenRect = childrenObj.GetComponent<RectTransform>();
                    childrenRect.localPosition = new Vector3(0, -50, 0);
                    childrenRect.localRotation = Quaternion.Euler(0, 0, 180);
                    childrenRect.sizeDelta = new Vector2(80, 25);

                    RectTransform childBGRect = childBG.GetComponent<RectTransform>();
                    Vector3 structurePos = childBGRect.localPosition + childrenRect.localPosition + new Vector3(0, -100, 0);
                    childrenButton.onClick.AddListener(() => createChildFamilyStructure(parent, null, childID, structurePos, 2));
                }
                posX--;
            }
        }

        private static void createChildFamilyStructure(GameObject parent, Actor actorSetup, string actorIDSetup, Vector3 pos, int increaseX)
        {
            float gapMovedX = 0f;
            Transform firstHolder = WindowManager.windowContents["familyWindow"].transform.Find("structureHolder");
            if (firstHolder != null)
            {
                gapMovedX = firstHolder.localPosition.x - ((contentRect.sizeDelta.x/2)+100);
                firstHolder.localPosition = new Vector3((contentRect.sizeDelta.x/2)+100, -80, 0);
            }
            Transform otherStructure = parent.transform.Find("structureHolder");
            if (otherStructure != null)
            {
                Destroy(otherStructure.gameObject);
                contentRect.sizeDelta -= new Vector2(0, 100);
            }
            GameObject structure = createFamilyStructure(parent, actorSetup, actorIDSetup, pos, increaseX);
            contentRect.sizeDelta += new Vector2(0, 100);
            contentRect.position += new Vector3(gapMovedX, 0,  0);
        }

        public static void openHeadHistoryWindow(Family family)
        {
            foreach(Transform child in WindowManager.windowContents["familyWindow"].transform)
            {
                Destroy(child.gameObject);
            }
            contentRect.sizeDelta = originalContentSize;
            currentFamily = family;

            if (family.prevHeads.Count > 2)
            {
                int increaseY = family.prevHeads.Count - 3;
                contentRect.sizeDelta += new Vector2(0, (increaseY*60));
            }
            
            int gen = 1;
            int posY = family.prevHeads.Count;
            GameObject currentHeadBG = NewBGs.createAvatarBG(WindowManager.windowContents["familyWindow"], new Vector2(100, 100), new Vector3(130, -60, 0));
            Actor curHead = NewActions.getActorByIndex(family.HEADID, family.index);
            if (curHead != null)
            {
                NewBGs.createAvatar(curHead, currentHeadBG, 30, new Vector3(0, -30, 0));

                GameObject curHeadNameObj = currentHeadBG.transform.GetChild(0).gameObject;
                addSizedText(curHead.getName(), curHeadNameObj, 20, new Vector3(0, 0, 0));
            }
            NewBGs.addText($"Current Generation: ", currentHeadBG, 30, new Vector3(-200, -55, 0));
            foreach(string headID in family.prevHeads)
            {
                GameObject headBG = NewBGs.createAvatarBG(currentHeadBG, new Vector2(100, 100), new Vector3(0, (posY*-150), 0));
                string actorName = "";
                if (headID.Contains("dead"))
                {
                    deadActor deadHead = FamilyOverviewWindow.deadActorList[headID];
                    NewBGs.createAvatar(null, headBG, 30, new Vector3(0, -30, 0), headID);
                    actorName = deadHead.name;
                }
                else
                {
                    Actor head = NewActions.getActorByIndex(headID, currentFamily.index);
                    NewBGs.createAvatar(head, headBG, 30, new Vector3(0, -30, 0));
                    actorName = head.getName();
                }
                GameObject nameObj = headBG.transform.GetChild(0).gameObject;
                addSizedText(actorName, nameObj, 20, new Vector3(0, 0, 0));

                NewBGs.addText($"Generation {gen.ToString()}: ", headBG, 30, new Vector3(-200, -45, 0));
                posY--;
                gen++;
            }
            Windows.ShowWindow("familyWindow");
        }

        private static Text addSizedText(string text, GameObject pObject, int size, Vector3 pos)
        {
            int newSize = size;
            if (text.Length > 6 && text.Length < 11)
            {
                newSize = size - (text.Length - 6);
            }
            else if (text.Length >= 11)
            {
                newSize = size - 6;
            }
            return NewBGs.addText(text, pObject, newSize, pos);
        }

        public static int nextID()
        {
            int newID = curID;
            curID++;
            return newID;
        }
    }
}
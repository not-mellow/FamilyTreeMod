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
    class FamilyInspectWindow : MonoBehaviour
    {
        private static GameObject contents;
        private static GameObject scrollView;
        private static Button treeButton;
        private static Button familyButton;
        private static RectTransform contentRect;
        private static Vector2 originalContentSize;
        private static Vector3 currentZoom = new Vector3(0.33f, 0.33f, 0);

        public static void init()
        {
            contents = WindowManager.windowContents["familyInspectWindow"];
            scrollView = GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/familyInspectWindow/Background/Scroll View");
            RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
            scrollViewRect.sizeDelta = new Vector2((float)200, (float)215.21);
            scrollViewRect.localPosition = new Vector3(0, 0, 0);

            ScrollRect scrollRect = scrollView.gameObject.GetComponent<ScrollRect>();
            scrollRect.horizontal = true;

            contentRect = contents.GetComponent<RectTransform>();
            originalContentSize = contentRect.sizeDelta;
            
            GameObject inspectBG = GameObject.Find("Canvas Container Main/Canvas - Windows/windows/inspect_unit/Background");
            Button inspectButton = NewBGs.createBGWindowButton(
                inspectBG,  
                -55, 
                "family_icon", 
                "familyInspect",
                "Inspect This Person's Family Info"
            );
            inspectButton.onClick.AddListener(openWindow);

            treeButton = NewBGs.createBGWindowButton(
                scrollView,  
                -55, 
                "family_icon", 
                "familyTree",
                "Inspect This Person's Family Tree"
            );

            familyButton = NewBGs.createBGWindowButton(
                scrollView,  
                0, 
                "family_icon", 
                "CurrentFamily",
                "Inspect This Person's Current Family"
            );
        }

        public static void openWindow()
        {
            foreach(Transform child in contents.transform)
            {
                Destroy(child.gameObject);
            }
            if (FamilyActor.getFamily(Config.selectedUnit) != null)
            {
                Actor selectedActor = Config.selectedUnit;
                showFamilyInfo(selectedActor);
                treeButton.onClick.RemoveAllListeners();
                treeButton.onClick.AddListener(() => openIndividualWindow(selectedActor));
                Family thisFamily = FamilyOverviewWindow.getFromFamilies(ref FamilyActor.getFamily(selectedActor).familyIndex);
                if (thisFamily == null)
                {
                    familyButton.onClick.RemoveAllListeners();
                }
                else
                {
                    familyButton.onClick.RemoveAllListeners();
                    familyButton.onClick.AddListener(() => RelationsWindow.openWindow(thisFamily));
                }
            }
            else
            {
                showFamilySelect(Config.selectedUnit);
                treeButton.onClick.RemoveAllListeners();
                familyButton.onClick.RemoveAllListeners();
            }
            Windows.ShowWindow("familyInspectWindow");
            return;
        }

        private static void showFamilyInfo(Actor actor)
        {
            WindowManager.createdWindows["familyInspectWindow"].titleText.text = $"{actor.getName()}'s Family Info";
            contentRect.sizeDelta = originalContentSize;
            GameObject actorBG = NewBGs.createAvatarBG(contents, new Vector2(120, 120), new Vector3(60, -60, 0));
            NewBGs.createAvatar(actor, actorBG, 30, new Vector3(0, -30, 0), null);
            NewBGs.addText(actor.getName(), actorBG, 14, new Vector3(0, 60, 0));

            FamilyActor actorFamily = FamilyActor.getFamily(actor);

            deadActor lover = FamilyOverviewWindow.getDeadActor(actorFamily.deadLoverID);
            if (lover != null)
            {
                NewBGs.addText($"Lover: {lover.name}", contents, 8, new Vector3(130, -35, 0));
            }
            else
            {
                NewBGs.addText($"Lover: None", contents, 8, new Vector3(130, -35, 0));
            }
            deadActor father = FamilyOverviewWindow.getDeadActor(actorFamily.deadFatherID);
            if (father != null)
            {
                NewBGs.addText($"Father: {father.name}", contents, 8, new Vector3(130, -55, 0));
            }
            else
            {
                NewBGs.addText($"Father: None", contents, 8, new Vector3(130, -55, 0));
            }
            deadActor mother = FamilyOverviewWindow.getDeadActor(actorFamily.deadMotherID);
            if (mother != null)
            {
                NewBGs.addText($"Mother: {mother.name}", contents, 8, new Vector3(130, -75, 0));
            }
            else
            {
                NewBGs.addText($"Mother: None", contents, 8, new Vector3(130, -75, 0));
            }
            Config.selectedUnit = actor;
        }

        private static void showFamilySelect(Actor actor)
        {
            foreach(Transform child in WindowManager.windowContents["familyInspectWindow"].transform)
            {
                Destroy(child.gameObject);
            }
            WindowManager.createdWindows["familyInspectWindow"].titleText.text = "Select A Family";

            if (FamilyOverviewWindow.families.Count > 30)
            {
                int height = FamilyOverviewWindow.families.Count - 30;
                contentRect.sizeDelta += new Vector2(0, height * 30);
            }
            int posX = -1;
            int posY = 0;
            foreach(KeyValuePair<string, Family> kv in FamilyOverviewWindow.families)
            {
                if (posX > 3)
                {
                    posX = -1;
                    posY++;
                }
                GameObject familyHolder = NewBGs.createFamilyElement(WindowManager.windowContents["familyInspectWindow"], kv.Value, posX, posY);
                familyHolder.transform.Find("bannerHolder").GetComponent<Button>().onClick.AddListener(() => addUnitToFamily(kv.Value, actor));
                posX++;
            }
            Config.selectedUnit = actor;
        }

        private static void addUnitToFamily(Family family, Actor actor)
        {
            FamilyActor actorFamily = actor.gameObject.AddComponent<FamilyActor>();
            actorFamily.familyName = actor.getName();
            actorFamily.founderName = family.founderName;
            actorFamily.familyIndex = family.index;
            actorFamily.deadID = "dead_" + FamilyWindow.nextID().ToString();
            if (actor.data.gender == ActorGender.Male)
            {
                actorFamily.isMale = true;
            }
            else
            {
                actorFamily.isMale = false;
            }

            Actor curHeir = NewActions.getActorByIndex(family.heirID, family.index);
            Actor curHead = NewActions.getActorByIndex(family.HEADID, family.index);
            // if (curHead == null && curHeir != null)
            // {
            //     curFamily.HEADID = curFamily.heirID;
            //     FamilyActor.getFamily(curHeir).isHead = true;
            //     FamilyActor.getFamily(curHeir).isHeir = false;
            //     curFamily.heirID = null;
            // }
            if (curHead == null && curHeir == null)
            {
                family.HEADID = actor.data.actorID;
                actorFamily.isHead = true;
            }

            family.actors.Add(actor);

            FamilyOverviewWindow.deadActorList.Add(
                actorFamily.deadID,
                new deadActor().copyFamily(actorFamily, actor.getName())
            );

            foreach(Transform child in contents.transform)
            {
                Destroy(child.gameObject);
            }

            treeButton.onClick.RemoveAllListeners();
            treeButton.onClick.AddListener(() => openIndividualWindow(actor));
            familyButton.onClick.RemoveAllListeners();
            familyButton.onClick.AddListener(() => RelationsWindow.openWindow(family));
            showFamilyInfo(actor);
            Localization.AddOrSet("add_newUnit_family_dej", "$name$ Has Been Added To Family!");
            WorldTip.addWordReplacement("$name$", actor.coloredName);
            WorldTip.showNowTop("add_newUnit_family_dej");
        }

        public static void openIndividualWindow(Actor actor)
        {
            WindowManager.createdWindows["familyInspectWindow"].titleText.text = $"{actor.getName()}'s Tree";
            FamilyActor actorFamily = FamilyActor.getFamily(actor);
            Family family = FamilyOverviewWindow.getFromFamilies(ref actorFamily.familyIndex);

            foreach(Transform child in WindowManager.windowContents["familyInspectWindow"].transform)
            {
                Destroy(child.gameObject);
            }
            contentRect.sizeDelta = originalContentSize;
            // currentFamily = family;
            string IDsetup = actor.data.actorID;
            GameObject structure = FamilyWindow.createFamilyStructure(WindowManager.windowContents["familyInspectWindow"], actor, IDsetup, new Vector3(130, -80, 0));
            createReverseStructure(structure, actorFamily);
            if (contentRect.sizeDelta.x > 200)
            {
                structure.transform.localPosition = new Vector3((contentRect.sizeDelta.x/2)+100, -80, 0);
                contentRect.localPosition = new Vector3(contentRect.sizeDelta.x/-2, contentRect.localPosition.x, contentRect.localPosition.y);
            }
            structure.transform.localScale = currentZoom;
            Windows.ShowWindow("familyInspectWindow");
        }

        private static void createReverseStructure(GameObject parent, FamilyActor actorFamily)
        {
            if (actorFamily.fatherID == null)
            {
                return;
            }
            int counter = 1;
            GameObject structure = fatherLoop(parent, actorFamily.fatherID, new Vector3(-80, 130, 0), actorFamily, null, ref counter);
            contentRect.sizeDelta += new Vector2(120, (counter*55));
            contentRect.localPosition -= new Vector3(100, (counter*55), 0);
            parent.transform.localPosition -= new Vector3(-70, (counter*50), 0);
            return;
        }

        private static GameObject fatherLoop(GameObject parent, string fatherID, Vector3 pos, FamilyActor actorFamily, deadActor deadFamily, ref int counter)
        {
            GameObject structure = new GameObject("parentsHolder");
            structure.transform.SetParent(parent.transform);
            structure.transform.localPosition = pos;

            GameObject tempObj = null;
            NewBGs.deadOrAliveActorBG(structure, fatherID, new Vector3(-80, 0, 0), ref tempObj);

            string loverID = null;
            string newFatherID = null;
            FamilyActor newFatherFamily = null;
            deadActor newDeadFather = null;
            if (fatherID == null)
            {
                return null;
            }
            else if (fatherID.Contains("dead"))
            {
                deadActor actorDead = FamilyOverviewWindow.getDeadActor(fatherID);
                loverID = actorDead.loverID;
                newFatherID = actorDead.fatherID;
                newDeadFather = actorDead;
            }
            else
            {
                Actor father = null;
                if (actorFamily != null)
                {
                    father = NewActions.getActorByIndex(fatherID, actorFamily.familyIndex, actorFamily.fatherFamilyIndex, actorFamily.motherFamilyIndex);
                }
                else
                {
                    father = NewActions.getActorByIndex(fatherID, deadFamily.familyIndex, deadFamily.fatherFamilyIndex, deadFamily.motherFamilyIndex);
                }
                newFatherFamily = FamilyActor.getFamily(father);
                loverID = newFatherFamily.loverID;
                newFatherID = newFatherFamily.fatherID;
            }

            if (loverID == null)
            {
                return structure;
            }
            Vector3 loverPos = new Vector3(80, 0, 0);
            NewBGs.deadOrAliveActorBG(structure, loverID, loverPos, ref tempObj);

            if (newFatherID != null && newFatherFamily != null)
            {
                counter++;
                fatherLoop(structure, newFatherID, new Vector3(-80, 130, 0), newFatherFamily, null, ref counter);
            }
            else if (newFatherID != null && newDeadFather != null)
            {
                counter++;
                fatherLoop(structure, newFatherID, new Vector3(-80, 130, 0), null, newDeadFather, ref counter);
            }

            structure.transform.localScale = new Vector3(1, 1, 1);
            return structure;
        }
    }
}
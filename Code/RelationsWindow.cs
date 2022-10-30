using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;

namespace FamilyTreeMod
{
    class RelationsWindow : MonoBehaviour
    {
        private static GameObject scrollView;
        private static GameObject headBG;
        private static GameObject heirBG;
        private static GameObject infoBG;
        private static Family currentFamily;

        public static void init()
        {
            scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/relationsWindow/Background/Scroll View");
            initSideButtons();
            initHeader();
            initBody();
        }

        private static void initSideButtons()
        {
            Button headsButton = NewBGs.createSideButton(scrollView, 40, new Vector2(80, 80), "familyOverview_icon2", "Family Head", "Open Head's Family Tree").GetComponent<Button>();
            headsButton.onClick.AddListener(() => FamilyWindow.openHeadWindow(currentFamily));
            Button curHeadButton = NewBGs.createSideButton(scrollView, 0, new Vector2(80, 80), "familyOverview_icon2", "Family History", "Open List of Previous Heads").GetComponent<Button>();
            curHeadButton.onClick.AddListener(() => FamilyWindow.openHeadHistoryWindow(currentFamily));
            Button familyButton = NewBGs.createSideButton(scrollView, -40, new Vector2(80, 80), "familyOverview_icon2", "Family Founder", "Open Founder's Family Tree").GetComponent<Button>();
            familyButton.onClick.AddListener(() => FamilyWindow.openFounderWindow(currentFamily));
        }

        private static void initHeader()
        {

            headBG = NewBGs.createAvatarBG(WindowManager.windowContents["relationsWindow"], new Vector2(120, 120), new Vector3(60, -30, 0));

            addSizedText("Family Head", headBG, 25, new Vector3(0, -70, 0));

            heirBG = NewBGs.createAvatarBG(WindowManager.windowContents["relationsWindow"], new Vector2(120, 120), new Vector3(105, -30, 0));

            addSizedText("Family Heir", heirBG, 25, new Vector3(0, -65, 0));
        }

        private static void initBody()
        {
            infoBG = new GameObject("infoBG");
            infoBG.transform.SetParent(WindowManager.windowContents["relationsWindow"].transform);
            Image BGImage = infoBG.AddComponent<Image>();
            BGImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.windowInnerSliced.png");
            RectTransform BGRect = infoBG.GetComponent<RectTransform>();
            BGRect.localPosition = new Vector3(130, -140, 0);
            BGRect.sizeDelta = new Vector2(580, 440);
        }

        public static void openWindow(Family family)
        {
            if (headBG.transform.childCount > 2)
            {
                if (headBG.transform.GetChild(0).childCount > 0)
                {
                    Destroy(headBG.transform.GetChild(0).GetChild(0).gameObject);
                }
                Destroy(headBG.transform.GetChild(2).gameObject);
            }
            if (heirBG.transform.childCount > 2)
            {
                if (heirBG.transform.GetChild(0).childCount > 0)
                {
                    Destroy(heirBG.transform.GetChild(0).GetChild(0).gameObject);
                }
                Destroy(heirBG.transform.GetChild(2).gameObject);
            }
            foreach (Transform child in infoBG.transform)
            {
                Destroy(child.gameObject);
            }
            WindowManager.createdWindows["relationsWindow"].titleText.text = $"{family.founderName} Family";

            Actor headActor = NewActions.getActorByIndex(family.HEADID, family.index);
            GameObject headObj = NewBGs.createAvatar(headActor, headBG, 30, new Vector3(0, -30, 0));
            GameObject headName = headBG.transform.GetChild(0).gameObject;
            string headActorName = "";
            if (headActor != null)
            {
                addSizedText(headActor.getName(), headName, 20, new Vector3(0, 0, 0));
                headActorName = headActor.getName();
                switch (headActor.data.profession)
                {
                    case UnitProfession.Unit:
                        family.title = "Peasant";
                        break;
                    case UnitProfession.Warrior:
                        family.title = "Knight";
                        break;
                    case UnitProfession.Leader:
                        family.title = "Noble";
                        break;
                    case UnitProfession.King:
                        family.title = "Royalty";
                        break;
                    default:
                        family.title = "Peasant";
                        break;
                }
            }

            Actor heirActor = NewActions.getActorByIndex(family.heirID, family.index);
            GameObject heirObj = NewBGs.createAvatar(heirActor, heirBG, 30, new Vector3(0, -30, 0));
            GameObject heirName = heirBG.transform.GetChild(0).gameObject;
            if (heirActor != null)
            {
                addSizedText(heirActor.getName(), heirName, 20, new Vector3(0, 0, 0));
            }

            List<Actor> toRemove = new List<Actor>();
            foreach(Actor actor in family.actors)
            {
                if(actor == null || !actor.data.alive)
                {
                    toRemove.Add(actor);
                }
            }

            foreach(Actor removeActor in toRemove)
            {
                family.actors.Remove(removeActor);
            }

            currentFamily = family;

            string labels = @"
            Current Head:

            Founder:

            Age:

            Generations:

            Title:

            Popularity:

            Alive Members:";
            NewBGs.addText(labels, infoBG, 20, new Vector3(-60, 20, 0)).alignment = TextAnchor.UpperLeft;

            string stats = @$"
            {headActorName}

            {family.founderName}

            {MapBox.instance.mapStats.year - family.founderDate}

            {family.currentGeneration}

            {family.title}

            ??

            {family.actors.Count}";
            NewBGs.addText(stats, infoBG, 20, new Vector3(20, 20, 0)).alignment = TextAnchor.UpperRight;
            // Debug.Log($"headID: {family.headID}");
            // Debug.Log($"heirID: {family.heirID}");
            // Debug.Log($"Total Members: {family.actors.Count}");
            Windows.ShowWindow("relationsWindow");
        }

        private static Text addSizedText(string text, GameObject pObject, int size, Vector3 pos)
        {
            int newSize = size;
            if (text.Length > 6)
            {
                newSize = size - (text.Length - 6);
            }
            return NewBGs.addText(text, pObject, newSize, pos);
        }
    }
}
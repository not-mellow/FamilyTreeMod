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
        private static GameObject familyButton;
        public static GameObject scrollView;
        private static ActorParent currentHead;
        private static ActorHead infoHead;
        public static Dictionary<Kingdom, string> royalFamilies = new Dictionary<Kingdom, string>();
        public static Dictionary<City, string> nobleFamilies = new Dictionary<City, string>();

        public static void init()
        {
            scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/relationsWindow/Background/Scroll View");
        }

        public static void showRelations(ActorParent actorParent)
        {
            currentHead = actorParent;
            foreach(KeyValuePair<ActorParent, ActorHead> kv in FamilyOverviewWindow.familyHeads)
            {
                if(kv.Key == currentHead)
                {
                    infoHead = kv.Value;
                    break;
                }
            }
            familyButton = NewActions.createFamilyButton(scrollView, actorParent.originalFamily);
            showInfo();
            Windows.ShowWindow("relationsWindow");
        }

        public static void showInfo()
        {
            // Delete all the shit
            foreach(Transform child in WindowManager.windowContents["relationsWindow"].transform)
            {
                Destroy(child.gameObject);
            }

            WindowManager.createdWindows["relationsWindow"].titleText.text = $"{currentHead.originalFamily.familyName} Family";

            addAvatar();
            addRelations();
            addInfo();
        }

        public static void addAvatar()
        {
            // Avatar and Background
            GameObject avatarHolder = new GameObject("avatarHolder");
            avatarHolder.transform.SetParent(WindowManager.windowContents["relationsWindow"].transform);
            RectTransform avatarHolderRect = avatarHolder.AddComponent<RectTransform>();
            avatarHolderRect.localPosition = new Vector3(70, -40, 0);
            avatarHolderRect.sizeDelta = new Vector2(190, 190);
            Image avatarBG = avatarHolder.AddComponent<Image>();
            avatarBG.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.windowAvatarElement.png");

            GameObject banner = NewActions.addBanner(avatarHolder, currentHead.parentActor.kingdom, currentHead);
            RectTransform bannerRect = banner.AddComponent<RectTransform>();
            bannerRect.localPosition = new Vector3(0, 0, 0);
            foreach(Transform bannerChild in banner.transform)
            {
                if(bannerChild.gameObject.name == "deadBanner")
                {
                    continue;
                }
                RectTransform bannerChildRect = bannerChild.GetComponent<RectTransform>();
                bannerChildRect.sizeDelta = new Vector2(200, 200);
            }

            GameObject avatar = NewActions.addUnitAvatar(currentHead.parentActor, WindowManager.windowContents["relationsWindow"]);
            avatar.transform.SetParent(avatarHolder.transform);
            RectTransform avatarRect = avatar.GetComponent<RectTransform>();
            avatarRect.localPosition = new Vector3(70, -50, 0);
            if(avatar.transform.GetChildCount() >= 1)
            {
                GameObject avatarImage = avatar.transform.GetChild(0).gameObject;
                RectTransform avatarImageRect = avatarImage.GetComponent<RectTransform>();
                avatarImageRect.sizeDelta -= new Vector2(15, 15);
                avatarImageRect.localPosition = new Vector3(0, -7, 0);
            }

            if (avatar.transform.GetChildCount() >= 2)
            {
                GameObject avatarItem = avatar.transform.GetChild(1).gameObject;
                RectTransform avatarItemRect = avatarItem.GetComponent<RectTransform>();
                avatarItemRect.sizeDelta -= new Vector2(4, 8);
                avatarItemRect.localPosition = new Vector3(-5, 0, 0);
            }
        }

        public static void addRelations()
        {
            GameObject relationsBar = new GameObject("relationsBar");
            relationsBar.transform.SetParent(WindowManager.windowContents["relationsWindow"].transform);
            Image relationsImage = relationsBar.AddComponent<Image>();
            relationsImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.windowInnerSliced.png");

            RectTransform relationsRect = relationsBar.GetComponent<RectTransform>();
            relationsRect.localPosition = new Vector3(170, -40);
            relationsRect.sizeDelta = new Vector2(350, 70);

            Text relationsText = NewActions.addText("Relations", relationsBar, 30);
            Color textColor = relationsText.color;
            relationsText.color = new Color(textColor.r, textColor.g, textColor.b, 0.2f);
            RectTransform textRect = relationsText.gameObject.GetComponent<RectTransform>();
            textRect.localPosition = new Vector3(0, 0, 0);

        }

        public static void addInfo()
        {
            GameObject infoHolder = new GameObject("infoHolder");
            infoHolder.transform.SetParent(WindowManager.windowContents["relationsWindow"].transform);
            Image infoBG = infoHolder.AddComponent<Image>();
            infoBG.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.windowInnerSliced.png");

            RectTransform infoRect = infoHolder.GetComponent<RectTransform>();
            infoRect.localPosition = new Vector3(130, -150, 0);
            infoRect.sizeDelta = new Vector2(550, 400);

            string infoCaptions = @"
            Current Head:

            Age:

            Generations:

            Title:

            Popularity:

            Alive Members:";

            Text textOne = NewActions.addText(infoCaptions, infoHolder, 20);
            textOne.alignment = TextAnchor.UpperLeft;
            RectTransform textOneRect = textOne.gameObject.GetComponent<RectTransform>();
            textOneRect.localPosition = new Vector3(-30, 70, 0);

            string infoText = @$"
            {currentHead.parentActor.getName()}

            {infoHead.getAge()}

            {infoHead.currentGeneration}

            {infoHead.getTitle()}

            ??

            {infoHead.numOfMembers}";

            Text textTwo = NewActions.addText(infoText, infoHolder, 20);
            textTwo.alignment = TextAnchor.UpperRight;
            RectTransform textTwoRect = textTwo.gameObject.GetComponent<RectTransform>();
            textTwoRect.localPosition = new Vector3(-5, 70, 0);
        }
    }
}
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
    class NewBGs : MonoBehaviour
    {
        public static GameObject createBanner(GameObject parent, Kingdom kingdom, Vector2 size)
        {
            GameObject bannerGO = new GameObject("bannerHolder");
            bannerGO.transform.SetParent(parent.transform);
            bannerGO.AddComponent<CanvasRenderer>();
            bannerGO.transform.localPosition = new Vector3(0, 0,0);
            bannerGO.AddComponent<Button>();

            if (kingdom == null)
            {
                GameObject deadBanner = new GameObject("deadBanner");
                deadBanner.transform.SetParent(bannerGO.transform);
                Image deadImage = deadBanner.AddComponent<Image>();
                deadImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.ghost_icon.png");
                RectTransform deadRect = deadBanner.GetComponent<RectTransform>();
                deadRect.localPosition = new Vector3(0, 0, 0);
                deadRect.sizeDelta = size - new Vector2(size.x/2, size.y/2);
                return bannerGO;
            }else if (kingdom.id == "nomads_human" || kingdom.id == "nomads_elf" || kingdom.id == "nomads_orc" || kingdom.id == "nomads_dwarf" || kingdom.id == "mad")
            {
                GameObject nomadBanner = new GameObject("nomadBanner");
                nomadBanner.transform.SetParent(bannerGO.transform);
                Image nomadImage = nomadBanner.AddComponent<Image>();
                nomadImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.icon_tech_house_tier_0.png");
                RectTransform nomadRect = nomadBanner.GetComponent<RectTransform>();
                nomadRect.localPosition = new Vector3(0, 0, 0);
                nomadRect.sizeDelta = size;
                return bannerGO;
            }

            GameObject backgroundGO = new GameObject("background");
            backgroundGO.transform.SetParent(bannerGO.transform);
            Image backgroundImage = backgroundGO.AddComponent<Image>();
            RectTransform bgRect = backgroundGO.GetComponent<RectTransform>();
            bgRect.localPosition = new Vector3(0, 0, 0);
            bgRect.sizeDelta = size;

            GameObject iconGO = new GameObject("icon");
            iconGO.transform.SetParent(bannerGO.transform);
            Image iconImage = iconGO.AddComponent<Image>();
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.localPosition = new Vector3(0, 0, 0);
            iconRect.sizeDelta = size;

            BannerLoader bannerLoader = bannerGO.AddComponent<BannerLoader>();
            bannerLoader.partIcon = iconImage;
            bannerLoader.partBackround = backgroundImage;
            bannerLoader.load(kingdom);

            return bannerGO;
        }

        public static GameObject createAvatar(Actor actor, GameObject parent, int size, Vector3 pos, string deadID = null)
        {
            if (actor == null || !actor.data.alive)
            {
                var ghostObject = new GameObject("ghostAvatar");
                ghostObject.transform.SetParent(parent.transform);
                RectTransform ghostRectTransform = ghostObject.AddComponent<RectTransform>();
                ghostRectTransform.localPosition = pos + new Vector3(0, 30, 0);
                ghostRectTransform.sizeDelta = new Vector2(size*2, size*2);
                Image img = ghostObject.AddComponent<Image>();
                img.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.ghost_icon.png");

                if (deadID != null)
                {
                    ghostObject.AddComponent<Button>().onClick.AddListener(() => deadOnClick(deadID));
                }
                return ghostObject;
            }
            var avatarObject = new GameObject("avatar");
            avatarObject.transform.SetParent(parent.transform);
            RectTransform rectTransform = avatarObject.AddComponent<RectTransform>();
            rectTransform.localPosition = pos;
            rectTransform.sizeDelta = new Vector2(100, 100);
            var avatarLoader = avatarObject.AddComponent<UnitAvatarLoader>() as UnitAvatarLoader;
            var avatarButton = avatarObject.AddComponent<Button>() as Button;
            avatarButton.onClick.AddListener(() => avatarOnClick(actor));
            avatarLoader.load(actor);
            GameObject avatarImage = avatarObject.transform.GetChild(0).gameObject;
            GameObject avatarItem = null;
            if (avatarObject.transform.GetChildCount() == 2)
            {
                avatarItem = avatarObject.transform.GetChild(1).gameObject;
            }
            RectTransform avatarImageRect = avatarImage.GetComponent<RectTransform>();
            avatarImageRect.localPosition = new Vector3(0, 0, 0);
            avatarImageRect.sizeDelta = new Vector2(size, size);
            if (avatarItem != null)
            {
                RectTransform avatarItemRect = avatarItem.GetComponent<RectTransform>();
                avatarItemRect.localPosition = new Vector3(-size/3, size/2, 0);
                avatarItemRect.sizeDelta = new Vector2(size/3, size/2);
            }
            return avatarObject;
        }

        private static void avatarOnClick(Actor actor)
        {
            Config.selectedUnit = actor;
            Windows.ShowWindow("inspect_unit");
        }

        private static void deadOnClick(string deadID)
        {
            DeadActorWindow.openWindow(deadID);
        }

        public static GameObject createCrownIcon(GameObject parent, string title, Vector3 pos)
        {
            GameObject crownIcon = new GameObject("Crown Icon");
            crownIcon.transform.SetParent(parent.transform);
            Image crownImage = crownIcon.AddComponent<Image>();
            crownImage.sprite = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.Icons.{title}_icon.png");
            RectTransform crownRect = crownIcon.GetComponent<RectTransform>();
            crownRect.localPosition = pos;
            crownRect.sizeDelta -= new Vector2(20, 20);
            if(title == "Royalty" || title == "Noble")
            {
                crownRect.sizeDelta -= new Vector2(30, 30);
            }
            return crownIcon;
        }
        public static GameObject createProgressBar(GameObject parent)
        {
            GameObject researchBar = GameObjects.FindEvenInactive("HealthBar");
            GameObject progressBar = Instantiate(researchBar, parent.transform);
            progressBar.name = "ProgressBar";
            progressBar.SetActive(true);

            RectTransform progressRect = progressBar.GetComponent<RectTransform>();
            progressRect.localPosition = new Vector3(0, 0, 0);

            StatBar statBar = progressBar.GetComponent<StatBar>();
            statBar.restartBar();

            TipButton tipButton = progressBar.GetComponent<TipButton>();
            tipButton.textOnClick = "Progress Bar";

            GameObject icon = progressBar.transform.Find("Icon").gameObject;
            icon.SetActive(false);

            return progressBar;
        }

        public static GameObject createCanvasRenderer(GameObject parent, Vector3 pos)
        {
            GameObject bgHolder = new GameObject("bgHolder");
            bgHolder.transform.SetParent(parent.transform);
            bgHolder.AddComponent<CanvasRenderer>();
            Image bgImage = bgHolder.AddComponent<Image>();
            var newSprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.windowInnerSliced.png");
            bgImage.sprite = newSprite;
            RectTransform bgRect = bgHolder.GetComponent<RectTransform>();
            bgRect.localPosition = pos;
            bgRect.sizeDelta = new Vector2(180, 140);
            bgRect.localScale = new Vector3(0, 0, 0.68f);

            return bgHolder;
        }

        public static Text addText(string textString, GameObject parent, int sizeFont, Vector3 pos)
        {
            GameObject textRef = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyWindow/Background/Name");
            GameObject textGo = Instantiate(textRef, parent.transform);
            textGo.SetActive(true);

            var textComp = textGo.GetComponent<Text>();
            textComp.text = textString;
            textComp.fontSize = sizeFont;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.position = new Vector3(0,0,0);
            textRect.localPosition = pos;
            textRect.sizeDelta = new Vector2(textComp.preferredWidth, textComp.preferredHeight);
        
            return textComp;
        }

        public static GameObject createRedButton(GameObject parent, Vector2 size, Vector3 pos)
        {
            GameObject redButton = new GameObject("RedButton");
            redButton.transform.SetParent(parent.transform);
            Image buttonImage = redButton.AddComponent<Image>();
            buttonImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.special_buttonRed_insides.png");
            RectTransform buttonRect = redButton.GetComponent<RectTransform>();
            buttonRect.localPosition = pos;
            buttonRect.sizeDelta = size;

            return redButton;
        }

        public static GameObject createSideButton(GameObject parent, int posY, Vector2 iconSize, string iconName, string buttonName, string buttonDesc)
        {
            PowerButton button = PowerButtons.CreateButton(
                buttonName,
                Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.Icons.{iconName}.png"),
                buttonName,
                buttonDesc,
                new Vector2(118, posY),
                ButtonType.Click,
                parent.transform,
                null
            );

            Image buttonBG = button.gameObject.GetComponent<Image>();
            buttonBG.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.backgroundTabButton.png");

            return button.gameObject;
        }

        public static GameObject createFamilyElement(GameObject parent, Family familyValue, int posX, int posY)
        {
            Actor head = NewActions.getActorByIndex(familyValue.HEADID, familyValue.index);
            GameObject familyHolder = new GameObject("familyHolder");
            familyHolder.transform.SetParent(parent.transform);
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
            NewBGs.createAvatar(head, familyHolder, 20, new Vector3(20, -30, 0));
            NewBGs.addText(familyValue.founderName, familyHolder, 20, new Vector3(0, -50, 0));
            return familyHolder;
        }

        public static GameObject createAvatarBG(GameObject parent, Vector2 size, Vector3 pos)
        {
            GameObject avatarBG = new GameObject("avatarBG");
            avatarBG.transform.SetParent(parent.transform);
            Image BGImage = avatarBG.AddComponent<Image>();
            BGImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.windowAvatarElement.png");
            RectTransform BGRect = avatarBG.GetComponent<RectTransform>();
            BGRect.localPosition = pos;
            BGRect.sizeDelta = size;

            GameObject nameHolder = new GameObject("nameHolder");
            nameHolder.transform.SetParent(avatarBG.transform);
            Image nameImage = nameHolder.AddComponent<Image>();
            nameImage.sprite = Mod.EmbededResources.LoadSprite($"{Mod.Info.Name}.Resources.UI.longButton.png");
            RectTransform nameRect = nameHolder.GetComponent<RectTransform>();
            nameRect.localPosition = new Vector3(0, 55, 0);
            nameRect.sizeDelta = new Vector2(120, 40);
            return  avatarBG;
        }

        public static Button createBGWindowButton(GameObject parent, int posY, string iconName, string buttonName, string buttonDesc)
        {
            PowerButton button = PowerButtons.CreateButton(
                buttonName,
                Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.Icons.{iconName}.png"),
                buttonName,
                buttonDesc,
                new Vector2(118, posY),
                ButtonType.Click,
                parent.transform,
                null
            );

            Image buttonBG = button.gameObject.GetComponent<Image>();
            buttonBG.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.backgroundTabButton.png");
            Button buttonButton = button.gameObject.GetComponent<Button>();
            buttonBG.rectTransform.localScale = Vector3.one;

            // buttonButton.onClick.AddListener(() => checkToggledIcon(toggleIcon, name));

            return buttonButton;
        }

        public static List<string> deadOrAliveActorBG(GameObject parent, string actorID, Vector3 pos, ref GameObject actorBG)
        {
            List<string> newChildrenID = new List<string>();
            Actor actor = MapBox.instance.getActorByID(actorID);
            actorBG = NewBGs.createAvatarBG(parent, new Vector2(100, 100), pos);
            if (actor != null && actor.data.alive && FamilyActor.getFamily(actor) != null)
            {
                NewBGs.createAvatar(actor, actorBG, 30, new Vector3(0, -30, 0));
                GameObject name = actorBG.transform.GetChild(0).gameObject;
                addSizedText(actor.getName(), name, 20, new Vector3(0, 0, 0));
                newChildrenID = FamilyActor.getFamily(actor).childrenID;
            }
            else if (actorID != null && actorID.Contains("dead"))
            {
                deadActor dead = FamilyOverviewWindow.getDeadActor(actorID);
                NewBGs.createAvatar(null, actorBG, 30, new Vector3(0, -30, 0), actorID);
                GameObject name = actorBG.transform.GetChild(0).gameObject;
                addSizedText(dead.name, name, 20, new Vector3(0, 0, 0));
                newChildrenID = dead.childrenID;
            }
            return newChildrenID;
        }

        public static Text addSizedText(string text, GameObject parent, int size, Vector3 pos)
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
            return NewBGs.addText(text, parent, newSize, pos);
        }
    }
}
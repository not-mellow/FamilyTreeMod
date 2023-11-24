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
// using UnityEngine.EventSystems;
using UnityEngine.UI;
// using ReflectionUtility;

namespace FamilyTreeMod
{
    public class UI : MonoBehaviour
    {
        public static Dictionary<int, string> ButtonsToRename = new Dictionary<int, string>();
        public static Dictionary<string, bool> ToggleValues = new Dictionary<string, bool>();
        public static Dictionary<string, PowerButton> CustomButtons = new Dictionary<string, PowerButton>();
        public static Dictionary<string, ScrollWindow> AllWindows = new Dictionary<string, ScrollWindow>();
        public static GameObject avatarRef = null;
        private static GameObject textRef;


        public static void init()
		{
            AllWindows = ScrollWindow.allWindows;

            ScrollWindow.checkWindowExist("inspect_unit");
            AllWindows["inspect_unit"].gameObject.SetActive(false);
            avatarRef = GameObject.Find(
                $"Canvas Container Main/Canvas - Windows/windows/inspect_unit/Background/Scroll View/Viewport/Content/Part 1/BackgroundAvatar"
            );
		}

        #region PowerButton Code
        public static PowerButton CreateButton(string name, Sprite sprite, string localName, string localDescription, Vector2 position, ButtonType type = ButtonType.Click, Transform parent = null, UnityAction call = null)
        {
            Utils.AddOrSet(name, localName);
            Utils.AddOrSet(name + " Description", localDescription);
            bool flag = type == ButtonType.GodPower;
            GameObject gameObject2;
            if (flag)
            {
                GameObject gameObject = Utils.FindInActiveObjectByName("inspect");
                gameObject.SetActive(false);
                gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
                gameObject.SetActive(true);
            }
            else
            {
                GameObject gameObject3 = Utils.FindInActiveObjectByName("WorldLaws");
                gameObject3.SetActive(false);
                gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject3);
                gameObject3.SetActive(true);
            }
            ButtonsToRename.Add(gameObject2.GetInstanceID(), name);
            gameObject2.SetActive(true);
            bool flag2 = parent != null;
            if (flag2)
            {
                gameObject2.transform.SetParent(parent);
            }
            Image component = gameObject2.transform.Find("Icon").GetComponent<Image>();
            component.sprite = sprite;
            gameObject2.transform.localPosition = position;
            Button component2 = gameObject2.GetComponent<Button>();
            component2.onClick.RemoveAllListeners();
            PowerButton component3 = gameObject2.GetComponent<PowerButton>();
            component3.open_window_id = string.Empty;
            component2.onClick = new Button.ButtonClickedEvent();
            bool flag3 = type == ButtonType.Click;
            if (flag3)
            {
                component3.type = PowerButtonType.Library;
            }
            else
            {
                bool flag4 = type == ButtonType.GodPower;
                if (flag4)
                {
                    component3.type = PowerButtonType.Active;
                }
                else
                {
                    bool flag5 = type == ButtonType.Toggle;
                    if (flag5)
                    {
                        GameObject gameObject4 = new GameObject("toggleIcon");
                        gameObject4.transform.SetParent(gameObject2.transform);
                        gameObject4.transform.localScale = new Vector3(0.12f, 0.07f, 1f);
                        gameObject4.transform.localPosition = new Vector3(0f, 14.57f, 0f);
                        Image image = gameObject4.AddComponent<Image>();
                        image.sprite = AssetLoader.cached_assets_list["FamilyTreeUI/buttonToggleIndicator_1.png"][0];
                        component3.type = PowerButtonType.Library;
                        ToggleValues.Add(name, false);
                        component2.onClick.AddListener(delegate
                        {
                            ToggleButton(name);
                        });
                    }
                }
            }
            bool flag6 = call != null;
            if (flag6)
            {
                component2.onClick.AddListener(call);
            }
            if (CustomButtons.ContainsKey(name))
            {
                CustomButtons.Remove(name);
            }
            CustomButtons.Add(name, component3);
            return component3;
        }

        public static void ToggleButton(string name)
		{
			bool flag = !ToggleValues.ContainsKey(name);
			if (!flag)
			{
				ToggleValues[name] = !ToggleValues[name];
				Image component = CustomButtons[name].transform.Find("toggleIcon").GetComponent<Image>();
				bool flag2 = ToggleValues[name];
				if (flag2)
				{
					component.sprite = AssetLoader.cached_assets_list["FamilyTreeUI/buttonToggleIndicator_0.png"][0];
				}
				else
				{
					component.sprite = AssetLoader.cached_assets_list["FamilyTreeUI/buttonToggleIndicator_1.png"][0];
				}
				return;
			}
			bool flag3 = !CustomButtons.ContainsKey(name);
			if (flag3)
			{
				throw new Exception("Button with name " + name + " doesn't exists");
			}
			throw new Exception("Button with name " + name + " isn't of type ButtonType.Toggle");
		}

        public static bool GetToggleValue(string name)
		{
			bool flag = !ToggleValues.ContainsKey(name);
			if (!flag)
			{
				return ToggleValues[name];
			}
			bool flag2 = !CustomButtons.ContainsKey(name);
			if (flag2)
			{
				throw new Exception("Button with name " + name + " doesn't exists");
			}
			throw new Exception("Button with name " + name + " isn't of type ButtonType.Toggle");
		}
        #endregion

        #region Windows & Tabs Code
        public static ScrollWindow CreateNewWindow(string windowId, string windowTitle)
        {
            bool flag = AllWindows.ContainsKey(windowId);
            ScrollWindow scrollWindow;
            if (flag)
            {
                scrollWindow = AllWindows[windowId];
            }
            else
            {
                ScrollWindow scrollWindow2 = (ScrollWindow)Resources.Load("windows/empty", typeof(ScrollWindow));
                ScrollWindow scrollWindow3 = UnityEngine.Object.Instantiate<ScrollWindow>(scrollWindow2, CanvasMain.instance.transformWindows);
                UnityEngine.Object.Destroy(scrollWindow3.titleText.GetComponent<LocalizedText>());
                scrollWindow3.screen_id = windowId;
                scrollWindow3.name = windowId;
                scrollWindow3.titleText.text = windowTitle;
                scrollWindow3.create(true);
                bool flag2 = AllWindows.ContainsKey(windowId);
                if (flag2)
                {
                    scrollWindow = AllWindows[windowId];
                }
                else
                {
                    AllWindows.Add(windowId, scrollWindow3);
                    scrollWindow = AllWindows[windowId];
                }
            }
            return scrollWindow;
        }

        public static void ShowWindow(string windowId)
        {
            bool flag = AllWindows.ContainsKey(windowId) && !ScrollWindow.currentWindows.Contains(GetWindow(windowId));
            if (flag)
            {
                AllWindows[windowId].clickShow();
            }
        }

        public static ScrollWindow GetWindow(string windowId)
        {
            bool flag = AllWindows.ContainsKey(windowId);
            ScrollWindow scrollWindow;
            if (flag)
            {
                scrollWindow = AllWindows[windowId];
            }
            else
            {
                scrollWindow = null;
            }
            return scrollWindow;
        }

        public static void createTab(string buttonID, string tabID, string name, string desc, int xPos)
        {
            GameObject OtherTabButton = Utils.FindInActiveObjectByName("Button_Other");
            if (OtherTabButton != null)
            {
                Utils.AddOrSet(buttonID, name);
                Utils.AddOrSet($"{buttonID} Description", desc);
                Utils.AddOrSet("dej_mod_creator",  "Made By: Dej#0594");
                Utils.AddOrSet(tabID, name);


                GameObject newTabButton = GameObject.Instantiate(OtherTabButton);
                newTabButton.transform.SetParent(OtherTabButton.transform.parent);
                Button buttonComponent = newTabButton.GetComponent<Button>();
                TipButton tipButton = buttonComponent.gameObject.GetComponent<TipButton>();
                tipButton.textOnClick = buttonID;
                tipButton.textOnClickDescription = $"{buttonID} Description";
                tipButton.text_description_2 = "dej_mod_creator";



                newTabButton.transform.localPosition = new Vector3(xPos, 49.62f);
                newTabButton.transform.localScale = new Vector3(1f, 1f);
                newTabButton.name = buttonID;

                Sprite spriteForTab = AssetLoader.cached_assets_list["FamilyTreeUI/icon.png"][0];
                newTabButton.transform.Find("Icon").GetComponent<Image>().sprite = spriteForTab;


                GameObject OtherTab = Utils.FindInActiveObjectByName("Tab_Other");
                foreach (Transform child in OtherTab.transform)
                {
                    child.gameObject.SetActive(false);
                }

                GameObject additionalPowersTab = GameObject.Instantiate(OtherTab);

                foreach (Transform child in additionalPowersTab.transform)
                {
                    if (child.gameObject.name == "tabBackButton" || child.gameObject.name == "-space")
                    {
                        child.gameObject.SetActive(true);
                        continue;
                    }

                    GameObject.Destroy(child.gameObject);
                }

                foreach (Transform child in OtherTab.transform)
                {
                    child.gameObject.SetActive(true);
                }


                additionalPowersTab.transform.SetParent(OtherTab.transform.parent);
                PowersTab powersTabComponent = additionalPowersTab.GetComponent<PowersTab>();
                powersTabComponent.powerButton = buttonComponent;
                powersTabComponent.powerButtons.Clear();


                additionalPowersTab.name = tabID;
                powersTabComponent.powerButton.onClick = new Button.ButtonClickedEvent();
                powersTabComponent.powerButton.onClick.AddListener(() => tabOnClick(tabID));
                powersTabComponent.parentObj = OtherTab.transform.parent.parent.gameObject;

                additionalPowersTab.SetActive(true);
                powersTabComponent.powerButton.gameObject.SetActive(true);

            }
        }

        public static void tabOnClick(string tabID)
        {
            GameObject AdditionalTab = Utils.FindInActiveObjectByName(tabID);
            PowersTab AdditionalPowersTab = AdditionalTab.GetComponent<PowersTab>();

            AdditionalPowersTab.showTab(AdditionalPowersTab.powerButton);
        }
        #endregion

        
        public static void createActorUI(Actor actor, GameObject parent, Vector3 pos, Vector3 newScale, DeadFamilyMember dead = null)
        {
            Config.selectedUnit = null;
            GameObject GO = Instantiate(avatarRef);
            GO.transform.SetParent(parent.transform);
            var avatarElement = GO.GetComponent<UiUnitAvatarElement>();
            avatarElement.show_banner_clan = true;
            avatarElement.show_banner_kingdom = true;
            if (actor != null && actor.isAlive())
            {
                avatarElement.show(actor);
            }
            RectTransform GORect = GO.GetComponent<RectTransform>();
            GORect.localPosition = pos;
            GORect.localScale = newScale;

            Button GOButton = GO.AddComponent<Button>();
            GOButton.onClick.AddListener(() => FamilyMemberWindow.openWindow(actor, dead));
            GO.AddComponent<GraphicRaycaster>();
            if ((actor == null || !actor.isAlive()) && dead != null)
            {
                GO.SetActive(true);
                GO.transform.Find("Mask").GetChild(0).gameObject.SetActive(true);
                GOButton.OnHover(
                    new UnityAction(delegate{
                    Utils.AddOrSet("AvatarNameHover", $"{dead.name} (DEAD)");
                    Utils.AddOrSet("AvatarDescHover", 
                    @$"This Person Was Once A {dead.prof}
                Family Index: {dead.familyIndex}
                Parent Index: {dead.parentIndex}
                Parent Index 2: {dead.parentIndex2}
                Children Index: {dead.childrenIndex.Split(',').Length}
                Member Index: {dead.memberIndex}"
                    );
                    Tooltip.show(GO, "normal", new TooltipData
                    {
                        tip_name = "AvatarNameHover",
                        tip_description = "AvatarDescHover"
                    });
                    })
                );
			    GOButton.OnHoverOut(new UnityAction(Tooltip.hideTooltip));
                GO.transform.Find("Mask").GetChild(1).gameObject.SetActive(false);
                return;
            }
            else if ((actor == null || !actor.isAlive()) && dead == null)
            {
                GO.SetActive(true);
                GO.transform.Find("Mask").GetChild(0).gameObject.SetActive(true);
                GOButton.OnHover(
                    new UnityAction(delegate{
                    Utils.AddOrSet("NullAvatarHover", "This Unit Is Either Missing Or In Their Mother's Womb");
                    Utils.AddOrSet("NullAvatarDescHover", "Who Knows");
                    Tooltip.show(GO, "normal", new TooltipData
                    {
                        tip_name = "NullAvatarHover",
                        tip_description = "NullAvatarDescHover"
                    });
                    })
                );
			    GOButton.OnHoverOut(new UnityAction(Tooltip.hideTooltip));
                GO.transform.Find("Mask").GetChild(1).gameObject.SetActive(false);
                return;
            }

            GOButton.OnHover(new UnityAction(() => actorTooltip(actor)));
			GOButton.OnHoverOut(new UnityAction(Tooltip.hideTooltip));

            Transform avatarTransform = GO.transform.Find("Mask").GetChild(1);
            avatarTransform.localPosition = new Vector3(0, 8, 0);
            Button avatarButton = avatarTransform.gameObject.AddComponent<Button>();
            avatarButton.onClick.AddListener(() => showActor(actor));
        }

        private static void actorTooltip(Actor actor)
        {
            if (actor == null)
            {
                return;
            }
            string text = "actor";
            if (actor.isKing())
            {
                text = "actor_king";
            }
            else if (actor.isCityLeader())
            {
                text = "actor_leader";
            }
            Tooltip.show(actor, text, new TooltipData
            {
                actor = actor,
            });
            return;
        }

        public static void showActor(Actor pActor)
        {
            Config.selectedUnit = pActor;
            ScrollWindow.showWindow("inspect_unit");
        }

        public static Button createBGWindowButton(GameObject parent, int posY, Sprite sprite, string buttonName, string buttonTitle, 
        string buttonDesc, UnityAction call)
        {
            PowerButton button = CreateButton(
                buttonName,
                sprite,
                buttonTitle,
                buttonDesc,
                new Vector2(118, posY),
                ButtonType.Click,
                parent.transform,
                call
            );

            Image buttonBG = button.gameObject.GetComponent<Image>();
            buttonBG.sprite = AssetLoader.cached_assets_list["FamilyTreeUI/backgroundTabButton.png"][0];
            Button buttonButton = button.gameObject.GetComponent<Button>();
            buttonBG.rectTransform.localScale = Vector3.one;

            return buttonButton;
        }

        public static Text addText(string textString, GameObject parent, int sizeFont, Vector3 pos, Vector2 addSize = default(Vector2))
        {
            textRef = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyUnitTreeWindow/Background/Title");
            GameObject textGo = Instantiate(textRef, parent.transform);
            textGo.SetActive(true);

            var textComp = textGo.GetComponent<Text>();
            textComp.fontSize = sizeFont;
            textComp.resizeTextMaxSize = sizeFont;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.position = new Vector3(0,0,0);
            textRect.localPosition = pos + new Vector3(0, -50, 0);
            textRect.sizeDelta = new Vector2(100, 100) + addSize;
            textGo.AddComponent<GraphicRaycaster>();
            textComp.text = textString;
        
            return textComp;
        }

    }

    public enum ButtonType
	{
		Click,
		GodPower,
		Toggle
	}
}
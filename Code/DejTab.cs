using System;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;

namespace FamilyTreeMod
{
    class DejTab
    {
        public static GameObject additionalPowersTab;
        public static PowersTab powersTabComponent;
        public static void init()
        {
            if (Config.gameLoaded)
            {
                var OtherTabButton = NCMS.Utils.GameObjects.FindEvenInactive("Button_Other");
                if (OtherTabButton != null)
                {
                    NCMS.Utils.Localization.addLocalization("newButton_Dej", "FamilyTreeMod");
                    NCMS.Utils.Localization.addLocalization("newTab_Dej", "FamilyTreeMod");


                    var newTabButton = GameObject.Instantiate(OtherTabButton);
                    newTabButton.transform.SetParent(OtherTabButton.transform.parent);
                    var buttonComponent = newTabButton.GetComponent<Button>();


                    newTabButton.transform.localPosition = new Vector3(-110f, 49.62f);
                    newTabButton.transform.localScale = new Vector3(1f, 1f);
                    newTabButton.name = "newButton_Dej";

                    var spriteForTab = NCMS.Utils.Sprites.LoadSprite($"{Mod.Info.Path}/icon.png");
                    newTabButton.transform.Find("Icon").GetComponent<Image>().sprite = spriteForTab;




                    var OtherTab = NCMS.Utils.GameObjects.FindEvenInactive("Tab_Other");
                    foreach (Transform child in OtherTab.transform)
                    {
                        child.gameObject.SetActive(false);
                    }

                    additionalPowersTab = GameObject.Instantiate(OtherTab);

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
                    powersTabComponent = additionalPowersTab.GetComponent<PowersTab>();
                    powersTabComponent.powerButton = buttonComponent;


                    additionalPowersTab.name = "Tab_Additional_Dej";
                    powersTabComponent.powerButton.onClick = new Button.ButtonClickedEvent();
                    powersTabComponent.powerButton.onClick.AddListener(Button_Dej_Powers_Click);
                    Reflection.SetField<GameObject>(powersTabComponent, "parentObj", OtherTab.transform.parent.parent.gameObject);
                    powersTabComponent.tipKey = "newTab_Dej";

                    additionalPowersTab.SetActive(true);
                    powersTabComponent.powerButton.gameObject.SetActive(false);
                }
            }
        }

        public static void Button_Dej_Powers_Click()
        {
            var AdditionalTab = NCMS.Utils.GameObjects.FindEvenInactive("Tab_Additional_Dej");
            var AdditionalPowersTab = AdditionalTab.GetComponent<PowersTab>();

            AdditionalPowersTab.showTab(AdditionalPowersTab.powerButton);
        }
    }
}
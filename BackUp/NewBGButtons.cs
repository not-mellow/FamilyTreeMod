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
    class NewBGButtons : MonoBehaviour
    {
        private static Actor currentActor = null;

        public static void familyInspectCheck()
        {
            if (FamilyOverviewWindow.familyActors.Contains(Config.selectedUnit) && currentActor != Config.selectedUnit)
            {
                currentActor = Config.selectedUnit;
                GameObject prevFamilyButton = GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/inspect_unit/Background/ButtonContainerFamily");
                if (prevFamilyButton != null)
                {
                    Destroy(prevFamilyButton);
                }
                foreach(ActorParent actorParent in FamilyOverviewWindow.families)
                {
                    if (actorParent.parentActor == Config.selectedUnit)
                    {
                        addFamilyInspectButton(GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/inspect_unit/Background"), actorParent);
                        break;
                    }
                }
            }
            else if (!FamilyOverviewWindow.familyActors.Contains(Config.selectedUnit) && currentActor != Config.selectedUnit)
            {
                currentActor = Config.selectedUnit;
                GameObject prevFamilyButton = GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/inspect_unit/Background/ButtonContainerFamily");
                if (prevFamilyButton != null)
                {
                    Destroy(prevFamilyButton);
                }
            }
        }
        public static GameObject addFamilyInspectButton(GameObject parent, ActorParent actorParent)
        {
            GameObject buttonHolderRef = GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/inspect_unit/Background/ButtonContainerTraits");
            GameObject buttonHolder = Instantiate(buttonHolderRef, parent.transform);
            buttonHolder.transform.name = "ButtonContainerFamily";
            Image buttonImage = buttonHolder.GetComponent<Image>();
            buttonImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.backgroundTabButton.png");
            RectTransform buttonRect = buttonHolder.GetComponent<RectTransform>();
            buttonRect.localPosition += new Vector3(0, -80, 0);
            Destroy(buttonHolder.transform.GetChild(0).gameObject);

            GameObject buttonGO = new GameObject("Button");
            buttonGO.transform.SetParent(buttonHolder.transform);
            Button buttonComp = buttonGO.AddComponent<Button>();
            buttonComp.onClick.AddListener(() => FamilyWindow.showFamily(actorParent, true));

            Image buttonIcon = buttonGO.AddComponent<Image>();
            buttonIcon.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.family_icon.png");

            RectTransform buttonImageRect = buttonGO.GetComponent<RectTransform>();
            buttonImageRect.localPosition = new Vector3(0, 0, 0);
            buttonImageRect.sizeDelta = new Vector2(50, 50);

            return buttonHolder;
        }

        // public static void searchFamilyUI(ActorParent actorParent)
        // {
        //     FamilyOverviewWindow.showFamily(actorParent.foundingFamily);
        //     return;
        // }
    }
}
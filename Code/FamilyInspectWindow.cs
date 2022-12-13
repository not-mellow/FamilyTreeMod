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

        public static void init()
        {
            contents = WindowManager.windowContents["familyInspectWindow"];
            GameObject inspectBG = GameObject.Find("Canvas Container Main/Canvas - Windows/windows/inspect_unit/Background");
            Button inspectButton = NewBGs.createBGWindowButton(
                inspectBG,  
                -55, 
                "family_icon", 
                "familyInspect",
                "Inspect This Person's Family Info"
            );
            inspectButton.onClick.AddListener(openWindow);
        }

        public static void openWindow()
        {
            Actor actor = Config.selectedUnit;
            foreach(Transform child in contents.transform)
            {
                Destroy(child.gameObject);
            }
            if (FamilyActor.getFamily(actor) != null)
            {
                Debug.Log(actor);
                showFamilyInfo(actor);
            }
            else
            {
                showFamilySelect();
            }
            Windows.ShowWindow("familyInspectWindow");
            return;
        }

        private static void showFamilyInfo(Actor actor)
        {
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
        }

        private static void showFamilySelect()
        {
            foreach(Transform child in WindowManager.windowContents["familyInspectWindow"].transform)
            {
                Destroy(child.gameObject);
            }

            if (FamilyOverviewWindow.families.Count > 30)
            {
                int height = FamilyOverviewWindow.families.Count - 30;
                WindowManager.windowContents["familyInspectWindow"].GetComponent<RectTransform>().sizeDelta += new Vector2(0, height * 30);
            }
            int posX = 0;
            int posY = 0;
            foreach(KeyValuePair<string, Family> kv in FamilyOverviewWindow.families)
            {
                if (posX > 4)
                {
                    posX = 0;
                    posY++;
                }
                GameObject familyHolder = NewBGs.createFamilyElement(WindowManager.windowContents["familyInspectWindow"], kv.Value, posX, posY);
                familyHolder.transform.Find("bannerHolder").GetComponent<Button>().onClick.AddListener(() => RelationsWindow.openWindow(kv.Value));
                posX++;
            }
        }
    }
}
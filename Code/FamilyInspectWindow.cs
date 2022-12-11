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
            NewBGs.createBGWindowButton(
                inspectBG,  
                -55, 
                "family_icon", 
                "familyInspect",
                "Inspect This Person's Family Info"
            );
        }

        public static void openWindow()
        {
            Actor actor = Config.selectedUnit;
            Debug.Log(actor);
            if (FamilyActor.getFamily(actor) != null)
            {
                showFamilyInfo(actor);
            }
            Windows.ShowWindow("familyInspectWindow");
            return;
        }

        private static void showFamilyInfo(Actor actor)
        {
            GameObject actorBG = NewBGs.createAvatarBG(contents, new Vector2(20, 20), new Vector3(60, -60, 0));
            NewBGs.createAvatar(actor, actorBG, 15, new Vector3(0, 0, 0), null);

            FamilyActor actorFamily = FamilyActor.getFamily(actor);
        }
    }
}
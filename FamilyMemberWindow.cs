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
using UnityEngine.UI;

namespace FamilyTreeMod
{
    public class FamilyMemberWindow : MonoBehaviour
    {
        private static Actor currentActor = null;
        private static DeadFamilyMember currentDead = null;
        private static GameObject contents;
        private static int currentMemberIndex;
        private static int currentFamilyIndex;

        public static void init()
        {
            contents = WindowManager.windowContents["familyMemberWindow"];

            UI.createBGWindowButton(
                GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyMemberWindow/Background"),
                0, 
                "", 
                "familyTreeButton", 
                "Family Tree", 
                "View This Unit's Family Tree", 
                () => FamilyUnitTreeWindow.openWindow(currentFamilyIndex, currentMemberIndex)
            );
            return;
        }

        public static void openWindow(Actor pActor, DeadFamilyMember pDead = null)
        {
            foreach(Transform child in contents.transform)
            {
                Destroy(child.gameObject);
            }

            currentActor = pActor;
            currentDead = pDead;
            loadContents();
            UI.ShowWindow("familyMemberWindow");
        }

        private static void loadContents()
        {
            UI.createActorUI(
                currentActor,
                contents,
                new Vector3(70, -40, 0),
                new Vector3(1, 1, 1),
                currentDead
            );

            int actorFamilyIndex = -1;
            string actorChildrenIndex = "";
            int actorParentIndex = -1;
            int actorParentIndex2 = -1;
            int actorMemberIndex = -1;
            int actorSpouseIndex = -1;
            if (currentActor == null && currentDead != null)
            {
                actorFamilyIndex = currentDead.familyIndex;
                actorChildrenIndex = currentDead.childrenIndex;
                actorParentIndex = currentDead.parentIndex;
                actorParentIndex2 = currentDead.parentIndex2;
                actorMemberIndex = currentDead.memberIndex;
                actorSpouseIndex = currentDead.spouseIndex;
            }
            else
            {
                currentActor.data.get("familyIndex", out actorFamilyIndex, -1);
                currentActor.data.get("childrenIndex", out actorChildrenIndex, "");
                currentActor.data.get("parentIndex", out actorParentIndex, -1);
                currentActor.data.get("parentIndex2", out actorParentIndex2, -1);
                currentActor.data.get("memberIndex", out actorMemberIndex, -1);
                currentActor.data.get("spouseIndex", out actorSpouseIndex, -1);
            }
            currentFamilyIndex = actorFamilyIndex;
            currentMemberIndex = actorMemberIndex;

            UI.addText(
                $"Family Index: {actorFamilyIndex}\nParent Index: {actorParentIndex}\nParent Index 2: {actorParentIndex2}\nMember Index: {actorMemberIndex}\nChildren Index: {actorChildrenIndex.Split(',').Length}\nSpouse Index: {actorSpouseIndex}",
                contents, 
                20,
                new Vector3(150, 0, 0),
                default(Vector2)
            );
        }
    }
}
using BepInEx;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;


namespace FamilyTreeMod
{
    public class Actions
    {
        public static void init()
        {
            return;
        }

        public static bool inspectFamilyMember(WorldTile pTile = null, string pPower = null)
        {
            Actor actor;
            if (pTile == null)
            {
                actor = World.world.getActorNearCursor();
            }
            else
            {
                actor = ActionLibrary.getActorFromTile(pTile);
            }
            if (actor == null)
            {
                return false;
            }
            Config.selectedUnit = actor;
            int actorFamilyIndex = -1;
            int actorMemberIndex = -1;
            actor.data.get("familyIndex", out actorFamilyIndex, -1);
            actor.data.get("memberIndex", out actorMemberIndex, -1);

            if (actorFamilyIndex != -1 && actorMemberIndex != -1)
            {
                FamilyUnitTreeWindow.openWindow(actorFamilyIndex, actorMemberIndex);
            }
            else
            {
                WorldTip.instance.show("ERROR: Unit Does Not Belong To A Family", false, "top", 3f);
            }
            return true;
        }

        public static bool createFamily(WorldTile pTile = null, string pPower = null)
        {
            Actor actor;
            if (pTile == null)
            {
                actor = World.world.getActorNearCursor();
            }
            else
            {
                actor = ActionLibrary.getActorFromTile(pTile);
            }
            if (actor == null)
            {
                return false;
            }
            Config.selectedUnit = actor;
            FamilyInfo.create_family(Config.selectedUnit);
            return true;
        }
    }
}
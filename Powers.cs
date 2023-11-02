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
    public class Powers
    {
        public static void init()
        {
            loadPowers();
        }

        private static void loadPowers()
        {
            GodPower t = AssetManager.powers.clone("inspectMember_Dej", "inspect");
            t.name = t.id;
            t.click_action = (PowerActionWithID)Delegate.Combine(t.click_action, new PowerActionWithID(Actions.inspectFamilyMember));
            t.allow_unit_selection = false;

            t = AssetManager.powers.clone("createFamily_Dej", "inspectMember_Dej");
            t.name = t.id;
            t.click_action = (PowerActionWithID)Delegate.Combine(t.click_action, new PowerActionWithID(Actions.createFamily));
        }
    }
}
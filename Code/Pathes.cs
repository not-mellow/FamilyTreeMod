using ReflectionUtility;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using NCMS;
using NCMS.Utils;
using Newtonsoft.Json;
using ai;
using ai.behaviours;
using HarmonyLib;

namespace FamilyTreeMod
{
    class Patches
    {
        public static Harmony harmony = new Harmony("dej.mymod.wb.familymodtwo");

        public static void init()
        {
            harmony.Patch(
                AccessTools.Method(typeof(City), "updateConquest"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "updateConquest_Prefix"))
            );
        }

        public static bool updateConquest_Prefix(Actor pActor, City __instance)
        {
            if (!pActor.kingdom.isCiv())
            {
                return false;
            }
            // if (pActor.kingdom.race != __instance.kingdom.race)
            // {
            //     return false;
            // }
            if (pActor.kingdom != __instance.kingdom && !pActor.kingdom.isEnemy(__instance.kingdom))
            {
                return false;
            }
            __instance.addCapturePoints(pActor, 1);
            return false;
        }
    }
}
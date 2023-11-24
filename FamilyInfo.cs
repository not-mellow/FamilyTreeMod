using BepInEx;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using Newtonsoft.Json;


namespace FamilyTreeMod
{
    [Serializable]
    public class FamilyInfo
    {
        public double worldTime;
        public int familyIndex = 0;
        public int memberIndex = 0;
        public string familyName = "";
        public Dictionary<string, DeadFamilyMember> deadMembers = new Dictionary<string, DeadFamilyMember>();
        public List<string> deadHeads = new List<string>();
        public List<string> deadFavs = new List<string>();

        public static FamilyInfo create_family(Actor pActor)
        {
            // Checks If There Is Currently A Save File For The World
            if (string.IsNullOrEmpty(SaveManager.currentSavePath))
            {
                WorldTip.instance.show("ERROR: World Not Saved Or Save Path Does Not Exist", false, "top", 3f);
                return null;
            }
            int hasFamily = -1;
            pActor.data.get("familyIndex", out hasFamily, -1);
            if (hasFamily >= 0)
            {
                WorldTip.instance.show("ERROR: Unit Already Has A Family", false, "top", 3f);
                return null;
            }

            //Add Info To Settings
            FamilyInfo info = new FamilyInfo();
            if (Plugin.settings.families.ContainsKey(SaveManager.currentSavePath))
            {
                Plugin.settings.families[SaveManager.currentSavePath].Add(info);
            }
            else
            {
                Plugin.settings.families.Add(SaveManager.currentSavePath, new List<FamilyInfo>{info});
            }
            info.worldTime = World.world.mapStats.worldTime;
            info.familyName = pActor.getName();
            info.familyIndex = Plugin.settings.families[SaveManager.currentSavePath].IndexOf(info);
            info.memberIndex++;

            //Add Info To Actor
            pActor.data.set("familyIndex", info.familyIndex);
            pActor.data.set("memberIndex", info.memberIndex);

            WorldTip.instance.show($"{pActor.getName()} Has Successfully Created A Family!", false, "top", 3f);
            return info;
        }

        public static void add_member(Actor pActor, Actor pActor2, ActorData pActorData)
        {
            int actorFamilyIndex = -1;
            pActor.data.get("familyIndex", out actorFamilyIndex, -1);
            if (actorFamilyIndex < 0)
            {
                return;
            }
            pActorData.set("familyIndex", actorFamilyIndex);

            FamilyInfo info = Plugin.settings.families[SaveManager.currentSavePath][actorFamilyIndex];
            info.memberIndex++;
            pActorData.set("memberIndex", info.memberIndex);

            //Add Child To Parent's ChildrenIndex
            string actorChildrenIndex = "";
            pActor.data.get("childrenIndex", out actorChildrenIndex, "");
            if (string.IsNullOrEmpty(actorChildrenIndex))
            {
                pActor.data.set("childrenIndex", $"{info.memberIndex}");
            }
            else
            {
                pActor.data.set("childrenIndex", $"{actorChildrenIndex},{info.memberIndex}");
            }

            string actorChildrenIndex2 = "";
            pActor2.data.get("childrenIndex", out actorChildrenIndex2, "");
            if (string.IsNullOrEmpty(actorChildrenIndex2))
            {
                pActor2.data.set("childrenIndex", $"{info.memberIndex}");
            }
            else
            {
                pActor2.data.set("childrenIndex", $"{actorChildrenIndex2},{info.memberIndex}");
            }

            //Add Parent To Children's ParentIndex
            int actorMemberIndex = 0;
            pActor.data.get("memberIndex", out actorMemberIndex, 0);
            pActorData.set("parentIndex", actorMemberIndex);
            int actorMemberIndex2 = 0;
            pActor2.data.get("memberIndex", out actorMemberIndex2, 0);
            pActorData.set("parentIndex2", actorMemberIndex2);
            WorldTip.instance.show("Unit has been added to family", false, "top", 3f);

            //Add Last Name To Child
            pActorData.generateName(pActor.asset, pActor.race);
            if (!string.IsNullOrEmpty(info.familyName))
            {
                pActorData.setName($"{pActorData.name} {pActor.getName()[0]}. {info.familyName}");

                if (pActor.isKing() || pActor2.isKing())
                {
                    if (pActorData.gender == ActorGender.Male)
                    {
                        pActorData.setName($"Prince, {pActorData.name}");
                    }
                    else
                    {
                        pActorData.setName($"Princess, {pActorData.name}");
                    }
                }
            }
        }

        public static void add_dead_member(Actor pActor)
        {
            // Checks If Actor Has A Family
            int actorFamilyIndex = -1;
            pActor.data.get("familyIndex", out actorFamilyIndex, -1);
            if (actorFamilyIndex < 0)
            {
                return;
            }

            // Adds Dead Info To Family Info And Save File
            FamilyInfo info = Plugin.settings.families[SaveManager.currentSavePath][actorFamilyIndex];

            int actorMemberIndex = 0;
            int actorParentIndex = 0;
            int actorParentIndex2 = 0;
            string actorChildrenIndex = "";
            int actorSpouseIndex = 0;
            pActor.data.get("memberIndex", out actorMemberIndex, -1);
            pActor.data.get("parentIndex", out actorParentIndex, -1);
            pActor.data.get("parentIndex2", out actorParentIndex2, -1);
            pActor.data.get("childrenIndex", out actorChildrenIndex, "");
            pActor.data.get("spouseIndex", out actorSpouseIndex, -1);
            DeadFamilyMember dead = new DeadFamilyMember(actorFamilyIndex, actorParentIndex, actorParentIndex2, actorChildrenIndex, actorMemberIndex, actorSpouseIndex, pActor.getName(), pActor.data.profession.ToString());
            info.deadMembers.Add(actorMemberIndex.ToString(), dead);
            WorldTip.instance.show("Unit in family has died", false, "top", 3f);
        }

        public static void add_spouse(Actor pActor, Actor pActor2)
        {

            int actorFamilyIndex = -1;
            int actorMemberIndex = -1;
            pActor.data.get("familyIndex", out actorFamilyIndex, -1);
            pActor.data.get("memberIndex", out actorMemberIndex, -1);
            FamilyInfo info = Plugin.settings.families[SaveManager.currentSavePath][actorFamilyIndex];
            info.memberIndex++;
            // Setting Index For New Spouse
            pActor2.data.set("memberIndex", info.memberIndex);
            pActor2.data.set("familyIndex", actorFamilyIndex);

            // Adding Spouse Index
            pActor.data.set("spouseIndex", info.memberIndex);
            pActor2.data.set("spouseIndex", actorMemberIndex);
        }

        public static void add_existing_spouse(Actor pActor, Actor pActor2)
        {
            int actorMemberIndex = -1;
            int actorMemberIndex2 = -1;
            pActor.data.get("memberIndex", out actorMemberIndex, -1);
            pActor2.data.get("memberIndex", out actorMemberIndex2, -1);

            // Adding Spouse Index
            pActor.data.set("spouseIndex", actorMemberIndex2);
            pActor2.data.set("spouseIndex", actorMemberIndex);
        }
    }

}
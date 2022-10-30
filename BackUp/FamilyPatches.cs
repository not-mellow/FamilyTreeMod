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
    class FamilyPatches
    {
        public static Harmony harmony = new Harmony("dej.mymod.wb.familymod");
        public static int maxChildCount = 10;

        public static void init()
        {
            patchMethod(
                AccessTools.Method(typeof(CityBehProduceUnit), "produceNewCitizen"),
                AccessTools.Method(typeof(FamilyPatches), "produceNewCitizen_Prefix"),
                true);
            
            patchMethod(
                AccessTools.Method(typeof(MapBox), "spawnAndLoadUnit"),
                AccessTools.Method(typeof(FamilyPatches), "spawnAndLoadUnit_Postfix"),
                false);
            
            patchMethod(
                AccessTools.Method(typeof(Baby), "update"),
                AccessTools.Method(typeof(FamilyPatches), "babyUpdate_Prefix"),
                true);

            patchMethod(
                AccessTools.Method(typeof(Actor), "killHimself"),
                AccessTools.Method(typeof(FamilyPatches), "killHimself_Prefix"),
                true);
            
            patchMethod(
                AccessTools.Method(typeof(KingdomBehCheckKing), "findKing"),
                AccessTools.Method(typeof(FamilyPatches), "findKing_Prefix"),
                true);

            patchMethod(
                AccessTools.Method(typeof(CityBehFindLeader), "execute"),
                AccessTools.Method(typeof(FamilyPatches), "findLeader_Prefix"),
                true);
        }

        public static void patchMethod(MethodInfo original, MethodInfo patch, bool isPrefix)
        {
            if(isPrefix)
            {
                harmony.Patch(original, prefix: new HarmonyMethod(patch));
            }else
            {
                harmony.Patch(original, postfix: new HarmonyMethod(patch));
            }
            
        }

        #region AddingBabyToFamily
        public static bool produceNewCitizen_Prefix(Building pBuilding, City pCity, ref bool __result)
		{
			List<Actor> simpleList = pCity.units.getSimpleList();
			Actor randomParent = (Actor)Reflection.CallStaticMethod(typeof(CityBehProduceUnit), "getRandomParent", simpleList, null);
			if (randomParent == null)
			{
                __result = false;
				return false;
			}
			if (randomParent.haveTrait("infected") && Toolbox.randomBool())
			{
                __result = false;
				return false;
			}
			Actor randomParent2 = (Actor)Reflection.CallStaticMethod(typeof(CityBehProduceUnit), "getRandomParent", simpleList, randomParent);
            var randData = (ActorStatus)Reflection.GetField(typeof(Actor), randomParent, "data");
			// randData.children++;
			ResourceAsset foodItem = (ResourceAsset)pCity.CallMethod("getFoodItem", "");
			pCity.CallMethod("eatFoodItem", foodItem.id);
			pCity.status.housingFree--;
            var cityData = (CityData)Reflection.GetField(typeof(City), pCity, "data");
			cityData.born++;
            var cityKingdom = (Kingdom)Reflection.GetField(typeof(City), pCity, "kingdom");
			if (cityKingdom != null)
			{
				cityKingdom.born++;
			}
			ActorStats stats = randomParent.stats;
			ActorData actorData = new ActorData();
			actorData.cityID = cityData.cityID;
			actorData.status = new ActorStatus();
			actorData.status.statsID = stats.id;
            var randRace = (Race)Reflection.GetField(typeof(Actor), randomParent, "race");
			ActorBase.generateCivUnit(randomParent.stats, actorData.status, randRace);
			actorData.status.generateTraits(stats, randRace);
			actorData.status.CallMethod("inheritTraits", randData.traits);
			actorData.status.hunger = stats.maxHunger / 2;
			if (randomParent2 != null)
			{
                var randDataTwo = (ActorStatus)Reflection.GetField(typeof(Actor), randomParent2, "data");
				actorData.status.CallMethod("inheritTraits", randDataTwo.traits);
				// randDataTwo.children++;
			}
			actorData.status.skin = ai.ActorTool.getBabyColor(randomParent, randomParent2);
			actorData.status.skin_set = randData.skin_set;
			Culture babyCulture = (Culture)Reflection.CallStaticMethod(typeof(CityBehProduceUnit), "getBabyCulture", randomParent, randomParent2);
			if (babyCulture != null)
			{
				actorData.status.culture = babyCulture.id;
				actorData.status.level = babyCulture.getBornLevel();
			}

            // Add Family Member
            bool flag = checkParent(randomParent, actorData.status);
            if ((!flag && randomParent2 != null) || (SettingsWindow.toggleBools["ParentOption"] && randomParent2 != null))
            {
                flag = checkParent(randomParent2, actorData.status);
            }

            if ((!flag && randomParent2 != null) && !SettingsWindow.toggleBools["ParentOption"])
            {
                randomParent2.data.children++;
                randomParent.data.children++;
            }
            else if (!flag && !SettingsWindow.toggleBools["ParentOption"])
            {
                randomParent.data.children++;
            }

            pCity.addPopPoint(actorData);
            __result = true;
			return false;
		}

        public static bool checkParent(Actor parent, ActorStatus childData)
        {
            if (!FamilyOverviewWindow.familyActors.Contains(parent))
            {
                return false;
            }
            ActorParent currentParent = null;
            foreach(ActorParent actorParent in FamilyOverviewWindow.families)
            {
                if (actorParent.parentActor == parent)
                {
                    currentParent = actorParent;
                    break;
                }
            }
            if (currentParent == null)
            {
                return false;
            }
            // foreach(KeyValuePair<ActorParent, ActorHead> kv in FamilyOverviewWindow.familyHeads)
            // {
            //     if(kv.Key.originalFamily == currentParent.originalFamily)
            //     {
            //         kv.Value.changeMemberInfo(1);
            //         break;
            //     }
            // }
            currentParent.childrenData.Add(childData);
            parent.data.children++;
            return true;
        }

        public static void spawnAndLoadUnit_Postfix(Actor __result)
        {
            List<ActorParent> familiesCopy = (from family in FamilyOverviewWindow.families
            select family).ToList();

            bool hasFamily = false;
            foreach(ActorParent actorParent in familiesCopy)
            {
                if (actorParent.childrenData.Contains(__result.data))
                {
                    actorParent.children.Add(__result);
                    __result.getName();
                    if (SettingsWindow.toggleBools["NameOption"])
                    {
                        __result.data.firstName += " " + actorParent.originalFamily.parentActor.getName().Split(' ').Last();
                    }
                    else
                    {
                        __result.data.firstName += " " + actorParent.familyName;
                    }
                    if (!hasFamily)
                    {
                        ActorParent newParent = new ActorParent(__result.getName(), __result, new List<Actor>(), new List<ActorStatus>(), actorParent, actorParent.originalFamily);
                        FamilyOverviewWindow.families.Add(newParent);
                        FamilyOverviewWindow.familyActors.Add(__result);
                        hasFamily = true;
                    }
                    foreach(KeyValuePair<ActorParent, ActorHead> kv in FamilyOverviewWindow.familyHeads)
                    {
                        if(kv.Key.originalFamily == actorParent.originalFamily)
                        {
                            kv.Value.changeMemberInfo(1);
                            break;
                        }
                    }
                    // break;
                }
            }
        }
        #endregion

        public static bool babyUpdate_Prefix(float pElapsed, Baby __instance)
        {
            // var name = ()Reflection.GetField(typeof(), , );
            var pActor = (Actor)Reflection.GetField(typeof(Baby), __instance, "actor");
            var pActorData = (ActorStatus)Reflection.GetField(typeof(Actor), pActor, "data");
            var pWorld = (MapBox)Reflection.GetField(typeof(Baby), __instance, "world");
            if (!pActorData.alive)
            {
                return false;
            }
            if ((bool)pWorld.CallMethod("isPaused"))
            {
                return false;
            }
            // base.update(pElapsed);
            __instance.timerGrow -= pElapsed;
            if (__instance.timerGrow <= 0f)
            {
                Actor actor = pWorld.createNewUnit(pActor.stats.growIntoID, pActor.currentTile, null, 0f, null);
                var newActorData = (ActorStatus)Reflection.GetField(typeof(Actor), actor, "data");
                actor.startBabymakingTimeout();
                newActorData.hunger = actor.stats.maxHunger / 2;
                var gameStatsData = (GameStatsData)Reflection.GetField(typeof(GameStats), pWorld.gameStats, "data");
                gameStatsData.creaturesBorn--;
                if (pActor.stats.unit)
                {
                    if (pActor.city != null)
                    {
                        pActor.city.addNewUnit(actor, true, true);
                    }
                    actor.CallMethod("setKingdom", pActor.kingdom);
                }
                newActorData.diplomacy = pActorData.diplomacy;
                newActorData.intelligence = pActorData.intelligence;
                newActorData.stewardship = pActorData.stewardship;
                newActorData.warfare = pActorData.warfare;
                newActorData.culture = pActorData.culture;
                newActorData.experience = pActorData.experience;
                newActorData.level = pActorData.level;
                newActorData.setName(pActorData.firstName);
                if (pActorData.skin != -1)
                {
                    newActorData.skin = pActorData.skin;
                }
                if (pActorData.skin_set != -1)
                {
                    newActorData.skin_set = pActorData.skin_set;
                }
                newActorData.age = pActorData.age;
                newActorData.bornTime = pActorData.bornTime;
                newActorData.health = pActorData.health;
                newActorData.gender = pActorData.gender;
                newActorData.kills = pActorData.kills;
                foreach (string text in pActorData.traits)
                {
                    if (!(text == "peaceful"))
                    {
                        actor.addTrait(text, false);
                    }
                }
                if (pActorData.favorite)
                {
                    newActorData.favorite = true;
                }
                if (MoveCamera.inSpectatorMode() && MoveCamera.focusUnit == pActor)
                {
                    MoveCamera.focusUnit = actor;
                }
                addGrownUp(actor, pActor);
                pActor.killHimself(true, AttackType.GrowUp, false, false);
            }
            return false;
        }

        public static void addGrownUp(Actor newActor, Actor prevActor)
        {
            if (!FamilyOverviewWindow.familyActors.Contains(prevActor))
            {
                return;
            }
            List<ActorParent> familiesCopy = (from family in FamilyOverviewWindow.families
            select family).ToList();

            foreach(ActorParent actorParent in familiesCopy)
            {
                if (actorParent.children.Contains(prevActor))
                {
                    actorParent.children.Add(newActor);

                    actorParent.children.Remove(prevActor);
                    actorParent.childrenData.Remove(prevActor.data);
                    break;
                }
            }
            foreach(ActorParent actorParent in familiesCopy)
            {
                if (actorParent.parentActor == prevActor)
                {
                    FamilyOverviewWindow.familyActors.Add(newActor);
                    ActorParent newParent = new ActorParent(newActor.getName(), newActor, new List<Actor>(), new List<ActorStatus>(), actorParent.parentFamily, actorParent.originalFamily);
                    FamilyOverviewWindow.families.Add(newParent);
                    if(FamilyOverviewWindow.familyHeads.ContainsKey(actorParent))
                    {
                        ActorHead prevHead = FamilyOverviewWindow.familyHeads[actorParent];
                        ActorHead newHead = new ActorHead(newParent,
                            prevHead.foundingYear,
                            prevHead.currentGeneration,
                            prevHead.numOfMembers
                        );
                        FamilyOverviewWindow.familyHeads.Add(newParent, newHead);
                        FamilyOverviewWindow.familyHeads.Remove(actorParent);
                    }
                    FamilyOverviewWindow.families.Remove(actorParent);
                    break;
                }
            }

            FamilyOverviewWindow.familyActors.Remove(prevActor);
        }

        public static bool killHimself_Prefix(Actor __instance, AttackType pType, bool pCountDeath)
        {
            if(!FamilyOverviewWindow.familyActors.Contains(__instance))
            {
                return true;
            }
            foreach(ActorParent actorParent in FamilyOverviewWindow.familyHeads.Keys.ToList())
            {
                if(actorParent.parentActor == __instance)
                {
                    if(pType != AttackType.GrowUp)
                    {
                        FamilyOverviewWindow.familyHeads[actorParent].changeMemberInfo(-1);
                    }
                    FamilyOverviewWindow.replaceHead(actorParent);
                    return true;
                }
            }
            if (!pCountDeath)
            {
                return true;
            }
            if(pType == AttackType.GrowUp)
            {
                return true;
            }
            foreach(ActorParent actorParent in FamilyOverviewWindow.families)
            {
                if(actorParent.parentActor == __instance)
                {
                    foreach(KeyValuePair<ActorParent, ActorHead> kv in FamilyOverviewWindow.familyHeads)
                    {
                        if(actorParent.originalFamily == kv.Key.originalFamily)
                        {
                            kv.Value.changeMemberInfo(-1);
                            return true;
                        }
                    }
                    break;
                }
            }
            return true;
        }

        public static bool findKing_Prefix(Kingdom pKingdom)
        {
            if(SettingsWindow.toggleBools["InheritanceOption"])
            {
                return true;
            }
            if (RelationsWindow.royalFamilies.ContainsKey(pKingdom))
            {
                foreach(KeyValuePair<ActorParent, ActorHead> kv in FamilyOverviewWindow.familyHeads)
                {
                    string nameCheck = kv.Key.originalFamily.familyName;
                    if (nameCheck != RelationsWindow.royalFamilies[pKingdom])
                    {
                        continue;
                    }
                    if (kv.Key.parentActor == null || !kv.Key.parentActor.data.alive || kv.Key.parentActor.kingdom != pKingdom)
                    {
                        RelationsWindow.royalFamilies.Remove(pKingdom);
                        break;
                    }
                    if(!RelationsWindow.nobleFamilies.ContainsValue(nameCheck))
                    {
                        if(kv.Key.parentActor.isProfession(UnitProfession.Leader))
                        {
                            kv.Key.parentActor.city.removeLeader();
                        }
                        pKingdom.setKing(kv.Key.parentActor);
			            WorldLog.logNewKing(pKingdom);
                        break;
                    }
                    if(RelationsWindow.nobleFamilies.ContainsValue(nameCheck))
                    {
                        foreach(KeyValuePair<City, string> noblekv in RelationsWindow.nobleFamilies.ToList())
                        {
                            if (RelationsWindow.nobleFamilies[noblekv.Key] == nameCheck)
                            {
                                RelationsWindow.nobleFamilies.Remove(noblekv.Key);
                                break;
                            }
                        }
                        break;
                    }
                }
                return false;
            }
            bool flag = false;
            List<ActorHead> heads = new List<ActorHead>();
            foreach(ActorHead head in FamilyOverviewWindow.familyHeads.Values)
            {
                if(head.getKingdom() == pKingdom && !RelationsWindow.nobleFamilies.ContainsValue(head.headFamily.originalFamily.familyName))
                {
                    flag = true;
                    heads.Add(head);
                }
            }

            if (!flag)
            {
                return true;
            }

            heads.Shuffle<ActorHead>();
			ActorHead actorHead = null;
			int num = 0;
			foreach (ActorHead actorHead2 in heads)
			{
				int num2 = ActorTool.attributeDice(actorHead2.headFamily.parentActor, 2);
				if (actorHead == null || num2 > num)
				{
					num = num2;
					actorHead = actorHead2;
				}
			}
			if (actorHead == null || actorHead.headFamily.parentActor == null)
			{
				return true;
			}

            RelationsWindow.royalFamilies.Add(pKingdom, actorHead.headFamily.originalFamily.familyName);

            pKingdom.setKing(actorHead.headFamily.parentActor);
			WorldLog.logNewKing(pKingdom);

            return false;
        }

        public static bool findLeader_Prefix(City pCity)
        {
            if(SettingsWindow.toggleBools["InheritanceOption"])
            {
                return true;
            }
            if (pCity.leader != null)
			{
				return true;
			}
			if (pCity.captureTicks > 0f)
			{
				return true;
			}
            if (RelationsWindow.nobleFamilies.ContainsKey(pCity))
            {
                string newSuccessor = "";
                foreach(KeyValuePair<ActorParent, ActorHead> kv in FamilyOverviewWindow.familyHeads)
                {
                    string nameCheck = kv.Key.originalFamily.familyName;
                    if(nameCheck == RelationsWindow.nobleFamilies[pCity])
                    {
                        if(!RelationsWindow.royalFamilies.ContainsValue(nameCheck))
                        {
                            if(kv.Key.parentActor.isProfession(UnitProfession.King))
                            {
                                RelationsWindow.nobleFamilies.Remove(pCity);
                                RelationsWindow.royalFamilies.Remove(kv.Key.parentActor.kingdom);
                                RelationsWindow.royalFamilies.Add(kv.Key.parentActor.kingdom, nameCheck);
                                break;
                            }
                            if(kv.Key.parentActor.city == null)
                            {
                                pCity.addNewUnit(kv.Key.parentActor, true, false);
                            }
                            else if(kv.Key.parentActor.city != pCity)
                            {
                                kv.Key.parentActor.city.removeCitizen(kv.Key.parentActor, false, AttackType.Other);
                                pCity.addNewUnit(kv.Key.parentActor, true, false);
                            }
                            City.makeLeader(kv.Key.parentActor, pCity);
                            newSuccessor = nameCheck;
                            Debug.Log($"{kv.Key.parentActor.getName()} Became Leader Through Inheritance!");
                            break;
                        }
                        else if(RelationsWindow.royalFamilies.ContainsValue(nameCheck))
                        {
                            RelationsWindow.nobleFamilies.Remove(pCity);
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(newSuccessor))
                {
                    City cityRemoval = null;
                    foreach(KeyValuePair<City, string> kv in RelationsWindow.nobleFamilies)
                    {
                        if(kv.Key != pCity && kv.Value == newSuccessor)
                        {
                            cityRemoval = kv.Key;
                            break;
                        }
                    }
                    if(cityRemoval != null)
                    {
                        RelationsWindow.nobleFamilies.Remove(cityRemoval);
                    }
                }
                return false;
            }

            bool flag = false;
            List<ActorHead> heads = new List<ActorHead>();
            foreach(ActorHead head in FamilyOverviewWindow.familyHeads.Values)
            {
                if(head.getCity() == pCity && !RelationsWindow.royalFamilies.ContainsValue(head.headFamily.originalFamily.familyName))
                {
                    flag = true;
                    heads.Add(head);
                }
            }

            if (!flag)
            {
                return true;
            }

            heads.Shuffle<ActorHead>();
			ActorHead actorHead = null;
			int num = 0;
			foreach (ActorHead actorHead2 in heads)
			{
				int num2 = ActorTool.attributeDice(actorHead2.headFamily.parentActor, 2);
				if (actorHead == null || num2 > num)
				{
					num = num2;
					actorHead = actorHead2;
				}
			}
			if (actorHead == null || actorHead.headFamily.parentActor == null)
			{
				return true;
			}

            RelationsWindow.nobleFamilies.Add(pCity, actorHead.headFamily.originalFamily.familyName);
            City.makeLeader(actorHead.headFamily.parentActor, pCity);
            Debug.Log($"Family Head {actorHead.headFamily.parentActor.getName()} Successfully Became Leader!");

            return false;
        }
    }
}
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
            
            harmony.Patch(
                AccessTools.Method(typeof(KingdomBehCheckKing), "findKing"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(FamilyPatches), "findKing_Prefix"))
                // postfix: new HarmonyMethod(AccessTools.Method(typeof(FamilyPatches), "findKing_Postfix"))
            );

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

        public static bool produceNewCitizen_Prefix(Building pBuilding, City pCity, ref bool __result)
		{
			List<Actor> simpleList = pCity.units.getSimpleList();
			Actor randomParent = CityBehProduceUnit.getRandomParent(simpleList, null);
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
            FamilyActor firstFamily = FamilyActor.getFamily(randomParent);
            bool hasFamilyComponent = firstFamily != null;
            Actor randomParent2 = null;
            if (hasFamilyComponent)
            {
                bool childrenOptionBool = int.Parse(SettingsWindow.inputOptions["ChildrenOption"]) >= 0;
                if (randomParent.data.children >= int.Parse(SettingsWindow.inputOptions["ChildrenOption"]) && childrenOptionBool)
                {
                    __result = false;
                    return false;
                }
                if (firstFamily.loverID == null || NewActions.getActorByIndex(firstFamily.loverID, firstFamily.familyIndex) == null)
                {
                    randomParent2 = getLover(simpleList, randomParent);
                }
                else
                {
                    randomParent2 = NewActions.getActorByIndex(firstFamily.loverID, firstFamily.familyIndex);
                }

                if (randomParent2 == null || (randomParent2.data.children >= int.Parse(SettingsWindow.inputOptions["ChildrenOption"]) && childrenOptionBool))
                {
                    __result = false;
                    return false;
                }
            }
            else
            {
                randomParent2 = CityBehProduceUnit.getRandomParent(simpleList, randomParent);
                if (randomParent2 == null || FamilyActor.getFamily(randomParent2) != null)
                {
                    randomParent2 = null;
                }  
            }
			randomParent.data.children++;
			ResourceAsset foodItem = pCity.getFoodItem(null);
			pCity.eatFoodItem(foodItem.id);
			pCity.status.housingFree--;
			pCity.data.born++;
			if (pCity.kingdom != null)
			{
				pCity.kingdom.born++;
			}
			ActorStats stats = randomParent.stats;
			ActorData actorData = new ActorData();
			actorData.cityID = pCity.data.cityID;
			actorData.status = new ActorStatus();
			actorData.status.statsID = stats.id;
			ActorBase.generateCivUnit(randomParent.stats, actorData.status, randomParent.race);
			actorData.status.generateTraits(stats, randomParent.race);
			actorData.status.inheritTraits(randomParent.data.traits);
			actorData.status.hunger = stats.maxHunger / 2;
			if (randomParent2 != null)
			{
				actorData.status.inheritTraits(randomParent2.data.traits);
				randomParent2.data.children++;
			}
			actorData.status.skin = ActorTool.getBabyColor(randomParent, randomParent2);
			actorData.status.skin_set = randomParent.data.skin_set;
			Culture babyCulture = CityBehProduceUnit.getBabyCulture(randomParent, randomParent2);
			if (babyCulture != null)
			{
				actorData.status.culture = babyCulture.id;
				actorData.status.level = babyCulture.getBornLevel();
			}

            if (hasFamilyComponent)
            {
                int inheritValue = -1;
                int.TryParse(SettingsWindow.inputOptions["InheritTraits"], out inheritValue);
                if (inheritValue != -1)
                {
                    float chance = (float)inheritValue/100f;
                    inheritTraitsChance(randomParent.data.traits, actorData.status, chance);
                    inheritTraitsChance(randomParent2.data.traits, actorData.status, chance);
                }
                // Family familyOfChild = FamilyOverviewWindow.getFromFamilies(ref firstFamily.familyIndex);
                // if(familyOfChild != null)
                // {
                //     actorData.status.addTrait(familyOfChild.signatureTrait);
                // }
                bool isMale = false;
                if (actorData.status.gender == ActorGender.Male)
                {
                    isMale = true;
                }
                UnbornActor newUnborn = new UnbornActor();
                newUnborn.copyFamily(randomParent, randomParent2, isMale);
                FamilyOverviewWindow.unbornActorList.Add(actorData.status, newUnborn);
            }

			pCity.addPopPoint(actorData);

            __result = true;
			return false;
		}

        private static void inheritTraitsChance(List<string> pTraits, ActorStatus status, float chance)
        {
            for (int i = 0; i < pTraits.Count; i++)
            {
                string pID = pTraits[i];
                ActorTrait actorTrait = AssetManager.traits.get(pID);
                if (actorTrait != null)
                {
                    bool giveTrait = Toolbox.randomChance(chance);
                    if (giveTrait && !status.traits.Contains(actorTrait.id) && !status.haveOppositeTrait(actorTrait))
                    {
                        status.addTrait(actorTrait.id);
                    }
                }
            }
        }

        private static Actor getLover(List<Actor> list, Actor actor)
        {
            Actor lover = null;
            ActorGender gender = actor.data.gender;

            foreach(Actor pActor in list)
            {
                if (gender == pActor.data.gender || pActor.stats.baby || !pActor.data.alive || pActor == actor || pActor == null)
                {
                    continue;
                }
                if (SettingsWindow.toggleBools["ParentOption"] && FamilyActor.getFamily(pActor) != null)
                {
                    if (FamilyActor.getFamily(pActor).familyIndex == FamilyActor.getFamily(actor).familyIndex)
                    {
                        lover = pActor;
                        break;
                    }
                }
                else if (!SettingsWindow.toggleBools["ParentOption"] || FamilyActor.getFamily(pActor) == null)
                {
                    lover = pActor;
                    break;
                }
            }
            if (FamilyActor.getFamily(lover) != null)
            {
                FamilyActor loverFamily = FamilyActor.getFamily(lover);
                FamilyActor actorFamily = FamilyActor.getFamily(actor);
                if (loverFamily.loverID == null || loverFamily.loverID.Contains("dead"))
                {
                    loverFamily.loverID = actor.data.actorID;
                    loverFamily.deadLoverID = actorFamily.deadID;
                    Family aFamily = FamilyOverviewWindow.getFromFamilies(ref loverFamily.familyIndex);
                    aFamily.addActor(actor);
                    FamilyOverviewWindow.deadActorList[loverFamily.deadID].loverID = actor.data.actorID;

                    actorFamily.loverID = lover.data.actorID;
                    actorFamily.deadLoverID = loverFamily.deadID;
                    Family bFamily = FamilyOverviewWindow.getFromFamilies(ref actorFamily.familyIndex);
                    bFamily.addActor(lover);
                    FamilyOverviewWindow.deadActorList[actorFamily.deadID].loverID = lover.data.actorID;
                }
                else
                {
                    lover = null;
                }
            }
            else if (lover != null)
            {
                FamilyActor loverFamily = lover.gameObject.AddComponent<FamilyActor>();
                FamilyActor actorFamily = FamilyActor.getFamily(actor);
                loverFamily.familyName = actorFamily.familyName;
                loverFamily.founderName = actorFamily.founderName;
                loverFamily.loverID = actor.data.actorID;
                loverFamily.familyIndex = actorFamily.familyIndex;
                loverFamily.isMale = !actorFamily.isMale;
                loverFamily.deadID = "dead_" + FamilyWindow.nextID().ToString();
                loverFamily.deadLoverID = actorFamily.deadID;

                actorFamily.loverID = lover.data.actorID;
                actorFamily.deadLoverID = loverFamily.deadID;

                Family family = FamilyOverviewWindow.getFromFamilies(ref loverFamily.familyIndex);

                FamilyOverviewWindow.deadActorList.Add(
                    loverFamily.deadID,
                    new deadActor().copyFamily(loverFamily, lover.getName())
                );

                FamilyOverviewWindow.deadActorList[actorFamily.deadID].loverID = lover.data.actorID;
                FamilyOverviewWindow.deadActorList[actorFamily.deadID].deadLoverID = loverFamily.deadID;

                family.addActor(lover);
            }

            return lover;
        }

        public static void spawnAndLoadUnit_Postfix(ActorData pSaveData, Actor __result)
        {
            if (FamilyOverviewWindow.unbornActorList.ContainsKey(pSaveData.status))
            {
                UnbornActor unbornInfo = FamilyOverviewWindow.unbornActorList[pSaveData.status];
                FamilyActor actorFamily = __result.gameObject.AddComponent<FamilyActor>();
                actorFamily.copyFamily(null, unbornInfo);
                actorFamily.deadID = "dead_" + FamilyWindow.nextID().ToString();

                string fatherName = "";
                string motherName = "";
                Actor fatherActor = NewActions.getActorByIndex(actorFamily.fatherID, actorFamily.familyIndex, actorFamily.fatherFamilyIndex, actorFamily.motherFamilyIndex);

                if (fatherActor == null)
                {
                    deadActor deadFather = FamilyOverviewWindow.deadActorList[actorFamily.deadFatherID];
                    deadFather.addChild(__result, __result.data.actorID, false, actorFamily.deadID);
                    actorFamily.fatherID = actorFamily.deadFatherID;
                    fatherName = deadFather.name.Split(' ')[0];
                }
                else
                {
                    FamilyActor father = FamilyActor.getFamily(fatherActor);
                    father.addChild(__result, __result.data.actorID, false, actorFamily.deadID);
                    fatherName = fatherActor.getName().Split(' ')[0];
                }
                
                Actor motherActor = NewActions.getActorByIndex(actorFamily.motherID, actorFamily.familyIndex, actorFamily.fatherFamilyIndex, actorFamily.motherFamilyIndex);

                if (motherActor == null)
                {
                    deadActor deadMother = FamilyOverviewWindow.deadActorList[actorFamily.deadMotherID];
                    deadMother.addChild(__result, __result.data.actorID, false, actorFamily.deadID);
                    actorFamily.motherID = actorFamily.deadMotherID;
                    motherName = deadMother.name.Split(' ')[0];
                }
                else
                {
                    FamilyActor mother = FamilyActor.getFamily(motherActor);
                    mother.addChild(__result, __result.data.actorID, false, actorFamily.deadID);
                    motherName = motherActor.getName().Split(' ')[0];
                }

                if (SettingsWindow.toggleBools["NameOption"])
                {
                    __result.data.firstName = __result.getName() + " " + fatherName + " " + motherName;
                }
                else
                {
                    __result.data.firstName = __result.getName() + " " + actorFamily.founderName;
                }

                // if (fatherActor != null && !string.IsNullOrEmpty(fatherName))
                // {
                //     if (FamilyActor.getFamily(fatherActor).childrenID.Count == 1)
                //     {
                //         __result.data.firstName = fatherName + " II";
                //     }
                // }

                FamilyOverviewWindow.deadActorList.Add(
                    actorFamily.deadID,
                    new deadActor().copyFamily(actorFamily, __result.getName())
                );

                FamilyOverviewWindow.unbornActorList.Remove(pSaveData.status);
            }
        }

        public static bool babyUpdate_Prefix(float pElapsed, Baby __instance)
        {
            if (!__instance.actor.data.alive)
            {
                return false;
            }
            if (__instance.world.isPaused())
            {
                return false;
            }
            __instance.timerGrow -= pElapsed;
            if (__instance.timerGrow <= 0f)
            {
                Actor actor = __instance.world.createNewUnit(__instance.actor.stats.growIntoID, __instance.actor.currentTile, null, 0f, null);
                actor.startBabymakingTimeout();
                actor.data.hunger = actor.stats.maxHunger / 2;
                __instance.world.gameStats.data.creaturesBorn--;
                if (__instance.actor.stats.unit)
                {
                    if (__instance.actor.city != null)
                    {
                        __instance.actor.city.addNewUnit(actor, true, true);
                    }
                    actor.setKingdom(__instance.actor.kingdom);
                }
                actor.data.diplomacy = __instance.actor.data.diplomacy;
                actor.data.intelligence = __instance.actor.data.intelligence;
                actor.data.stewardship = __instance.actor.data.stewardship;
                actor.data.warfare = __instance.actor.data.warfare;
                actor.data.culture = __instance.actor.data.culture;
                actor.data.experience = __instance.actor.data.experience;
                actor.data.level = __instance.actor.data.level;
                actor.data.setName(__instance.actor.data.firstName);
                if (__instance.actor.data.skin != -1)
                {
                    actor.data.skin = __instance.actor.data.skin;
                }
                if (__instance.actor.data.skin_set != -1)
                {
                    actor.data.skin_set = __instance.actor.data.skin_set;
                }
                actor.data.age = __instance.actor.data.age;
                actor.data.bornTime = __instance.actor.data.bornTime;
                actor.data.health = __instance.actor.data.health;
                actor.data.gender = __instance.actor.data.gender;
                actor.data.kills = __instance.actor.data.kills;
                foreach (string text in __instance.actor.data.traits)
                {
                    if (!(text == "peaceful"))
                    {
                        actor.addTrait(text, false);
                    }
                }
                if (__instance.actor.data.favorite)
                {
                    actor.data.favorite = true;
                }
                if (MoveCamera.inSpectatorMode() && MoveCamera.focusUnit == __instance.actor)
                {
                    MoveCamera.focusUnit = actor;
                }

                if (FamilyActor.getFamily(__instance.actor) != null /*&& __instance.actor.data.actorID != "fool"*/)
                {
                    addGrownUp(actor, __instance.actor);
                    // __instance.actor.data.actorID = "fool";
                }
                __instance.actor.killHimself(true, AttackType.GrowUp, false, false);
            }
            return false;
        }

        public static void addGrownUp(Actor newActor, Actor prevActor)
        {
            FamilyActor prevActorFamily = FamilyActor.getFamily(prevActor);

            FamilyActor newActorFamily = newActor.gameObject.AddComponent<FamilyActor>();
            newActorFamily.copyFamily(prevActorFamily, null);

            Actor father = NewActions.getActorByIndex(newActorFamily.fatherID, newActorFamily.fatherFamilyIndex, newActorFamily.motherFamilyIndex);
            if (father != null && FamilyActor.getFamily(father) != null)
            {
                FamilyActor.getFamily(father).removeChild(prevActor, prevActor.data.actorID);
                FamilyActor.getFamily(father).addChild(newActor, newActor.data.actorID, prevActorFamily.isHead, newActorFamily.deadID);
            }
            else if (newActorFamily.deadFatherID != null)
            {
                deadActor deadFather = FamilyOverviewWindow.deadActorList[newActorFamily.deadFatherID];
                deadFather.removeChild(prevActor, prevActor.data.actorID);
                deadFather.addChild(newActor, newActor.data.actorID, prevActorFamily.isHead, newActorFamily.deadID);
                newActorFamily.fatherID = newActorFamily.deadFatherID;
            }
            
            Actor mother = NewActions.getActorByIndex(newActorFamily.motherID, newActorFamily.fatherFamilyIndex, newActorFamily.motherFamilyIndex);
            if (mother != null && FamilyActor.getFamily(mother) != null)
            {
                FamilyActor.getFamily(mother).removeChild(prevActor, prevActor.data.actorID);
                FamilyActor.getFamily(mother).addChild(newActor, newActor.data.actorID, prevActorFamily.isHead, newActorFamily.deadID);
            }
            else if (newActorFamily.deadMotherID != null)
            {
                deadActor deadMother = FamilyOverviewWindow.deadActorList[newActorFamily.deadMotherID];
                deadMother.removeChild(prevActor, prevActor.data.actorID);
                deadMother.addChild(newActor, newActor.data.actorID, prevActorFamily.isHead, newActorFamily.deadID);
                newActorFamily.motherID = newActorFamily.deadMotherID;
            }

            Family sFatherFamily = FamilyOverviewWindow.getFromFamilies(ref newActorFamily.fatherFamilyIndex);
            if (sFatherFamily != null)
            {
                sFatherFamily.actors.Remove(prevActor);
                sFatherFamily.addActor(newActor);
            }
            Family sMotherFamily = FamilyOverviewWindow.getFromFamilies(ref newActorFamily.motherFamilyIndex);
            if (sMotherFamily != null)
            {
                sMotherFamily.actors.Remove(prevActor);
                sMotherFamily.addActor(newActor);
            }
            
            Family curFamily = FamilyOverviewWindow.getFromFamilies(ref newActorFamily.familyIndex);
            if (prevActorFamily.isHeir)
            {
                curFamily.heirID = newActor.data.actorID;
                newActorFamily.isHeir = true;
            }
            if (prevActorFamily.isHead)
            {
                curFamily.HEADID = newActor.data.actorID;
                newActorFamily.isHead = true;
            }

            FamilyOverviewWindow.deadActorList[newActorFamily.deadID] = new deadActor().copyFamily(newActorFamily, newActor.getName());
        }

        public static bool killHimself_Prefix(bool pDestroy, AttackType pType, bool pCountDeath, Actor __instance)
        {
            if (pType == AttackType.GrowUp || FamilyActor.getFamily(__instance) == null || pDestroy || !pCountDeath)
            {
                return true;
            }
            FamilyActor actorFamily = FamilyActor.getFamily(__instance);

            if (actorFamily.isHead)
            {
                actorFamily.refreshHead(__instance);
            }
            else if (actorFamily.isHeir)
            {
                actorFamily.refreshHeir(__instance);
            }
            Family family = FamilyOverviewWindow.getFromFamilies(ref actorFamily.familyIndex);
            actorFamily.actorDied(__instance, family, pType);

            return true;
        }

        public static bool findKing_Prefix(Kingdom pKingdom, KingdomBehCheckKing __instance)
        {
            List<Kingdom> kingdomsToRemove = new List<Kingdom>();
            foreach(KeyValuePair<Kingdom, Family> kv in FamilyOverviewWindow.kingdomFamilies)
            {
                if (!kv.Key.alive)
                {
                    kingdomsToRemove.Add(kv.Key);
                }
            }
            foreach(Kingdom kdom in kingdomsToRemove)
            {
                Family familyRemoved = FamilyOverviewWindow.kingdomFamilies[kdom];
                FamilyOverviewWindow.kingdomFamilies.Remove(kdom);
            } 

            if (!FamilyOverviewWindow.kingdomFamilies.ContainsKey(pKingdom))
            {
                foreach (KeyValuePair<string, Family> kv in FamilyOverviewWindow.families)
                {
                    Actor head = NewActions.getActorByIndex(kv.Value.HEADID, kv.Value.index);
                    if (head == null || head.kingdom == null || FamilyOverviewWindow.cityFamilies.ContainsValue(kv.Value))
                    {
                        continue;
                    }
                    if (head.kingdom == pKingdom)
                    {
                        FamilyOverviewWindow.kingdomFamilies.Add(pKingdom, kv.Value);
                        makeIntoKing(head, pKingdom);
                        return false;
                    }
                }
                return true;
            }
            Family thisFamily = FamilyOverviewWindow.kingdomFamilies[pKingdom];
            Actor actor = MapBox.instance.getActorByID(thisFamily.HEADID);
            if ((actor == null && thisFamily.actors.Count <= 0) || (actor != null && actor.kingdom != pKingdom) || FamilyOverviewWindow.cityFamilies.ContainsValue(thisFamily))
            {
                FamilyOverviewWindow.kingdomFamilies.Remove(pKingdom);
                return false;
            }

            makeIntoKing(actor, pKingdom);

            if (actor != null && actor.city != pKingdom.capital)
            {
                actor.city.units.Remove(actor);
                actor.city.setStatusDirty();
                actor.city.unitsDirty = true;
                pKingdom.capital.addNewUnit(actor, false);
            }
            return false;
        }

        private static void makeIntoKing(Actor actor, Kingdom pKingdom)
        {
            if (actor == null)
			{
				return;
			}
			if (actor.city.leader == actor)
			{
				actor.city.removeLeader();
			}
            if (actor.unitGroup != null)
            {
                actor.unitGroup.removeUnit(actor);
            }
			pKingdom.setKing(actor);
			WorldLog.logNewKing(pKingdom);
        }

        public static void findKing_Postfix(Kingdom pKingdom)
        { 
            return;
        }

        public static bool findLeader_Prefix(City pCity, ref BehResult __result)
        {
            List<City> citiesToRemove = new List<City>();
            foreach(KeyValuePair<City, Family> kv in FamilyOverviewWindow.cityFamilies)
            {
                if (!kv.Key.alive)
                {
                    citiesToRemove.Add(kv.Key);
                }
            }
            foreach(City city in citiesToRemove)
            {
                FamilyOverviewWindow.cityFamilies.Remove(city);
            } 

			// if (pCity.leader != null)
			// {
			// 	__result = BehResult.Continue;
            //     return false;
			// }
			if (pCity.captureTicks > 0f)
			{
				__result = BehResult.Continue;
                return false;
			}
            if (!FamilyOverviewWindow.cityFamilies.ContainsKey(pCity) && pCity.kingdom.king != null)
            {
                foreach(KeyValuePair<string, Family> kv in FamilyOverviewWindow.families)
                {
                    Actor head = NewActions.getActorByIndex(kv.Value.HEADID, kv.Value.index);
                    if (head == null || !head.data.alive || head.city == null || FamilyOverviewWindow.kingdomFamilies.ContainsValue(kv.Value))
                    {
                        continue;
                    }
                    if (pCity.leader == head)
                    {
                        FamilyOverviewWindow.cityFamilies.Add(pCity, kv.Value);
                        __result = BehResult.Continue;
                        return false;
                    }
                    if (head.city == pCity)
                    {
                        FamilyOverviewWindow.cityFamilies.Add(pCity, kv.Value);
                        City.makeLeader(head, pCity);
                        __result = BehResult.Continue;
                        return false;
                    }
                }
                __result = BehResult.Continue;
                return true;
            }
            else if (pCity.kingdom.king == null)
            {
                __result = BehResult.Continue;
                return true;
            }

            Family thisFamily = FamilyOverviewWindow.cityFamilies[pCity];
            Actor actor = NewActions.getActorByIndex(thisFamily.HEADID, thisFamily.index);
            if (pCity.leader == actor)
            {
                __result = BehResult.Continue;
                return false;
            }
			if ((actor == null && thisFamily.actors.Count <= 0) || (actor != null && actor.city != pCity) || FamilyOverviewWindow.kingdomFamilies.ContainsValue(thisFamily))
			{
                FamilyOverviewWindow.cityFamilies.Remove(pCity);
                __result = BehResult.Continue;
                return false;
			}

            City.makeLeader(actor, pCity);
			__result = BehResult.Continue;
            return false;
        }
    }
}
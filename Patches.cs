using BepInEx;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using ai;
using ai.behaviours;


namespace FamilyTreeMod
{
    public class Patches
    {
        public static Harmony harmony = new Harmony("wb.dej.familytreemod.product");

        public static void init()
        {
			// This Patch Adds Main Components Of Family Logic
			// Does Skip Logic
			// May Be Incompatible With Other Mods That Patch This
            harmony.Patch(
                AccessTools.Method(typeof(CityBehProduceUnit), "produceNewCitizen"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "produceNewCitizen_Prefix"))
            );

			// This Patch Adds Dead Member To Family Info And Save File
			// Does Not Skip Logic
            harmony.Patch(
                AccessTools.Method(typeof(Actor), "killHimself"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "killHimself_Prefix"))
            );

			// This Patch Is For Naming Buttons
			// Does Not Skip Logic
			harmony.Patch(
                AccessTools.Method(typeof(PowerButton), "init"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "init_PowerButton_Prefix"))
            );

			// This Patch Is To Save Family Tree While Saving World
			// Does Not Skip Logic
			harmony.Patch(
                AccessTools.Method(typeof(SaveManager), "saveToCurrentPath"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "saveToCurrentPath_Postfix"))
            );

			// This Patch Adds "King" Prefix To Unit's Name
			// Does Not Skip Logic
			harmony.Patch(
                AccessTools.Method(typeof(Kingdom), "setKing"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "setKing_Postfix"))
            );
        }

        public static bool produceNewCitizen_Prefix(Building pBuilding, City pCity, CityBehProduceUnit __instance, ref bool __result)
		{
			Actor actor = __instance._possibleParents.Pop<Actor>();
			if (actor == null)
			{
				__result = false;
                return false;
			}
			if (!Toolbox.randomChance(actor.stats[S.fertility]))
			{
				__result = false;
                return false;
			}
			if (__instance._possibleParents.Count <= 0)
			{
                __result = false;
				return false;
			}
			// Getting Family Info From Actor
			int actorSpouseIndex = -1;
			actor.data.get("spouseIndex", out actorSpouseIndex, -1);
			int actorFamilyIndex = -1;
			actor.data.get("familyIndex", out actorFamilyIndex, -1);
			Actor actor2 = findActorByMemberIndex(actorSpouseIndex, actorFamilyIndex);


			// Finding Possible Spouse For Family Actor
			if (actor2 == null)
			{
            	actor2 = __instance._possibleParents.Pop<Actor>();
				int actorFamilyIndex2 = -1;
				actor2.data.get("familyIndex", out actorFamilyIndex2, -1);
				if (actorFamilyIndex != -1 && actorFamilyIndex2 == -1)
				{
					FamilyInfo.add_spouse(actor, actor2);
				}
				else if (actorFamilyIndex == -1 && actorFamilyIndex2 != -1)
				{
					FamilyInfo.add_spouse(actor2, actor);
				}
				else if (actorFamilyIndex == actorFamilyIndex2 && actorFamilyIndex != -1 && actorFamilyIndex2 != -1)
				{
					FamilyInfo.add_existing_spouse(actor, actor2);
				}
				else
				{
					__result = false;
					return false;
				}
			}
			ResourceAsset foodItem = pCity.getFoodItem(null);
			pCity.eatFoodItem(foodItem.id);
			pCity.status.housingFree--;
			pCity.data.born++;
			if (pCity.kingdom != null)
			{
				pCity.kingdom.data.born++;
			}
			ActorAsset asset = actor.asset;
			ActorData actorData = new ActorData();
			actorData.created_time = BehaviourActionBase<City>.world.getCreationTime();
			actorData.cityID = pCity.data.id;
			actorData.id = BehaviourActionBase<City>.world.mapStats.getNextId("unit");
			actorData.asset_id = asset.id;
			ActorBase.generateCivUnit(actor.asset, actorData, actor.race);
			actorData.generateTraits(asset, actor.race);
			actorData.inheritTraits(actor.data.traits);
			actorData.hunger = asset.maxHunger / 2;
			actor.data.makeChild(BehaviourActionBase<City>.world.getCurWorldTime());
			if (actor2 != null)
			{
				actorData.inheritTraits(actor2.data.traits);
				actor2.data.makeChild(BehaviourActionBase<City>.world.getCurWorldTime());
			}
			Clan clan = CityBehProduceUnit.checkGreatClan(actor, actor2);
			actorData.skin = ActorTool.getBabyColor(actor, actor2);
			actorData.skin_set = actor.data.skin_set;
			Culture babyCulture = CityBehProduceUnit.getBabyCulture(actor, actor2);
			if (babyCulture != null)
			{
				actorData.culture = babyCulture.data.id;
				actorData.level = babyCulture.getBornLevel();
			}
			if (clan != null)
			{
				Actor actor3 = pCity.spawnPopPoint(actorData, actor.currentTile);
				clan.addUnit(actor3);
			}
			else
			{
				pCity.addPopPoint(actorData);
			}

            // Add Child To Family
            FamilyInfo.add_member(actor, actor2, actorData);
			__result = true;
            return false;
		}

        public static bool killHimself_Prefix(bool pDestroy, AttackType pType, bool pCountDeath, bool pLaunchCallbacks, bool pLogFavorite, Actor __instance)
        {
            if (pDestroy) return true;
            FamilyInfo.add_dead_member(__instance);
            return true;
        }

		public static bool init_PowerButton_Prefix(PowerButton __instance, RectTransform ___rectTransform, Image ___image, Button ___button, GodPower ___godPower)
		{
			int instanceID = __instance.gameObject.GetInstanceID();
			bool flag = UI.ButtonsToRename.ContainsKey(instanceID);
			if (flag)
			{
				__instance.name = UI.ButtonsToRename[instanceID];
			}
			return true;
		}

		private static Actor findActorByMemberIndex(int index, int currentFamilyIndex)
        {
			if (index == -1)
			{
				return null;
			}
            foreach (KeyValuePair<string, Race> kvp in AssetManager.raceLibrary.dict)
            {
                foreach(Actor actor in kvp.Value.units.getSimpleList())
                {
                    int actorFamilyIndex = -1;
                    actor.data.get("familyIndex", out actorFamilyIndex, -1);
                    if (actorFamilyIndex < 0 || actorFamilyIndex != currentFamilyIndex)
                    {
                        continue;
                    }
                    int actorMemberIndex = -1;
                    actor.data.get("memberIndex", out actorMemberIndex, -1);
                    if (actorMemberIndex != index)
                    {
                        continue;
                    }
                    return actor;
                }
                
            }
            return null;
        }

		public static void saveToCurrentPath_Postfix()
		{
			// This Removes Old Family Info
			if (Plugin.settings.families.ContainsKey(SaveManager.currentSavePath))
			{
				List<FamilyInfo> infoToBeRemoved = new List<FamilyInfo>();
				foreach(FamilyInfo info in Plugin.settings.families[SaveManager.currentSavePath])
				{
					if (World.world.mapStats.worldTime < info.worldTime)
					{
						infoToBeRemoved.Add(info);
					}
					else
					{
						info.worldTime = World.world.mapStats.worldTime;
					}
				}
				foreach(FamilyInfo info2 in infoToBeRemoved)
				{
					Plugin.settings.families[SaveManager.currentSavePath].Remove(info2);
				}
			}
			FamilyTreeSettings.save_settings();
		}

		public static void setKing_Postfix(Actor pActor)
		{
			if (pActor.getName().Contains("Prince") || pActor.getName().Contains("Princess"))
			{
				char[] removeChars = {'P', 'r', 'i', 'n', 'c', 'e', 's'};
				pActor.data.setName($"{pActor.getName().TrimStart(removeChars).Trim(',')}");
			}

			pActor.data.setName($"King, {pActor.getName()}");
		}
    }
}
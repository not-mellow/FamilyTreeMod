using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;
using Assets.SimpleZip;

namespace FamilyTreeMod
{
    class NewActions : MonoBehaviour
    {
        public static City selectedConvertCity = null;

        public static void action_convert_city(WorldTile pTile = null, string pDropID = null)
        {
            if (pTile.zone.city != null)
            {
                if (selectedConvertCity == null)
                {
                    selectedConvertCity = pTile.zone.city;
                    WorldTip.instance.show("Please Select A Different Kingdom To Convert This City", false, "top", 3f);
                }
                else if (selectedConvertCity.data.race == pTile.zone.city.data.race)
                {
                    selectedConvertCity.joinAnotherKingdom(pTile.zone.city.kingdom);
                    WorldTip.instance.show("City Has Been Converted!", false, "top", 3f);
                    selectedConvertCity = null;
                }
                else if (selectedConvertCity.data.race != pTile.zone.city.data.race)
                {
                    WorldTip.instance.show("Cannot Convert City To A Kingdom With Different Race!", false, "top", 3f);
                }
            }
            else
            {
                WorldTip.instance.show("There Is No City In This Zone!", false, "top", 3f);
            }
        } 

        public static Actor getActorByIndex(string id, int index, int secondIndex = -1, int thirdIndex = -1)
        {
            if (id == null)
            {
                return null;
            }

            Family family = (Family)FamilyOverviewWindow.families[index.ToString()];
            foreach(Actor actor in family.actors)
            {
                if (actor == null)
                {
                    continue;
                }
                if (actor.data.actorID == id)
                {
                    return actor;
                }
            }

            if (secondIndex == -1)
            {
                return null;
            }

            Family secondFamily = (Family)FamilyOverviewWindow.families[secondIndex.ToString()];
            foreach(Actor actor in secondFamily.actors)
            {
                if (actor == null)
                {
                    continue;
                }
                if (actor.data.actorID == id)
                {
                    return actor;
                }
            }

            if (thirdIndex == -1)
            {
                return null;
            }

            Family thirdFamily = (Family)FamilyOverviewWindow.families[thirdIndex.ToString()];
            foreach(Actor actor in thirdFamily.actors)
            {
                if (actor == null)
                {
                    continue;
                }
                if (actor.data.actorID == id)
                {
                    return actor;
                }
            }
            return null;
        }

        public static void action_family(WorldTile pTile = null, string pDropID = null)
        {
            MapBox.instance.getObjectsInChunks(pTile, 3, MapObjectType.Actor);
            for (int i = 0; i < MapBox.instance.temp_map_objects.Count; i++)
            {
                Actor pActor = (Actor)MapBox.instance.temp_map_objects[i];
                if (!pActor.stats.unit || pActor.kingdom == null || FamilyActor.getFamily(pActor) != null || pActor.kingdom.id == "nomads_human" || pActor.kingdom.id == "nomads_elf" || pActor.kingdom.id == "nomads_orc" || pActor.kingdom.id == "nomads_dwarf" || pActor.kingdom.id == "mad")
                {
                    continue;
                }
                int familyID = FamilyOverviewWindow.nextID();
                Family newFamily = new Family(pActor.data.actorID, pActor, pActor.getName(), familyID, MapBox.instance.mapStats.year, 0);
                FamilyOverviewWindow.families.Add(familyID.ToString(), newFamily);
                FamilyActor actorFamily = pActor.gameObject.AddComponent<FamilyActor>();
                actorFamily.familyName = pActor.getName();
                actorFamily.founderName = pActor.getName();
                actorFamily.familyIndex = familyID;
                actorFamily.isHead = true;
                actorFamily.deadID = "dead_" + FamilyWindow.nextID().ToString();
                
                FamilyOverviewWindow.deadActorList.Add(
                    actorFamily.deadID,
                    new deadActor().copyFamily(actorFamily, pActor.getName())
                );
                if (pActor.data.gender == ActorGender.Male)
                {
                    actorFamily.isMale = true;
                }
                else
                {
                    actorFamily.isMale = false;
                }
                Localization.AddOrSet("add_parent_dej", "$name$ Has Created A Family!");
                WorldTip.addWordReplacement("$name$", pActor.coloredName);
                WorldTip.showNowTop("add_parent_dej");
            }
        }

        public static void action_warrior(WorldTile pTile = null, string pDropID = null)
        {
            MapBox.instance.CallMethod("getObjectsInChunks", pTile, 3, MapObjectType.Actor);
            var temp_objs = (List<BaseSimObject>)Reflection.GetField(typeof(MapBox), MapBox.instance, "temp_map_objects");
            for (int i = 0; i < temp_objs.Count; i++)
            {
                Actor pActor = (Actor)temp_objs[i];
                if (!pActor.stats.unit || pActor.city == null)
                {
                    continue;
                }
                pActor.CallMethod("setProfession", UnitProfession.Warrior);
                var pAI = (AiSystemActor)Reflection.GetField(typeof(Actor), pActor, "ai");
                if (pActor.equipment.weapon.isEmpty())
                {
                    City.giveItem(pActor, pActor.city.getEquipmentList(EquipmentType.Weapon), pActor.city);
                }
                if (pActor.city.getArmy() == 0 && pActor.city.army == null)
                {
                    UnitGroup army = MapBox.instance.unitGroupManager.createNewGroup(pActor.city);
                    pActor.city.army = army;
                }
                pActor.city.status.warriorCurrent++;
                pAI.setJob("attacker");
            }
        }

        public static void action_civilian(WorldTile pTile = null, string pDropID = null)
        {
            MapBox.instance.CallMethod("getObjectsInChunks", pTile, 3, MapObjectType.Actor);
            var temp_objs = (List<BaseSimObject>)Reflection.GetField(typeof(MapBox), MapBox.instance, "temp_map_objects");
            for (int i = 0; i < temp_objs.Count; i++)
            {
                Actor pActor = (Actor)temp_objs[i];
                if (!pActor.stats.unit || pActor.isProfession(UnitProfession.Unit))
                {
                    continue;
                }
                if(pActor.isProfession(UnitProfession.Warrior) && pActor.city != null)
                {
                    pActor.city.status.warriorCurrent--;
                }
                pActor.CallMethod("setProfession", UnitProfession.Unit);
            }
        }

        // public static void action_death(WorldTile pTile = null, string pDropID = null)
        // {
        //     MapBox.instance.CallMethod("getObjectsInChunks", pTile, 3, MapObjectType.Actor);
        //     var temp_objs = (List<BaseSimObject>)Reflection.GetField(typeof(MapBox), MapBox.instance, "temp_map_objects");
        //     for (int i = 0; i < temp_objs.Count; i++)
        //     {
        //         Actor pActor = (Actor)temp_objs[i];
        //         if (!pActor.stats.unit)
        //         {
        //             continue;
        //         }
        //         if(TexturePatches.UnitDict.ContainsKey(pActor))
        //         {
        //             continue;
        //         }
        //         Reflection.SetField<Sprite>(pActor, "s_head_sprite", null);
        //         TexturePatches.UnitDict.Add(pActor, "t_death");
        //         pActor.CallMethod("loadTexture");
        //         // Reflection.SetField<bool>(pActor, "_positionDirty", true);
        //     }
        // }

        // public static void action_ice_monkey(WorldTile pTile = null, string pDropID = null)
        // {
        //     MapBox.instance.CallMethod("getObjectsInChunks", pTile, 3, MapObjectType.Actor);
        //     var temp_objs = (List<BaseSimObject>)Reflection.GetField(typeof(MapBox), MapBox.instance, "temp_map_objects");
        //     for (int i = 0; i < temp_objs.Count; i++)
        //     {
        //         Actor pActor = (Actor)temp_objs[i];
        //         if (!pActor.stats.unit)
        //         {
        //             continue;
        //         }
        //         if(TexturePatches.UnitDict.ContainsKey(pActor))
        //         {
        //             continue;
        //         }
        //         Reflection.SetField<Sprite>(pActor, "s_head_sprite", null);
        //         TexturePatches.UnitDict.Add(pActor, "t_romanSoldier");
        //         pActor.CallMethod("loadTexture");
        //         // Reflection.SetField<bool>(pActor, "_positionDirty", true);
        //     }
        // }

        public static void loadSave()
        {
            if (!File.Exists($"{SaveManager.currentSavePath}familyActors.json"))
            {
                WorldTip.instance.show("ERROR: familyActors.json Not Detected", false, "top", 3f);
                return;
            }
            FamilyOverviewWindow.families.Clear();
            FamilyOverviewWindow.deadActorList.Clear();

            string data = Zip.Decompress(File.ReadAllBytes($"{SaveManager.currentSavePath}familyActors.json"));
            SavedDicts loadedData = JsonConvert.DeserializeObject<SavedDicts>(data);

            Dictionary<string, Family> newFamilies = new Dictionary<string, Family>();
            foreach(KeyValuePair<string, SavedFamily> kv in loadedData.families)
            {
                Family newFamily = new Family().copyFamily(kv.Value);
                newFamilies.Add(kv.Key, newFamily);
            }

            FamilyOverviewWindow.families = newFamilies;
            FamilyOverviewWindow.deadActorList = loadedData.deadActors;
            FamilyOverviewWindow.curID = loadedData.overviewCurID;
            FamilyWindow.curID = loadedData.windowCurID;

            WorldTip.instance.show("Families Have Been Loaded From Save File", false, "top", 3f);

        }

        public static void createSave()
        {
            if (!Directory.Exists(SaveManager.currentSavePath))
			{
				// Directory.CreateDirectory(SaveManager.currentSavePath);
                WorldTip.instance.show("ERROR: World Not Saved Or Save Path Does Not Exist", false, "top", 3f);
                return;
			}

            if (FamilyOverviewWindow.families.Count <= 0)
            {
                WorldTip.instance.show("ERROR: World Cannot Find Any Families In World", false, "top", 3f);
                return;
            }

            Dictionary<string, Family> curFamilies = FamilyOverviewWindow.families;
            Dictionary<string, SavedFamily> savedFamilies = new Dictionary<string, SavedFamily>();
            Dictionary<string, deadActor> savedDeadActors = FamilyOverviewWindow.deadActorList;

            foreach(KeyValuePair<string, Family> kv in curFamilies)
            {
                if (kv.Value == null)
                {
                    continue;
                }
                SavedFamily newSavedFamily = new SavedFamily();
                newSavedFamily.copyFamily(kv.Value);

                savedFamilies.Add(kv.Key, newSavedFamily);
            }

            SavedDicts savedDicts = new SavedDicts();
            savedDicts.families = savedFamilies;
            savedDicts.deadActors = savedDeadActors;
            savedDicts.overviewCurID = FamilyOverviewWindow.curID;
            savedDicts.windowCurID = FamilyWindow.curID;

            // File.Delete($"{SaveManager.currentSavePath}SavedFamilies.json");
            // string json = JsonConvert.SerializeObject(savedDicts, Formatting.Indented);
            // File.WriteAllText($"{SaveManager.currentSavePath}SavedFamilies.json", json);

            File.Delete($"{SaveManager.currentSavePath}familyActors.json");
            string json = JsonConvert.SerializeObject(savedDicts, Formatting.Indented);
            byte[] bytes = Zip.Compress(json);
            File.WriteAllBytes($"{SaveManager.currentSavePath}familyActors.json", bytes);

            WorldTip.instance.show("Families Have Been Saved Into A File", false, "top", 3f);
             
        }
    }

    [Serializable]
    public class SavedDicts
    {
        public Dictionary<string, SavedFamily> families = new Dictionary<string, SavedFamily>();

        public Dictionary<string, deadActor> deadActors = new Dictionary<string, deadActor>();

        public int overviewCurID = 0;

        public int windowCurID = 0;
    }

    [Serializable]
    public class SavedFamily
    {
        public string founderName;

        public string founderID;

        public string headID;

        public string heirID;

        public Dictionary<string, SavedFamilyActor> actors = new Dictionary<string, SavedFamilyActor>();

        public int index;

        public int founderDate;

        public int currentGeneration;

        public List<string> prevHeads = new List<string>();

        public string title;

        public void copyFamily(Family family)
        {
            foreach (FieldInfo prop in this.GetType().GetFields())
            {
                FieldInfo thisField = family.GetType().GetField(prop.Name);
                if (prop.Name == "actors")
                {
                    Dictionary<string, SavedFamilyActor> actorsTemp = new Dictionary<string, SavedFamilyActor>();
                    HashSet<Actor> fieldValue = (HashSet<Actor>)thisField.GetValue(family);
                    foreach (Actor actor in fieldValue)
                    {
                        if (actor == null)
                        {
                            continue;
                        }
                        actorsTemp.Add(actor.data.actorID, new SavedFamilyActor().copyFamily(FamilyActor.getFamily(actor)));
                    }
                    prop.SetValue(this, actorsTemp);
                    continue;
                }
                
                if (thisField == null)
                {
                    continue;
                }
                prop.SetValue(this, thisField.GetValue(family));
            }

            PropertyInfo headProp = family.GetType().GetProperty("HEADID");
            this.headID = (string)headProp.GetValue(family);
        }
    }

    [Serializable]
    public class SavedFamilyActor
    {
        public List<string> childrenID = new List<string>();

        public string fatherID;

        public string deadFatherID;

        public string motherID;

        public string deadMotherID;

        public string familyName;

        public string founderName;

        public string loverID;

        public int fatherFamilyIndex;

        public int motherFamilyIndex;

        public int familyIndex;
        
        public bool isHead = false;

        public bool isMale;

        public bool isHeir = false;

        public string deadID;

        public SavedFamilyActor copyFamily(FamilyActor actorFamily)
        {
            foreach (FieldInfo prop in this.GetType().GetFields())
            {
                if (actorFamily == null)
                {
                    Debug.Log("family NULL");
                }
                FieldInfo thisField = actorFamily.GetType().GetField(prop.Name);
                if (thisField == null)
                {
                    continue;
                }
                prop.SetValue(this, thisField.GetValue(actorFamily));
            }
            return this;
        }
    }
}

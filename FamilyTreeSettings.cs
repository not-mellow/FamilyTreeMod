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
    public class FamilyTreeSettings
    {
        public Dictionary<string, List<FamilyInfo>> families = new Dictionary<string, List<FamilyInfo>>();

        public static void create_settings()
        {
            string json = JsonConvert.SerializeObject(new FamilyTreeSettings(), Formatting.Indented);
            File.WriteAllText($"{Paths.PluginPath}/CollectionMod/FamilyTreeModSettings.json", json);
        }

        public static void load_settings()
        {
            string data = File.ReadAllText($"{Paths.PluginPath}/CollectionMod/FamilyTreeModSettings.json");
            FamilyTreeSettings loadedData = JsonConvert.DeserializeObject<FamilyTreeSettings>(data);
            Plugin.settings = loadedData;
        }

        public static void save_settings()
        {
            File.Delete($"{Paths.PluginPath}/CollectionMod/FamilyTreeModSettings.json");
            string json = JsonConvert.SerializeObject(Plugin.settings, Formatting.Indented);
            File.WriteAllText($"{Paths.PluginPath}/CollectionMod/FamilyTreeModSettings.json", json);
            WorldTip.instance.show("Family Tree Settings Have Been Saved", false, "top", 3f);
        }
    }
}
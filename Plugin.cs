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
    // Commenting My Code Cause Of Haters SMH! (jk key and cody, i luv u)
    [BepInPlugin("org.bepinex.plugins.familytreemod", "FamilyTreeMod", "1.1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static FamilyTreeSettings settings = new FamilyTreeSettings();
        public static bool initialized = false;

        private void Awake()
        {
            // Plugin Startup Logic
            Logger.LogInfo($"Plugin org.bepinex.plugins.familytreemod is loaded!");
        }

        IEnumerator Start()
        {
            // Settings Logic
            if (!File.Exists($"{Paths.PluginPath}/FamilyTreeMod/FamilyTreeModSettings.json"))
            {
                FamilyTreeSettings.create_settings();
            }
            else
            {
                FamilyTreeSettings.load_settings();
            }

            // Initializing Stuff
            AssetLoader.init();
            Patches.init();
            yield return new WaitForSeconds(1f);

            // This Waits For Canvas Instance For UI Logic
            while(CanvasMain.instance == null)
            {
                yield return new WaitForSeconds(2f);
            }
            UI.init();
            WindowManager.init();
            TabManager.init();
        }

        void Update()
        {
            if (!global::Config.gameLoaded) return;

            // Other Initializing Stuff
            if (!initialized)
            {
                Powers.init();
                initialized = true;
            }
        }

    }
}

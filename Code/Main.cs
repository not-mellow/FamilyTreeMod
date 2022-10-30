using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using UnityEngine;
using ReflectionUtility;

namespace FamilyTreeMod
{
    [ModEntry]
    class Main : MonoBehaviour
    {
        private static bool tabInitialized = false;
        private static bool tabsHidden = false;

        void Awake()
        {
            FamilyPatches.init();
            // TexturePatches.init();
            // ActorPatches.init();
            NewGodPowers.init();
            WindowManager.init();
            InvokeRepeating("FixedUpdate", 3f, 0.05f);
        }

        public void FixedUpdate()
        {
            if (!Config.gameLoaded)
            {
                return;
            }

            if (!tabInitialized)
            {
                DejTab.init();
                RelationsWindow.init();
                FamilyWindow.init();
                // FamilyOverviewWindow.init();
                SettingsWindow.init();
                SearchWindow.init();
                TabButtons.init();
                // StatsWindow.init();
                // AddStatsWindow.init();
                tabInitialized = true;
            }

            if (Input.GetKeyUp(KeyCode.J))
            {
                hideAllTabs(tabsHidden);
                DejTab.powersTabComponent.powerButton.gameObject.SetActive(!tabsHidden);
                tabsHidden = !tabsHidden;
            }
        }
        public static void hideAllTabs(bool isActive)
        {
            //MapBox.instance.canvas.gameObject.SetActive(false);
            var tabController = (PowerTabController)Reflection.GetField(typeof(PowerTabController), null, "instance");
            if (tabController == null)
            {
                return;
            }
            //tabController.t_main.gameObject.SetActive(false);
            tabController.t_nature.gameObject.SetActive(isActive);
            tabController.t_kingdoms.gameObject.SetActive(isActive);
            tabController.t_drawing.gameObject.SetActive(isActive);
            tabController.t_creatures.gameObject.SetActive(isActive);
            tabController.t_bombs.gameObject.SetActive(isActive);
            tabController.t_other.gameObject.SetActive(isActive);
        }
    }
}
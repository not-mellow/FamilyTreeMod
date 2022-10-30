using System;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using ReflectionUtility;

namespace FamilyTreeMod
{
    class TabButtons
    {
        public static void init()
        {
            PowerButtons.CreateButton(
                "familySelect",
                Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.family_icon.png"),
                "Create A Family",
                "Select A Unit To Make Them The Head Of A New Family",
                createPos(1,0),
                ButtonType.GodPower,
                DejTab.additionalPowersTab.transform,
                null
                );

            PowerButtons.CreateButton(
                "familyOverviewButton",
                Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.familyOverview_icon.png"),
                "Family Overview",
                "View The Status Of All Families",
                createPos(2,0),
                ButtonType.Click,
                DejTab.additionalPowersTab.transform,
                () => NewActions.openFamilyOverview()
                );
            
            PowerButtons.CreateButton(
                "ModSettings",
                Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.debug_icon.png"),
                "Mod Settings",
                "Change the settings to your liking",
                createPos(3,0),
                ButtonType.Click,
                DejTab.additionalPowersTab.transform,
                () => Windows.ShowWindow("modSettingsWindow")
                );

            PowerButtons.CreateButton(
                "civilianProf",
                Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.civilian_icon.png"),
                "Set Civilian",
                "Make Units into A Civilian",
                createPos(4,0),
                ButtonType.GodPower,
                DejTab.additionalPowersTab.transform,
                null
                );

            PowerButtons.CreateButton(
                "warriorProf",
                Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.warrior_icon.png"),
                "Set Warrior",
                "Make Units Into A Warrior",
                createPos(5,0),
                ButtonType.GodPower,
                DejTab.additionalPowersTab.transform,
                null
                );

            PowerButtons.CreateButton(
                "loadFamily",
                Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.load_icon.png"),
                "Load Saved Family",
                "Load Family From Save File",
                createPos(6,0),
                ButtonType.Click,
                DejTab.additionalPowersTab.transform,
                NewActions.loadSave
                );
            
            PowerButtons.CreateButton(
                "saveFamily",
                Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.save_icon.png"),
                "Save Families",
                "Save Families Into A File",
                createPos(7,0),
                ButtonType.Click,
                DejTab.additionalPowersTab.transform,
                NewActions.createSave
                );
            
            PowerButtons.CreateButton(
                "searchButton",
                Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.iconWorldInfo.png"),
                "Search Unit",
                "Opens window that will show units",
                createPos(8,0),
                ButtonType.Click,
                DejTab.additionalPowersTab.transform,
                () => SearchWindow.openWindow()
                );
            
            // PowerButtons.CreateButton(
            //     "statButton",
            //     Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.iconWorldInfo.png"),
            //     "Stat Limits",
            //     "Opens window that will show stats that you can limit",
            //     createPos(9,0),
            //     ButtonType.Click,
            //     DejTab.additionalPowersTab.transform,
            //     () => Windows.ShowWindow("statsWindow")
            //     );

            // PowerButtons.CreateButton(
            //     "addStatButton",
            //     Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.iconWorldInfo.png"),
            //     "Add Stats",
            //     "Edit how many stats you can add to units",
            //     createPos(10,0),
            //     ButtonType.Click,
            //     DejTab.additionalPowersTab.transform,
            //     () => Windows.ShowWindow("addStatsWindow")
            //     );

            PowerButtons.CreateButton(
                "cultureButton",
                Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.iconWorldInfo.png"),
                "Edit Cultures",
                "Opens window that has options on culture tech",
                createPos(10,0),
                ButtonType.Click,
                DejTab.additionalPowersTab.transform,
                CultureTechWindow.openCultureWindow
                );


            // NCMS.Utils.Localization.addLocalization("guy", "Guy");
            // NCMS.Utils.Localization.addLocalization("guy Description", "Spawn New Guy");
            // PowerButtons.CreateButton(
            //     "spawnGuy",
            //     Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.warrior_icon.png"),
            //     "Spawn New Guy",
            //     "Make New Guy",
            //     createPos(9,0),
            //     ButtonType.GodPower,
            //     DejTab.additionalPowersTab.transform,
            //     null
            //     );

            // PowerButtons.CreateButton(
            //     "monkeUnit",
            //     Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.romanSoldier_icon.png"),
            //     "Set Roman Soldier",
            //     "Make Units Into A Roman Soldier",
            //     createPos(9,0),
            //     ButtonType.GodPower,
            //     DejTab.additionalPowersTab.transform,
            //     null
            //     );
        }

        private static Vector2 createPos(int rowIndex, int colIndex)
        {
            float startX = 100;
            Vector2 size = new Vector2(40, 25);
            float posX = rowIndex * size.x;
            float posY = colIndex * -size.y;

            var result = new Vector2(startX + posX, posY);
            return result;
        }
    }
}
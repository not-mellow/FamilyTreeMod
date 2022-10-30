using System;
using System.Collections;
using System.Collections.Generic;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using ReflectionUtility;

namespace FamilyTreeMod
{
    class NewGodPowers
    {
        public static void init()
        {
            initPowers();
            // initStats();
        }

        public static bool callSpawnDrops(WorldTile tTile, GodPower pPower)
        {
            AssetManager.powers.CallMethod("spawnDrops", tTile, pPower);
            return true;
        }

        public static bool callFlashPixel(WorldTile pTile, GodPower pPower)
        {
            AssetManager.powers.CallMethod("flashPixel", pTile, pPower);
            return true;
        }

        public static bool callLoopBrush(WorldTile pTile, GodPower pPower)
        {
            AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower);
            return true;
        }

        public static bool callSpawnUnit(WorldTile pTile, string pPowerID)
        {
            AssetManager.powers.CallMethod("spawnUnit", pTile, pPowerID);
            return true;
        }

        public static void initPowers()
        {
            DropAsset famliyDrop = AssetManager.drops.clone("famliyDrop", "blessing");
            famliyDrop.action_landed = new DropsAction(NewActions.action_family);

            GodPower familyPower = AssetManager.powers.clone("familySelect", "_drops");
            familyPower.name = "familySelect";
            familyPower.dropID = "famliyDrop";
            familyPower.fallingChance = 0.01f;
            familyPower.click_power_action = new PowerAction(callSpawnDrops);
            // civilianPower.click_power_action = (PowerAction)Delegate.Combine(warriorPower.click_power_action, new PowerAction(callFlashPixel));
            familyPower.click_power_brush_action = new PowerAction(callLoopBrush);

            DropAsset warriorDrop = AssetManager.drops.clone("warriorDrop", "blessing");
            warriorDrop.action_landed = new DropsAction(NewActions.action_warrior);

            GodPower warriorPower = AssetManager.powers.clone("warriorProf", "_drops");
            warriorPower.name = "warriorProf";
            warriorPower.dropID = "warriorDrop";
            warriorPower.fallingChance = 0.01f;
            warriorPower.click_power_action = new PowerAction(callSpawnDrops);
            // warriorPower.click_power_action = (PowerAction)Delegate.Combine(warriorPower.click_power_action, new PowerAction(callFlashPixel));
            warriorPower.click_power_brush_action = new PowerAction(callLoopBrush);

            DropAsset civilianDrop = AssetManager.drops.clone("civilianDrop", "blessing");
            civilianDrop.action_landed = new DropsAction(NewActions.action_civilian);

            GodPower civilianPower = AssetManager.powers.clone("civilianProf", "_drops");
            civilianPower.name = "civilianProf";
            civilianPower.dropID = "civilianDrop";
            civilianPower.fallingChance = 0.01f;
            civilianPower.click_power_action = new PowerAction(callSpawnDrops);
            // civilianPower.click_power_action = (PowerAction)Delegate.Combine(warriorPower.click_power_action, new PowerAction(callFlashPixel));
            civilianPower.click_power_brush_action = new PowerAction(callLoopBrush);

            // DropAsset deathDrop = AssetManager.drops.clone("deathDrop", "blessing");
            // deathDrop.action_landed = new DropsAction(NewActions.action_death);

            // GodPower deathPower = AssetManager.powers.clone("deathUnit", "_drops");
            // deathPower.name = "deathUnit";
            // deathPower.dropID = "deathDrop";
            // deathPower.fallingChance = 0.01f;
            // deathPower.click_power_action = new PowerAction(callSpawnDrops);
            // // deathPower.click_power_action = (PowerAction)Delegate.Combine(deathPower.click_power_action, new PowerAction(callFlashPixel));
            // deathPower.click_power_brush_action = new PowerAction(callLoopBrush);

            // DropAsset monkeDrop = AssetManager.drops.clone("monkeDrop", "blessing");
            // monkeDrop.action_landed = new DropsAction(NewActions.action_ice_monkey);

            // GodPower monkePower = AssetManager.powers.clone("monkeUnit", "_drops");
            // monkePower.name = "monkeUnit";
            // monkePower.dropID = "monkeDrop";
            // monkePower.fallingChance = 0.01f;
            // monkePower.click_power_action = new PowerAction(callSpawnDrops);
            // // deathPower.click_power_action = (PowerAction)Delegate.Combine(deathPower.click_power_action, new PowerAction(callFlashPixel));
            // deathPower.click_power_brush_action = new PowerAction(callLoopBrush);
        }

        public static void initStats()
        {
            GodPower spawnGuy = AssetManager.powers.clone("spawnGuy", "dragon");
            spawnGuy.name = "guy";
            spawnGuy.actorStatsId = "guy";
            spawnGuy.click_action = new PowerActionWithID(callSpawnUnit);
            
            var newGuyStats = AssetManager.unitStats.clone("guy", "dragon");
            newGuyStats.prefab = "p_guy";
        }
    }
}
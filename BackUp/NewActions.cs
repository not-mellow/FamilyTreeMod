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
        public static GameObject createCrownIcon(GameObject parent, string title)
        {
            GameObject crownIcon = new GameObject("Crown Icon");
            crownIcon.transform.SetParent(parent.transform);
            Image crownImage = crownIcon.AddComponent<Image>();
            crownImage.sprite = Mod.EmbededResources.LoadSprite($"FamilyTreeMod.Resources.Icons.{title}_icon.png");
            RectTransform crownRect = crownIcon.GetComponent<RectTransform>();
            crownRect.localPosition = new Vector3(0, 0, 0);
            crownRect.sizeDelta -= new Vector2(20, 20);
            if(title == "Royalty" || title == "Noble")
            {
                crownRect.sizeDelta -= new Vector2(30, 30);
            }
            return crownIcon;
        }
        public static GameObject createProgressBar(GameObject parent)
        {
            GameObject researchBar = GameObjects.FindEvenInactive("HealthBar");
            GameObject progressBar = Instantiate(researchBar, parent.transform);
            progressBar.name = "ProgressBar";
            progressBar.SetActive(true);

            RectTransform progressRect = progressBar.GetComponent<RectTransform>();
            progressRect.localPosition = new Vector3(0, 0, 0);

            StatBar statBar = progressBar.GetComponent<StatBar>();
            statBar.restartBar();

            TipButton tipButton = progressBar.GetComponent<TipButton>();
            tipButton.textOnClick = "Progress Bar";

            GameObject icon = progressBar.transform.Find("Icon").gameObject;
            icon.SetActive(false);

            return progressBar;
        }

        public static GameObject createZoomButton(GameObject parent)
        {
            GameObject zoomHolder = new GameObject("Zoom Holder");
            zoomHolder.transform.SetParent(parent.transform);

            GameObject zoomOut = new GameObject("Zoom Out");
            zoomOut.transform.SetParent(zoomHolder.transform);
            zoomOut.AddComponent<CanvasRenderer>();
            Button buttonOut = zoomOut.AddComponent<Button>();
            Image imageOut = zoomOut.AddComponent<Image>();
            buttonOut.onClick.AddListener(() => FamilyWindow.zoomContent(-0.05f));
            imageOut.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.backgroundTabButton.png");

            GameObject zoomOutIcon = new GameObject("icon");
            zoomOutIcon.transform.SetParent(zoomOut.transform);
            RectTransform outIconRect = zoomOutIcon.AddComponent<RectTransform>();
            outIconRect.localPosition = new Vector3(0, 0, 0);
            Image outIcon = zoomOutIcon.AddComponent<Image>();
            outIcon.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.iconUpvote.png");

            RectTransform outRect = zoomOut.GetComponent<RectTransform>();
            outRect.localPosition = new Vector3(0, -100, 0);

            GameObject zoomIn = new GameObject("Zoom In");
            zoomIn.transform.SetParent(zoomHolder.transform);
            zoomIn.AddComponent<CanvasRenderer>();
            Button buttonIn = zoomIn.AddComponent<Button>();
            Image imageIn = zoomIn.AddComponent<Image>();
            buttonIn.onClick.AddListener(() => FamilyWindow.zoomContent(0.05f));
            imageIn.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.backgroundTabButton.png");

            GameObject zoomInIcon = new GameObject("icon");
            zoomInIcon.transform.SetParent(zoomIn.transform);
            RectTransform inIconRect = zoomInIcon.AddComponent<RectTransform>();
            inIconRect.localPosition = new Vector3(0, 0, 0);
            inIconRect.localRotation = Quaternion.Euler(0, 0, 180);
            Image inIcon = zoomInIcon.AddComponent<Image>();
            inIcon.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.iconUpvote.png");

            RectTransform inRect = zoomIn.GetComponent<RectTransform>();
            inRect.localPosition = new Vector3(0, 100, 0);

            RectTransform zoomRect = zoomHolder.AddComponent<RectTransform>();
            zoomRect.localPosition = new Vector3(118, 0, 0);

            return zoomHolder;
        }

        public void addDeleteButton(GameObject parent, ActorParent actorParent)
        {
            GameObject deleteGO = new GameObject("deleteButton");
            deleteGO.transform.SetParent(parent.transform);
            Image deleteImage = deleteGO.AddComponent<Image>();
            deleteImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.delete_ui.png");
            Button deleteButton = deleteGO.AddComponent<Button>();
            deleteButton.onClick.AddListener(() => StartCoroutine(FamilyOverviewWindow.deleteFamily(parent, actorParent)));
            
            RectTransform deleteRect = deleteGO.GetComponent<RectTransform>();
            deleteRect.localPosition = new Vector3(80, 60, 0);
            deleteRect.sizeDelta = new Vector2(50, 50);
        }

        public static GameObject addBanner(GameObject parent, Kingdom kingdom, ActorParent actorParent)
        {
            GameObject bannerGO = new GameObject("bannerHolder");
            bannerGO.transform.SetParent(parent.transform);
            bannerGO.AddComponent<CanvasRenderer>();
            Button backgroundButton = bannerGO.AddComponent<Button>();
            backgroundButton.onClick.AddListener(() => RelationsWindow.showRelations(actorParent));

            if (kingdom == null || kingdom.id == "nomads_human" || kingdom.id == "nomads_elf" || kingdom.id == "nomads_orc" || kingdom.id == "nomads_dwarf" || kingdom.id == "mad")
            {
                GameObject deadBanner = new GameObject("deadBanner");
                deadBanner.transform.SetParent(bannerGO.transform);
                Image deadImage = deadBanner.AddComponent<Image>();
                deadImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.ghost_icon.png");
                RectTransform deadRect = deadBanner.GetComponent<RectTransform>();
                deadRect.localPosition = new Vector3(0, 0, 0);
                deadRect.sizeDelta = new Vector2(55, 55);
                bannerGO.transform.localPosition = new Vector3(0, -15,0);
                return bannerGO;
            }

            GameObject backgroundGO = new GameObject("background");
            backgroundGO.transform.SetParent(bannerGO.transform);
            Image backgroundImage = backgroundGO.AddComponent<Image>();

            GameObject iconGO = new GameObject("icon");
            iconGO.transform.SetParent(bannerGO.transform);
            Image iconImage = iconGO.AddComponent<Image>();

            BannerLoader bannerLoader = bannerGO.AddComponent<BannerLoader>();
            bannerLoader.partIcon = iconImage;
            bannerLoader.partBackround = backgroundImage;
            bannerLoader.load(kingdom);

            bannerGO.transform.localPosition = new Vector3(0, -15,0);
            return bannerGO;
        }

        public static void addShowChildrenButton(GameObject parent, List<GameObject> targetList)
        {
            GameObject buttonGO = new GameObject("showButton");
            buttonGO.transform.SetParent(parent.transform);
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.iconArrowBack.png");
            var buttonComp = buttonGO.AddComponent<Button>() as Button;
            string name = parent.transform.name;
            buttonComp.onClick.AddListener(() => showGameObjects(targetList));

            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            // buttonRect.sizeDelta = new Vector2(125, 15);
            buttonRect.localPosition = new Vector3(-65, -50, 0);
        }

        public static void showGameObjects(List<GameObject> GOList)
        {
            List<GameObject> activeGOs = new List<GameObject>();
            foreach(GameObject GObject in GOList)
            {
                if (GObject.activeSelf == false)
                {
                    GObject.SetActive(true);
                    activeGOs.Add(GObject);
                }else
                {
                    string inactiveName = GObject.transform.name;
                    int inactiveIndex = (int)inactiveName[inactiveName.Length - 1];
                    if (FamilyWindow.activeGameObjects.ContainsKey(inactiveIndex))
                    {
                        FamilyWindow.activeGameObjects.Remove(inactiveIndex);
                    }
                    GObject.SetActive(false);
                }
            }

            if (activeGOs.Count == 0)
            {
                return;
            }
            string name = activeGOs.ElementAt(0).transform.name;
            int index = (int)name[name.Length - 1];

            if (FamilyWindow.activeGameObjects.ContainsKey(index))
            {
                hideGameObjects(FamilyWindow.activeGameObjects[index]);
                FamilyWindow.activeGameObjects.Remove(index);
            }
            FamilyWindow.activeGameObjects.Add(index, activeGOs);
        }

        public static void hideGameObjects(List<GameObject> GOList)
        {
            foreach(GameObject GObject in GOList)
            {
                GObject.SetActive(false);
            }
        }

        public static GameObject addArrow(GameObject parent, GameObject targetObj)
        {
            GameObject arrowGO = new GameObject("arrow");
            arrowGO.transform.SetParent(parent.transform);
            Image arrowImage = arrowGO.AddComponent<Image>();
            arrowImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.arrow_ui.png");
            RectTransform arrowRect = arrowGO.GetComponent<RectTransform>();
            arrowRect.sizeDelta = new Vector2(125, 15);
            arrowRect.localPosition = new Vector3(0, -60, 0);
            arrowRect.pivot = new Vector2(0, (float)0.5);

            RectTransform targetRect = targetObj.GetComponent<RectTransform>();

            var angleRadians = Mathf.Atan2(targetRect.localPosition.x, targetRect.localPosition.y);
            var angleDegrees = angleRadians * Mathf.Rad2Deg;
            if (angleDegrees>0) angleDegrees+=180;
            if (angleDegrees<0) angleDegrees+=360;

            arrowRect.localRotation = Quaternion.Euler(0, 0, angleDegrees);
            return arrowGO;
        }

        public static GameObject addCanvasRenderer(GameObject canvasParent, Vector3 pos)
        {
            GameObject bgHolder = new GameObject("bgHolder");
            bgHolder.transform.SetParent(canvasParent.transform);
            bgHolder.AddComponent<CanvasRenderer>();
            Image bgImage = bgHolder.AddComponent<Image>();
            var newSprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.windowInnerSliced.png");
            bgImage.sprite = newSprite;
            RectTransform bgRect = bgHolder.GetComponent<RectTransform>();
            bgRect.localPosition = pos;
            bgRect.sizeDelta = new Vector2(180, 140);
            bgRect.localScale += new Vector3(0, 0, 0.68f);

            return bgHolder;
        }

        public static GameObject addUnitAvatar(Actor actor, GameObject parent)
        {
            if (actor == null)
            {
                var ghostObject = new GameObject("ghostAvatar");
                ghostObject.transform.SetParent(parent.transform);
                RectTransform ghostRectTransform = ghostObject.AddComponent<RectTransform>();
                ghostRectTransform.localPosition = new Vector3(0, -20, 0);
                ghostRectTransform.sizeDelta = new Vector2(70, 70);
                Image img = ghostObject.AddComponent<Image>();
                img.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.ghost_icon.png");
                return ghostObject;
            }
            RectTransform rectTransform;
            var avatarObject = new GameObject("avatar");
            avatarObject.transform.SetParent(parent.transform);
            rectTransform = avatarObject.AddComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(0, 0, 0);
            rectTransform.sizeDelta = new Vector2(500, 100);
            var avatarLoader = avatarObject.AddComponent<UnitAvatarLoader>() as UnitAvatarLoader;
            var avatarButton = avatarObject.AddComponent<Button>() as Button;
            avatarButton.onClick.AddListener(() => avatarOnClick(actor));
            avatarLoader.load(actor);
            GameObject avatarImage = avatarObject.transform.GetChild(0).gameObject;
            GameObject avatarItem = null;
            if (avatarObject.transform.GetChildCount() == 2)
            {
                avatarItem = avatarObject.transform.GetChild(1).gameObject;
            }
            RectTransform avatarImageRect = avatarImage.GetComponent<RectTransform>();
            avatarImageRect.localPosition = new Vector3(0, -20, 0);
            avatarImageRect.sizeDelta = new Vector2(30, 30);
            if (avatarItem != null)
            {
                RectTransform avatarItemRect = avatarItem.GetComponent<RectTransform>();
                avatarItemRect.localPosition = new Vector3(-12, -5, 0);
                avatarItemRect.sizeDelta = new Vector2(8, 16);
            }
            return avatarObject;
        }

        private static void avatarOnClick(Actor actor)
        {
            Config.selectedUnit = actor;
            Windows.ShowWindow("inspect_unit");
        }

        public static Text addText(string textString, GameObject parent, int sizeFont)
        {
            GameObject textRef = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyWindow/Background/Name");
            GameObject textGo = Instantiate(textRef, parent.transform);
            textGo.SetActive(true);

            var textComp = textGo.GetComponent<Text>();
            textComp.text = textString;
            textComp.fontSize = sizeFont;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.position = new Vector3(0,0,0);
            textRect.localPosition = new Vector3(0, 25, 0);
            textRect.sizeDelta = new Vector2(textComp.preferredWidth, textComp.preferredHeight);
        
            return textComp;
        }

        public static GameObject createRelationsButton(GameObject parent, ActorParent actorParent)
        {
            GameObject relationsGO = new GameObject("relatonsButton");
            relationsGO.transform.SetParent(parent.transform);
            RectTransform relationsRect = relationsGO.AddComponent<RectTransform>();
            relationsRect.localPosition = new Vector3(118, 50, 0);
            Button relationsButton = relationsGO.AddComponent<Button>();
            relationsButton.onClick.AddListener(() => RelationsWindow.showRelations(actorParent));
            Image relationsImage = relationsGO.AddComponent<Image>();
            relationsImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.backgroundTabButton.png");

            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(relationsGO.transform);
            Image iconImage = iconGO.AddComponent<Image>();
            iconImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.ghost_icon.png");
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.localPosition = new Vector3(0, 0, 0);

            return relationsGO;
        }

        public static GameObject createFamilyButton(GameObject parent, ActorParent actorParent)
        {
            GameObject relationsGO = new GameObject("familyButton");
            relationsGO.transform.SetParent(parent.transform);
            RectTransform relationsRect = relationsGO.AddComponent<RectTransform>();
            relationsRect.localPosition = new Vector3(118, 50, 0);
            Button relationsButton = relationsGO.AddComponent<Button>();
            relationsButton.onClick.AddListener(() => FamilyWindow.showFamily(actorParent, false));
            Image relationsImage = relationsGO.AddComponent<Image>();
            relationsImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.UI.backgroundTabButton.png");

            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(relationsGO.transform);
            Image iconImage = iconGO.AddComponent<Image>();
            iconImage.sprite = Mod.EmbededResources.LoadSprite("FamilyTreeMod.Resources.Icons.family_icon.png");
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.localPosition = new Vector3(0, 0, 0);

            return relationsGO;
        }

        public static void openFamilyOverview()
        {
            FamilyOverviewWindow.loadBanners();
            Windows.ShowWindow("familyOverview");
        }

        public static void action_family(WorldTile pTile = null, string pDropID = null)
        {
            MapBox.instance.getObjectsInChunks(pTile, 3, MapObjectType.Actor);
            for (int i = 0; i < MapBox.instance.temp_map_objects.Count; i++)
            {
                Actor pActor = (Actor)MapBox.instance.temp_map_objects[i];
                if (!pActor.stats.unit || pActor.kingdom == null || FamilyOverviewWindow.familyActors.Contains(pActor) || pActor.kingdom.id == "nomads_human" || pActor.kingdom.id == "nomads_elf" || pActor.kingdom.id == "nomads_orc" || pActor.kingdom.id == "nomads_dwarf" || pActor.kingdom.id == "mad")
                {
                    continue;
                }
                FamilyOverviewWindow.addParent(pActor);
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
            if (!File.Exists($"{SaveManager.currentSavePath}familyHeads.json"))
            {
                WorldTip.instance.show("ERROR: familyHeads.json Not Detected", false, "top", 3f);
                return;
            }
            if (!File.Exists($"{SaveManager.currentSavePath}familyActors.json"))
            {
                WorldTip.instance.show("ERROR: familyActors.json Not Detected", false, "top", 3f);
                return;
            }
            FamilyOverviewWindow.familyHeads.Clear();
            FamilyOverviewWindow.familyActors.Clear();
            FamilyOverviewWindow.families.Clear();
            FamilyOverviewWindow.originalFamilies.Clear();
            FamilyOverviewWindow.numOfFamilies = 0;
            RelationsWindow.royalFamilies.Clear();
            RelationsWindow.nobleFamilies.Clear();

            string headsData = File.ReadAllText($"{SaveManager.currentSavePath}familyHeads.json");
            List<SavedActor> loadedHeadsData = JsonConvert.DeserializeObject<List<SavedActor>>(headsData);

            foreach(SavedActor actor in loadedHeadsData)
            {
                // string objectName = actor.savedStatsID + "_" + actor.savedActorID;
                // var newObject = (GameObject)GameObjects.FindEvenInactive(objectName);
                // if (newObject == null)
                // {
                //     continue;
                // }
                Actor actorComp = MapBox.instance.getActorByID(actor.savedActorID);
                if(actorComp == null)
                {
                    continue;
                }
                List<Actor> childActorCompList = new List<Actor>();

                foreach(SavedActor childActor in actor.savedChildren)
                {
                    string childObjectName = childActor.savedStatsID + "_" + childActor.savedActorID;
                    // GameObject newChildObject = (GameObject)GameObjects.FindEvenInactive(childObjectName);
                    // if (newChildObject == null)
                    // {
                    //     continue;
                    // }
                    Actor childActorComp = MapBox.instance.getActorByID(childActor.savedActorID);
                    if(childActorComp == null)
                    {
                        continue;
                    }
                    childActorCompList.Add(childActorComp);
                }
                FamilyOverviewWindow.loadParent(actorComp, childActorCompList, actor.savedHead, actor.savedOriginal);
            }

            string actorsData = Zip.Decompress(File.ReadAllBytes($"{SaveManager.currentSavePath}familyActors.json"));
            List<SavedActor> loadedActorsData = JsonConvert.DeserializeObject<List<SavedActor>>(actorsData);

            foreach(SavedActor sActor in loadedActorsData)
            {
                // string objectName = sActor.savedStatsID + "_" + sActor.savedActorID;
                // GameObject newObject = (GameObject)GameObjects.FindEvenInactive(objectName);
                // if (newObject == null)
                // {
                //     continue;
                // }
                Actor actorComp = MapBox.instance.getActorByID(sActor.savedActorID);
                if(actorComp == null)
                {
                    continue;
                }
                List<Actor> childActorCompList = new List<Actor>();
                foreach(SavedActor childActor in sActor.savedChildren)
                {
                    // string childObjectName = childActor.savedStatsID + "_" + childActor.savedActorID;
                    // GameObject newChildObject = (GameObject)GameObjects.FindEvenInactive(childObjectName);
                    // if (newChildObject == null)
                    // {
                    //     continue;
                    // }
                    Actor childActorComp = MapBox.instance.getActorByID(childActor.savedActorID);
                    if(childActorComp == null)
                    {
                        continue;
                    }
                    childActorCompList.Add(childActorComp);
                }
                FamilyOverviewWindow.loadParent(actorComp, childActorCompList, null, sActor.savedOriginal);
            }

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

            // List<Actor> actorsCopy = (from element in FamilyOverviewWindow.familyActors where element != null select element).ToList();
            Dictionary<ActorParent, ActorHead> headsCopy = (from kv in FamilyOverviewWindow.familyHeads where kv.Key.parentActor != null select kv).ToDictionary(kv => kv.Key, kv => kv.Value);
            List<ActorParent> familiesCopy = (from element in FamilyOverviewWindow.families where element.parentActor != null select element).ToList();
            
            List<SavedActor> headList = new List<SavedActor>();
            List<SavedActor> familyActorList = new List<SavedActor>();

            foreach(KeyValuePair<ActorParent, ActorHead> kvp in headsCopy)
            {
                List<SavedActor> childrenList = new List<SavedActor>();
                foreach(Actor actor in kvp.Key.children)
                {
                    if(actor == null)
                    {
                        continue;
                    }
                    childrenList.Add(
                        new SavedActor{
                            savedActorID = actor.data.actorID,
                            savedStatsID = actor.data.statsID,
                        });
                }
                headList.Add(
                    new SavedActor{
                        savedActorID = kvp.Key.parentActor.data.actorID,
                        savedStatsID = kvp.Key.parentActor.data.statsID,
                        savedChildren = childrenList,
                        savedHead = new SavedHead{
                            foundingYear = kvp.Value.foundingYear,
                            currentGeneration = kvp.Value.currentGeneration,
                            numOfMembers = kvp.Value.numOfMembers},
                        savedOriginal = new SavedParent{
                            familyName = kvp.Key.originalFamily.familyName,
                            fullName = kvp.Key.originalFamily.parentActor.getName()}
                        });
            }

            if (headList.Count > 0)
            {
                File.Delete($"{SaveManager.currentSavePath}familyHeads.json");
                string json = JsonConvert.SerializeObject(headList, Formatting.Indented);
                File.WriteAllText($"{SaveManager.currentSavePath}familyHeads.json", json);
            }

            foreach(ActorParent family in familiesCopy)
            {
                List<SavedActor> childrenList = new List<SavedActor>();
                foreach(Actor actor in family.children)
                {
                    if (actor == null)
                    {
                        continue;
                    }
                    childrenList.Add(
                        new SavedActor{
                            savedActorID = actor.data.actorID,
                            savedStatsID = actor.data.statsID,
                    });
                }
                familyActorList.Add(
                    new SavedActor{
                        savedActorID = family.parentActor.data.actorID,
                        savedStatsID = family.parentActor.data.statsID,
                        savedChildren = childrenList,
                        savedOriginal = new SavedParent{
                            familyName = family.originalFamily.familyName,
                            fullName = family.originalFamily.parentActor.getName()}
                });
            }

            if (familyActorList.Count > 0)
            {
                File.Delete($"{SaveManager.currentSavePath}familyActors.json");
                string json = JsonConvert.SerializeObject(familyActorList, Formatting.Indented);
                // File.WriteAllText($"{SaveManager.currentSavePath}familyActors.json", json);
                byte[] bytes = Zip.Compress(json);
                File.WriteAllBytes($"{SaveManager.currentSavePath}familyActors.json", bytes);
            }

            WorldTip.instance.show("Families Have Been Saved Into A File", false, "top", 3f);
             
        }
    }

    [Serializable]
    public class SavedActor
    {
        public string savedActorID;
        public string savedStatsID;
        public List<SavedActor>? savedChildren;
        public SavedHead? savedHead;
        public SavedParent? savedOriginal;
    }

    [Serializable]
    public class SavedHead
    {
        public int foundingYear;
        public int currentGeneration;
        public int numOfMembers;
    }

    [Serializable]
    public class SavedParent
    {
        public string familyName;
        public string fullName;
    }
}

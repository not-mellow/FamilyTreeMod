using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;

namespace FamilyTreeMod
{
    class DeadActorWindow : MonoBehaviour
    {
        private static GameObject contents;

        private static List<string> statNames = new List<string>{"health", "speed", "armor", "crit", "damage", "attackSpeed"};

        public static void init()
        {
            contents = WindowManager.windowContents["deadActorWindow"];
        }

        public static void openWindow(string deadID)
        {
            foreach(Transform child in contents.transform)
            {
                Destroy(child.gameObject);
            }
            createUI(deadID, null);
            Windows.ShowWindow("deadActorWindow");
        }

        private static void createUI(string deadID, string actorID)
        {
            Actor aliveActor = MapBox.instance.getActorByID(actorID);
            deadActor dActor = FamilyOverviewWindow.getDeadActor(deadID);
            deadActor father = FamilyOverviewWindow.getDeadActor(dActor.deadFatherID);
            deadActor mother = FamilyOverviewWindow.getDeadActor(dActor.deadMotherID);
            deadActor lover = FamilyOverviewWindow.getDeadActor(dActor.deadLoverID);
            ActorHead headActor = FamilyOverviewWindow.getHeadActor(deadID);

            NewBGs.addText($"Name: {dActor.name}", contents, 12, new Vector3(130, -20, 0));
            if (lover != null)
            {
                NewBGs.addText($"Lover: {lover.name}", contents, 8, new Vector3(130, -35, 0));
            }
            else
            {
                NewBGs.addText($"Lover: None", contents, 8, new Vector3(130, -35, 0));
            }
            if (father != null)
            {
                NewBGs.addText($"Father: {father.name}", contents, 8, new Vector3(130, -55, 0));
            }
            else
            {
                NewBGs.addText($"Father: None", contents, 8, new Vector3(130, -55, 0));
            }
            if (mother != null)
            {
                NewBGs.addText($"Mother: {mother.name}", contents, 8, new Vector3(130, -75, 0));
            }
            else
            {
                NewBGs.addText($"Mother: None", contents, 8, new Vector3(130, -75, 0));
            }
            if (headActor != null)
            {
                NewBGs.addText($"Cause Of Death: {headActor.getDeathCause()}", contents, 5, new Vector3(130, -100, 0));

                GameObject actorSpr = showSpritePart(headActor.getUnitTex(), new Vector3(0, 0, 0));
                NewBGs.addText($"Title: {headActor.title}", contents, 10, new Vector3(130, -115, 0));
                NewBGs.addText($"Traits:", contents, 10, new Vector3(70, -115, 0));
                int Ypos = 0;
                foreach(string trait in headActor.traitIds)
                {
                    NewBGs.addText(trait, contents, 8, new Vector3(70, -130+(Ypos*-10), 0));
                    Ypos++;
                }
                NewBGs.addText($"Stats:", contents, 10, new Vector3(190, -135, 0));
                NewBGs.addText($"Kills: {headActor.kills}", contents, 8, new Vector3(190, -145, 0));
                NewBGs.addText($"Age: {headActor.age}", contents, 8, new Vector3(190, -155, 0));
                NewBGs.addText($"Level: {headActor.level}", contents, 8, new Vector3(190, -165, 0));
                Ypos = 0;
                foreach(FieldInfo field in headActor.curStats.GetType().GetFields())
                {
                    if (!statNames.Contains(field.Name))
                    {
                        continue;
                    }
                    NewBGs.addText($"{field.Name}: {field.GetValue(headActor.curStats).ToString()}", contents, 8, new Vector3(190, -175+(Ypos*-10), 0));
                    Ypos++;
                }
            }
            // NewBGs.addText($"Family Index: {dActor.familyIndex}", contents, 15, new Vector3(130, -100, 0));
            
        }

        private static GameObject showSpritePart(Sprite pSprite, Vector3 pPos)
        {
            if (pSprite == null)
            {
                return null;
            }
            GameObject avatarLoader = new GameObject("avatarLoader");
            avatarLoader.transform.SetParent(contents.transform);
            avatarLoader.transform.localPosition = new Vector3(60, -80, 0);
            avatarLoader.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            GameObject gameObject = new GameObject();
            Image image = gameObject.AddComponent<Image>();
            image.sprite = pSprite;
            image.rectTransform.sizeDelta = new Vector2(image.sprite.textureRect.width, image.sprite.textureRect.height);
            gameObject.transform.SetParent(avatarLoader.transform);
            image.rectTransform.SetAnchor(AnchorPresets.BottonCenter, 0f, 0f);
            float x = /*image.sprite.pivot.x*/5 / image.sprite.textureRect.width;
            float y = /*image.sprite.pivot.y*/0 / image.sprite.textureRect.height;
            image.rectTransform.pivot = new Vector2(/*0.5f*/x, 0);
            image.rectTransform.anchoredPosition = pPos;
            // image.rectTransform.localPosition = pPos;
            gameObject.transform.localScale = Vector3.one;
            image.rectTransform.sizeDelta += new Vector2(10, 10);

            
            return gameObject;
        }
    }
}
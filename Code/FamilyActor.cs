using System;
using System.IO;
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
    [Serializable]
    public class FamilyActor : MonoBehaviour
    {
        public List<string> childrenID = new List<string>();

        public string fatherID;

        public string deadFatherID;

        public string motherID;

        public string deadMotherID;

        public string familyName;

        public string founderName;

        public string loverID;

        public string deadLoverID;

        public int fatherFamilyIndex;

        public int motherFamilyIndex;

        public int familyIndex;
        
        public bool isHead = false;

        public bool isMale;

        public bool isHeir = false;

        public string deadID;

        public void copyFamily(FamilyActor newFamily, UnbornActor newUnborn, List<string> ignore = null)
        {
            if (newFamily != null)
            {
                foreach (FieldInfo prop in this.GetType().GetFields())
                {
                    if (ignore != null && ignore.Contains(prop.Name))
                    {
                        continue;
                    }
                    prop.SetValue(this, prop.GetValue(newFamily));
                }
            }
            else if (newUnborn != null)
            {
                foreach (FieldInfo prop in this.GetType().GetFields())
                {
                    if (ignore != null && ignore.Contains(prop.Name))
                    {
                        continue;
                    }
                    FieldInfo thisField = newUnborn.GetType().GetField(prop.Name);
                    if (thisField == null)
                    {
                        continue;
                    }
                    prop.SetValue(this, thisField.GetValue(newUnborn));
                }
            }
        }

        public FamilyActor copySavedFamily(SavedFamilyActor sFamilyActor)
        {
            foreach (FieldInfo prop in this.GetType().GetFields())
            {
                FieldInfo thisField = sFamilyActor.GetType().GetField(prop.Name);
                if (thisField == null)
                {
                    continue;
                }
                if (thisField.FieldType == typeof(string))
                {
                    prop.SetValue(this, (string)thisField.GetValue(sFamilyActor));
                }
                else if (thisField.FieldType == typeof(int))
                {
                    prop.SetValue(this, (int)thisField.GetValue(sFamilyActor));
                }
                else if (thisField.FieldType == typeof(bool))
                {
                    prop.SetValue(this, (bool)thisField.GetValue(sFamilyActor));
                }
                else if (thisField.FieldType == typeof(List<string>))
                {
                    prop.SetValue(this, (List<string>)thisField.GetValue(sFamilyActor));
                }
            }
            return this;
        }

        public static FamilyActor getFamily(Actor actor)
        {
            if (actor == null)
            {
                return null;
            }
            return actor.gameObject.GetComponent<FamilyActor>();
        }

        public void addChild(Actor actor, string id, bool becomeHead, string newDeadID)
        {
            Family fatherFamily = FamilyOverviewWindow.getFromFamilies(ref this.fatherFamilyIndex);
            Family motherFamily = FamilyOverviewWindow.getFromFamilies(ref this.motherFamilyIndex);
            Family curFamily = FamilyOverviewWindow.getFromFamilies(ref this.familyIndex);
            FamilyActor actorFamily = FamilyActor.getFamily(actor);
            fatherFamily.addActor(actor);
            motherFamily.addActor(actor);
            curFamily.addActor(actor);
            this.childrenID.Add(id);

            Actor curHeir = NewActions.getActorByIndex(curFamily.heirID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
            Actor curHead = NewActions.getActorByIndex(curFamily.HEADID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
            if (/*becomeHead || */(curHead == null && curHeir != null))
            {
                curFamily.HEADID = curFamily.heirID;
                FamilyActor.getFamily(curHeir).isHead = true;
                FamilyActor.getFamily(curHeir).isHeir = false;
                curFamily.heirID = null;
            }
            if ((curFamily.heirID == null || curHeir == null) && isHead)
            {
                Actor newHeir = curFamily.getChildOrMember(
                    this.gameObject.GetComponent<Actor>().data.actorID,
                    this.childrenID, 
                    false, 
                    false, 
                    this.fatherFamilyIndex, 
                    this.motherFamilyIndex,
                    true,
                    this.gameObject.GetComponent<Actor>().kingdom
                );
                if (newHeir != null)
                {
                    curFamily.heirID = newHeir.data.actorID;
                    FamilyActor.getFamily(newHeir).isHeir = true;
                }
            }
        }

        public void removeChild(Actor actor, string id)
        {
            Family curFamily = FamilyOverviewWindow.getFromFamilies(ref this.familyIndex);
            curFamily.actors.Remove(actor);
            this.childrenID.Remove(id);
        }

        public void actorDied(Actor actor, Family family, AttackType pType)
        {
            if (family.founderID == actor.data.actorID)
            {
                family.founderID = this.deadID;
            }

            if(this.isHead)
            {
                family.prevHeads.Add(this.deadID);
                if (!FamilyOverviewWindow.headInfoList.ContainsKey(this.deadID))
                {
                    FamilyOverviewWindow.headInfoList.Add(this.deadID, new ActorHead{
                        traitIds = actor.data.traits,
                        curStats = actor.curStats,
                        title = FamilyOverviewWindow.getTitle(actor.data.profession),
                        kills = actor.data.kills,
                        age = actor.data.age,
                        level = actor.data.level,
                        familyIndex = this.familyIndex,
                        cause = pType
                    });
                }
                FamilyOverviewWindow.getHeadActor(this.deadID).setUnitTex(actor);
            }
            // if (!FamilyOverviewWindow.headInfoList.ContainsKey(this.deadID))
            // {
            //     FamilyOverviewWindow.headInfoList.Add(this.deadID, new ActorHead{
            //         traitIds = actor.data.traits,
            //         curStats = actor.curStats,
            //         title = FamilyOverviewWindow.getTitle(actor.data.profession),
            //         kills = actor.data.kills,
            //         age = actor.data.age,
            //         level = actor.data.level,
            //         familyIndex = this.familyIndex
            //     });
            // }
            // FamilyOverviewWindow.getHeadActor(this.deadID).setUnitTex(actor);

            FamilyOverviewWindow.getFromFamilies(ref this.fatherFamilyIndex);
            FamilyOverviewWindow.getFromFamilies(ref this.motherFamilyIndex);
            if (this.fatherID != null)
            {
                Actor father = NewActions.getActorByIndex(this.fatherID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
                if (father != null)
                {
                    FamilyActor.getFamily(father).childrenID.Remove(actor.data.actorID);
                    FamilyActor.getFamily(father).childrenID.Add(this.deadID);
                }
                else
                {
                    deadActor deadFather = FamilyOverviewWindow.deadActorList[this.deadFatherID];
                    deadFather.childrenID.Remove(actor.data.actorID);
                    deadFather.childrenID.Add(this.deadID);
                    this.fatherID = this.deadFatherID;
                }
                FamilyOverviewWindow.getFromFamilies(ref this.fatherFamilyIndex).actors.Remove(actor);
            }
            
            if (this.motherID != null)
            {
                Actor mother = NewActions.getActorByIndex(this.motherID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
                if (mother != null)
                {
                    FamilyActor.getFamily(mother).childrenID.Remove(actor.data.actorID);
                    FamilyActor.getFamily(mother).childrenID.Add(this.deadID);
                }
                else
                {
                    deadActor deadMother = FamilyOverviewWindow.deadActorList[this.deadMotherID];
                    deadMother.childrenID.Remove(actor.data.actorID);
                    deadMother.childrenID.Add(this.deadID);
                    this.motherID = this.deadMotherID;
                }
                FamilyOverviewWindow.getFromFamilies(ref this.motherFamilyIndex).actors.Remove(actor);
            }

            foreach (string childID in this.childrenID)
            {
                Actor child = NewActions.getActorByIndex(childID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
                if (child == null)
                {
                    deadActor deadChild = FamilyOverviewWindow.getDeadActor(childID);
                    if (deadChild == null)
                    {
                        continue;
                    }
                    if (isMale)
                    {
                        deadChild.fatherID = this.deadID;
                    }
                    else
                    {
                        deadChild.motherID = this.deadID;
                    }
                    continue;
                }
                FamilyActor childFamily = FamilyActor.getFamily(child);
                if (isMale)
                {
                    childFamily.fatherID = this.deadID;
                }
                else
                {
                    childFamily.motherID = this.deadID;
                }

            }
            if (this.loverID != null)
            {
                Actor lover = NewActions.getActorByIndex(this.loverID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
                if (lover != null)
                {
                    FamilyActor.getFamily(lover).loverID = this.deadID;
                    FamilyActor.getFamily(lover).deadLoverID = this.deadID;
                }
                else
                {
                    deadActor deadLover = null;
                    if (this.deadLoverID != null)
                    {
                        deadLover = FamilyOverviewWindow.deadActorList[this.deadLoverID];
                    }
                    else
                    {
                        deadLover = FamilyOverviewWindow.deadActorList[this.loverID];
                    }
                    if (deadLover == null)
                    {
                        Debug.Log(deadLover);
                        Debug.Log("This is null: " + this.deadLoverID);
                    }
                    deadLover.loverID = this.deadID;
                    deadLover.deadLoverID = this.deadID;
                }
            }
            string updateName = FamilyOverviewWindow.deadActorList[this.deadID].name;
            FamilyOverviewWindow.deadActorList[this.deadID] = new deadActor().copyFamily(this, updateName);
        }

        public void refreshHead(Actor pActor)
        {
            FamilyOverviewWindow.getFromFamilies(ref this.fatherFamilyIndex);
            FamilyOverviewWindow.getFromFamilies(ref this.motherFamilyIndex);
            Family family = FamilyOverviewWindow.getFromFamilies(ref this.familyIndex);
            bool setHead = false;
            Actor heirActor = NewActions.getActorByIndex(family.heirID, family.index, this.fatherFamilyIndex, this.motherFamilyIndex);
            if (heirActor == null)
            {
                Actor randomHead = family.getChildOrMember(
                    pActor.data.actorID,
                    this.childrenID, 
                    false, 
                    false, 
                    this.fatherFamilyIndex,
                    this.motherFamilyIndex,
                    true,
                    pActor.kingdom
                );
                if (randomHead != null)
                {
                    family.HEADID = randomHead.data.actorID;
                    setHead = true;
                }
            }
            else if (heirActor.kingdom == pActor.kingdom)
            {
                family.HEADID = family.heirID;
                family.heirID = null;
                setHead = true;
            }
            else if (heirActor.kingdom != pActor.kingdom)
            {
                family.HEADID = null;
                family.heirID = null;
                this.refreshHead(pActor);
                return;
            }

            if (setHead)
            {
                Actor newHead = NewActions.getActorByIndex(family.HEADID, family.index, this.fatherFamilyIndex, this.fatherFamilyIndex);
                FamilyActor headActorFamily = FamilyActor.getFamily(newHead);
                FamilyOverviewWindow.getFromFamilies(ref headActorFamily.fatherFamilyIndex);
                FamilyOverviewWindow.getFromFamilies(ref headActorFamily.motherFamilyIndex);
                headActorFamily.isHead = true;
                headActorFamily.isHeir = false;
                family.currentGeneration++;
                Actor newHeir = family.getChildOrMember(
                    newHead.data.actorID,
                    headActorFamily.childrenID, 
                    false, 
                    false, 
                    headActorFamily.fatherFamilyIndex, 
                    headActorFamily.motherFamilyIndex,
                    true,
                    newHead.kingdom
                );
                if (newHeir != null && newHeir != newHead)
                {
                    family.heirID = newHeir.data.actorID;
                    FamilyActor.getFamily(newHeir).isHeir = true;
                    FamilyActor.getFamily(newHeir).isHead = false;
                    // Debug.Log($"New Heir: {family.heirID}");
                }
            }
        }

        public void refreshHeir(Actor pActor)
        {
            Family family = FamilyOverviewWindow.getFromFamilies(ref this.familyIndex);
            FamilyOverviewWindow.getFromFamilies(ref this.fatherFamilyIndex);
            FamilyOverviewWindow.getFromFamilies(ref this.motherFamilyIndex);
            Actor newHeir = family.getChildOrMember(
                pActor.data.actorID,
                this.childrenID, 
                false, 
                false, 
                this.fatherFamilyIndex, 
                this.motherFamilyIndex,
                true,
                pActor.kingdom
            );
            if (newHeir != null)
            {
                family.heirID = newHeir.data.actorID;
                FamilyActor.getFamily(newHeir).isHeir = true;
                FamilyActor.getFamily(newHeir).isHead = false;
            }
        }
    }

    [Serializable]
    public class UnbornActor
    {
        public string fatherID;

        public string deadFatherID;

        public string motherID;

        public string deadMotherID;

        public string familyName;

        public string founderName;

        public int fatherFamilyIndex;

        public int motherFamilyIndex;

        public int familyIndex;

        public bool isMale;

        public void copyFamily(Actor firstActor, Actor secondActor, bool isMale)
        {
            Actor fatherActor = secondActor;
            Actor motherActor = firstActor;
            if (firstActor.data.gender == ActorGender.Male)
            {
                fatherActor = firstActor;
                motherActor = secondActor;
            }
            FamilyActor father = FamilyActor.getFamily(fatherActor);
            FamilyActor mother = FamilyActor.getFamily(motherActor);
            FamilyActor firstFamily = FamilyActor.getFamily(firstActor);
            foreach (FieldInfo prop in this.GetType().GetFields())
            {
                FieldInfo thisField = firstFamily.GetType().GetField(prop.Name);
                if (thisField == null)
                {
                    continue;
                }
                prop.SetValue(this, thisField.GetValue(father));
            }

            this.fatherID = fatherActor.data.actorID;
            this.motherID = motherActor.data.actorID;
            this.deadFatherID = father.deadID;
            this.deadMotherID = mother.deadID;
            this.fatherFamilyIndex = father.familyIndex;
            this.motherFamilyIndex = findMotherIndex(father, mother);
            this.isMale = isMale;

            if (!FamilyOverviewWindow.deadActorList.ContainsKey(this.deadFatherID))
            {
                FamilyOverviewWindow.deadActorList.Add(
                    this.deadFatherID,
                    new deadActor().copyFamily(father, fatherActor.getName())
                );
            }
            if (!FamilyOverviewWindow.deadActorList.ContainsKey(this.deadMotherID))
            {
                FamilyOverviewWindow.deadActorList.Add(
                    this.deadMotherID,
                    new deadActor().copyFamily(mother, motherActor.getName())
                );
            }
        }

        private static int findMotherIndex(FamilyActor father, FamilyActor mother)
        {
            if (father.familyIndex != mother.familyIndex)
            {
                return mother.familyIndex;
            }
            
            if (mother.motherFamilyIndex != null && mother.motherFamilyIndex != father.familyIndex)
            {
                return mother.motherFamilyIndex;
            }

            if (mother.fatherFamilyIndex != null && mother.fatherFamilyIndex != father.familyIndex)
            {
                return mother.fatherFamilyIndex;
            }

            return mother.familyIndex;
        }
    }

    public class Family
    {
        public string founderName;

        public string founderID;

        private string headID;

        public string HEADID
        {
            get { return headID; }   // get method
            set 
            {
                Actor head = MapBox.instance.getActorByID(value);
                if (head != null)
                {
                    switch (head.data.profession)
                    {
                        case UnitProfession.Unit:
                            this.title = "Peasant";
                            break;
                        case UnitProfession.Warrior:
                            this.title = "Knight";
                            break;
                        case UnitProfession.Leader:
                            this.title = "Noble";
                            break;
                        case UnitProfession.King:
                            this.title = "Royalty";
                            break;
                        default:
                            this.title = "Peasant";
                            break;
                    }
                }
                headID = value;
            }
        }

        public string heirID;

        public HashSet<Actor> actors = new HashSet<Actor>();

        public int index;

        public int founderDate;

        public int currentGeneration;

        public List<string> prevHeads = new List<string>();

        public string title = "Peasant";

        public Family(string founder = null, Actor head = null, string name = null, int id = -1, int year = -1, int generation = -1)
        {
            this.founderID = founder;
            this.founderName = name;
            if (head != null)
            {
                this.HEADID = head.data.actorID;
                this.actors.Add(head);
            }
            this.index = id;
            this.founderDate = year;
            this.currentGeneration = generation;
        }

        public Family copyFamily(SavedFamily sFamily)
        {
            foreach (FieldInfo prop in this.GetType().GetFields())
            {
                FieldInfo thisField = sFamily.GetType().GetField(prop.Name);
                if (prop.Name == "actors")
                {
                    HashSet<Actor> actorsTemp = new HashSet<Actor>();
                    Dictionary<string, SavedFamilyActor> fieldValue = (Dictionary<string, SavedFamilyActor>)thisField.GetValue(sFamily);
                    foreach(KeyValuePair<string, SavedFamilyActor> kv in fieldValue)
                    {
                        Actor newActor = MapBox.instance.getActorByID(kv.Key);
                        if (newActor == null)
                        {
                            continue;
                        }
                        if (newActor.gameObject.GetComponent<FamilyActor>() == null)
                        {
                            FamilyActor newFamilyActor = newActor.gameObject.AddComponent<FamilyActor>();
                            newFamilyActor.copySavedFamily(kv.Value);
                        }
                        actorsTemp.Add(newActor);
                    }
                    prop.SetValue(this, actorsTemp);
                    continue;
                }
                if (thisField == null)
                {
                    continue;
                }
                prop.SetValue(this, thisField.GetValue(sFamily));
            }
            this.HEADID = (string)sFamily.GetType().GetField("headID").GetValue(sFamily);
            return this;
        }

        public void addActor(Actor actor)
        {
            if (this.actors.Contains(actor))
            {
                return;
            }
            this.actors.Add(actor);
        }

        public Actor getChildOrMember(string parentID, List<string> childrenID, bool isHeir, bool isHead, int secondIndex, int thirdIndex, bool checkKingdom = false, Kingdom kingdom = null)
        {
            secondIndex = (secondIndex == null) ? -1 : secondIndex;
            thirdIndex = (thirdIndex == null) ? -1 : thirdIndex;
            Actor possibleResult = null;
            FamilyActor possibleFamilyActor = null;
            foreach (string childID in childrenID)
            {
                Actor child = NewActions.getActorByIndex(childID, this.index, secondIndex, thirdIndex);
                if (child == null || !child.data.alive)
                {
                    continue;
                }
                FamilyActor childFamily = FamilyActor.getFamily(child);
                if ((childFamily.isHeir && !isHeir) || (childFamily.isHead && !isHead))
                {
                    continue;
                }
                if (parentID == childID)
                {
                    continue;
                }
                if (checkKingdom && kingdom != child.kingdom)
                {
                    if (possibleResult == null)
                    {
                        possibleFamilyActor = childFamily;
                        possibleResult = child;
                    }
                    continue;
                }
                childFamily.familyIndex = index;
                return child;
            }
            if (possibleResult != null)
            {
                possibleFamilyActor.familyIndex = index;
                return possibleResult;
            }
            
            foreach(Actor actor in this.actors)
            {
                if (actor == null)
                {
                    continue;
                }
                if (!actor.data.alive)
                {
                    continue;
                }
                FamilyActor actorFamily = FamilyActor.getFamily(actor);
                if (actorFamily == null || (actorFamily.isHeir && !isHeir) || (actorFamily.isHead && !isHead))
                {
                    continue;
                }
                if (parentID == actor.data.actorID)
                {
                    continue;
                }
                if (checkKingdom && kingdom != actor.kingdom)
                {
                    if (possibleResult == null)
                    {
                        possibleFamilyActor = actorFamily;
                        possibleResult = actor;
                    }
                    continue;
                }
                actorFamily.familyIndex = index;
                return actor;
            }
            if (possibleResult != null)
            {
                possibleFamilyActor.familyIndex = index;
                return possibleResult;
            }
            return null;
        }
    }
    
    [Serializable]
    public class deadActor
    {
        public string name;

        public string fatherID;

        public string motherID;

        public string deadFatherID;

        public string deadMotherID;

        public int fatherFamilyIndex;

        public int motherFamilyIndex;

        public int familyIndex;

        public List<string> childrenID;

        public string loverID;

        public string deadLoverID;

        public deadActor copyFamily(FamilyActor actorFamily, string newName)
        {
            foreach (FieldInfo prop in this.GetType().GetFields())
            {
                FieldInfo thisField = actorFamily.GetType().GetField(prop.Name);
                if (thisField == null)
                {
                    continue;
                }
                prop.SetValue(this, thisField.GetValue(actorFamily));
            }
            this.name = newName;
            return this;
        }

        public void addChild(Actor actor, string id, bool becomeHead, string newDeadID)
        {
            Family fatherFamily = FamilyOverviewWindow.getFromFamilies(ref this.fatherFamilyIndex);
            Family motherFamily = FamilyOverviewWindow.getFromFamilies(ref this.motherFamilyIndex);
            Family curFamily = FamilyOverviewWindow.getFromFamilies(ref this.familyIndex);
            if (fatherFamily != null)
            {
                fatherFamily.addActor(actor);
            }
            if (motherFamily != null)
            {
                motherFamily.addActor(actor);
            }
            if (curFamily != null)
            {
                curFamily.addActor(actor);
            }
            this.childrenID.Add(id);
        }

        public void removeChild(Actor actor, string id)
        {
            Family curFamily = FamilyOverviewWindow.getFromFamilies(ref this.familyIndex);
            curFamily.actors.Remove(actor);
            this.childrenID.Remove(id);
        }
    }

    [Serializable]
    public class ActorHead
    {
        public List<string> traitIds;

        public BaseStats curStats = new BaseStats();

        public SerializeTexture unitTex;

        public string title;

        public int kills;

        public int age;

        public int level;

        public int familyIndex;

        public AttackType cause;

        public void setUnitTex(Actor actor)
        {
            actor.updateAnimation(0f, true);
            actor.spriteAnimation.setFrameIndex(0);
            actor.forceAnimation();
		    actor.checkSpriteConstructor();
            Sprite actorSpr = actor.spriteRenderer.sprite;
            var croppedTexture = new Texture2D( (int)actorSpr.rect.width, (int)actorSpr.rect.height );
            if (!actorSpr.texture.isReadable)
            {
                return;
            }
            var pixels = actorSpr.texture.GetPixels(  (int)actorSpr.textureRect.x, 
                                                    (int)actorSpr.textureRect.y, 
                                                    (int)actorSpr.textureRect.width, 
                                                    (int)actorSpr.textureRect.height );
            croppedTexture.SetPixels( pixels );
            croppedTexture.Apply();
            this.unitTex = new SerializeTexture();
            unitTex.x = croppedTexture.width;
            unitTex.y = croppedTexture.height;
            unitTex.bytes = ImageConversion.EncodeToPNG(croppedTexture);
        }

        public Sprite getUnitTex()
        {
            if (unitTex == null)
            {
                return null;
            }
            Texture2D tex = new Texture2D(unitTex.x, unitTex.y);
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(unitTex.bytes);
            Sprite mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one);
            return mySprite;
        }

        public string getDeathCause()
        {
            switch(this.cause)
            {
                case AttackType.Acid:
                    return "Acid - Warning, don't touch that green yucky stuff on the ground.";
                case AttackType.Fire:
                    return "Fire - This person was too hot.";
                case AttackType.Plague:
                    return "Plague - Maybe wear a mask next time you go out.";
                case AttackType.Tumor:
                    return "Tumor Infection - Did someone plant a mushroom farm here?";
                case AttackType.Other:
                    return "Combat - Held a great last stand o7";
                case AttackType.Hunger:
                    return "Starvation - They just wanted a sandwich.";
                case AttackType.Eaten:
                    return "Mauled By Beast - Twas the nature of the circle of life.";
                case AttackType.Age:
                    string start = "Old Age -";
                    if (title == "Royalty")
                    {
                        return $"{start} They served their kingdom well while living a long life as Royalty. ";
                    }
                    return $"{start} They lived a long and good life as a {title}";
                case AttackType.None:
                    return "Hmm... This is odd, their cause of death was not recorded.";
                case AttackType.GrowUp:
                    return "This person's cause of death is an error. Pls report to Dej on WorldBox Discord.";
                case AttackType.Poison:
                    return "Poison - If only they had more health points ;(";
                case AttackType.Block:
                    return "Terrain - their life was on a ~dangerous~ path";
                default:
                    return "IDFK";
            }
        }
    }

    [Serializable]
    public class SerializeTexture
    {
        [SerializeField]
        public int x;
        [SerializeField]
        public int y;
        [SerializeField]
        public byte[] bytes;
    }
}
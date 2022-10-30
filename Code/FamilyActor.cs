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
                // if (thisField == null)
                // {
                //     continue;
                // }
                prop.SetValue(this, thisField.GetValue(sFamilyActor));
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
            Family fatherFamily = FamilyOverviewWindow.families[this.fatherFamilyIndex.ToString()];
            Family motherFamily = FamilyOverviewWindow.families[this.motherFamilyIndex.ToString()];
            Family curFamily = FamilyOverviewWindow.families[this.familyIndex.ToString()];
            FamilyActor actorFamily = FamilyActor.getFamily(actor);
            fatherFamily.addActor(actor);
            motherFamily.addActor(actor);
            curFamily.addActor(actor);
            this.childrenID.Add(id);

            Actor curHeir = NewActions.getActorByIndex(curFamily.heirID, this.fatherFamilyIndex, this.motherFamilyIndex);
            Actor curHead = NewActions.getActorByIndex(curFamily.HEADID, this.fatherFamilyIndex, this.motherFamilyIndex);
            if (/*becomeHead || */(curHead == null && curHeir != null))
            {
                curFamily.HEADID = curFamily.heirID;
                FamilyActor.getFamily(curHeir).isHead = true;
                FamilyActor.getFamily(curHeir).isHeir = false;
                curFamily.heirID = null;
            }
            if ((curFamily.heirID == null || curHeir == null) && isHead)
            {
                Actor newHeir = curFamily.getChildOrMember(this.childrenID, false, false, this.fatherFamilyIndex, this.motherFamilyIndex);
                if (newHeir != null)
                {
                    curFamily.heirID = newHeir.data.actorID;
                    FamilyActor.getFamily(newHeir).isHeir = true;
                    Debug.Log($"New Child Became Heir: {curFamily.heirID}");
                }
            }
        }

        public void actorDied(Actor actor, Family family/*, FamilyActor actorFamily*/)
        {
            if (family.founderID == actor.data.actorID)
            {
                family.founderID = this.deadID;
            }

            if(this.isHead)
            {
                family.prevHeads.Add(this.deadID);
            }
            if (this.fatherID != null)
            {
                Actor father = NewActions.getActorByIndex(this.fatherID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
                if (father != null)
                {
                    FamilyActor.getFamily(father).childrenID.Remove(actor.data.actorID);
                    FamilyActor.getFamily(father).childrenID.Add(deadID);
                }
                else
                {
                    deadActor deadFather = FamilyOverviewWindow.deadActorList[this.deadFatherID];
                    deadFather.childrenID.Remove(actor.data.actorID);
                    deadFather.childrenID.Add(deadID);
                    this.fatherID = this.deadFatherID;
                }
                FamilyOverviewWindow.families[fatherFamilyIndex.ToString()].actors.Remove(actor);
            }
            
            if (this.motherID != null)
            {
                Actor mother = NewActions.getActorByIndex(this.motherID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
                if (mother != null)
                {
                    FamilyActor.getFamily(mother).childrenID.Remove(actor.data.actorID);
                    FamilyActor.getFamily(mother).childrenID.Add(deadID);
                }
                else
                {
                    deadActor deadMother = FamilyOverviewWindow.deadActorList[this.deadMotherID];
                    deadMother.childrenID.Remove(actor.data.actorID);
                    deadMother.childrenID.Add(deadID);
                    this.motherID = this.deadMotherID;
                }
                FamilyOverviewWindow.families[motherFamilyIndex.ToString()].actors.Remove(actor);
            }

            foreach (string childID in this.childrenID)
            {
                Actor child = NewActions.getActorByIndex(childID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
                if (child == null)
                {
                    deadActor deadChild = FamilyOverviewWindow.deadActorList[childID];
                    if (isMale)
                    {
                        deadChild.fatherID = deadID;
                    }
                    else
                    {
                        deadChild.motherID = deadID;
                    }
                    continue;
                }
                FamilyActor childFamily = FamilyActor.getFamily(child);
                if (isMale)
                {
                    childFamily.fatherID = deadID;
                }
                else
                {
                    childFamily.motherID = deadID;
                }

            }
            if (this.loverID != null)
            {
                Actor lover = NewActions.getActorByIndex(this.loverID, this.familyIndex, this.fatherFamilyIndex, this.motherFamilyIndex);
                if (lover != null)
                {
                    FamilyActor.getFamily(lover).loverID = deadID;
                }
                else
                {
                    deadActor deadLover = FamilyOverviewWindow.deadActorList[this.loverID];
                    if (deadLover == null)
                    {
                        Debug.Log(deadLover);
                        Debug.Log("This is null: " + this.loverID);
                    }
                    deadLover.loverID = deadID;
                }
            }
            string updateName = FamilyOverviewWindow.deadActorList[this.deadID].name;
            FamilyOverviewWindow.deadActorList[this.deadID] = new deadActor().copyFamily(this, updateName);
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
                this.headID = head.data.actorID;
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

        public Actor getChildOrMember(List<string> childrenID, bool isHeir, bool isHead, int secondIndex, int thirdIndex)
        {
            secondIndex = (secondIndex == null) ? -1 : secondIndex;
            thirdIndex = (thirdIndex == null) ? -1 : thirdIndex;
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
                childFamily.familyIndex = index;
                return child;
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
                if ((actorFamily.isHeir && !isHeir) || (actorFamily.isHead && !isHead))
                {
                    continue;
                }
                actorFamily.familyIndex = index;
                return actor;
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

        public int fatherFamilyIndex;

        public int motherFamilyIndex;

        public int familyIndex;

        public List<string> childrenID;

        public string loverID;

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
            Family fatherFamily = FamilyOverviewWindow.families[this.fatherFamilyIndex.ToString()];
            Family motherFamily = FamilyOverviewWindow.families[this.motherFamilyIndex.ToString()];
            Family curFamily = FamilyOverviewWindow.families[this.familyIndex.ToString()];
            fatherFamily.addActor(actor);
            motherFamily.addActor(actor);
            curFamily.addActor(actor);
            this.childrenID.Add(id);
        }
    }
}
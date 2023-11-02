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
    public class DeadFamilyMember
    {
        public int familyIndex;
        public int parentIndex;
        public int parentIndex2;
        public string childrenIndex;
        public int memberIndex;
        public int spouseIndex;
        public string name;
        public string prof;

        public DeadFamilyMember(int family, int p1, int p2, string cI, int mI, int sI, string name, string prof)
        {
            this.familyIndex = family;
            this.parentIndex = p1;
            this.parentIndex2 = p2;
            this.childrenIndex = cI;
            this.memberIndex = mI;
            this.name = name;
            this.prof = prof;
            if (prof == "Unit")
            {
                this.prof = "Peasant";
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{

    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("Altered.Carbon").PatchAll();
        }
    }
}


using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace FrankWilco.RimWorld
{
    public class LifeSupportMod : Mod
    {
        public static LifeSupportModSettings Settings;

        internal static Type Hediff_DeathRattle = null;

        public LifeSupportMod(ModContentPack content)
            : base(content)
        {
            Settings = GetSettings<LifeSupportModSettings>();

            Hediff_DeathRattle = AccessTools.TypeByName("DeathRattle.Hediff_DeathRattle");
            if (!(Hediff_DeathRattle is null))
            {
                Log.Message("[LifeSupport] found DeathRattle");
            }

            var harmony = new Harmony("io.frankwilco.ng.lifesupport");
            harmony.PatchAll();
        }

        public override string SettingsCategory()
        {
            return "lfsp.prefs.title".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}

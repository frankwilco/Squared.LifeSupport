using Verse;
using RimWorld;
using Verse.AI;
using HarmonyLib;
using System.Collections.Generic;
using static Verse.PawnCapacityUtility;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ShouldBeDeadFromRequiredCapacity))]
        public static bool Patch_ShouldBeDeadFromRequiredCapacity(
            ref Pawn_HealthTracker __instance,
            ref PawnCapacityDef __result)
        {
            // Check if consciousness is there. If it is then its okay.

            Pawn_HealthTracker health = __instance;
            Pawn pawn = health.hediffSet.pawn;

            if (!health.hediffSet.HasHediff(LifeSupportDefOf.QE_LifeSupport))
            {
                // Not on life support
                return true;
            }
            else if (!pawn.ValidLifeSupportNearby())
            {
                // Life support is unpowered
                return true;
            }
            else if (!health.capacities.CapableOf(PawnCapacityDefOf.Consciousness))
            {
                // No consciousness
                return true;
            }

            __result = null;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Toils_LayDown), nameof(Toils_LayDown.LayDown))]
        public static void Patch_LayDown(ref Toil __result)
        {
            bool debug = false;
            if (debug) Log.Message("Patch_LayDown");
            Toil toil = __result;
            if (toil == null)
                return;

            toil.AddPreTickAction(delegate ()
            {
                Pawn pawn = toil.actor;
                if (pawn is null || pawn.Dead)
                {
                    return;
                }

                pawn.SetHediffs();
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateLimbEfficiency))]
        public static bool Patch_CalculateLimbEfficiency(
            ref float __result,
            HediffSet diffSet,
            BodyPartTagDef limbCoreTag,
            BodyPartTagDef limbSegmentTag,
            BodyPartTagDef limbDigitTag,
            float appendageWeight,
            out float functionalPercentage,
            List<CapacityImpactor> impactors)
        {
            functionalPercentage = 0f;

            if (limbCoreTag != BodyPartTagDefOf.MovingLimbCore)
            {
                return true;
            }

            var hediff = diffSet.GetFirstHediffOfDef(LifeSupportDefOf.QE_LifeSupport);
            if (hediff is null)
            {
                return true;
            }

            if (hediff.Severity < 1f)
            {
                return true;
            }

            __result = 0f;

            if (!(impactors is null))
            {
                var capacityImpactor = new CapacityImpactorHediff();
                capacityImpactor.hediff = hediff;
                impactors.Add(capacityImpactor);
            }

            return false;
        }
    }
}

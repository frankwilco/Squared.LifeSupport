using Verse;
using RimWorld;
using Verse.AI;
using HarmonyLib;
using System.Collections.Generic;
using static Verse.PawnCapacityUtility;

namespace FrankWilco.RimWorld
{
    [HarmonyPatch]
    public static class LifeSupportPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_HealthTracker),
            nameof(Pawn_HealthTracker.ShouldBeDeadFromRequiredCapacity))]
        public static bool Patch_ShouldBeDeadFromRequiredCapacity(
            ref Pawn_HealthTracker __instance,
            ref PawnCapacityDef __result)
        {
            Pawn_HealthTracker health = __instance;
            Pawn pawn = health.hediffSet.pawn;

            if (!health.hediffSet.HasHediff(LifeSupportDefOf.QE_LifeSupport))
            {
                // Pawn is not on life support.
                return true;
            }
            else if (!pawn.ValidLifeSupportNearby())
            {
                // Linked life support is unpowered.
                return true;
            }
            else if (!health.capacities.CapableOf(PawnCapacityDefOf.Consciousness))
            {
                // Pawn is unconscious.
                return true;
            }

            // If none of the conditions above were met, let the pawn
            // stay alive and prevent the original method from running.
            __result = null;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Toils_LayDown), nameof(Toils_LayDown.LayDown))]
        public static void Patch_LayDown(ref Toil __result)
        {
            Toil toil = __result;
            // Check for nearby life support before laying the pawn down
            // and apply the hediff if applicable.
            toil?.AddPreTickAction(delegate
            {
                Pawn pawn = toil?.actor;
                // Don't bother if the pawn is null for some reason or is
                // already dead.
                if (pawn is null || pawn.Dead)
                {
                    return;
                }
                pawn.SetHediffs();
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PawnCapacityUtility),
            nameof(PawnCapacityUtility.CalculateLimbEfficiency))]
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
                // Ignore if we're not dealing with the "moving" capacity.
                return true;
            }

            var lifeSupportHediff = diffSet.GetFirstHediffOfDef(LifeSupportDefOf.QE_LifeSupport);
            if (lifeSupportHediff is null)
            {
                // Pawn is not on life support.
                return true;
            }
            if (lifeSupportHediff.Severity < 1f)
            {
                // Ignore if pawn will not die without life support.
                return true;
            }

            // Prevent the pawn from walking by zeroing their limb efficiency
            // and prevent the original method from running.
            __result = 0f;
            impactors?.Add(new CapacityImpactorHediff()
            {
                hediff = lifeSupportHediff
            });
            return false;
        }
    }
}

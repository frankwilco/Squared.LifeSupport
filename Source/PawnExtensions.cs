using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FrankWilco.RimWorld
{
    public static class PawnExtensions
    {
        public static bool ValidLifeSupportNearby(this Pawn pawn)
        {
            if (!(pawn.CurrentBed() is Building_Bed bed))
            {
                // Pawn is not occupying a bed.
                return false;
            }

            var targetComp = bed.TryGetComp<CompAffectedByFacilities>();
            if (targetComp == null)
            {
                Log.Warning("The pawn's bed must be affected by facilities.");
                return false;
            }

            foreach (var thing in targetComp.LinkedFacilitiesListForReading)
            {
                if (thing.TryGetComp<LifeSupportComp>() is LifeSupportComp lifeSupport)
                {
#if DEBUG
                    Log.Message("Found an active life support thing.");
#endif
                    return lifeSupport.Active;
                }
            }

            return false;
        }

        internal static bool WouldDieWithoutLifeSupport(this Pawn pawn)
        {
            PawnCapacitiesHandler capacitiesHandler = pawn.health.capacities;
            bool isFlesh = pawn.RaceProps.IsFlesh;
            foreach (PawnCapacityDef pawnCapacityDef in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
            {
                if (isFlesh ? !pawnCapacityDef.lethalFlesh : !pawnCapacityDef.lethalMechanoids)
                {
                    // Not deadly
                }
                else if (!capacitiesHandler.CapableOf(pawnCapacityDef))
                {
                    return true;
                }
            }
            return false;
        }

        public static void SetHediffs(this Pawn pawn)
        {
            bool validLifeSupportNearby = pawn.ValidLifeSupportNearby();
            pawn.SetHediffs(validLifeSupportNearby);
        }

        public static void SetHediffs(this Pawn pawn, bool validLifeSupportNearby)
        {
            var health = pawn.health;
            var hediff_deathrattle = new List<Hediff>();

            Hediff hediff_lifesupport = null;
            var Hediff_DeathRattle = LifeSupportMod.Hediff_DeathRattle;
            foreach (var hediff in health.hediffSet.hediffs)
            {
                if (hediff.def == LifeSupportDefOf.QE_LifeSupport)
                {
                    hediff_lifesupport = hediff;
                }
                else if (!(Hediff_DeathRattle is null) && Hediff_DeathRattle.IsInstanceOfType(hediff))
                {
                    hediff_deathrattle.Add(hediff);
                }
            }

            if (validLifeSupportNearby)
            {
                if (hediff_lifesupport is null)
                {
                    hediff_lifesupport = health.AddHediff(LifeSupportDefOf.QE_LifeSupport);
                }
                hediff_lifesupport.Severity = pawn.WouldDieWithoutLifeSupport() ? 1.0f : 0.5f;

                foreach (var hediff in hediff_deathrattle)
                {
                    health.RemoveHediff(hediff);
                }
            }
            else if (!(hediff_lifesupport is null))
            {
                health.RemoveHediff(hediff_lifesupport);
            }
        }
    }
}

using RimWorld;
using Verse;

namespace FrankWilco.RimWorld
{
    public class Hediff_LifeSupport : HediffWithComps
    {
        public override bool ShouldRemove => pawn.CurrentBed() == null;
        public override bool Visible => true;
    }
}

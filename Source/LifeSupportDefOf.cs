using RimWorld;
using Verse;

namespace FrankWilco.RimWorld
{
    [DefOf]
    public static class LifeSupportDefOf
    {
        public static HediffDef QE_LifeSupport;

        static LifeSupportDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(LifeSupportDefOf));
        }
    }
}

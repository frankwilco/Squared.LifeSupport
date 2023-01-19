using RimWorld;
using Verse;

namespace FrankWilco.RimWorld
{
    public class LifeSupportComp : ThingComp
    {
        public bool Active => !(parent.TryGetComp<CompPowerTrader>() is CompPowerTrader power) || power.PowerOn;

        public override void ReceiveCompSignal(string signal)
        {
            if (signal != "PowerTurnedOn" && signal != "PowerTurnedOff")
            {
                return;
            }

            var targetComp = parent.TryGetComp<CompFacility>();
            if (targetComp == null)
            {
                Log.Warning("This thing must be attached to a facility.");
                return;
            }

            // Check for state change in surrounding pawns in beds.
            foreach (Thing thing in targetComp.LinkedBuildings)
            {
                if (!(thing is Building_Bed bed))
                {
                    Log.Warning("This thing must be linked only to beds.");
                    continue;
                }
                foreach (var pawn in bed.CurOccupants)
                {
                    if (pawn is null || pawn.health.Dead)
                    {
                        continue;
                    }
                    pawn.SetHediffs(false);
                    pawn.health.CheckForStateChange(null, null);
                }
            }
        }
    }
}

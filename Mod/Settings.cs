using Verse;

namespace Lilly.MaxFishPopulation
{
    public class Settings : ModSettings
    {
        public static bool onDebug = true;
        public static bool onPatch = true;
        public static int maxFishPopulation = 1000000;

        public override void ExposeData()
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars && Scribe.mode != LoadSaveMode.Saving) return;

            MyLog.Message($"<color=#00FF00FF>{Scribe.mode}</color>");
            base.ExposeData();
            Scribe_Values.Look(ref onDebug, "onDebug", false);
            Scribe_Values.Look(ref onPatch, "onPatch", true);
            Scribe_Values.Look(ref maxFishPopulation, "maxFishPopulation", 1000000);

            Patch.OnPatch(true);
        }
    }
}

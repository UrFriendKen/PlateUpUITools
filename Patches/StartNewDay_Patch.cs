using HarmonyLib;
using Kitchen;

namespace UITools.Patches
{
    [HarmonyPatch]
    static class StartNewDay_Patch
    {
        [HarmonyPatch(typeof(StartNewDay), "OnUpdate")]
        [HarmonyPrefix]
        static bool OnUpdate_Prefix()
        {
            return !SStartDayWarnings_Patch.HighestWarningLevel.IsBlocking();
        }
    }
}

using HarmonyLib;
using Kitchen;
using Unity.Entities;

namespace KitchenUITools.Patches
{
    [HarmonyPatch]
    static class StartNewDay_Patch
    {
        [HarmonyPatch(typeof(StartNewDay), "OnUpdate")]
        [HarmonyPrefix]
        static bool OnUpdate_Prefix(StartNewDay __instance)
        {
            return !CustomStartDayWarningsRegistry.HighestWarningLevel.IsBlocking();
        }
    }
}

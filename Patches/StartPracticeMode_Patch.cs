using HarmonyLib;
using Kitchen;
using KitchenData;
using System.Reflection;

namespace KitchenUITools.Patches
{
    [HarmonyPatch]
    static class StartPracticeMode_Patch
    {
        static PropertyInfo p_PopupUtilities = typeof(GenericSystemBase).GetProperty("PopupUtilities", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(StartPracticeMode), "OnUpdate")]
        [HarmonyPrefix]
        static bool OnUpdate_Prefix(StartPracticeMode __instance)
        {
            if (__instance.HasSingleton<CRequestPracticeMode>() && CustomStartDayWarningsRegistry.HighestWarningLevel.IsBlocking())
            {
                ((PopupUtilities)p_PopupUtilities?.GetValue(__instance))?.RequestManagedPopup(PopupType.PracticeBlockedByParcelOrHolding);

                if (__instance.TryGetSingletonEntity<CRequestPracticeMode>(out var value))
                {
                    __instance.EntityManager.DestroyEntity(value);
                }
                return false;
            }
            return true;
        }
    }
}

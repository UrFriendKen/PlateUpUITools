using HarmonyLib;
using Kitchen;
using System;
using System.Collections.Generic;

namespace KitchenUITools.Patches
{
    [HarmonyPatch]
    internal static class SStartDayWarnings_Patch
    {
        [HarmonyPatch(typeof(SStartDayWarnings), "Primary", MethodType.Getter)]
        [HarmonyPrefix]
        [HarmonyPriority(int.MinValue)]
        static bool Primary_Get_Prefix(bool __runOriginal, ref (StartDayWarning, WarningLevel) __result)
        {
            if (!__runOriginal)
                return true;

            bool hasResult = CustomStartDayWarningsRegistry.TryGetActiveWarning(StartDayWarningDefinition.WarningPriority.High, out __result);
            
            if (!hasResult)
                return true;
            return __result.Item2.IsActive();
        }

        [HarmonyPatch(typeof(SStartDayWarnings), "Primary", MethodType.Getter)]
        [HarmonyPostfix]
        static void Primary_Get_Postfix(ref (StartDayWarning, WarningLevel) __result)
        {
            if (__result.Item1 != StartDayWarning.PlayersNotReady)
                return;

            bool hasResult = CustomStartDayWarningsRegistry.TryGetActiveWarning(StartDayWarningDefinition.WarningPriority.Normal, out (StartDayWarning, WarningLevel) tempResult);
            if (!hasResult)
                return;

            __result = tempResult;
        }
    }
}

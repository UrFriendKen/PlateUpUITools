using HarmonyLib;
using Kitchen;
using System;
using System.Collections.Generic;

namespace UITools.Patches
{
    public class StartDayWarningDefinition
    {
        internal StartDayWarning ID;
        public string DisplayTitle;
        public string Description;
        internal WarningLevel CurrentWarningLevel => GetCurrentWarningLevel != default ? GetCurrentWarningLevel() : WarningLevel.Safe;
        public Func<WarningLevel> GetCurrentWarningLevel;

        public StartDayWarningDefinition(string displayText, string description, Func<WarningLevel> getCurrentWarningLevel)
        {
            DisplayTitle = displayText;
            Description = description;
            GetCurrentWarningLevel = getCurrentWarningLevel;
        }
    }

    [HarmonyPatch]
    internal static class SStartDayWarnings_Patch
    {
        private static Dictionary<StartDayWarning, StartDayWarningDefinition> CustomWarnings = new Dictionary<StartDayWarning, StartDayWarningDefinition>();

        internal static WarningLevel HighestWarningLevel = WarningLevel.Safe;

        [HarmonyPatch(typeof(SStartDayWarnings), "Primary", MethodType.Getter)]
        [HarmonyPostfix]
        static void Primary_Get_Postfix(ref StartDayWarning __result)
        {
            if (__result != StartDayWarning.Ready && __result != StartDayWarning.PlayersNotReady)
                return;

            WarningLevel tempWarningLevel = WarningLevel.Safe;
            foreach (StartDayWarningDefinition startDayWarningDefinition in CustomWarnings.Values)
            {
                WarningLevel warningLevel = startDayWarningDefinition.CurrentWarningLevel;
                if (warningLevel.IsActive() &&
                    warningLevel > tempWarningLevel)
                {
                    tempWarningLevel = warningLevel;
                    __result = startDayWarningDefinition.ID;
                    if (warningLevel == WarningLevel.Error)
                        break;
                }
            }
            HighestWarningLevel = tempWarningLevel;
        }

        internal static void AddWarning(StartDayWarningDefinition startDayWarningDefinition)
        {
            if (!CustomWarnings.ContainsKey(startDayWarningDefinition.ID))
            {
                Main.LogWarning($"Added start day warning {startDayWarningDefinition.ID} ({startDayWarningDefinition.DisplayTitle})");
                CustomWarnings.Add(startDayWarningDefinition.ID, startDayWarningDefinition);
            }
        }
    }
}

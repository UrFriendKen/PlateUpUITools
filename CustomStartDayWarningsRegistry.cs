using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using static KitchenUITools.StartDayWarningDefinition;

namespace KitchenUITools
{
    public class StartDayWarningDefinition
    {
        public enum WarningPriority
        {
            Normal,
            High
        }

        internal StartDayWarning ID;
        public string DisplayTitle;
        public string Description;
        public WarningPriority Priority;

        internal WarningLevel CurrentWarningLevel => GetCurrentWarningLevel != default ? GetCurrentWarningLevel() : WarningLevel.Safe;
        public Func<WarningLevel> GetCurrentWarningLevel;

        public StartDayWarningDefinition(string displayText, string description, Func<WarningLevel> getCurrentWarningLevel)
        {
            DisplayTitle = displayText;
            Description = description;
            GetCurrentWarningLevel = getCurrentWarningLevel;
            Priority = default;
        }

        public StartDayWarningDefinition(string displayText, string description, Func<WarningLevel> getCurrentWarningLevel, WarningPriority warningPriority = default)
        {
            DisplayTitle = displayText;
            Description = description;
            GetCurrentWarningLevel = getCurrentWarningLevel;
            Priority = warningPriority;
        }
    }

    internal static class CustomStartDayWarningsRegistry
    {
        private static Dictionary<StartDayWarning, StartDayWarningDefinition> CustomWarnings = new Dictionary<StartDayWarning, StartDayWarningDefinition>();

        private static List<StartDayWarning> Priority = new List<StartDayWarning>();

        private static Dictionary<StartDayWarning, WarningLevel> WarningLevels = new Dictionary<StartDayWarning, WarningLevel>();

        private static Queue<StartDayWarningDefinition> LocalisationRegistrationQueue = new Queue<StartDayWarningDefinition>();

        internal static WarningLevel HighestWarningLevel
        {
            get
            {
                if (WarningLevels.Count < 1)
                    return WarningLevel.Safe;
                return WarningLevels.Values.Max();
            }
        }

        internal static void AddWarning(StartDayWarningDefinition startDayWarningDefinition)
        {
            if (!CustomWarnings.ContainsKey(startDayWarningDefinition.ID))
            {
                Main.LogWarning($"Added start day warning {startDayWarningDefinition.ID} ({startDayWarningDefinition.DisplayTitle})");
                CustomWarnings.Add(startDayWarningDefinition.ID, startDayWarningDefinition);

                if (startDayWarningDefinition.Priority == WarningPriority.High)
                {
                    int index = Priority.FindLastIndex(id => CustomWarnings[id].Priority == WarningPriority.High);
                    if (index == -1)
                        Priority.Insert(0, startDayWarningDefinition.ID);
                    else
                        Priority.Insert(++index, startDayWarningDefinition.ID);
                }
                else
                {
                    Priority.Add(startDayWarningDefinition.ID);
                }

            }
            RegisterLocalisation(startDayWarningDefinition);
        }

        private static void RegisterLocalisation(StartDayWarningDefinition startDayWarningDefinition)
        {
            LocalisationRegistrationQueue.Enqueue(startDayWarningDefinition);
        }

        internal static StartDayWarningDefinition DequeueNextLocalisationToRegister()
        {
            if (LocalisationRegistrationQueue.Count < 1)
                return null;
            return LocalisationRegistrationQueue.Dequeue();
        }

        internal static void UpdateWarningLevels()
        {
            foreach (KeyValuePair<StartDayWarning, StartDayWarningDefinition> kvp in CustomWarnings)
            {
                WarningLevels[kvp.Key] = kvp.Value.CurrentWarningLevel;
            }
        }

        internal static WarningLevel GetLevel(StartDayWarning startDayWarning)
        {
            return CustomWarnings.TryGetValue(startDayWarning, out StartDayWarningDefinition startDayWarningDefinition) ?
                startDayWarningDefinition.CurrentWarningLevel :
                WarningLevel.Unknown;
        }

        internal static WarningLevel GetLevel(StartDayWarningDefinition startDayWarningDefinition)
        {
            return startDayWarningDefinition.CurrentWarningLevel;
        }

        internal static bool TryGetActiveWarning(StartDayWarningDefinition.WarningPriority warningPriority, out (StartDayWarning Type, WarningLevel Level) result)
        {
            foreach (StartDayWarning item in Priority)
            {
                if (!CustomWarnings.TryGetValue(item, out StartDayWarningDefinition startDayWarningDefinition) ||
                    startDayWarningDefinition.Priority != warningPriority)
                    continue;

                WarningLevel level = GetLevel(item);
                if (level.IsActive())
                {
                    result = (Type: item, Level: level);
                    return true;
                }
            }
            result = default;
            return false;
        }
    }
}

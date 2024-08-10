using Kitchen;
using KitchenData;
using KitchenMods;
using System.Collections.Generic;
using KitchenUITools.Patches;

namespace KitchenUITools
{
    public class RegisterStartDayWarningLocalisation : GenericSystemBase, IModSystem
    {
        static Queue<StartDayWarningDefinition> ToRegister = new Queue<StartDayWarningDefinition>();

        protected override void OnUpdate()
        {
            if (ToRegister.Count > 0)
            {
                StartDayWarningDefinition startDayWarningDefinition = ToRegister.Dequeue();
                GenericLocalisationStruct loc = new GenericLocalisationStruct()
                {
                    Name = startDayWarningDefinition.DisplayTitle,
                    Description = startDayWarningDefinition.Description
                };
                if (!GameData.Main.GlobalLocalisation.StartDayWarningLocalisation.Text.ContainsKey(startDayWarningDefinition.ID))
                {
                    GameData.Main.GlobalLocalisation.StartDayWarningLocalisation.Text.Add(startDayWarningDefinition.ID, loc);
                    Main.LogInfo($"Added start day warning localisation for {startDayWarningDefinition.ID} ({loc.Name})");
                }
                else
                {
                    GameData.Main.GlobalLocalisation.StartDayWarningLocalisation.Text[startDayWarningDefinition.ID] = loc;
                    Main.LogWarning($"Overwritten start day warning localisation for {startDayWarningDefinition.ID} ({loc.Name})");
                }
            }
        }

        internal static void Register(StartDayWarningDefinition startDayWarningDefinition)
        {
            ToRegister.Enqueue(startDayWarningDefinition);
        }
    }
}

using Kitchen;
using KitchenData;
using KitchenMods;
using System.Collections.Generic;
using KitchenUITools.Patches;

namespace KitchenUITools
{
    public class RegisterStartDayWarningLocalisation : GenericSystemBase, IModSystem
    {
        protected override void OnUpdate()
        {
            StartDayWarningDefinition startDayWarningDefinition = CustomStartDayWarningsRegistry.DequeueNextLocalisationToRegister();
            if (startDayWarningDefinition == null)
                return;

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
}

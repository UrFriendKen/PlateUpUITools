using Kitchen;
using KitchenMods;

namespace KitchenUITools
{
    public class UpdateCustomStartDayWarnings : RestaurantSystem, IModSystem
    {
        protected override void OnUpdate()
        {
            CustomStartDayWarningsRegistry.UpdateWarningLevels();
        }
    }
}

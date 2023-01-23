using System;

namespace Scripts.EventsManagement
{
    public static class PlayEvents
    {
        public static event Action<bool> OnLookModeActiveChanged;
        
        // **********>>>>>> Triggers <<<<<<********** //

        public static void TriggerOnLookModeActiveChanged(bool isActive) => OnLookModeActiveChanged?.Invoke(isActive);
    }
}
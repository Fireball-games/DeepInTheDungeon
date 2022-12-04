using System;
using static Scripts.MapEditor.Enums;

namespace Scripts.EventsManagement
{
    public static class EditorEvents
    {
        public static event Action OnNewMapCreated;
        public static event Action<EWorkMode> OnWorkModeChanged; 

        // ***********    Triggers    ***********

        public static void TriggerOnNewMapCreated() => OnNewMapCreated?.Invoke();

        public static void TriggerOnWorkModeChanged(EWorkMode workMode) => OnWorkModeChanged?.Invoke(workMode);
    }
}
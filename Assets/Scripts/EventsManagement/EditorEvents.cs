using System;

namespace Scripts.EventsManagement
{
    public static class EditorEvents
    {
        public static event Action OnNewMapCreated;
        
        // ***********    Triggers    ***********

        public static void TriggerOnNewMapCreated() => OnNewMapCreated?.Invoke();
    }
}
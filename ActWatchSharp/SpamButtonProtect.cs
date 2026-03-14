namespace ActWatchSharp
{
    static class SpamButtonProtect
    {
        static Dictionary<uint, long> g_Buttons = [];
        static Dictionary<uint, long> g_Triggers = [];

        public static bool ButtonAvailableToShow(uint iID)
        {
            if (Cvar.ButtonSpam <= 0.0f) return true;
            long iTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (g_Buttons.GetValueOrDefault(iID) + Cvar.ButtonSpam * 1000 < iTime)
            {
                g_Buttons[iID] = iTime;
                return true;
            }

            return false;
        }

        public static bool TriggersAvailableToShow(uint iID)
        {
            if (Cvar.TriggerSpam <= 0.0f) return true;
            long iTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (g_Triggers.GetValueOrDefault(iID) + Cvar.TriggerSpam * 1000 < iTime)
            {
                g_Triggers[iID] = iTime;
                return true;
            }

            return false;
        }

        public static void MapStartClear()
        {
            g_Buttons.Clear();
            g_Triggers.Clear();
        }
    }
}

using static ExtradimensionalItems.Modules.ExtradimensionalItemsPlugin;

namespace ExtradimensionalItems.Modules
{
    public static class MyLogger
    {
        private static BepInEx.Logging.ManualLogSource logger;

        public static void Init(BepInEx.Logging.ManualLogSource log)
        {
            logger = log;
        }

        public static void LogWarning(object data)
        {
            logger.LogWarning(data);
        }

        public static void LogInfo(object data)
        {
            logger.LogInfo(data);
        }

        public static void LogMessage(object data)
        {
            if (ExtensiveLogging.Value)
            {
                logger.LogMessage(data);
            }
        }
    }
}

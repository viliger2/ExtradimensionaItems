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

        public static void LogError(object data)
        {
            logger.LogError(data);
        }

        public static void LogMessage(string data, params string[] args)
        {
            if (ExtensiveLogging.Value)
            {
                logger.LogMessage(string.Format(data, args));
            }
        }
    }
}

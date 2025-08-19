namespace Infrastructure
{
    public static class GlobalParams
    {
        public const string InstallersContextMenu = "Installers/";

        public const string ConfigPath = "Configs/";
        public const string TaskConfigPath = ConfigPath + "Tasks/";

        public static string ToTaskPath(this string configName)
        {
            return TaskConfigPath + configName;
        }
    }
}

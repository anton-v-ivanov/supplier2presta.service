using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public static class RobotSettings
    {
        static RobotSettings()
        {
            Config = ((RobotSettingsSection)(ConfigurationManager.GetSection("robot-settings")));
        }

        public static RobotSettingsSection Config { get; private set; }
    }
}

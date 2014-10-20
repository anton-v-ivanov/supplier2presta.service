using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public sealed class RobotSettings
    {
        private static RobotSettingsSection _config;

        static RobotSettings()
        {
            _config = ((RobotSettingsSection)(ConfigurationManager.GetSection("robot-settings")));
        }

        private RobotSettings()
        {
        }

        public static RobotSettingsSection Config
        {
            get
            {
                return _config;
            }
        }
    }
}

using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;

namespace SpotifAds
{
    public class Settings
    {
        private string _port;
        public string Port { get => _port; set { _port = value; } }

        private bool _proxyEnabled;
        public bool ProxyEnabled { get => _proxyEnabled; set { _proxyEnabled = value; } }

        private bool _launchAtStartup;
        public bool LaunchAtStartup { get => _launchAtStartup; set { _launchAtStartup = value; } }

        private bool _minimizeWhenClosed;
        public bool MinimizeWhenClosed { get => _minimizeWhenClosed; set { _minimizeWhenClosed = value; } }

        private Visibility _windowVisibility;
        public Visibility WindowVisibility { get => _windowVisibility; set { _windowVisibility = value; } }


        public void Save()
        {
            string userSettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.xml");

            using (StreamWriter sw = new StreamWriter(userSettingsPath))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(Settings));
                xmls.Serialize(sw, this);
            }
        }

        public Settings Read()
        {
            string userSettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.xml");

            if (!File.Exists(Assembly.GetExecutingAssembly().Location.Replace("SpotifAds.dll", "settings.xml")))
            {
                Port = "2468";
                Save();
            }

            using (StreamReader sw = new StreamReader(userSettingsPath))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(Settings));
                return xmls.Deserialize(sw) as Settings;
            }
        }
    }
}
